using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using DiscordNET.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DiscordNET.Modules
{
	public class MusicModule : ModuleBase<SocketCommandContext>
	{
		private readonly LavaNode _lavaNode;
		private readonly MusicManager _musicManager;
		private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

		public MusicModule ( LavaNode lavaNode, MusicManager musicManager, InteractiveService interactivity )
		{
			_musicManager = musicManager;
			_lavaNode = lavaNode;
			_interactivity = interactivity;
		}

		[Command("Join")]
		public async Task JoinAsync ()
		{
			if (_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync("I'm already connected to a voice channel!");
				return;
			}

			var voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}

			try
			{
				await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
				await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
			}
			catch (Exception exception)
			{
				await ReplyAsync(exception.Message);
			}
		}

		[Command("Leave")]
		public async Task LeaveAsync ()
		{
			if (!_lavaNode.TryGetPlayer (Context.Guild, out var player))
			{
				await ReplyAsync("I'm not connected to any voice channels!");
				return;
			}

			var voiceChannel = (Context.User as IVoiceState).VoiceChannel ?? player.VoiceChannel;
			if (voiceChannel == null)
			{
				await ReplyAsync("Not sure which voice channel to disconnect from.");
				return;
			}

			try
			{
				await _lavaNode.LeaveAsync(voiceChannel);
				await ReplyAsync($"I've left {voiceChannel.Name}!");
			}
			catch (Exception exception)
			{
				await ReplyAsync(exception.Message);
			}
		}

		[Command("play")]
		public async Task Play ([Remainder] string  query)
		{
			var results = await _lavaNode.SearchAsync(query);
			var player = _lavaNode.GetPlayer(Context.Guild);

			LavaTrack selectedTrack = default(LavaTrack);

			switch (results.LoadStatus)
			{
				case LoadStatus.TrackLoaded:
					selectedTrack = results.Tracks[0];
					break;
				case LoadStatus.PlaylistLoaded:
					foreach (var track in results.Tracks)
					{
						await _musicManager.Queue.Enqueue(track, Context);
					}

					await _musicManager.PlaylistEmbed(query, Context);

					if (player.PlayerState != Victoria.Enums.PlayerState.Playing && player.PlayerState != Victoria.Enums.PlayerState.Paused)
					{
						var list = await _musicManager.Queue.GetItems();

						var trackCollection = list.FirstOrDefault();

						var result = await _musicManager.Queue.TryDequeue(trackCollection.Track);

						if (result)
						{
							await player.PlayAsync(trackCollection.Track);
							await _musicManager.MusicEmbed(trackCollection);
						}
						return;
					}
					break;
				case LoadStatus.SearchResult:
					break;
				case LoadStatus.LoadFailed:
				case LoadStatus.NoMatches:
					var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
					var choice = new List<string>();

					for (int i = 1; i <= 5; i++)
					{
						var item = searchResponse.Tracks.ElementAt(i - 1);
						choice.Add($"{i}.\t{item.Title}...\t({item.Duration})");
					}

					var tracksEmbed = new EmbedBuilder()
					{
						Title = "Please Select the Desired Track",
						Description = string.Join("\n", choice),
						Color = Color.DarkPurple
					}.Build();

					var searchMessage = await Context.Channel.SendMessageAsync(embed: tracksEmbed);

					var interactivity = new InteractiveService(Context.Client.GetShardFor(Context.Guild));
					var response = await interactivity.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromMinutes(1));

					if (int.TryParse(response.Content, out int index))
					{
						if (index > 5 || index < 1)
						{
							await Context.Channel.SendMessageAsync("Did not respond correctly");
							await Context.Channel.DeleteMessageAsync(searchMessage);
							return;
						}

						selectedTrack = searchResponse.Tracks[index - 1];
					}
					else
					{
						await Context.Channel.SendMessageAsync("Did not respond correctly");
						await Context.Channel.DeleteMessageAsync(searchMessage);
						return;
					}
					break;
				default:
					break;
			}

			if (player.PlayerState == Victoria.Enums.PlayerState.Playing || player.PlayerState == Victoria.Enums.PlayerState.Paused)
			{
				await _musicManager.Queue.Enqueue(selectedTrack, Context);

				await _musicManager.QueueEmbed(selectedTrack, Context);

				return;
			}
			else
			{
				await _musicManager.MusicEmbed(selectedTrack, Context);
				await _lavaNode.GetPlayer(Context.Guild).PlayAsync(selectedTrack);
				return;
			}
		}

		[Command("pause")]
		public async Task Pause ()
		{
			var player = _lavaNode.GetPlayer(Context.Guild);

			if (player.PlayerState != Victoria.Enums.PlayerState.Playing)
			{
				await Context.Channel.SendMessageAsync("Nothing is playing.");
				return;
			}

			await player.PauseAsync();
		}

		[Command("skip")]
		public async Task Skip ( int count = 1 )
		{
			var player = _lavaNode.GetPlayer(Context.Guild);

			if (count > await _musicManager.Queue.GetQueueCount())
			{
				await Context.Channel.SendMessageAsync("Not Enough Items to Skip");
				return;
			}

			var tracks = await _musicManager.Queue.GetItems();
			var track = tracks.ElementAt(count - 1);

			await _musicManager.Queue.Remove(count);

			await player.PlayAsync(track.Track);
			await _musicManager.MusicEmbed(track);
		}

		[Command("resume")]
		public async Task Resume ()
		{
			var player = _lavaNode.GetPlayer(Context.Guild);

			if (player.PlayerState != Victoria.Enums.PlayerState.Paused && player.PlayerState != Victoria.Enums.PlayerState.Stopped)
			{
				if(await _musicManager.Queue.GetQueueCount() != 0)
				{
					var list = await _musicManager.Queue.GetItems();

					var trackCollection = list.FirstOrDefault();

					var result = await _musicManager.Queue.TryDequeue(trackCollection.Track);

					if (result)
					{
						await player.PlayAsync(trackCollection.Track);
						await _musicManager.MusicEmbed(trackCollection);
					}
					return;
				}
				else
				{
					await ReplyAsync("Nothing is paused rigth now");
					return;
				}
			}

			await player.ResumeAsync();
		}

		[Command("volume")]
		public async Task Volume ( ushort? volume )
		{
			var player = _lavaNode.GetPlayer(Context.Guild);
			if (volume > 100 || volume < 0)
			{
				await Context.Channel.SendMessageAsync("Volume setting must be between 100 and 0");
				return;
			}

			if (volume == null)
			{
				await Context.Channel.SendMessageAsync($":speaker: **Current Volume Setting:** {player.Volume}");
				return;
			}

			await player.UpdateVolumeAsync(Convert.ToUInt16(volume));
		}

		[Command("Stop")]
		public async Task Stop ()
		{
			var player = _lavaNode.GetPlayer(Context.Guild);

			if (player.PlayerState != Victoria.Enums.PlayerState.Playing)
			{
				await Context.Channel.SendMessageAsync("Nothing is Playing");
				return;
			}

			await player.StopAsync();
		}

		[Command("fastforward")]
		[Alias("forward")]
		public async Task FastForward ( TimeSpan time )
		{
			var player = _lavaNode.GetPlayer(Context.Guild);
			var currentTrack = player.Track;

			if (currentTrack.Position + time > currentTrack.Duration)
			{
				await Context.Channel.SendMessageAsync("Cannot skip further than the end of track!");
				return;
			}

			await player.SeekAsync(currentTrack.Position + time);
		}

		[Command("rewind")]
		[Alias("back")]
		public async Task Rewind ( TimeSpan time )
		{
			var player = _lavaNode.GetPlayer(Context.Guild);
			var currentTrack = player.Track;

			if (currentTrack.Position - time < TimeSpan.Zero)
			{
				await player.SeekAsync(TimeSpan.Zero);
				return;
			}

			await player.SeekAsync(currentTrack.Position - time);
		}

		[Command("dispose")]
		[Alias("clear")]
		public async Task Dispose ()
		{
			var player = _lavaNode.GetPlayer(Context.Guild);
			await player.StopAsync();
			await _musicManager.Queue.Dispose();
		}

		[Command("lyrics")]
		public async Task Lyrics ()
		{
			if(!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
				await ReplyAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await ReplyAsync("Woaaah there, I'm not playing any tracks.");
				return;
			}

			var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
			if (string.IsNullOrWhiteSpace(lyrics))
			{
				await ReplyAsync($"No lyrics found for {player.Track.Title}");
				return;
			}

			var splitLyrics = lyrics.Split('\n');
			var stringBuilder = new StringBuilder();
			foreach (var line in splitLyrics)
			{
				if (Range.Contains(stringBuilder.Length))
				{
					await ReplyAsync($"```{stringBuilder}```");
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.AppendLine(line);
				}
			}

			await ReplyAsync($"```{stringBuilder}```");
		}

		[Command("lyrics alt")]
		public async Task Lyrics2 ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("I'm not connected to a voice channel.");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await ReplyAsync("Woaaah there, I'm not playing any tracks.");
				return;
			}

			var lyrics = await player.Track.FetchLyricsFromOVHAsync();
			if (string.IsNullOrWhiteSpace(lyrics))
			{
				await ReplyAsync($"No lyrics found for {player.Track.Title}");
				return;
			}

			var splitLyrics = lyrics.Split('\n');
			var stringBuilder = new StringBuilder();
			foreach (var line in splitLyrics)
			{
				if (Range.Contains(stringBuilder.Length))
				{
					await ReplyAsync($"```{stringBuilder}```");
					stringBuilder.Clear();
				}
				else
				{
					stringBuilder.AppendLine(line);
				}
			}

			await ReplyAsync($"```{stringBuilder}```");
		}

	[Command("now playing")]
		[Alias("now")]
		public async Task NowPlaying()
		{
			var player = _lavaNode.GetPlayer(Context.Guild);

			if (player.PlayerState != PlayerState.Playing)
			{
				await ReplyAsync("Woaaah there, I'm not playing any tracks.");
				return;
			}

			var track = player.Track;
			await _musicManager.MusicEmbed(track, Context);
		}

		[Command("queue")]
		public async Task Queue()
		{
			var queue = _musicManager.Queue;
			var items = await queue.GetItems();

			var description = new List<string>();
   
			if(items.Count > 10)
			{
				for(int i = 0; i < 10; i++)
				{
					var trackCollection = items[i];
					description.Add($"**{i + 1}.** `{trackCollection.Track.Title}` [{trackCollection.Track.Duration}] Added by: {trackCollection.User.Username}");
				}
			}
			else if (items.Count > 0)
			{
				var count = 1;
				foreach(var trackCollection in items)
				{
					description.Add($"**{count}.** `{trackCollection.Track.Title}` [{trackCollection.Track.Duration}] Added by: {trackCollection.User.Username}");
					count++;
				}
			}
			else
			{
				await Context.Channel.SendMessageAsync("There are no tracks in the queue.");
			}

			var queueEmbed = new EmbedBuilder
			{
				Title = $"Queue for {Context.Guild.Name}",
				Description = string.Join("\n", description),
				Footer = new EmbedFooterBuilder
				{
					Text = $"Number of Tracks: {items.Count}"
				},
				Color = Color.DarkPurple
			}.Build();

			await Context.Channel.SendMessageAsync(embed: queueEmbed);
		}
	}
}

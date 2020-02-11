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
	public class MusicModule : ModuleBase<ShardedCommandContext>
	{
		private readonly LavaNode _lavaNode;
		private readonly MusicManager _musicManager;
		private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

		public MusicModule ( LavaNode lavaNode, MusicManager musicManager )
		{
			_musicManager = musicManager;
			_lavaNode = lavaNode;
		}

		[Command("Join")]
		[Summary("Create a music player instance and summon it to your current voice channel")]
		public async Task Join ()
		{
			await JoinAsync();
		}

		private async Task<bool> JoinAsync ()
		{
			if (_lavaNode.HasPlayer(Context.Guild))
			{
				await ReplyAsync(embed: new EmbedBuilder
				{
					Description = "I'm already connected to a voice channel",
					Fields = new List<EmbedFieldBuilder> { new EmbedFieldBuilder { IsInline = true, Name = "Hint", Value = "To change the voice channel, use the `MOVE` command." } }
				}.Build());
				return false;
			}

			var voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return false;
			}

			try
			{
				await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
				await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
				return true;
			}
			catch (Exception exception)
			{
				await ReplyAsync(exception.Message);
				return false;
			}
		}

		[Command("Leave")]
		[Summary("Make the bot leave its current voice channel, consequently stopping the currently playing track")]
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

		[Command("move")]
		[Summary("Use to move the active instance of the bot to a different voice channel")]
		public async Task Move()
		{
			var voiceState = Context.User as IVoiceState;
			if(voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}
			await _lavaNode.MoveChannelAsync(voiceState.VoiceChannel);
		}

		[Command("play")]
		[Summary("Play music from a search query, youtube track link or queue a youtube playlist using URL links")]
		public async Task Play ([Summary("Youtube search query")][Remainder] string  query)
		{
			var voiceState = Context.User as IVoiceState;
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var _player) || _player.VoiceChannel != voiceState.VoiceChannel)
			{
				if (!await JoinAsync()) return;
			}
   
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
						Color = Color.DarkPurple,
						Footer = new EmbedFooterBuilder
						{
							Text = "Type `cancel` to Cancel the Query"
						}
					}.Build();

					var searchMessage = await Context.Channel.SendMessageAsync(embed: tracksEmbed);

					var interactivity = new InteractiveService(Context.Client.GetShardFor(Context.Guild));
					var response = await interactivity.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromMinutes(1));
					if(response.Content.ToLower() == "cancel")
					{
						await Context.Channel.SendMessageAsync("Canceled the  Query");
						await Context.Channel.DeleteMessageAsync(searchMessage);
						return;
					}

					if (int.TryParse(response.Content, out int index))
					{
						if (index > 5 || index < 1)
						{
							await Context.Channel.SendMessageAsync("Did not respond correctly");
							await Context.Channel.DeleteMessageAsync(searchMessage);
							return;
						}

						selectedTrack = searchResponse.Tracks[index - 1];
						await Context.Channel.DeleteMessageAsync(searchMessage);
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

			if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
			{
				await _musicManager.Queue.Enqueue(selectedTrack, Context);

				await _musicManager.QueueEmbed(selectedTrack, Context);
			}
			else
			{
				await _musicManager.MusicEmbed(selectedTrack, Context);
				await _lavaNode.GetPlayer(Context.Guild).PlayAsync(selectedTrack);
			}
		}

		[Command("pause")]
		[Summary("Pause the currently playing song to be resumed later")]
		public async Task Pause ()
		{
			if(!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await Context.Channel.SendMessageAsync("Nothing is playing.");
				return;
			}

			await player.PauseAsync();
		}

		[Command("skip")]
		[Summary("Skip the  desired number of tracks from the queue")]
		public async Task Skip ( [Summary("Number of tracks to be skipped")]int count = 1 )
		{
			if(!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if(count < 0)
			{
				await ReplyAsync("Please enter a valid value");
				return;
			}

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
		[Summary("Resume the paused track")]
		public async Task Resume ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (player.PlayerState != PlayerState.Paused && player.PlayerState != PlayerState.Stopped)
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
		[Summary("Set the volume of the music player or get the current volume value")]
		public async Task Volume ( [Summary("Value to be set as the volume gain(Leave blank to get the current value)")]ushort? volume = null )
		{
			if(!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("There is currently no active player in this server!");
				return;
			}
			if (volume > 100 || volume < 0)
			{
				await Context.Channel.SendMessageAsync("Volume setting must be between 100 and 0");
				return;
			}

			if (volume == null)
			{
				var currentVolume = player?.Volume;
				await Context.Channel.SendMessageAsync($":speaker: **Current Volume Setting:** {currentVolume}");
				return;
			}

			await player.UpdateVolumeAsync(Convert.ToUInt16(volume));
			await ReplyAsync($":speaker: **Volume Set To:** {volume}");
		}

		[Command("Stop")]
		[Summary("Stop music playback for the current track")]
		public async Task Stop ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await Context.Channel.SendMessageAsync("Nothing is Playing");
				return;
			}

			await player.StopAsync();
		}

		[Command("fastforward")]
		[Alias("forward")]
		[Summary("Fast forward the current track a desired amount of time")]
		public async Task FastForward ( [Summary("Format: `xx`H`xx`M`xx`S")]TimeSpan time )
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
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
		[Summary("Rewind the current track a desired amount of time")]
		public async Task Rewind ( [Summary("Format: `xx`H`xx`M`xx`S")]TimeSpan time )
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
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
		[Summary("Stop the  music playback and clear the queue")]
		public async Task Dispose ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
			await player.StopAsync();
			await _musicManager.Queue.Dispose();
		}

		[Command("genius")]
		[Summary("Get the lyrics for the currently playing  song from genius")]
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

		[Command("lyrics")]
		[Summary("Get the lyrics for the currently playing song from alternate source")]
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
		[Summary("Get the info card for the currently playing song")]
		public async Task NowPlaying()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await ReplyAsync("Woaaah there, I'm not playing any tracks.");
				return;
			}

			var track = player.Track;
			await _musicManager.MusicEmbed(track, Context);
		}

		[Command("queue")]
		[Summary("Get a listing of the queue")]
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

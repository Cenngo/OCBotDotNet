using Discord;
using Discord.Commands;
using DiscordNET.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using DiscordNET.Extensions;
using System.Text.RegularExpressions;
using Discord.Addons.Interactive;
using Discord.Rest;
using Victoria.Responses.Rest;
using DiscordNET.Handlers;
using Discord.WebSocket;
using DiscordNET.Data.Genius;
using DiscordNET.Data;

namespace DiscordNET.Modules
{
	public class MusicModule : ModuleBase<ShardedCommandContext>
	{
		private readonly LavaNode _lavaNode;
		private readonly MusicManager _musicManager;
		private readonly Auth _auth;
		private readonly DiscordShardedClient _client;

		public MusicModule ( LavaNode lavaNode, MusicManager musicManager, Auth auth, DiscordShardedClient client )
		{
			_musicManager = musicManager;
			_lavaNode = lavaNode;
			_auth = auth;
			_client = client;
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

			IVoiceState voiceState = Context.User as IVoiceState;

			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return false;
			}

			try
			{
				var player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
				if (player != null)
				{
					await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
					return true;
				}
				else
					return false;
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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("I'm not connected to any voice channels!");
				return;
			}

			IVoiceChannel voiceChannel = (Context.User as IVoiceState).VoiceChannel ?? player.VoiceChannel;
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
		public async Task Move ()
		{
			IVoiceState voiceState = Context.User as IVoiceState;
			if (voiceState?.VoiceChannel == null)
			{
				await ReplyAsync("You must be connected to a voice channel!");
				return;
			}
			await _lavaNode.MoveChannelAsync(voiceState.VoiceChannel);
		}

		[Command("play")]
		[Summary("Play music from a search query, youtube track link or queue a youtube playlist using URL links")]
		public async Task Play ( [Summary("Youtube search query")][Remainder] string query )
		{
			IVoiceState voiceState = Context.User as IVoiceState;
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer _player) || _player.VoiceChannel != voiceState.VoiceChannel)
			{
				if (!await JoinAsync()) return;
			}

			SearchResponse results = await _lavaNode.SearchAsync(query);
			LavaPlayer player = _lavaNode.GetPlayer(Context.Guild);

			LavaTrack selectedTrack = default;

			switch (results.LoadStatus)
			{
				case LoadStatus.TrackLoaded:
					selectedTrack = results.Tracks[0];
					break;
				case LoadStatus.PlaylistLoaded:
					foreach (LavaTrack track in results.Tracks)
					{
						player.Queue.Enqueue(new LavaTrackWithUser(track, Context.User, Context.Channel));
					}

					await _musicManager.PlaylistEmbed(query, Context);

					if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
					{
						if(player.Queue.TryDequeue(out var track))
						{
							await player.PlayAsync(((LavaTrackWithUser) track).Track);
							await _musicManager.MusicEmbed(((LavaTrackWithUser)track));
						}
						return;
					}
					return;
				case LoadStatus.SearchResult:
					break;
				case LoadStatus.LoadFailed:
				case LoadStatus.NoMatches:
					SearchResponse searchResponse = await _lavaNode.SearchYouTubeAsync(query);
					List<string> choice = new List<string>();

					for (int i = 1; i <= 5; i++)
					{
						LavaTrack item = searchResponse.Tracks.ElementAt(i - 1);
						choice.Add($"{i}.\t{item.Title}...\t({item.Duration})");
					}

					Embed tracksEmbed = new EmbedBuilder()
					{
						Author = new EmbedAuthorBuilder
						{
							Name = "Write just the number of trak. Exp:\" 4 \""
						},
						Title = "Please Select the Desired Track",
						Description = string.Join("\n", choice),
						Color = Color.DarkPurple,
						Footer = new EmbedFooterBuilder
						{
							Text = "Type `cancel` to Cancel the Query"
						}
					}.Build();

					RestUserMessage searchMessage = await Context.Channel.SendMessageAsync(embed: tracksEmbed);

					InteractiveService interactivity = new InteractiveService(Context.Client.GetShardFor(Context.Guild));
					SocketMessage response = await interactivity.NextMessageAsync(Context, true, true, TimeSpan.FromMinutes(1));
					if (response.Content.ToLower() == "cancel")
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
				player.Queue.Enqueue(new LavaTrackWithUser(selectedTrack, Context.User, Context.Channel));

				await _musicManager.QueueEmbed(new LavaTrackWithUser(selectedTrack, Context.User, Context.Channel),
					player.Queue.Items.Count());
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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (count < 0)
			{
				await ReplyAsync("Please enter a valid value");
				return;
			}

			if(count == 1 && player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
			{
				await player.StopAsync();
			}

			if (count > player.Queue.Count)
			{
				await Context.Channel.SendMessageAsync("Not Enough Items to Skip");
				return;
			}

			if (count == 1)
			{
				await player.PlayAsync((player.Queue.Items.First() as LavaTrackWithUser).Track);
				await _musicManager.MusicEmbed(player.Queue.Items.ElementAt(0) as  LavaTrackWithUser);
				player.Queue.RemoveAt(0);
			}
			else
			{
				await player.PlayAsync((player.Queue.Items.ElementAt(count - 1) as LavaTrackWithUser).Track);
				player.Queue.RemoveRange(0, count - 1);
				await _musicManager.MusicEmbed(((IEnumerable<LavaTrackWithUser>) player.Queue.Items).First());
			}
		}

		[Command("resume")]
		[Summary("Resume the paused track")]
		public async Task Resume ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}


			await player.ResumeAsync();
		}

		[Command("volume")]
		[Summary("Set the volume of the music player or get the current volume value")]
		public async Task Volume ( [Summary("Value to be set as the volume gain(Leave blank to get the current value)")]ushort? volume = null )
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
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
				int? currentVolume = player?.Volume;
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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
			LavaTrack currentTrack = player.Track;

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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
			LavaTrack currentTrack = player.Track;

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
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}
			await player.StopAsync();
			player.Queue.Clear();
		}

		[Command("lyrics")]
		[Summary("Get the lyrics for the currently playing  song from Genius")]
		public async Task Lyrics ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;
			
			LavaTrack track = player.Track;
			string query = track.Title;
			string lyrics = await track.GeniusLyrics(_auth.GeniusToken);

			if (lyrics == null)
			{
				await ReplyAsync($"No lyrics were found for {query}");
				return;
			}

			GSResult result = track.SearchGenius(_auth.GeniusToken).Response.Hits.First().Result;

			string message = $"**{result.FullTitle.ToUpper()}**\n```ini\n{lyrics}\n```";

			await ReplyAsync(message);
			
		}

		[Command("now playing")]
		[Alias("now")]
		[Summary("Get the info card for the currently playing song")]
		public async Task NowPlaying ()
		{
			if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{
				await ReplyAsync("No active music player found!");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				await ReplyAsync("Woaaah there, I'm not playing any tracks.");
				return;
			}

			LavaTrack track = player.Track;
			await _musicManager.MusicEmbed(track, Context);
		}

		[Command("queue")]
		[Summary("Get a listing of the queue")]
		public async Task Queue (string scope = "top10")
		{
			if (_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
			{

				var Queue = player.Queue.Items.Cast<LavaTrackWithUser>();
				var duration = new TimeSpan();

				foreach(var item in Queue)
                {
					duration += item.Track.Duration;
				}

				List<string> description = new List<string>();

				if(Queue.Count() < 1)
				{
					await ReplyAsync("There are no items in the queue");
					return;
				}

				for(int i = 0; i < Queue.Count() && (scope == "all" || i < 10); i++)
                {
					var track = Queue.ElementAt(i);
					description.Add($"**{i + 1}.** `{track.Track.Title}` [{track.Track.Duration}] Added by: {track.User.Username}");
				}

				Embed queueEmbed = new EmbedBuilder
				{
					Title = $"Queue for {Context.Guild.Name}",
					Description = string.Join("\n", description),
					Footer = new EmbedFooterBuilder
					{
						Text = $"Number of Tracks: {Queue.Count()}\tEstimated Duration: "
					},
					Color = Color.DarkPurple
				}.Build();

				await Context.Channel.SendMessageAsync(embed: queueEmbed);
			}
		}

		public async Task QueueSwap(int first, int  second)
        {
			if(_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
				var queue = player.Queue;
				
            }
        }

		[Command("rickroll")]
		[Alias("rr")]
		public async Task Rickroll (ulong channelId)
        {
			var channel = Context.Guild.GetVoiceChannel(channelId);
			if (channel == null)
				return;

			await PlayRickroll(channel);
        }

		[Command("rickroll")]
		[Alias("rr")]
		public async Task Rickroll (string mention )
        {
			var user = Context.Message.MentionedUsers.First();
			var channels = Context.Guild.VoiceChannels;

			foreach(var channel in channels)
            {
                if (channel.Users.Contains(user))
                {
					await PlayRickroll(channel);
					return;
				}
            }
			await ReplyAsync("Couldn't Find the User");
        }

		private async Task PlayRickroll ( IVoiceChannel channel )
        {
			var search = await _lavaNode.SearchYouTubeAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
			if (search.Tracks.Count > 0)
			{
				if (!_lavaNode.HasPlayer(Context.Guild))
				{
					var player = await _lavaNode.JoinAsync(channel);
					await player.PlayAsync(search.Tracks[0]);
				}
				else
				{
					await _lavaNode.MoveChannelAsync(channel);
					if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
					{
						await player.PlayAsync(search.Tracks[0]);
					}
				}
			}
		}
	}
}

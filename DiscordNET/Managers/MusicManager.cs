using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using R6Api.Models;
using Raven.Client.ServerWide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Rest;
using Timer = System.Timers.Timer;

namespace DiscordNET.Managers
{
	public class MusicManager
	{
		private readonly DiscordShardedClient _client;
		private readonly LavaNode _lavaNode;
		private Dictionary<LavaPlayer, DateTime> _playerTimeStamps;
		private Timer Timer;
		private int _cooldown;

		public MusicManager ( DiscordShardedClient client, LavaNode lavaNode, int? cooldown )
		{
			_client = client;
			_lavaNode = lavaNode;
			CheckVoiceChannel().GetAwaiter();

			_client.ShardReady += OnReady;
			_lavaNode.OnTrackEnded += OnTrackEnded;
			_lavaNode.OnTrackStuck += OnTrackStuck;
			_lavaNode.OnLog += LavaNode_OnLog;
			_lavaNode.OnTrackException += OnTrackException;
			_lavaNode.OnPlayerUpdated += OnPLayerUpdate;

			_cooldown = cooldown ?? 4;

			_playerTimeStamps = new Dictionary<LavaPlayer, DateTime>();
			Timer = new Timer(2000);
			Timer.Elapsed += CheckCooldown;
			Timer.Start();
		}

		private void CheckCooldown ( object sender, ElapsedEventArgs e )
		{
			foreach(var instance in _playerTimeStamps)
			{
				if (instance.Value + new TimeSpan(0, _cooldown, 0) < DateTime.Now)
				{
					_lavaNode.LeaveAsync(instance.Key.VoiceChannel);
					_playerTimeStamps.Remove(instance.Key);
				}
			}

			foreach(var player in _lavaNode.Players)
			{
				if (player.PlayerState != PlayerState.Playing)
				{
					if (!_playerTimeStamps.ContainsKey(player))
					{
						_playerTimeStamps.Add(player, DateTime.Now);
					}
				}
				else if (player.PlayerState == PlayerState.Playing)
				{
					if (_playerTimeStamps.ContainsKey(player))
						_playerTimeStamps.Remove(player);
				}
			}
		}

		private async Task OnPLayerUpdate ( PlayerUpdateEventArgs arg )
		{
			IChannel voiceChannel = arg.Player.VoiceChannel as IChannel;

			IEnumerable<IUser> users = await voiceChannel.GetUsersAsync().FlattenAsync();
			if (users.Count(x => !x.IsBot) == 0)
				await _lavaNode.LeaveAsync(arg.Player.VoiceChannel);

			/*if(arg.Player.PlayerState == Victoria.Enums.PlayerState.Stopped || arg.Player.PlayerState == Victoria.Enums.PlayerState.Paused)
			{
				if (_playerTimeStamps.ContainsKey(arg.Player))
					_playerTimeStamps.Remove(arg.Player);

				_playerTimeStamps.Add(arg.Player, DateTime.Now);
			}
			else if(arg.Player.PlayerState == PlayerState.Playing)
			{
				if (_playerTimeStamps.ContainsKey(arg.Player))
					_playerTimeStamps.Remove(arg.Player);
			}*/
		}

		private async Task CheckVoiceChannel()
		{
			/*while (true)
			{
				IEnumerable<SocketGuild> guilds = _client.Guilds;

				foreach (var guild in guilds)
				{
					if (_lavaNode.TryGetPlayer(guild, out var player))
					{
						var voiceChannel = player.VoiceChannel;

						var users = await voiceChannel.GetUsersAsync().FlattenAsync();

						if (users.Count(x => !x.IsBot) == 0)
						{
							await _lavaNode.LeaveAsync(voiceChannel);
						}

						if (player.VoiceChannel == guild.AFKChannel)
							await _lavaNode.LeaveAsync(voiceChannel);
					}
				}

				await Task.Delay(60000);
			}*/
		}


		private Task OnTrackException ( TrackExceptionEventArgs arg )
		{
			ITextChannel textChannel = arg.Player.TextChannel;
			textChannel.SendMessageAsync("An Error has Occured whilst Playing the Track");

			return Task.CompletedTask;
		}

		private Task LavaNode_OnLog ( LogMessage arg )
		{
			StringBuilder infoString = new StringBuilder();
			StringBuilder messageString = new StringBuilder();

			infoString.AppendJoin(" ", DateTime.Now.ToString("hh:mm:ss"), arg.Source);

			messageString.AppendJoin(" ", arg.Message, arg.Exception);

			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.Write(infoString.ToString());
			Console.ResetColor();
			Console.WriteLine($"\t {messageString}");

			return Task.CompletedTask;
		}

		private Task OnTrackStuck ( TrackStuckEventArgs arg )
		{
			LavaPlayer player = arg.Player;
			player.StopAsync();
			player.TextChannel.SendMessageAsync("Encountered a problem while playing");

			return Task.CompletedTask;
		}

		private async Task OnTrackEnded ( TrackEndedEventArgs arg )
		{
			if (!arg.Reason.ShouldPlayNext()) return;

			if (arg.Player.Queue.TryDequeue(out var track))
			{
				await arg.Player.PlayAsync(((LavaTrackWithUser) track).Track);
			}
		}

		private async Task OnReady (DiscordSocketClient client)
		{
			await _lavaNode.ConnectAsync();
		}

		public async Task MusicEmbed ( LavaTrackWithUser lavatrack )
		{
			string videoId = lavatrack.Track.Url.Substring(lavatrack.Track.Url.Length - 11);
			string thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			EmbedBuilder embed = new EmbedBuilder
			{
				Title = lavatrack.Track.Title,
				Url = lavatrack.Track.Url,
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = lavatrack.User.GetAvatarUrl(),
					Name = "Now Playing"
				}
			}
			.AddField("Channel", lavatrack.Track.Author, true)
			.AddField("Duration", lavatrack.Track.Duration.ToString(), true);

			Embed msg = embed.Build();

			await (lavatrack.Channel as ISocketMessageChannel).SendMessageAsync(embed: msg);
		}

		public async Task MusicEmbed ( LavaTrack track, ShardedCommandContext context )
		{
			string videoId = track.Url.Substring(track.Url.Length - 11);
			string thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			EmbedBuilder embed = new EmbedBuilder
			{
				Title = track.Title,
				Url = track.Url,
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = context.Message.Author.GetAvatarUrl(),
					Name = "Now Playing"
				}
			}
			.AddField("Channel", track.Author, true)
			.AddField("Duration", track.Duration.ToString(), true);

			Embed msg = embed.Build();

			await context.Channel.SendMessageAsync(embed: msg);
		}

		public async Task QueueEmbed ( LavaTrackWithUser lavatrack, int order )
		{
			string videoId = lavatrack.Track.Url.Substring(lavatrack.Track.Url.Length - 11);
			string thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			EmbedBuilder embed = new EmbedBuilder
			{
				Title = lavatrack.Track.Title,
				Url = lavatrack.Track.Url,
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = lavatrack.User.GetAvatarUrl(),
					Name = "Added to Queue"
				}
			}
			.AddField("Channel", lavatrack.Track.Author, true)
			.AddField("Duration", lavatrack.Track.Duration.ToString(), true)
			.AddField("Queue Order", order, true);

			Embed msg = embed.Build();

			await (lavatrack.Channel as ISocketMessageChannel).SendMessageAsync(embed: msg);
		}

		public async Task PlaylistEmbed ( string query, ShardedCommandContext context )
		{
			SearchResponse results = await _lavaNode.SearchAsync(query);

			LavaTrack track = results.Tracks[0];
			string name = results.Playlist.Name;
			TimeSpan duration = TimeSpan.Zero;
			int count = 0;

			string videoId = track.Url.Substring(track.Url.Length - 11);
			string thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			foreach (LavaTrack item in results.Tracks)
			{
				count++;
				duration += item.Duration;
			}

			EmbedBuilder embed = new EmbedBuilder
			{
				Title = name,
				Url = query,
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = context.User.GetAvatarUrl(),
					Name = "Added Playlist"
				}
			}
			.AddField("Number of Tracks", count, true)
			.AddField("Duration", duration, true);

			Embed msg = embed.Build();

			await context.Channel.SendMessageAsync(embed: msg);
		}
	}
}

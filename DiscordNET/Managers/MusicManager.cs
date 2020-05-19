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
		private Timer _timer;

		public MusicManager ( DiscordShardedClient client, LavaNode lavaNode )
		{
			_client = client;
			_lavaNode = lavaNode;

			_client.ShardReady += OnReady;
			_lavaNode.OnTrackEnded += OnTrackEnded;
			_lavaNode.OnTrackStuck += OnTrackStuck;
			_lavaNode.OnLog += LavaNode_OnLog;
			_lavaNode.OnTrackException += OnTrackException;
			_lavaNode.OnPlayerUpdated += OnPLayerUpdate;

			_playerTimeStamps = new Dictionary<LavaPlayer, DateTime>();
			_timer = new Timer(2000);
			_timer.Elapsed += CheckCooldown;
			_timer.Start();
		}

		private void CheckCooldown ( object sender, ElapsedEventArgs e )
		{
			foreach(var instance in _playerTimeStamps)
			{
				if (instance.Value + new TimeSpan(0, 4, 0) < DateTime.Now)
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

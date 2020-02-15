using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace DiscordNET.Managers
{
	public class MusicManager
	{
		private readonly DiscordShardedClient _client;
		private readonly LavaNode _lavaNode;
		public QueueHandler Queue;

		public MusicManager ( DiscordShardedClient client, LavaNode lavaNode, QueueHandler queue = null )
		{
			_client = client;
			_lavaNode = lavaNode;
			Queue = queue ?? new QueueHandler(_lavaNode, _client);

			_client.ShardReady += OnReady;
			_lavaNode.OnTrackEnded += OnTrackEnded;
			_lavaNode.OnTrackStuck += OnTrackStuck;
			_lavaNode.OnLog += LavaNode_OnLog;
			_lavaNode.OnTrackException += OnTrackException;
		}

		private Task OnTrackException ( TrackExceptionEventArgs arg )
		{
			ITextChannel textChannel = arg.Player.TextChannel;
			textChannel.SendMessageAsync("An Error has Occured whilst Playing the Track");

			return Task.CompletedTask;
		}

		private Task LavaNode_OnLog ( LogMessage arg )
		{
			var argArray = arg.ToString().Split(" ");
			var info = argArray[0] + " " + argArray[1] + " " + argArray[2];
			string remainder = string.Empty;

			for (int i = 3; i < argArray.Length; i++)
			{
				remainder += " " + argArray[i];
			}
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(info);
			Console.ResetColor();
			Console.Write(remainder + "\n");

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

			var list = await Queue.GetItems();

			var track = list.FirstOrDefault();

			var result = await Queue.TryDequeue(track.Track);

			if (result)
			{
				await arg.Player.PlayAsync(track.Track);
				await MusicEmbed(track);
			}
			else
			{
				return;
			}
		}

		private async Task OnReady (DiscordSocketClient client)
		{
			await _lavaNode.ConnectAsync();
		}

		public async Task MusicEmbed ( QueueTrack trackCollection )
		{
			var videoId = trackCollection.Track.Url.ToString().Substring(trackCollection.Track.Url.ToString().Length - 11);
			var thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			var embed = new EmbedBuilder
			{
				Title = trackCollection.Track.Title.ToString(),
				Url = trackCollection.Track.Url.ToString(),
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = trackCollection.User.GetAvatarUrl(),
					Name = "Now Playing"
				}
			}
			.AddField("Channel", trackCollection.Track.Author, true)
			.AddField("Duration", trackCollection.Track.Duration.ToString(), true);

			var msg = embed.Build();

			await trackCollection.Channel.SendMessageAsync(embed: msg);
		}

		public async Task MusicEmbed ( LavaTrack track, ShardedCommandContext context )
		{
			var videoId = track.Url.ToString().Substring(track.Url.ToString().Length - 11);
			var thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			var embed = new EmbedBuilder
			{
				Title = track.Title.ToString(),
				Url = track.Url.ToString(),
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

			var msg = embed.Build();

			await context.Channel.SendMessageAsync(embed: msg);
		}

		public async Task QueueEmbed ( QueueTrack trackCollection )
		{
			var videoId = trackCollection.Track.Url.ToString().Substring(trackCollection.Track.Url.ToString().Length - 11);
			var thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			var embed = new EmbedBuilder
			{
				Title = trackCollection.Track.Title.ToString(),
				Url = trackCollection.Track.Url.ToString(),
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = trackCollection.User.GetAvatarUrl(),
					Name = "Added to Queue"
				}
			}
			.AddField("Channel", trackCollection.Track.Author, true)
			.AddField("Duration", trackCollection.Track.Duration.ToString(), true)
			.AddField("Queue Order", Queue.GetQueueCount(), true);

			var msg = embed.Build();

			await trackCollection.Channel.SendMessageAsync(embed: msg);
		}

		public async Task QueueEmbed ( LavaTrack track, ShardedCommandContext context )
		{
			var videoId = track.Url.ToString().Substring(track.Url.ToString().Length - 11);
			var thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			var queueLength = await Queue.GetQueueCount();

			var embed = new EmbedBuilder
			{
				Title = track.Title.ToString(),
				Url = track.Url.ToString(),
				Color = Color.DarkPurple,
				ThumbnailUrl = thumbnailUrl,
				Author = new EmbedAuthorBuilder
				{
					IconUrl = context.User.GetAvatarUrl(),
					Name = "Added to Queue"
				}
			}
			.AddField("Queue Order", queueLength, true)
			.AddField("Channel", track.Author, true)
			.AddField("Duration", track.Duration.ToString(), true);

			var msg = embed.Build();

			await context.Channel.SendMessageAsync(embed: msg);
		}

		public async Task PlaylistEmbed ( string query, ShardedCommandContext context )
		{
			var results = await _lavaNode.SearchAsync(query);

			var track = results.Tracks[0];
			var name = results.Playlist.Name;
			var duration = TimeSpan.Zero;
			var count = 0;

			var videoId = track.Url.ToString().Substring(track.Url.ToString().Length - 11);
			var thumbnailUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

			foreach (var item in results.Tracks)
			{
				count++;
				duration += item.Duration;
			}

			var embed = new EmbedBuilder
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

			var msg = embed.Build();

			await context.Channel.SendMessageAsync(embed: msg);
		}
	}
}

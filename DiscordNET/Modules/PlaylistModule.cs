using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Data.Playlist;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Responses.Rest;

namespace DiscordNET.Modules
{
	[Group("Playlist")]
	public class PlaylistModule : CommandModule<ShardedCommandContext>
	{
		private readonly LiteDatabase _database;
		private readonly LiteCollection<DBPlaylistGuild> _playlistCollection;
		private readonly LavaNode _lavaNode;

		public PlaylistModule( LiteDatabase database, LavaNode lavaNode)
		{
			_database = database;
			_playlistCollection = _database.GetCollection<DBPlaylistGuild>("playlistGuilds");
			_lavaNode = lavaNode;
		}

		[Command("Add"), Summary("Register the current queue as a playlist")]
		public async Task AddPlaylist(string name)
		{
			if(_lavaNode.TryGetPlayer(Context.Guild, out var player))
			{
				var queue = player.Queue;
				if (_playlistCollection.Exists(x => x.GuildId == Context.Guild.Id && x.Playlists.Exists(x => x.Name == name)))
				{
					await ReplyAsync("Playlist with the given name already exists");
					return;
				}

                List<LavaTrack> list = new List<LavaTrack>
                {
                    player.Track
                };

                foreach (var item in queue)
				{
					list.Add(((LavaTrackWithUser) item));
				}

				DBList playlist = new DBList
				{
					Name = name,
					Playlist = list
				};

				if (_playlistCollection.Exists(x => x.GuildId == Context.Guild.Id))
				{
					DBPlaylistGuild guild = _playlistCollection.FindOne(x => x.GuildId == Context.Guild.Id);
					guild.Playlists.Add(playlist);
					_playlistCollection.Update(guild);
				}
				else
				{
					_playlistCollection.Insert(new DBPlaylistGuild
					{
						GuildId = Context.Guild.Id,
						Playlists = new List<DBList> { playlist }
					});
				}



				await ReplyAsync("Playlist Added");
			}
		}

		[Command("Load"), Summary("Load a playlist to the queue")]
		public async Task LoadPlaylist(string name)
		{
			try
			{
				DBPlaylistGuild guild = _playlistCollection.FindOne(x => x.GuildId == Context.Guild.Id);
				DBList playlist = guild.Playlists.Find(x => x.Name.ToLower() == name.ToLower());

				if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
				{
					foreach(var track in playlist.Playlist)
					{
						player.Queue.Enqueue(new LavaTrackWithUser(track, Context.User, Context.Channel));
					}
				}
				await ReplyAsync("Playlist Loaded");
			}
			catch (Exception)
			{

				await ReplyAsync($"Couldn't Find a Playlist with the name `{name}`");
				return;
			}
		}

		[Command("List"), Summary("List all of the playlists that  are registered to this guild")]
		public async Task ListPlaylists()
		{
			DBPlaylistGuild guild = _playlistCollection.FindOne(x => x.GuildId == Context.Guild.Id);

			List<DBList> playlists = guild.Playlists;

			if (playlists.Count() == 0)
			{
				await ReplyAsync("No Playlist is registered under this server");
				return;
			}
			StringBuilder listString = new StringBuilder();

			foreach(DBList playlist in playlists)
			{
				listString.AppendLine($"`{playlist.Name}`");
			}

			Embed embed = new EmbedBuilder
			{
				Title = $"Saved Playlists for {Context.Guild.Name}",
				Description = listString.ToString()
			}.Build();
			await ReplyAsync(embed: embed);
		}
		
		[Command("create"), Summary("Create a playlist from a youtube playlist link")]
		public async Task CreatePlaylist(string query, string name)
		{
			SearchResponse results = await _lavaNode.SearchAsync(query);
			if(results.LoadStatus != Victoria.Enums.LoadStatus.PlaylistLoaded)
			{
				await ReplyAsync("Couldnt Load a Playlist from the provided link");
				return;
			}

			IReadOnlyList<LavaTrack> tracks = results.Tracks;

			if (_playlistCollection.Exists(x => x.GuildId == Context.Guild.Id && x.Playlists.Exists(x => x.Name == name)))
			{
				await ReplyAsync("Playlist with the given name already exists");
				return;
			}

			List<LavaTrack> list = new List<LavaTrack>();

			foreach (LavaTrack item in tracks)
			{
				list.Add(item);
			}

			DBList playlist = new DBList
			{
				Name = name,
				Playlist = list
			};

			if (_playlistCollection.Exists(x => x.GuildId == Context.Guild.Id))
			{
				DBPlaylistGuild guild = _playlistCollection.FindOne(x => x.GuildId == Context.Guild.Id);
				guild.Playlists.Add(playlist);
				_playlistCollection.Update(guild);
			}
			else
			{
				_playlistCollection.Insert(new DBPlaylistGuild
				{
					GuildId = Context.Guild.Id,
					Playlists = new List<DBList> { playlist }
				});
			}
			await ReplyAsync("Playlist Created");
		}
	}
}

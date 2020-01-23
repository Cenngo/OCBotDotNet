using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

namespace DiscordNET.Modules
{
	[Group("Playlist")]
	public class PlaylistModule : ModuleBase<SocketCommandContext>
	{
		private readonly MusicManager _musicManager;
		private LiteDatabase _database;
		private LiteCollection<DBList> _playlistCollection;
		private readonly LavaNode _lavaNode;

		public PlaylistModule(MusicManager musicManager, LiteDatabase database, LavaNode lavaNode)
		{
			_musicManager = musicManager;
			_database = database;
			_playlistCollection = _database.GetCollection<DBList>("playlists");
			_lavaNode = lavaNode;
		}

		[Command("Add"), Summary("Register the current queue as a playlist")]
		public async Task AddPlaylist(string name)
		{
			var queue = await _musicManager.Queue.GetItems();
			if(_playlistCollection.Exists(x => x.Name == name && x.GuildId == Context.Guild.Id))
			{
				await ReplyAsync("Playlist with the given name already exists");
				return;
			}
			
			var list = new List<LavaTrack>();

			foreach (var item in queue)
			{
				list.Add(item.Track);
			}
	
			var playlist = new DBList
			{
				Name = name,
				GuildId = Context.Guild.Id,
				Playlist = list
			};

			_playlistCollection.Insert(playlist);


			await ReplyAsync("Playlist Added");
		}

		[Command("Load"), Summary("Load a playlist to the queue")]
		public async Task LoadPlaylist(string name)
		{
			try
			{
				var list = _playlistCollection.FindOne(x => x.GuildId == Context.Guild.Id && x.Name.ToLower() == name.ToLower());
				await _musicManager.Queue.EnqueueBulk(list.Playlist, Context);
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
			var guildLists = _playlistCollection.Find(x => x.GuildId == Context.Guild.Id);

			if (guildLists.Count() == 0)
			{
				await ReplyAsync("No Playlist is registered under this server");
				return;
			}
			var listString = new StringBuilder();

			foreach(var playlist in guildLists)
			{
				listString.AppendLine($"`{playlist.Name}`");
			}

			var embed = new EmbedBuilder
			{
				Title = $"Saved Playlists for {Context.Guild.Name}",
				Description = listString.ToString()
			}.Build();
			await ReplyAsync(embed: embed);
		}

		[Command("rename"), Summary("Rename a playlist")]
		public async Task RenamePlaylist(string currentName, string newName)
		{
			try
			{
				var playlist = _playlistCollection.FindOne(x => x.Name == currentName && x.GuildId == Context.Guild.Id);
				playlist.Name = newName;
				_playlistCollection.Update(playlist);
				await ReplyAsync("Playlist Successfully Updates");
			}
			catch (Exception)
			{

				await ReplyAsync($"Couldn't Find a Playlist with the name `{currentName}`");
				return;
			}			
		}

		[Command("create"), Summary("Create a playlist from a youtube playlist link")]
		public async Task CreatePlaylist(string query, string name)
		{
			var results = await _lavaNode.SearchAsync(query);
			if(results.LoadStatus != Victoria.Enums.LoadStatus.PlaylistLoaded)
			{
				await ReplyAsync("Couldnt Load a Playlist from the provided link");
				return;
			}

			var tracks = results.Tracks;

			var list = new List<LavaTrack>();

			foreach (var item in tracks)
			{
				list.Add(item);
			}

			var playlist = new DBList
			{
				Name = name,
				GuildId = Context.Guild.Id,
				Playlist = list
			};

			_playlistCollection.Insert(playlist);


			await ReplyAsync("Playlist Created");
		}
	}
}

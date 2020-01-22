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

		public PlaylistModule(MusicManager musicManager, LiteDatabase database)
		{
			_musicManager = musicManager;
			_database = database;
			_playlistCollection = _database.GetCollection<DBList>("playlists");
		}

		[Command("Add")]
		public async Task AddPlaylist(string name)
		{
			var queue = await _musicManager.Queue.GetItems();

			var list = new List<DBTrack>();

			foreach(var track in queue)
			{
				list.Add(new DBTrack
				{
					Hash = track.Track.Hash,
					Id = track.Track.Id,
					Title = track.Track.Title,
					Author = track.Track.Author,
					Url = track.Track.Url,
					Duration = track.Track.Duration.ToString(),
					Position = track.Track.Position.ToString(),
					CanSeek = track.Track.CanSeek,
					IsStream = track.Track.IsStream
				});
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

		[Command("Load")]
		public async Task LoadPlaylist(string name)
		{
			try
			{
				
			}
			catch (Exception)
			{

				await ReplyAsync($"Couldn't Find a PLaylist with the name `{name}`");
				return;
			}
		}

		[Command("List")]
		public async Task ListPlaylists()
		{

		}
	}
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data.Playlist;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using LiteDB;
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

		public PlaylistModule(MusicManager musicManager)
		{
			_musicManager = musicManager;
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
					Url = track.Track.Url
				});
			}

			using (var db = new LiteDatabase(@"BotData.db"))
			{
				var playlist = new DbList
				{
					Name = name,
					GuildId = Context.Guild.Id,
					Playlist = list
				};

				var playlists = db.GetCollection<DbList>("playlists");
				playlists.Insert(playlist);
			};

			await ReplyAsync("Playlist Added");
		}

		[Command("Load")]
		public async Task LoadPlaylist(string name)
		{			

		}

		[Command("List")]
		public async Task ListPlaylists()
		{

		}
	}
}

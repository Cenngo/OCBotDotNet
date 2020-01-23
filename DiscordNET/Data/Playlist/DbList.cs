using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace DiscordNET.Data.Playlist
{
	public class DBList
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<LavaTrack> Playlist { get; set; }
	}

	public class DBPlaylistGuild
	{
		public int Id { get; set; }
		public ulong GuildId { get; set; }
		public List<DBList> Playlists { get; set; }
	}
}

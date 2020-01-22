using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using Victoria;

namespace DiscordNET.Data.Playlist
{
	public class DbList
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public ulong GuildId { get; set; }
		public List<DBTrack> Playlist { get; set; }
	}
}

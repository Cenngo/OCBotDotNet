using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data
{
	public class GuildConfig
	{
		public int Id { get; set; }
		public ulong GuildId { get; set; }
		public List<string> WhiteList { get; set; }
		public bool Irritate { get; set; }
		public List<string> Prefix { get; set; }
	}
}

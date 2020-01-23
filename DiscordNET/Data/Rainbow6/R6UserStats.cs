using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data.Rainbow6
{
	public struct R6UserStats
	{
		public R6IdSearch stats { get; set; }
		public string rank { get; set; }
		public Discord.Color rankColor { get; set; }
		public string avatarUrl { get; set; }
	}

	public struct R6Rank
	{
		public string rank { get; set; }
		public Discord.Color rankColor { get; set; }
	}
}

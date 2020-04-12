using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data.Rainbow6
{
	public class R6PlayerDataByName
	{
		[JsonProperty("status")]
		public int Status { get; set; }
		[JsonProperty("foundmatch")]
		public bool FoundMatch { get; set; }
		[JsonProperty("requested")]
		public string RequestedName { get; set; }
		[JsonProperty("players")]
		public Players Players { get; set; }
	}

	public class Players
	{
		[JsonExtensionData]
		public IDictionary<string, R6Player> FoundPlayers { get; set; }
	} 
}

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using Newtonsoft.Json;

namespace DiscordNET.Data.Rainbow6
{
	public class R6Player
	{
		
	}

	public class Profile
	{
		[JsonProperty("p_id")]
		public string Id { get; set; }
		[JsonProperty("p_user")]
		public string User { get; set; }
		[JsonProperty("p_name")]
		public string Name { get; set; }
		[JsonProperty("p_platform")]
		public string Platform { get; set; }
		[JsonProperty("verified")]
		public bool Verified { get; set; }
	}

	public class Refresh
	{
		[JsonProperty("x")]
		public int LastUpdated { get; set; }
		[JsonProperty("s")]
		public  int LastEdited { get; set; }
	}

	public class Stats
	{
		[JsonProperty("level")]
		public int Level { get; set; }
	}
}

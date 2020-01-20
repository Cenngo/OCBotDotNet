using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DiscordNET.Data.Rainbow6
{
	public struct R6IdSearch
	{
		[JsonProperty("p_id")]
		public string playerID { get; }
		[JsonProperty("p_name")]
		public string playerName { get; }
		[JsonProperty("p_user")]
		public string userId { get; }
		[JsonProperty("p_level")]
		public string playerLevel { get; }
		[JsonProperty("utime")]
		public string upTime { get; }
		[JsonProperty("kd")]
		public string kd { get; }
		[JsonProperty("p_currentrank")]
		public string currentRank { get; }
		[JsonProperty("p_currentmmr")]
		public string currentMMR { get; }
		[JsonProperty("p_maxrank")]
		public string maxRank { get; }
		[JsonProperty("p_maxmmr")]
		public string maxMMR { get; }
		[JsonProperty("favattacker")]
		public string favAttacker { get; }
		[JsonProperty("favdefender")]
		public string favDefender { get; }
	}
}

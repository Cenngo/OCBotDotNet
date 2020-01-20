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
		public string playerID { get; set; }
		[JsonProperty("p_name")]
		public string playerName { get; set; }
		[JsonProperty("p_user")]
		public string userId { get; set; }
		[JsonProperty("p_level")]
		public string playerLevel { get; set; }
		[JsonProperty("utime")]
		public string upTime { get; set; }
		[JsonProperty("kd")]
		public string kd { get; set; }
		[JsonProperty("p_currentrank")]
		public string currentRank { get; set; }
		[JsonProperty("p_currentmmr")]
		public string currentMMR { get; set; }
		[JsonProperty("p_maxrank")]
		public string maxRank { get; set; }
		[JsonProperty("p_maxmmr")]
		public string maxMMR { get; set; }
		[JsonProperty("favattacker")]
		public string favAttacker { get; set; }
		[JsonProperty("favdefender")]
		public string favDefender { get; set; }
	}
}

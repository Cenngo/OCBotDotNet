using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data.Rainbow6
{
	public struct R6NameSearch
	{
		[JsonProperty("results")]
		public List<R6NameSearchResults> results { get; set; }
		[JsonProperty("totalresults")]
		public int resultCount { get; set; }
	}

	public struct R6NameSearchResults
	{
		[JsonProperty("p_id")]
		public string pId { get; set; }
		[JsonProperty("p_name")]
		public string pName { get; set; }
		[JsonProperty("p_level")]
		public string pLevel { get; set; }
		[JsonProperty("p_platform")]
		public string pPlatform { get; set; }
		[JsonProperty("p_user")]
		public string pUser { get; set; }
		[JsonProperty("p_currentmmr")]
		public string pMmr { get; set; }
		[JsonProperty("p_currentrank")]
		public string pRank { get; set; }
		[JsonProperty("kd")]
		public string kd { get; set; }
	}
}

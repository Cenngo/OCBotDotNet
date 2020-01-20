using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data.Rainbow6
{
	public struct R6NameSearch
	{
		[JsonProperty("results")]
		public List<R6NameSearchResults> results { get; private set; }
		[JsonProperty("totalresults")]
		public int resultCount { get; private set; }
	}

	public struct R6NameSearchResults
	{
		[JsonProperty("p_id")]
		public string pId { get; private set; }
		[JsonProperty("p_name")]
		public string pName { get; private set; }
		[JsonProperty("p_level")]
		public string pLevel { get; private set; }
		[JsonProperty("p_platform")]
		public string pPlatform { get; private set; }
		[JsonProperty("p_user")]
		public string pUser { get; private set; }
		[JsonProperty("p_currentmmr")]
		public string pMmr { get; private set; }
		[JsonProperty("p_currentrank")]
		public string pRank { get; private set; }
		[JsonProperty("kd")]
		public string kd { get; private set; }
	}
}

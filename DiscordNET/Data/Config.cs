using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscordNET.Data
{
	public struct Config
	{
		[JsonProperty("token")]
		public string Token { get; private set; }
		[JsonProperty("prefix")]
		public List<string> Prefix { get; private set; }
		[JsonProperty("activity")]
		public string Activity { get; private set; }
	}
}

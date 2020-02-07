using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordNET.Data
{
    public class userData
    {
        public int Id { get; set; }
		public ulong discordID { get; set; }
        public string dHandle { get; set; }
        public string langauge { get; set; }
	}

    public class InsultCollection
    {
        public int Id { get; set; }
		public string Language { get; set; }
        public List<String> Insults { get; set; }
    }

	public class InsultJSON
	{
		[JsonProperty("insults")]
		public List<string> Insults { get; set; }
	}
}

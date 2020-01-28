using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordNET.Data
{
    public struct UserCollection
    {
        [JsonProperty("Users")]
        public List<userData> userList { get; set; }
    }
    public class userData
    {
        //FIXME: non-private setters
        [JsonProperty("ID")]
        public ulong discordID { get; set; }

        [JsonProperty("handle")]
        public string dHandle { get; set; }

        [JsonProperty("lang")]
        public string langauge { get; set; }

        public userData()
        {
            discordID = ulong.MinValue;
            dHandle = string.Empty;
            langauge = "default";
        }
    }

    class InsultCollection
    {
        public string Language { get; set; }
        public List<String> Insults { get; set; }
    }
}

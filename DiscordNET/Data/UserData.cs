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

    class Insult
    {
        public List<string> SupportedLanguages = new List<string>
        {
            "tr","en"
        };

        [JsonProperty("TR")]
        public List<String> TR_insults { get; private set; }
        [JsonProperty("EN")]
        public List<String> EN_insults { get; private set; }
    }
}

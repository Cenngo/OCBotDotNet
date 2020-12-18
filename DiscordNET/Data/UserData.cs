using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordNET.Data
{
    public struct UserCollection
    {
        [JsonProperty("Users")]
        public List<UserData> UserList { get; set; }
    }
    public class UserData
    {
        //FIXME: non-private setters
        [JsonProperty("ID")]
        public ulong DiscordID { get; set; }

        [JsonProperty("handle")]
        public string DHandle { get; set; }

        [JsonProperty("lang")]
        public string Langauge { get; set; }
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

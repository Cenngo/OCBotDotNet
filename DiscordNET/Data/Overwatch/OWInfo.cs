using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscordNET.Handlers
{
    public struct OWInfo
    {
        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("rating")]
        public int avgSR { get; private set; }

        //A list of the 3 roles per player
        [JsonProperty("ratings")]
        public List<OWRole> OW_RoleList { get; private set; }
    }

    public class OWRole
    {
        [JsonProperty("level")]
        public int skillRating { get; private set; }

        [JsonProperty("role")]
        public string role { get; private set; }

        [JsonProperty("rankIcon")]
        public string rankIcon { get; private set; }

        [JsonProperty("roleIcon")]
        public string roleIcon { get; private set; }
        public OWRole()
        {
            skillRating = int.MinValue;
            role = string.Empty;
            rankIcon = string.Empty;
            roleIcon = string.Empty;
        }
    }
}
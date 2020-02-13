using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscordNET.Handlers
{
    public struct OWInfo
    {
        [JsonProperty("private")]
        public bool priv { get; private set;}
        [JsonProperty("name")]
        public string name { get; private set; }
        [JsonProperty("icon")]
        public string iconURL { get; private set; }
        [JsonProperty("prestige")]
        public int prestige { get; private set; }
        [JsonProperty("level")]
        public int level { get; private set; }
        [JsonProperty("endorsement")]
        public int endorsement { get; private set; }
        //Combined SR
        [JsonProperty("rating")]
        public int avgSR { get; private set; }

        //A list of the all applicable roles per player
        [JsonProperty("ratings")]
        public List<OWRole> OW_RoleList { get; private set; }

        [JsonProperty("competitiveStats")]
        public OwCareerStats CompStats { get; private set; }
        [JsonProperty("quickPlayStats")]
        public OwCareerStats QpStats { get; private set; }
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
    public class OwCareerStats
    {
        [JsonProperty("topHeroes")]
        public Dictionary<string, OwHero> allHeroes { get; private set; }
        //Fix when API is up @ https://ow-api.com/
        //[JsonProperty("careerStats")]
        //public type careerstats
    }

    public class OwHero
    {
        [JsonProperty("timePlayed")]
        public string TimePlayed { get; private set; }
        [JsonProperty("gamesWon")]
        public int GamesWon { get; private set; }
        [JsonProperty("winPercentage")]
        public int WinPercentage { get; private set; }
        [JsonProperty("weaponAccuracy")]
        public int WeaponAcc { get; private set; }
        [JsonProperty("eliminationsPerLife")]
        public double ElimsPerLife { get; private set; }
        [JsonProperty("multiKillBest")]
        public int BestKStreak { get; private set; }
        [JsonProperty("objectiveKills")]
        public int BestObjKills { get; private set; }
    }
}
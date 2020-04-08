using Newtonsoft.Json;
using System.Collections.Generic;

namespace DiscordNET.Handlers
{
    /// <summary>
    /// Info of profile pulled from ow-api
    /// </summary>
    public struct OWInfo
    {
        /// <summary>
        /// True if profile is private
        /// </summary>
        [JsonProperty("private")]
        public bool Priv { get; private set;}

        /// <summary>
        /// User battletag
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// URL of player icon png
        /// </summary>
        [JsonProperty("icon")]
        public string IconURL { get; private set; }

        /// <summary>
        /// Prestige level (how many hundred level)
        /// </summary>
        [JsonProperty("prestige")]
        public int Prestige { get; private set; }

        /// <summary>
        /// Level after prestige
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; private set; }

        /// <summary>
        /// Endorsement level
        /// </summary>
        [JsonProperty("endorsement")]
        public int Endorsement { get; private set; }
        /// <summary>
        /// Combined SR
        /// </summary>
        [JsonProperty("rating")]
        public int AvgSR { get; private set; }

        /// <summary>
        /// A list of the all applicable roles per player
        /// type of <c>OWRole</c>
        /// </summary>
        [JsonProperty("ratings")]
        public List<OWRole> OW_RoleList { get; private set; }

        /// <summary>
        /// <c>OwCareerStats</c> object for competitive play
        /// </summary>
        [JsonProperty("competitiveStats")]
        public OwCareerStats CompStats { get; private set; }

        /// <summary>
        /// <c>OwCareerStats</c> object for competitive play
        /// </summary>
        [JsonProperty("quickPlayStats")]
        public OwCareerStats QpStats { get; private set; }
    }

    /// <summary>
    /// Skill rating stats per role
    /// </summary>
    public class OWRole
    {
        /// <summary>
        /// Current SR
        /// </summary>
        [JsonProperty("level")]
        public int SkillRating { get; private set; }

        /// <summary>
        /// Name of role 
        /// </summary>
        /// <example>tank, damage, support</example>
        [JsonProperty("role")]
        public string Role { get; private set; }

        /// <summary>
        /// URL for rank icon png
        /// </summary>
        [JsonProperty("rankIcon")]
        public string RankIcon { get; private set; }

        /// <summary>
        /// URL for role icon png
        /// </summary>
        [JsonProperty("roleIcon")]
        public string RoleIcon { get; private set; }
        public OWRole()
        {
            SkillRating = int.MinValue;
            Role = string.Empty;
            RankIcon = string.Empty;
            RoleIcon = string.Empty;
        }
    }

    /// <summary>
    /// Career stats for all heroes
    /// </summary>
    public class OwCareerStats
    {
        /// <summary>
        /// <c>Dictionary</c> of <c>OwHero</c>s
        /// </summary>
        [JsonProperty("topHeroes")]
        public Dictionary<string, OwHero> AllHeroes { get; private set; }
        //Fix when API is up @ https://ow-api.com/
        //[JsonProperty("careerStats")]
        //public type careerstats
    }

    /// <summary>
    /// Generic collection of stats per hero
    /// </summary>
    public class OwHero
    {
        /// <summary>
        /// Time played in hours type of <c>String</c>
        /// </summary>
        [JsonProperty("timePlayed")]
        public string TimePlayed { get; private set; }

        /// <summary>
        /// Number of games won
        /// </summary>
        [JsonProperty("gamesWon")]
        public int GamesWon { get; private set; }

        /// <summary>
        /// Wins / 100
        /// </summary>
        /// <example><c>int</c> 65 is an equivalent of a 65% winrate</example>
        [JsonProperty("winPercentage")]
        public int WinPercentage { get; private set; }

        /// <summary>
        /// Accurate hits / 100
        /// </summary>
        /// <example><c>int</c> 65 is an equivalent of a 65% accuracy</example>
        [JsonProperty("weaponAccuracy")]
        public int WeaponAcc { get; private set; }

        /// <summary>
        /// Eliminations per life
        /// </summary>
        [JsonProperty("eliminationsPerLife")]
        public double ElimsPerLife { get; private set; }

        /// <summary>
        /// Best elimination streak
        /// </summary>
        [JsonProperty("multiKillBest")]
        public int BestKStreak { get; private set; }

        /// <summary>
        /// Best objective kills
        /// </summary>
        [JsonProperty("objectiveKills")]
        public int BestObjKills { get; private set; }
    }
}
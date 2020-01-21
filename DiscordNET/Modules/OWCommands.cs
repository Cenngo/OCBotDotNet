using Discord;
using Discord.Commands;
using DiscordNET.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("OW")]
    public class OWCommands : ModuleBase<SocketCommandContext>
    {
        [Command("profile")]
        public async Task GetOwProfile(string battleTag, string role, string platform = "pc", string region = "eu")
        {
            var OW = new Overwatch();
            var stats = await OW.RetrieveUserStats(battleTag, region, platform);
            List<OWRole> roles = stats.OW_RoleList;
            var roleMatch = roles.FirstOrDefault(x => x.role == role);

            if (roleMatch == default(OWRole))
            {
                await ReplyAsync("No SR for current role...");
                return;
            }
            else
            {
                var RankColorPair = new Dictionary<string, Color>()
                {
                    {"bronzetier", new Color(0x964B00)},
                    {"silvertier", new Color(0x808080)},
                    {"goldtier", new Color(0xffd700)},
                    {"platinumtier", new Color(0xe5e4e2)},
                    {"diamondtier", Color.Teal },
                    {"mastertier", new Color(0xfed8b1)},
                    {"grandmastertier", new Color(0xffff00) }
                };
                var rank = roleMatch.rankIcon.Substring(59);
                rank = rank.Remove(rank.IndexOf(".")).ToLower();
                var rankColor = RankColorPair[rank];
                var infoEmbed = new EmbedBuilder()
                {
                    Title = battleTag,
                    Color = rankColor,
                    ThumbnailUrl = stats.iconURL,
                    ImageUrl = roleMatch.rankIcon
                }
                .AddField("Role", roleMatch.role, true)
                .AddField("Rating", roleMatch.skillRating + " SR", true)
                .Build(); 
                await ReplyAsync(embed: infoEmbed);
            }
        }
    }
}

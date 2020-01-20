using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("OW")]
    public class OWCommands : ModuleBase<SocketCommandContext>
    {
        [Command("profile")]
        public async Task Profile(string battleTag, string role, string platform = "pc", string region = "eu")
        {
            var OW = new Handlers.Overwatch();
            var stats = await OW.RetrieveUserStats(battleTag, region, platform);

            // Dictionary for role list
            var OWClassPairs = new Dictionary<string, int>()
            {
                {"tank", 0 },
                {"damage", 1 },
                {"support", 2 }
            };

            //Sets role list index based on user role input
            var roleInt = OWClassPairs[role];

            var selectedRole = stats.OW_RoleList[roleInt];

            //Sets embed color based on rank of selected role

            var rank = selectedRole.rankIcon.Substring(59);
            rank = rank.Remove(rank.IndexOf(".")).ToLower();

            var RankColorPair = new Dictionary<string, Color>()
            {
                {"bronzetier", Color.DarkOrange},
                {"silvertier", Color.LightGrey },
                {"goldtier", Color.Gold },
                {"platinumtier", Color.LighterGrey },
                {"diamondtier", Color.Teal },
                {"mastertier", Color.Orange },
                {"grandmastertier", Color.LightOrange }
            };

            var rankColor = RankColorPair[rank];
            var embed = new EmbedBuilder
            {
                Title = $"Player Name: {stats.name}",
                Description = $"Avg. SR: {stats.avgSR}\n" +
                                $"Role: {selectedRole.role}\n" +
                                $"Role SR: {selectedRole.skillRating}\n" +
                                $"{rank}",
                ThumbnailUrl = selectedRole.rankIcon,

                Color = rankColor
            }.Build();
            await ReplyAsync(embed: embed);
        }
    }
}

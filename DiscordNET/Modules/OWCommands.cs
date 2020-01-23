using Discord;
using Discord.Commands;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("OW")]
    public class OWCommands : ModuleBase<SocketCommandContext>
    {
        [Command("profile")]
        public async Task GetOwProfile(string battleTag, string platform = "pc", string region = "eu")
        {
            try
            {
                var OW = new Overwatch();
                var stats = await OW.RetrieveUserStats(battleTag, region, platform);
                Dictionary<string, OwHero> comp = stats.CompStats.allHeroes;
                Dictionary<string, OwHero> qp = stats.CompStats.allHeroes;
                var bestComp = await OW.SortHero(comp);
                var bestQP = await OW.SortHero(qp);
                var infoEmbed = new EmbedBuilder()
                {
                    Title = stats.name,
                    //Color = rankColor,
                    ThumbnailUrl = stats.iconURL
                }
                                .AddField("Level", stats.prestige.ToString() + stats.level.ToString(), true)
                                .AddField("Endorsement", stats.endorsement, true)
                                .AddField("Best Competitive Hero", bestComp.ToUpper(), true)
                                .AddField("Best Quick-play Hero", bestQP.ToUpper(),true)
                                .WithAuthor("Profile Summary")
                                .Build();
                await ReplyAsync(embed: infoEmbed);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }
        [Command("rank")]
        public async Task GetOwRank(string battleTag, string role, string platform = "pc", string region = "eu")
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

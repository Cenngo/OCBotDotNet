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
                bool priv = stats.priv;
             
                if (!stats.priv){
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
                    .AddField("Best Competitive Hero", bestComp.ToUpper(), false)
                    .AddField("Best Quick-play Hero", bestQP.ToUpper(),false)
                    .WithAuthor("Profile Summary")
                    .Build();
                    await ReplyAsync(embed: infoEmbed);
                }
                else{
                    var infoEmbed = new EmbedBuilder()
                    {
                        Title = stats.name,
                        //Color = rankColor,
                        ThumbnailUrl = stats.iconURL
                    }
                    .AddField("Level", stats.prestige.ToString() + stats.level.ToString(), true)
                    .AddField("Endorsement", stats.endorsement, true)
                    .WithAuthor("Profile Summary").WithDescription("This profile is private")
                    .Build();   
                    await ReplyAsync(embed: infoEmbed);
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }
        [Command("rank")]
        public async Task GetOwRank(string battleTag, string platform = "pc", string region = "eu")
        {
            try
            {
                var OW = new Overwatch();
                var stats = await OW.RetrieveUserStats(battleTag, region, platform);
                List<OWRole> roles = stats.OW_RoleList;
                if (roles == null){
                    await ReplyAsync("No roles placed...");
                    return;
                }

                await ReplyAsync("Displaying ranks for " + stats.name);

                foreach (OWRole r in roles){
                    var roleName = r.role;
                    var roleLevel = r.skillRating;
                    var roleIcon = r.roleIcon;
                    var rankIcon = r.rankIcon;
                    var rank = r.rankIcon.Substring(59);
                    rank = rank.Remove(rank.IndexOf(".")).ToLower();
                    
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

                    var rankColor = RankColorPair[rank];
                    var infoEmbed = new EmbedBuilder()
                    {
                        Title = r.role,
                        Color = rankColor,
                        ThumbnailUrl = r.rankIcon,
                        Description = r.skillRating.ToString()
                    }
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

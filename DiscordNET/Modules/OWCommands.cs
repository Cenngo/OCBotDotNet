using Discord;
using Discord.Commands;
using DiscordNET.Handlers;
using DiscordNET.GeneralUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordNET.Extensions;

namespace DiscordNET.Modules
{
    [Group("OW")]
    public class OWCommands : ModuleBase<ShardedCommandContext>
    {
        [Command("profile")]
        public async Task GetOwProfile(string battleTag, string platform = "pc", string region = "eu")
        {
            try
            {
                var OW = new Overwatch();
                var stats = await OW.RetrieveUserStats(battleTag, region, platform);
             
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
                    .AddBlankField()
                    .AddField("Best Competitive Hero", bestComp.CaptFirst(), false)
                    .AddField("Best Quick-play Hero", bestQP.CaptFirst(),false)
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
                if (stats.priv){
                    await ReplyAsync(stats.name + "'s Profile is private...");
                    return; 
                }

                else if (roles == null){
                    await ReplyAsync(stats.name + " has no roles placed...");
                    return;
                }

                else{
                    await ReplyAsync("Displaying ranks for " + stats.name + "...");

                    foreach (OWRole r in roles){
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
                            Title = r.role.CaptFirst(),
                            Color = rankColor,
                            ThumbnailUrl = r.rankIcon,
                            Description = r.skillRating.ToString()
                        }
                        .Build();
                        await ReplyAsync(embed: infoEmbed);
                    }
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

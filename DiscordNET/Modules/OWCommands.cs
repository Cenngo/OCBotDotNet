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
    /// <summary>
    /// Module of a collection of commands related to Overwatch
    /// </summary>
    [Group("OW")]
    public class OWCommands : CommandModule<ShardedCommandContext>
    {
        /// <summary>
        /// Prints a summary of the player's profile
        /// </summary>
        /// <param name="battleTag">Battle.net tag of user</param>
        /// <param name="platform">Platform (defaults to "pc")</param>
        /// <param name="region">Region (defaults to "na")</param>
        /// <returns>Prints exception on NULL</returns>
        [Command("profile")]
        public async Task GetOwProfile(string battleTag, string platform = "pc", string region = "eu")
        {
            try
            {
                Overwatch OW = new Overwatch();
                OWInfo stats = await OW.RetrieveUserStats(battleTag, platform, region);
                string prestige;
                if (stats.Prestige == 0) prestige = string.Empty;
                else prestige = stats.Prestige.ToString();
             
                if (!stats.Priv){
                    Dictionary<string, OwHero> comp = stats.CompStats.AllHeroes;
                    Dictionary<string, OwHero> qp = stats.CompStats.AllHeroes;
                    string bestComp = OW.SortHero(comp);
                    string bestQP = OW.SortHero(qp);
                    
                    Embed infoEmbed = new EmbedBuilder()
                    {
                        Title = stats.Name,
                        //Color = rankColor,
                        ThumbnailUrl = stats.IconURL
                    }                    
                    .AddField("Level", prestige + stats.Level.ToString(), true)
                    .AddField("Endorsement", stats.Endorsement, true)
                    .AddBlankField()
                    .AddField("Best Competitive Hero", bestComp.CaptFirst(), false)
                    .AddField("Best Quick-play Hero", bestQP.CaptFirst(),false)
                    .WithAuthor("Profile Summary")
                    .Build();
                    await ReplyAsync(embed: infoEmbed);
                }
                else{
                    Embed infoEmbed = new EmbedBuilder()
                    {
                        Title = stats.Name,
                        //Color = rankColor,
                        ThumbnailUrl = stats.IconURL
                    }
                    .AddField("Level", prestige + stats.Level.ToString(), true)
                    .AddField("Endorsement", stats.Endorsement, true)
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

        /// <summary>
        /// Prints SR for each role
        /// </summary>
        /// <param name="battleTag"></param>
        /// <param name="platform"></param>
        /// <param name="region"></param>
        /// <returns>Message if no data is present or if profile is private</returns>
        [Command("rank")]
        public async Task GetOwRank(string battleTag, string platform = "pc", string region = "eu")
        {
            try
            {
                Overwatch OW = new Overwatch();
                OWInfo stats = await OW.RetrieveUserStats(battleTag, platform, region);
                List<OWRole> roles = stats.OW_RoleList;
                if (stats.Priv){
                    await ReplyAsync(stats.Name + "'s Profile is private...");
                    return; 
                }

                else if (roles == null){
                    await ReplyAsync(stats.Name + " has no roles placed...");
                    return;
                }

                else{
                    await ReplyAsync("Displaying ranks for " + stats.Name + "...");

                    foreach (OWRole r in roles){
                        string rank = r.RankIcon.Substring(59);
                        rank = rank.Remove(rank.IndexOf(".")).ToLower();
                        
                        Dictionary<string, Color> RankColorPair = new Dictionary<string, Color>()
                        {
                            {"bronzetier", new Color(0x964B00)},
                            {"silvertier", new Color(0x808080)},
                            {"goldtier", new Color(0xffd700)},
                            {"platinumtier", new Color(0xe5e4e2)},
                            {"diamondtier", Color.Teal },
                            {"mastertier", new Color(0xfed8b1)},
                            {"grandmastertier", new Color(0xffff00) }
                        };

                        Color rankColor = RankColorPair[rank];
                        Embed infoEmbed = new EmbedBuilder()
                        {
                            Title = r.Role.CaptFirst(),
                            Color = rankColor,
                            ThumbnailUrl = r.RankIcon,
                            Description = r.SkillRating.ToString()
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

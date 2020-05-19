using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Extensions;
using R6Api;
using R6Api.Enums;
using R6Api.Models;
using R6Api.Models.SearchResults;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("R6")]
    public class R6Module : ModuleBase<ShardedCommandContext>
    {
        private readonly R6Client _r6;

        public R6Module ()
        {
            _r6 = new R6Client(new R6Config
            {
                AutoCacheAvoidance = true,
                ApiKey = "2c5e6c9d-860a-419f-894e-66e2cd7fd3b7"
            });
        }

        [Command("profile")]
        public async Task Profile ( string username, string platform = "uplay" )
        {
            var player = await GetData(username, platform);

            if (player == null)
                return;

            var rankColor = player.Ranked.RankColor;

            var msg = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Rainbow 6 Siege Stats"
                },
                Title = player.Player.Name,
                Description = "Rank: " + player.Ranked.MaxRankName,
                Color = new Color(rankColor.R, rankColor.G, rankColor.B),
                ThumbnailUrl = player.Player.GetAvatar()
            }
            .AddField("Level", player.Stats.Level, true)
            .AddField("Max MMR", player.Ranked.MaxMmr, true)
            .AddField("Ranked K/D", player.Ranked.AllKillDeath, true)
            .AddField("Ranked W/L", player.Ranked.AllWinLose + "%", true)
            .AddField("General K/D", player.Stats.GeneralPvP.KillDeath, true)
            .AddField("General W/L", player.Stats.GeneralPvP.WinLose, true)
            .AddField("Ranked Play Time", $"{player.Stats.Ranked.HoursPlayed} Hours", true)
            .AddField("Penetration Kills", player.Stats.GeneralPvP.PenetrationKills, true)
            .AddField("Melee Kills", player.Stats.GeneralPvP.MeleeKills, true)
            .AddField("Headshots", player.Stats.GeneralPvP.Headshots, true)
            .AddField("Headshot Rate", player.Stats.GeneralPvP.HeadShotRate + " %", true)
            .AddField("Revives", player.Stats.GeneralPvP.Revives, true)
            .AddBlankField()
            .AddField("Most Played Op", player.Operators.TopPlayedOverall.First().Key, true)
            .AddField("Op with Highest Kills", player.Operators.TopKillsOverall.First().Key, true)
            .Build();

            await ReplyAsync(embed: msg);
        }

        [Command("Casual")]
        public async Task Casual ( string username, string platform = "uplay" )
        {
            var player = await GetData(username, platform);

            if (player == null)
                return;

            var casual = player.Stats.Casual;

            var rankColor = player.Ranked.RankColor;

            var msg = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Rainbow6S Casual Stats"
                },
                Title = player.Player.Name,
                Color = new Color(rankColor.R, rankColor.G, rankColor.B),
                ThumbnailUrl = player.Player.GetAvatar()
            }
            .AddField("Level", player.Stats.Level, true)
            .AddField("Kills", casual.Kills, true)
            .AddField("Deaths", casual.Deaths, true)
            .AddField("Casual Play Time", casual.HoursPlayed + " Hours", true)
            .AddField("K/D", casual.KillDeath, true)
            .AddField("W/L", casual.WinLose + "%", true)
            .Build();

            await ReplyAsync(embed: msg);
        }

        [Command("Operator")]
        public async Task Operator ( string username, string op, [Summary("Scope of the Stats... Overall/Seasonal")]string scope = "overall", string platform = "uplay" )
        {
            var player = await GetData(username, platform);

            var formattedName = op.Substring(0, 1).ToUpper() + op.Substring(1).ToLower();

            if (!player.Operators.OpStats.TryGetValue(formattedName, out var stats))
            {
                await ReplyAsync($"No Operator Data Found for {formattedName}");
                return;
            }

            OperatorStats printedStats;

            if (scope.ToLower() == "seasonal")
                printedStats = stats.SeasonalStats;
            else if (scope.ToLower() == "overall")
                printedStats = stats.OverallStats;
            else
            {
                await ReplyAsync("Undefined Scope...");
                return;
            }

            var rankColor = player.Ranked.RankColor;

            var msg = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = formattedName
                },
                Title = player.Player.Name,
                Color = new Color(rankColor.R, rankColor.G, rankColor.B),
                ThumbnailUrl = player.Player.GetAvatar()
            }
            .AddField("Kills", printedStats.Kills, true)
            .AddField("Deaths", printedStats.Deaths, true)
            .AddField("Wins", printedStats.Wins, true)
            .AddField("Losses", printedStats.Losses, true)
            .AddField("Time Played", new TimeSpan(0, 0, printedStats.TimePlayed).Hours + " Hours" + new TimeSpan(0, 0, printedStats.TimePlayed).Minutes + " Mins", true)
            .AddBlankField()
            .AddField("K/D", $"{printedStats.KD}", true)
            .AddField("W/L", $"{printedStats.WinRate}%", true)
            .Build();

            await ReplyAsync(embed: msg);
        }

        private async Task<DataById> GetData ( string username, string platform )
        {
            if (!Enum.TryParse(typeof(Platform), platform, true, out var _platform))
            {
                await ReplyAsync("Please Select a Valid Platform");
                return null;
            }

            var results = _r6.ParseByName(username, (Platform)_platform);

            if (results.FoundMatch == false)
            {
                await ReplyAsync($"No Match Found for {username} on {_platform}");
                return null;
            }

            var match = results.FoundPlayers.First().Value;

            return _r6.ParseById(match.Profile.User);
        }
    }
}

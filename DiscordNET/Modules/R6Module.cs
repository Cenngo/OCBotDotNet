using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordNET.Extensions;
using R6Api;
using R6Api.Enums;
using Raven.Client.Documents.Commands.Batches;
using System.Linq;
using R6Api.Models.SearchResults;

namespace DiscordNET.Modules
{
	[Group("R6")]
	public class R6Module : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;
		private readonly R6Client _r6;

		public R6Module(DiscordShardedClient client, CommandService commands)
		{
			_client = client;
			_r6 = new R6Client(new R6Config
			{
				AutoCacheAvoidance = true
			});
		}

		[Command("profile")]
		public async Task Profile(string username, string platform = "uplay")
		{
			var player = await GetData(username, platform);

			if (player == null)
				return;

			var msg = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = "Rainbow 6 Siege Stats"
				},
				Title = player.Player.Name,
				Description = "Rank: " + player.Ranked.MaxRankName,
				Color = await GetColor(player.Ranked.MaxRankName),
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
			.AddField("Most Played Op", player.OperatorData.GetTopPlayed().First(), true)
			.AddField("Top Winning Op", player.OperatorData.GetTopWins().First(), true)
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

			var msg = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = "Rainbow6S Casual Stats"
				},
				Title = player.Player.Name,
				Color = await GetColor(player.Ranked.MaxRankName),
				ThumbnailUrl = player.Player.GetAvatar()
			}
			.AddField("Level", player.Stats.Level, true)
			.AddField("Kills", casual.Kills, true)
			.AddField("Deaths", casual.Deaths, true)
			.AddField("Casual Play Time", casual.HoursPlayed + " Hours", true)
			.AddField("K/D", casual.KillDeath , true)
			.AddField("W/L", casual.WinLose + "%", true)
			.Build();

			await ReplyAsync(embed: msg);
		}

		[Command("Operator")]
		public async Task Operator (string username, string op, string platform = "uplay")
		{
			if(!Enum.TryParse(typeof(Operators), op, true, out var _op))
			{
				await ReplyAsync("Invalid Operator!");
				return;
			}

			var player = await GetData(username, platform);

			if (player == null)
				return;

			var opData = player.OperatorData.GetOperatorStats((Operators)_op);

			double kd = opData.Kills / opData.Deaths ;
			double wl = opData.Wins * 100 / (opData.Losses + opData.Wins) ;

			var msg = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = $"{_op}"
				},
				Title = player.Player.Name,
				Color = await GetColor(player.Ranked.MaxRankName),
				ThumbnailUrl = player.Player.GetAvatar()
			}
			.AddField("Kills", opData.Kills, true)
			.AddField("Deaths", opData.Deaths, true)
			.AddField("Wins", opData.Wins, true)
			.AddField("Losses", opData.Losses, true)
			.AddField("Time Played", new TimeSpan(0, 0, opData.TimePlayed).Hours + " Hours" + new TimeSpan(0, 0, opData.TimePlayed).Minutes + " Mins", true)
			.AddBlankField()
			.AddField("K/D", $"{kd}", true)
			.AddField("W/L", $"{wl}%", true)
			.Build();

			await ReplyAsync(embed: msg);
		}

		private async Task<DataById> GetData(string username, string platform)
		{
			if (!Enum.TryParse(typeof(R6Api.Enums.Platform), platform, true, out var _platform))
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

		private async Task<Color> GetColor(string rank )
		{
			var rankName = rank.Split(" ")[0].ToLower();

			Color color;

			switch(rankName)
			{
				case "copper":
					color= new Color(0x90040b);
					break;
				case "bronze":
					color = new Color(0x744a1d);
					break;
				case "silver":
					color = new Color(0xa1a1a1);
					break;
				case "gold":
					color = new Color(0xe3c61e);
					break;
				case "platinum":
					color = new Color(0x25a9a2);
					break;
				case "diamond":
					color = new Color(0x9a7cf4);
					break;
				case "champion":
					color = new Color(0xc00f59);
					break;
				default:
					color = Color.DarkerGrey;
					break;
			}

			return color;
		}
	}
}

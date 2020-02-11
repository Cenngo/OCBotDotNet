using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordNET.Extensions;

namespace DiscordNET.Modules
{
	[Group("R6")]
	public class R6Module : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commmands;
		private readonly R6Handler _r6;

		public R6Module(DiscordShardedClient client, CommandService commands)
		{
			_client = client;
			_commmands = commands;
			_r6 = new R6Handler();
		}

		[Command("rank")]
		public async Task Rank(string username, string platform = "uplay")
		{
			var results = await _r6.ParseComplete(username, platform);

			var embed = new EmbedBuilder
			{
				Title = $"{results.stats.playerName}",
				Description = $"{results.rank}",
				ThumbnailUrl = results.avatarUrl,
				ImageUrl = "https://ubistatic19-a.akamaihd.net/resource/en-us/game/rainbow6/siege-v3/r6-six.png",
				Color = results.rankColor
			}.Build();

			await Context.Channel.SendMessageAsync(embed: embed);
		}

		[Command("profile")]
		public async Task Profile(string username, string platform = "uplay")
		{
			var results = await _r6.ParseComplete(username, platform);
			var stats = results.stats;

			var kd = Convert.ToDouble(stats.kd) / 100;

			var favAttacker = await _r6.DecodeOperators(stats.favAttacker);
			var favDefender = await _r6.DecodeOperators(stats.favDefender);

			var embed = new EmbedBuilder
			{
				Title = $"{results.stats.playerName}",
				ThumbnailUrl = results.avatarUrl,
				ImageUrl = "https://ubistatic19-a.akamaihd.net/resource/en-us/game/rainbow6/siege-v3/r6-six.png",
				Color = results.rankColor
			}
			.AddField("Level", stats.playerLevel, true)
			.AddField("K/D", kd, true)
			.AddField("Max MMR", stats.maxMMR, true)
			.AddField("Current MMR", stats.currentMMR, true)
			.AddField("Current Rank", results.rank, true)
			.AddBlankField()
			.AddField("Favourite Attacker", favAttacker, true)
			.AddField("Favourite Defender", favDefender, true)	
			.Build();

			await ReplyAsync(embed: embed);
		}
	}
}

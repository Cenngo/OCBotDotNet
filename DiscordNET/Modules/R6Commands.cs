using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	[Group("R6")]
	public class R6Commands : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commmands;

		public R6Commands(DiscordSocketClient client, CommandService commands)
		{
			_client = client;
			_commmands = commands;
		}

		[Command("rank")]
		public async Task Rank(string username, string platform = "pc")
		{
			Console.WriteLine("Command Recieved");
			var r6handler = new R6Handler();
			var stats = await r6handler.ParseComplete(username, platform);

			Console.WriteLine("Parsed Stats");

			var embed = new EmbedBuilder
			{
				Title = $"Rank for {username}",
				Description = $"{stats.rank}",
				ImageUrl = stats.avatarUrl,
				Color = stats.rankColor
			}.Build();

			await Context.Channel.SendMessageAsync("Rank Comming");
			await Context.Channel.SendMessageAsync(embed: embed);
		}

		[Command("test")]
		public async Task Test(string username, string platform = "pc")
		{
			var r6handler = new R6Handler();

			Console.WriteLine("Command Recieved");
			var stats = await r6handler.ParseByName(username, platform);
			Console.WriteLine("Parsed");

			var ID = stats.results[0].pId;

			await ReplyAsync(message: ID);
		}
	}
}

﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET
{
	public class Bot
	{
		public Config jsonConfig { get; private set; }
		public DiscordShardedClient _client { get; private set; }
		public async Task MainAsync ()
		{
			Console.SetWindowSize(160, 25);
			Console.Title = "Discord Bot";
			//jsonConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
			//Set environment variable: DCBOTTOKEN with your Discord bot API token @
			//https://discordapp.com/developers/applications/
			string botToken = Environment.GetEnvironmentVariable("DCBOTTOKEN");
			while (botToken.Length == 0)
			{
				Console.WriteLine("Checking user environment for token...");
				botToken = Environment.GetEnvironmentVariable("DCBOTTOKEN",EnvironmentVariableTarget.User);
				//Console.WriteLine("no token");
			}
			Console.WriteLine("Token: " + botToken);
			
			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug,
				TotalShards = 5
			});

			await _client.StartAsync();
			await _client.LoginAsync(TokenType.Bot, botToken, true);

			CommandService _commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async,
				LogLevel  = LogSeverity.Debug
			});

			ServiceManager serviceManager = new ServiceManager(_client, _commands);
			EventManager eventManager = new EventManager(_client);

			IServiceProvider _services = serviceManager.BuildServiceProvider();

			CommandHandler handler = new CommandHandler(_client, _commands, _services);

			await Task.Delay(-1);
		}
	}
}

﻿using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data.Playlist;
using DiscordNET.Handlers;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using Victoria;

namespace DiscordNET.Managers
{
	public class ServiceManager
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private LiteDatabase _botDB;
		public ServiceManager ( DiscordSocketClient client = null, CommandService commands = null )
		{
			_client = client ?? new DiscordSocketClient();
			_commands = commands ?? new CommandService();
			_botDB = new LiteDatabase(@"BotData.db");
		}

		public IServiceProvider BuildServiceProvider () => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			.AddSingleton(_botDB)
			.AddSingleton<CommandHandler>()
			.AddSingleton<InteractiveService>()
			.AddSingleton<LavaConfig>()
			.AddSingleton<LavaNode>()
			.AddSingleton<MusicManager>()
			.BuildServiceProvider();
	}
}

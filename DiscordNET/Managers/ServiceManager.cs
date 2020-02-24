﻿using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
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
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commands;
		private readonly LiteDatabase _botDB;
		private readonly LiteCollection<GuildConfig> _guildConfig;
		private readonly LavaConfig _lavaConfig;
		public ServiceManager ( DiscordShardedClient client = null, CommandService commands = null )
		{
			_client = client ?? new DiscordShardedClient();
			_commands = commands ?? new CommandService();
			_botDB = new LiteDatabase(@"BotData.db");
			_guildConfig = _botDB.GetCollection<GuildConfig>("GuildConfigs");
			_lavaConfig = new LavaConfig
			{
				LogSeverity = Discord.LogSeverity.Debug,
				ResumeTimeout = new TimeSpan(0, 2, 0),
				SelfDeaf = false,
				EnableResume = true
			};
		}

		public IServiceProvider BuildServiceProvider () => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			.AddSingleton(_botDB)
			.AddSingleton(_guildConfig)
			.AddSingleton<CommandHandler>()
			.AddSingleton<InteractiveService>()
			.AddSingleton(_lavaConfig)
			.AddSingleton<LavaNode>()
			.AddSingleton<MusicManager>()
			.BuildServiceProvider();
	}
}

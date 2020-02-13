using Discord.Addons.InteractiveCommands;
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
		public ServiceManager ( DiscordShardedClient client = null, CommandService commands = null )
		{
			_client = client ?? new DiscordShardedClient();
			_commands = commands ?? new CommandService();
			_botDB = new LiteDatabase(@"BotData.db");
			_guildConfig = _botDB.GetCollection<GuildConfig>("GuildConfigs");
		}

		public IServiceProvider BuildServiceProvider () => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			.AddSingleton(_botDB)
			.AddSingleton(_guildConfig)
			.AddSingleton<CommandHandler>()
			.AddSingleton<InteractiveService>()
			.AddSingleton<LavaConfig>()
			.AddSingleton<LavaNode>()
			.AddSingleton<MusicManager>()
			.BuildServiceProvider();
	}
}

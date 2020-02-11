using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.TypeReaders;
using LiteDB;
using Sparrow.Platform.Posix.macOS;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
	public class CommandHandler
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;
		private readonly IServiceProvider _services;
		private readonly LiteDatabase _database;
		private readonly LiteCollection<GuildConfig> _guildConfig;

		public CommandHandler ( DiscordSocketClient client, CommandService commands, IServiceProvider services )
		{
			_client = client;
			_commands = commands;
			_services = services;

			_client.MessageReceived += HandleCommandAsync;
			_commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			_database = new LiteDatabase(@"BotData.db");
			_guildConfig = _database.GetCollection<GuildConfig>("GuildConfigs");
		}

		private async Task HandleCommandAsync ( SocketMessage msg )
		{
			var message = msg as SocketUserMessage;
			if (message == null) return;

			int argPos = 0;

			var context = new ShardedCommandContext(_client, message);

			if(!_guildConfig.Exists(x => x.GuildId == context.Guild.Id))
			{
				_guildConfig.Insert(new GuildConfig
				{
					GuildId = context.Guild.Id,
					Irritate = false,
					WhiteList = new List<string> { },
					Prefix = new List<string> { ">" }
				});
			}

			if (message.Author.IsBot || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				return;

			var currentConfig = _guildConfig.FindOne(x => x.GuildId == context.Guild.Id);
			var prefixes = currentConfig.Prefix;

			foreach(var prefix in prefixes)
			{
				if (message.HasStringPrefix(prefix, ref argPos)) 
				{
					var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
					return;
				}
			}
		}
	}
}


using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using Victoria;

namespace DiscordNET.Managers
{
	public class ServiceManager
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		public ServiceManager ( DiscordSocketClient client = null, CommandService commands = null )
		{
			_client = client ?? new DiscordSocketClient();
			_commands = commands ?? new CommandService();
		}

		public IServiceProvider BuildServiceProvider () => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			.AddSingleton<CommandHandler>()
			.AddSingleton<InteractiveService>()
			.AddSingleton<LavaConfig>()
			.AddSingleton<LavaNode>()
			.AddSingleton<MusicManager>()
			.BuildServiceProvider();
	}
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.TypeReaders;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
	public class CommandHandler
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;
		private readonly IServiceProvider _services;

		public CommandHandler ( DiscordSocketClient client, CommandService commands, IServiceProvider services )
		{
			_client = client;
			_commands = commands;
			_services = services;
		}

		public async Task InstallCommandsAsync ()
		{
			_client.MessageReceived += HandleCommandAsync;

			_commands.AddTypeReader(typeof(Emote), new EmojiTypeReader());
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
		}

		private async Task HandleCommandAsync ( SocketMessage msg )
		{
			var message = msg as SocketUserMessage;
			if (message == null) return;

			int argPos = 0;

			if (!(message.HasCharPrefix('!', ref argPos) ||
			message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
			message.Author.IsBot)
				return;

			var context = new SocketCommandContext(_client, message);

			var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);

		}
	}
}

using Discord;
using Discord.Addons.CommandsExtension;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Modules;
using DiscordNET.TypeReaders;
using LiteDB;
using Sparrow.Platform.Posix.macOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
	public class CommandHandler
	{
		private readonly CommandService _commands;
		private readonly DiscordShardedClient _client;
		private readonly IServiceProvider _services;
		private readonly LiteDatabase _database;
		private readonly LiteCollection<GuildConfig> _guildConfig;
		private readonly HelpModule _help;

		public CommandHandler ( DiscordShardedClient client, CommandService commands, IServiceProvider services )
		{
			_client = client;
			_commands = commands;
			_services = services;

			_client.MessageReceived += HandleCommandAsync;
			_commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			_database = new LiteDatabase(@"BotData.db");
			_guildConfig = _database.GetCollection<GuildConfig>("GuildConfigs");

			_help = new HelpModule(_commands);
		}

		private async Task HandleCommandAsync ( SocketMessage msg )
		{
			SocketUserMessage message = msg as SocketUserMessage;
			if (message == null) return;

			int argPos = 0;

			ShardedCommandContext context = new ShardedCommandContext(_client, message);

			if (message.Author.IsBot || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				return;

			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == context.Guild.Id);
			List<string> prefixes = currentConfig.Prefix;

			foreach(string prefix in prefixes)
			{
				if (message.HasStringPrefix(prefix, ref argPos)) 
				{
					IResult result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
					if(!result.IsSuccess)
                    {
						var embed = await HelpDialogHandler.ConstructHelpDialog(context, argPos, _commands);
						if(embed != null)
							await msg.Channel.SendMessageAsync(embed:embed, text: "This might be helpful:");
                    }
					return;
				}
			}
		}

		private async Task<Embed> ContructHelpDialog(ICommandContext context, int argPos )
        {
			SearchResult result = _commands.Search(context, argPos);
			if (!result.IsSuccess)
			{
				return null;
			}
			EmbedBuilder helpEmbed = new EmbedBuilder();
			helpEmbed.WithColor(Color.Green);

			CommandMatch command = result.Commands.First();

			helpEmbed.WithTitle(command.Command.GetCommandNameWithGroup().ToUpper());
			helpEmbed.WithDescription(command.Command.Summary);

			if (command.Command.Aliases.Count != 0)
			{
				IEnumerable<string> aliases = command.Command.Aliases.Select(x => $"`{x}`");
				helpEmbed.AddField("Aliases", string.Join(" ", aliases));
			}

			if (command.Command.Parameters.Count != 0)
			{
				StringBuilder paramString = new StringBuilder();

				foreach (Discord.Commands.ParameterInfo param in command.Command.Parameters)
				{
					paramString.AppendLine($"`<{param.Name}>` {param.Type.Name} - **Default:** *{param.DefaultValue ?? "null"}* -> {param.Summary ?? "`no context`"}");
				}
				helpEmbed.AddField("Parameters", paramString.ToString());
				helpEmbed.AddField("Usage", command.Command.Name + " " + string.Join(' ', command.Command.Parameters.Select(x => $"`{x.Name}`").ToArray()));
			}
			return helpEmbed.Build();
		}
	}
}

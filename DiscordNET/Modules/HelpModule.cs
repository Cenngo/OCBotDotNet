using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.CommandsExtension;
using Discord;
using System.Linq;

namespace DiscordNET.Modules
{
	public class HelpModule : ModuleBase<ShardedCommandContext>
	{
		private readonly CommandService _commands;

		public HelpModule ( CommandService commands )
		{
			_commands = commands;
		}

		[Command("help")]
		public async Task Help ( [Remainder] string arg = null )
		{
			if (arg == null)
			{
				List<ModuleInfo> modules = _commands.Modules.ToList();
				StringBuilder embedString = new StringBuilder();

				EmbedBuilder helpEmbed = new EmbedBuilder()
				{
					Title = "Commands",
					Color = Color.Green
				}.WithFooter("Use the `HELP` with any command to get command specific information");

				foreach (ModuleInfo module in modules)
				{
					List<CommandInfo> commands = module.Commands.ToList();

					if (commands.Count == 0) continue;

					StringBuilder commandString = new StringBuilder();

					foreach (CommandInfo command in commands)
					{
						commandString.Append($" `{command.GetCommandNameWithGroup()}`");
					}
					helpEmbed.AddField(module.Name, commandString.ToString());
				}

				await ReplyAsync(embed: helpEmbed.WithDescription(embedString.ToString()).Build());
			}
			else
			{
				SearchResult result = _commands.Search(arg);
				if (!result.IsSuccess)
				{
					await ReplyAsync($"No command found for {arg}");
					return;
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

					foreach (ParameterInfo param in command.Command.Parameters)
					{
						paramString.AppendLine($"`<{param.Name}>` {param.Type} - **Default:** *{param.DefaultValue ?? "null"}* -> {param.Summary ?? "`no context`"}");
					}
					helpEmbed.AddField("Parameters", paramString.ToString());
				}
				await ReplyAsync(embed: helpEmbed.Build());
			}
		}
	}
}

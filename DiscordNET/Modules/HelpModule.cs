using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.CommandsExtension;
using System.Linq;
using Discord;
using Raven.Client.Documents.Queries.MoreLikeThis;
using Raven.Client.Documents.Commands.Batches;

namespace DiscordNET.Modules
{
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _commands;

		public HelpModule(CommandService commands)
		{
			_commands = commands;
		}

		[Command("help")]
		public async Task Help([Remainder] string arg = null)
		{
			if(arg == null)
			{
				var modules = _commands.Modules.ToList();
				var embedString = new StringBuilder();

				EmbedBuilder helpEmbed = new EmbedBuilder()
				{
					Title = "Commands",
					Color = Color.Green
				}.WithFooter("Use the `HELP` with any command to get command specific information");

				foreach (var module in modules)
				{
					var commands = module.Commands.ToList();

					if (commands.Count == 0) continue;

					var commandString = new StringBuilder();

					foreach (var command in commands)
					{
						commandString.Append($" `{command.GetCommandNameWithGroup()}`");
					}
					helpEmbed.AddField(module.Name, commandString.ToString());
				}

				await ReplyAsync(embed: helpEmbed.WithDescription(embedString.ToString()).Build());
			}
			else
			{
				var result = _commands.Search(arg);
				if (!result.IsSuccess)
				{
					await ReplyAsync($"No command found for {arg}");
					return;
				}
				var helpEmbed = new EmbedBuilder();
				helpEmbed.WithColor(Color.Green);

				var command = result.Commands.First();

				helpEmbed.WithTitle(command.Command.GetCommandNameWithGroup().ToUpper());
				helpEmbed.WithDescription(command.Command.Summary);

				if (command.Command.Aliases.Count != 0)
				{
					var aliases = command.Command.Aliases.Select(x => $"`{x}`");
					helpEmbed.AddField("Aliases", string.Join(" ", aliases));
				}

				if(command.Command.Parameters.Count != 0)
				{
					var paramString = new StringBuilder();

					foreach(var param in command.Command.Parameters)
					{
						paramString.AppendLine($"`<{param.Name}>` {param.Type} - **Default:** *{param.DefaultValue ?? "null"}* -> {param.Summary}");
					}
					helpEmbed.AddField("Parameters", paramString.ToString());
				}
				await ReplyAsync(embed: helpEmbed.Build());
			}
		}
	}
}

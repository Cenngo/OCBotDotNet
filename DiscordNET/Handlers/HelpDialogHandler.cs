using Discord;
using Discord.Addons.CommandsExtension;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
    public static class HelpDialogHandler
    {
        public static async Task<Embed> ConstructHelpDialog(ICommandContext context, int  argPos, CommandService commands )
        {
			SearchResult result = commands.Search(context, argPos);
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
					paramString.AppendLine($"`<{param.Name}>` {param.Type.Name} - **Default:** *{param.DefaultValue ?? "null"}* : {param.Summary ?? "`no context`"}");
				}
				helpEmbed.AddField("Parameters", paramString.ToString());
				helpEmbed.AddField("Usage", command.Command.GetCommandNameWithGroup() + " " + string.Join(' ', command.Command.Parameters.Select(x => $"`{x.Name}`").ToArray()));
			}
			return helpEmbed.Build();
		}
    }
}

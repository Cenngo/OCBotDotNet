using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.CommandsExtension;

namespace DiscordNET.Modules
{
	public class HelpModule : ModuleBase<ShardedCommandContext>
	{
		private readonly CommandService _commands;

		public HelpModule(CommandService commands)
		{
			_commands = commands;
		}

		[Command("help")]
		public async Task Help([Remainder] string command = null)
		{
			var helpEmbed = _commands.GetDefaultHelpEmbed(command, ">");
			await ReplyAsync(embed: helpEmbed);
		}
	}
}

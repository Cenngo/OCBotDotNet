using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DiscordNET.TypeReaders
{
	public class EmojiTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync ( ICommandContext context, string input, IServiceProvider services )
		{
			if (Emote.TryParse(input, out var emote))
				return Task.FromResult(TypeReaderResult.FromSuccess(emote));

			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "The argumnet could not be parsed as a Discord Emote"));
		}
	}
}

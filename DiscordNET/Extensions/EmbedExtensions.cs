using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using System.Runtime.CompilerServices;

namespace DiscordNET.Extensions
{
	public static class EmbedExtensions
	{
		public static EmbedBuilder AddBlankField (this EmbedBuilder builder)
		{
			var field = new EmbedFieldBuilder()
				.WithIsInline(false)
				.WithName("\u200b")
				.WithValue("\u200b");
			builder.AddField(field);
			return builder;
		}
	}
}

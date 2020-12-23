using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET
{
    public abstract class CommandModule<T>: ModuleBase<T> where T  : class, ICommandContext
    {
        protected Discord.Color EmbedColor = Color.Default;
        
        protected async Task PrintText(string text)
        {
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Description = text,
                Color = EmbedColor
            }.Build());
        }

        protected async Task PrintText(string title, string text )
        {
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Title = title,
                Description = text,
                Color = EmbedColor
            }.Build());
        }
    }
}

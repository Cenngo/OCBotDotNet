using Discord;
using Discord.Addons.CommandsExtension;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Modules;
using LiteDB;
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

        public CommandHandler ( DiscordShardedClient client, CommandService commands, IServiceProvider services )
        {
            _client = client;
            _commands = commands;
            _services = services;

            _client.MessageReceived += HandleCommandAsync;
            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _database = new LiteDatabase(@"BotData.db");
            _guildConfig = _database.GetCollection<GuildConfig>("GuildConfigs");
        }

        private async Task HandleCommandAsync ( SocketMessage msg )
        {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            ShardedCommandContext context = new ShardedCommandContext(_client, message);

            if (message.Author.IsBot || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            if (context.IsPrivate)
                await HandleDM(context, argPos);
            else
                await HandleGuild(context, argPos);
        }

        private async Task HandleDM( ICommandContext context, int argPos )
        {
            if(context.Message.HasCharPrefix('>', ref argPos)){
                IResult result = await _commands.ExecuteAsync(context, argPos, _services);
                return;
            }
        }

        private async Task HandleGuild(ICommandContext context, int argPos)
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == context.Guild.Id);
            List<string> prefixes = currentConfig.Prefix;

            foreach (string prefix in prefixes)
            {
                if (context.Message.HasStringPrefix(prefix, ref argPos))
                {
                    IResult result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
                    if (!result.IsSuccess)
                    {
                        var embed = HelpDialogHandler.ConstructHelpDialog(context, argPos, _commands);
                        if (embed != null)
                            await context.Message.Channel.SendMessageAsync(embed: embed, text: "This might be helpful:");
                    }
                    return;
                }
            }
        }
    }
}

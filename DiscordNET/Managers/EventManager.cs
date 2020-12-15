using Discord;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
    public sealed class EventManager
    {
        private readonly DiscordShardedClient _client;
        private readonly LiteDatabase _botDB;
        private readonly LiteCollection<GuildConfig> _guildConfig;
        private readonly Random _randomizer;
        private ConsoleColor _logColor;

        public EventManager ( DiscordShardedClient client, Auth auth )
        {
            _client = client;
            _botDB = new LiteDatabase(@"BotData.db");
            _guildConfig = _botDB.GetCollection<GuildConfig>("GuildConfigs");
            _randomizer = new Random(DateTime.Now.Second);
            _logColor = auth.DiscordLogColor;

            _client.Log += OnLog;
            _client.ShardReady += OnReady;
            _client.UserJoined += OnUserJoined;
            _client.UserIsTyping += OnUserTyping;
            _client.JoinedGuild += OnJoinedGuild;
            _client.GuildAvailable += onGuildAvailable;
        }

        private Task onGuildAvailable ( SocketGuild arg )
        {
            var guildId = arg.Id;
            if (!_guildConfig.Exists(x => x.GuildId == guildId))
            {
                _guildConfig.Insert(new GuildConfig()
                {
                    GuildId = guildId,
                    Irritate = false,
                    WhiteList = new List<string> { },
                    Prefix = new List<string> { ">" },
                    useWhitelist = true,
                    BlackList = new List<string> { },
                    Curses = new List<string> { "Ne Yazıyon Lan Amkodum" },
                });
            }
            return Task.CompletedTask;
        }

        private Task OnJoinedGuild ( SocketGuild arg )
        {
            throw new NotImplementedException();
        }

        private Task OnUserJoined ( SocketGuildUser arg )
        {
            throw new NotImplementedException();
        }

        private async Task OnUserTyping ( SocketUser user, ISocketMessageChannel channel )
        {
            SocketGuild guild = ( channel as SocketGuildChannel )?.Guild;

            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == guild.Id);
            var opMode = currentConfig.useWhitelist;

            var checkList = opMode ? currentConfig.WhiteList : currentConfig.BlackList;

            if (currentConfig.Irritate && ( checkList.Exists(x => x == string.Join(" ", user.Username, user.Discriminator)) != opMode ))
            {
                var curse = currentConfig.Curses[_randomizer.Next(currentConfig.Curses.Count)];
                await channel.SendMessageAsync(curse);
            }
        }

        private Task OnReady ( DiscordSocketClient arg )
        {
            return Task.CompletedTask;
        }

        private Task OnLog ( LogMessage arg )
        {
            Console.ForegroundColor = _logColor;
            Console.WriteLine(string.Format("[{0,8}] {1,-10}: {2}", DateTime.Now.ToString("hh: mm:ss"), arg.Source, arg.Message));
            return Task.CompletedTask;
        }
    }
}

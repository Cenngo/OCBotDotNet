using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;
using System;
using System.IO;
using System.Xml.Serialization;
using Victoria;

namespace DiscordNET.Managers
{
    public class ServiceManager
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commands;
        private readonly LiteDatabase _botDB;
        private readonly LiteCollection<GuildConfig> _guildConfig;
        private readonly LavaConfig _lavaConfig;
        private readonly Auth _auth;
        private readonly Random _random;
        private readonly SpotifyClientConfig _spotifyConfig;
        public ServiceManager ( DiscordShardedClient client = null, CommandService commands = null )
        {
            _client = client ?? new DiscordShardedClient();
            _commands = commands ?? new CommandService();
            _botDB = new LiteDatabase(@"./BotData.db");
            _guildConfig = _botDB.GetCollection<GuildConfig>("GuildConfigs");

            var ser = new XmlSerializer(typeof(Auth));
            using (var reader = new FileStream("./auth.xml", System.IO.FileMode.Open))
                _auth = (Auth)ser.Deserialize(reader);

            _lavaConfig = new LavaConfig
            {
                LogSeverity = Discord.LogSeverity.Debug,
                ResumeTimeout = new TimeSpan(0, 2, 0),
                SelfDeaf = true,
                EnableResume = true,
                Port = _auth.LavalinkPort,
                ReconnectAttempts = 10,
            };

            _random = new Random(DateTime.Now.Second);
            _spotifyConfig = SpotifyClientConfig.CreateDefault().
                WithAuthenticator(new ClientCredentialsAuthenticator("560f7c2b97f54d429ec5fb926edafc89", "ab2eabd7aa894ea3b69b19f1a738948a"));
        }

        public IServiceProvider BuildServiceProvider ( ) => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_botDB)
            .AddSingleton(_guildConfig)
            .AddSingleton(_lavaConfig)
            .AddSingleton(_spotifyConfig)
            .AddSingleton<MusicManager>()
            .AddSingleton(_auth)
            .AddSingleton(_random)
            .AddSingleton<LavaNode>()
            .AddSingleton<SpotifyClient>()
            .BuildServiceProvider();
    }
}

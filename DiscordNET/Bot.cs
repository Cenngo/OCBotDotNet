using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DiscordNET
{
    public class Bot
    {
        public DiscordShardedClient _client { get; private set; }
        private Auth _auth;

        public Bot ( )
        {
            var ser = new XmlSerializer(typeof(Auth));

            if (!File.Exists("./auth.xml"))
            {
                using (var stream = File.Create("./auth.xml"))
                {
                    ser.Serialize(stream, new Auth());
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("PLEASE POPULATE THE AUTH FILE THAT CAN BE FOUND IN THE EXECUTION FOLDER!!");
                Console.ResetColor();
                Environment.Exit(0);
            }

            using (var reader = new FileStream("auth.xml", FileMode.Open))
                _auth = (Auth)ser.Deserialize(reader);
        }

        public async Task MainAsync ( )
        {
            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                TotalShards = 5
            });

            await _client.SetActivityAsync(new Game("Prefix: '>', For Help: '>help'", ActivityType.Playing, ActivityProperties.None));

            if(_auth.DiscordToken == string.Empty)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Bot Token Not Found");
                return;
            }

            await _client.StartAsync();
            await _client.LoginAsync(TokenType.Bot, _auth.DiscordToken, true);

            CommandService _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            ServiceManager serviceManager = new ServiceManager(_client, _commands);
            EventManager eventManager = new EventManager(_client, _auth);

            IServiceProvider _services = serviceManager.BuildServiceProvider();

            CommandHandler handler = new CommandHandler(_client, _commands, _services);

            await Task.Delay(-1);
        }
    }
}

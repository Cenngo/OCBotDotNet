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
        public DiscordShardedClient Client { get; private set; }
        private readonly Auth _auth;

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

            using var reader = new FileStream("auth.xml", FileMode.Open);
            _auth = (Auth)ser.Deserialize(reader);
        }

        public async Task MainAsync ( )
        {
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                TotalShards = 5
            });

            await Client.SetActivityAsync(new Game("Prefix: '>', For Help: '>help'", ActivityType.Playing, ActivityProperties.None));

            if(_auth.DiscordToken == string.Empty)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Bot Token Not Found");
                Console.ResetColor();
                return;
            }

            await Client.StartAsync();
            await Client.LoginAsync(TokenType.Bot, _auth.DiscordToken, true);

            CommandService _commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            ServiceManager serviceManager = new ServiceManager(Client, _commands);
            _ = new EventManager(Client, _auth);

            IServiceProvider _services = serviceManager.BuildServiceProvider();

            _ = new CommandHandler(Client, _commands, _services);

            await Task.Delay(-1);
        }
    }
}

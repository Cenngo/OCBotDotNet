using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DiscordNET
{
	public class Bot
	{
		public Config jsonConfig { get; private set; }
		public DiscordShardedClient _client { get; private set; }
		private Auth _auth;

		public Bot()
		{
			var ser = new XmlSerializer(typeof(Auth));

			using (var reader = new FileStream("auth.xml", FileMode.Open))
				_auth = (Auth)ser.Deserialize(reader);
		}

		public async Task MainAsync ()
		{	
			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug,
				TotalShards = 5
			});

			await _client.SetActivityAsync(new Game("Prefix: '>', For Help: '>help'", ActivityType.Playing, ActivityProperties.None));

			await _client.StartAsync();
			await _client.LoginAsync(TokenType.Bot, _auth.DiscordToken, true);

			CommandService _commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async,
				LogLevel  = LogSeverity.Debug
			});

			ServiceManager serviceManager = new ServiceManager(_client, _commands);
			EventManager eventManager = new EventManager(_client, _auth);

			IServiceProvider _services = serviceManager.BuildServiceProvider();

			CommandHandler handler = new CommandHandler(_client, _commands, _services);

			await Task.Delay(-1);
		}
	}
}

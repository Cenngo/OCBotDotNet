using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using LiteDB;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DiscordNET
{
	public class Bot
	{
		public Config jsonConfig { get; private set; }
		public DiscordShardedClient _client { get; private set; }
		public async Task MainAsync ()
		{
			jsonConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug,
				TotalShards = 2
			});

			await _client.SetGameAsync(">help", type: ActivityType.Playing);

			await _client.StartAsync();
			await _client.LoginAsync(TokenType.Bot, jsonConfig.Token, true);

			CommandService _commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async,
				LogLevel  = LogSeverity.Debug
			});

			var serviceManager = new ServiceManager(_client, _commands);
			var eventManager = new EventManager(_client);

			var _services = serviceManager.BuildServiceProvider();

			var handler = new CommandHandler(_client, _commands, _services);

			await Task.Delay(-1);
		}
	}
}

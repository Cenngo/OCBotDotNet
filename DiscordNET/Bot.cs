using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Handlers;
using DiscordNET.Managers;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DiscordNET
{
	public class Bot
	{
		public Config jsonConfig { get; private set; }
		public DiscordSocketClient _client { get; private set; }
		public async Task MainAsync ()
		{
			jsonConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Debug
			});

			await _client.SetGameAsync(">help", type: ActivityType.Playing);

			await _client.StartAsync();
			await _client.LoginAsync(Discord.TokenType.Bot, jsonConfig.Token, true);

			CommandService _commands = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async
			});

			var serviceManager = new ServiceManager(_client, _commands);
			var eventManager = new EventManager(_client);

			var _services = serviceManager.BuildServiceProvider();

			var handler = new CommandHandler(_client, _commands, _services);
			await handler.InstallCommandsAsync();

			await Task.Delay(-1);
		}
	}
}

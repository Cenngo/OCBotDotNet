using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordNET.Modules
{
	public class InteractiveModule : ModuleBase<SocketCommandContext>
	{
		private readonly InteractiveService _interactivity;
		private readonly DiscordSocketClient _client;

		public InteractiveModule ( DiscordSocketClient client, InteractiveService interactivity )
		{
			_interactivity = interactivity;
			_client = client;
		}
	}
}

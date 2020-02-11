using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordNET.Modules
{
	public class InteractiveModule : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;

		public InteractiveModule ( DiscordShardedClient client)
		{
			_client = client;
		}
	}
}

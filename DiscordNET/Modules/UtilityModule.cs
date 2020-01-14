using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class UtilityModule : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;

		public UtilityModule ( DiscordSocketClient client )
		{
			_client = client;
		}

		[Command("delete")]
		[RequireUserPermission(Discord.GuildPermission.ManageMessages)]
		[Summary("Deletes Messages")]
		public async Task Delete ( int count = 1 )
		{
			if (count > 100)
			{
				await Context.Channel.SendMessageAsync(":exclamation: You can't delete more than 100 messages at a time");
				return;
			}

			var messages = Context.Channel.GetMessagesAsync(count);

			//FIX BUG: No Method found for bulk deletion
			//await Context.Channel.DeleteMessagesAsync(messages);
		}

		[Command("dice")]
		[Summary("Random Number Generator")]
		[Alias("random")]
		public async Task Dice ( int maxValue = 6 )
		{
			var random = new Random(Convert.ToInt32(DateTime.UnixEpoch));

			await Context.Channel.SendMessageAsync(random.Next().ToString());
		}
	}
}

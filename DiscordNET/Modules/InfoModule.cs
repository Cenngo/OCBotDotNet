using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public InfoModule ( DiscordSocketClient client, CommandService commands )
		{
			_client = client;
			_commands = commands;
		}

		[Command("ping")]
		public async Task Ping ()
		{
			await Context.Channel.SendMessageAsync("Pong");
		}

		[Command("info")]
		public async Task Info ()
		{
			var guild = Context.Guild;
			var name = guild.Name;
			var owner = guild.Owner.Username;
			var members = guild.MemberCount;

			var infoEmbed = new EmbedBuilder()
			{
				Title = $"Info for Guild: {name}",
				Color = Color.Orange
			}
			.AddField("Owner", owner, true)
			.AddField("Member Count", members.ToString(), true)
			.Build();

			await Context.Channel.SendMessageAsync(embed: infoEmbed);
		}

		[Command("invite")]
		public async Task Invite()
		{
			var dmChannel = await Context.User.GetOrCreateDMChannelAsync();

			await dmChannel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=646070311371931661&permissions=0&scope=bot");
		}
	}
}

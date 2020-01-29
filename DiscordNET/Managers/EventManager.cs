using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
	public sealed class EventManager
	{
		private readonly DiscordShardedClient _client;

		public EventManager ( DiscordShardedClient client )
		{
			_client = client;

			_client.Log += OnLog;
			_client.ShardReady += OnReady;
			_client.JoinedGuild += OnJoinedGuild;
			_client.UserIsTyping += OnUserTyping;
		}

		private async Task OnUserTyping ( SocketUser user, ISocketMessageChannel channel )
		{
			//await channel.SendMessageAsync($"Ne yazyıon tipini siktigim {user.Mention}");
		}

		private async Task OnJoinedGuild ( SocketGuild arg )
		{
			var welcomeEmbed = new EmbedBuilder()
			{
				Title = $"Welcome to the {arg.Name} Server",
				Color = Color.Orange,
				Author = new EmbedAuthorBuilder
				{
					Name = $"{_client.CurrentUser.Username}",
					IconUrl = _client.CurrentUser.GetAvatarUrl().ToString()
				}
			}.Build();

			await arg.SystemChannel.SendMessageAsync(embed: welcomeEmbed);
		}

		private Task OnReady (DiscordSocketClient arg)
		{
			return Task.CompletedTask;
		}

		private async Task OnLog ( LogMessage arg )
		{
			Console.WriteLine(arg.ToString());
		}
	}
}

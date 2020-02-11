using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using System;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
	public sealed class EventManager
	{
		private readonly DiscordShardedClient _client;
		private readonly LiteDatabase _botDB;
		private readonly ILiteCollection<GuildConfig> _guildConfig;

		public EventManager ( DiscordSocketClient client )
		{
			_client = client;
			_botDB = new LiteDatabase(@"BotData.db");
			_guildConfig = _botDB.GetCollection<GuildConfig>("GuildConfigs");

			_client.Log += OnLog;
			_client.ShardReady += OnReady;
			_client.UserJoined += OnJoinedGuild;
			_client.UserIsTyping += OnUserTyping;
		}

		private async Task OnJoinedGuild ( SocketGuildUser arg )
		{
			SocketGuild guild = arg.Guild;

			Embed welcomeEmbed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.CurrentUser.Username,
					IconUrl = guild.CurrentUser.GetAvatarUrl()
				},
				Title = $"Welcome to the {guild.Name}"
			}.Build();
			await guild.DefaultChannel.SendMessageAsync(embed: welcomeEmbed);
		}

		private async Task OnUserTyping ( SocketUser user, ISocketMessageChannel channel )
		{
			var guild = (channel as SocketGuildChannel)?.Guild;

			var currentConfig = _guildConfig.FindOne(x => x.GuildId == guild.Id);
			var whitelist = currentConfig.WhiteList;

			if (!currentConfig.Irritate || whitelist.Exists(x => x == string.Join(" ", user.Username, user.Discriminator)))
			{
				return;
			}
			await channel.SendMessageAsync("Ne Yazıyon Lan Amkodum");
		}

		private Task OnReady ()
		{
			return Task.CompletedTask;
		}

		private Task OnLog ( LogMessage arg )
		{
			var argArray = arg.ToString().Split(" ");
			var info = argArray[0] + " " + argArray[1] + " " + argArray[2];
			string remainder = string.Empty;

			for(int i = 3; i < argArray.Length; i++)
			{
				remainder += " " + argArray[i];
			}
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write(info);
			Console.ResetColor();
			Console.Write(remainder + "\n");

			return Task.CompletedTask;
		}
	}
}

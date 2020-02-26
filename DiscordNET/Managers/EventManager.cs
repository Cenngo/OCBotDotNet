using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
	public sealed class EventManager
	{
		private readonly DiscordShardedClient _client;
		private readonly LiteDatabase _botDB;
		private readonly LiteCollection<GuildConfig> _guildConfig;

		public EventManager ( DiscordShardedClient client )
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
			SocketGuild guild = (channel as SocketGuildChannel)?.Guild;

			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == guild.Id);
			List<string> whitelist = currentConfig.WhiteList;

			if (!currentConfig.Irritate || whitelist.Exists(x => x == string.Join(" ", user.Username, user.Discriminator)))
			{
				return;
			}
			await channel.SendMessageAsync("Ne Yazıyon Lan Amkodum");
		}

		private Task OnReady (DiscordSocketClient arg)
		{
			return Task.CompletedTask;
		}

		private Task OnLog ( LogMessage arg )
		{
			StringBuilder infoString = new StringBuilder();
			StringBuilder messageString = new StringBuilder();

			infoString.AppendJoin(" ", DateTime.Now.ToString("hh:mm:ss"), arg.Source);

			messageString.AppendJoin(" ", arg.Message, arg.Exception);

			Console.ForegroundColor = ConsoleColor.Magenta;

			Console.Write(infoString.ToString());
			Console.ResetColor();
			Console.WriteLine($"\t {messageString}");

			return Task.CompletedTask;
		}
	}
}

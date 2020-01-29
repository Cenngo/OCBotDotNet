﻿using Discord;
using Discord.WebSocket;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DiscordNET.Managers
{
	public sealed class EventManager
	{
		private readonly DiscordSocketClient _client;

		public EventManager ( DiscordSocketClient client )
		{
			_client = client;

			_client.Log += OnLog;
			_client.Ready += OnReady;
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

		private Task OnReady ()
		{
			return Task.CompletedTask;
		}

		private Task OnLog ( LogMessage arg )
		{
			var argArray = arg.ToString().Split(" ");
			var info = argArray[0] + " " + argArray[1];
			string remainder = string.Empty;

			for(int i = 2; i < argArray.Length; i++)
			{
				remainder += " " + argArray[i];
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(info);
			Console.ResetColor();
			Console.Write(remainder + "\n");

			return Task.CompletedTask;
		}
	}
}

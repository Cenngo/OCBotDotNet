﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class InfoModule : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commands;

		public InfoModule ( DiscordShardedClient client, CommandService commands, LiteDatabase database )
		{
			_client = client;
			_commands = commands;
		}

		[Command("ping")]
		public async Task Ping ()
		{	
			await ReplyAsync("Pong");
		}

		[Command("info")]
		[Summary("Get the guild information")]
		public async Task Info ()
		{
			SocketGuild guild = Context.Guild;
			string name = guild.Name;
			string owner = guild.Owner.Username;
			int members = guild.MemberCount;

			Embed infoEmbed = new EmbedBuilder()
			{
				Title = $"Info for Guild: {name}",
				Color = Color.Orange
			}
			.AddField("Owner", owner, true)
			.AddField("Member Count", members.ToString(), true)
			.Build();

			await ReplyAsync(embed: infoEmbed);
		}

		[Command("invite")]
		[Summary("Get the invite link for the bot")]
		public async Task Invite()
		{
			IDMChannel dmChannel = await Context.User.GetOrCreateDMChannelAsync();

			await dmChannel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=646070311371931661&permissions=0&scope=bot");
		}

		[Command("showuser")]
		public async Task ShowUser()
		{
			UserCollection userData = JsonConvert.DeserializeObject<UserCollection>(File.ReadAllText("users.json"));

			UserData user = userData.userList.FirstOrDefault(x => x.discordID == Context.User.Id);
			if (user == default(UserData))
			{
				await ReplyAsync("User is not registered to a language");
				return;
			}
			else
			{
				Embed infoEmbed = new EmbedBuilder()
				{
					Title = $"User: {Context.User}",
					Color = Color.Orange
				}
			.AddField("Handle", user.dHandle, true)
			.AddField("Language", user.langauge.ToString(), true)
			.Build();

				await ReplyAsync(embed: infoEmbed);
			}
		}
		[Command("registerlang")]
		public async Task RegisterLang(string lang)
		{
			UserCollection userData = JsonConvert.DeserializeObject<UserCollection>(File.ReadAllText("users.json"));

			UserData userMatch = userData.userList.FirstOrDefault(x => x.discordID == Context.User.Id);
			Insult insults = JsonConvert.DeserializeObject<Insult>(File.ReadAllText("insults.json"));
			//try
			//{
			//	RegionInfo myRI1 = new RegionInfo("anan");
			//	await ctx.Channel.SendMessageAsync(myRI1.TwoLetterISORegionName);
			//}
			//catch (ArgumentException e)
			//{
			//	await ctx.Channel.SendMessageAsync(e.ToString());
			//}
			if (!insults.SupportedLanguages.Contains(lang)){
				await ReplyAsync("Language "+lang+" not supported.");
			}
			else if (userMatch != default(UserData))
			{
				userMatch.langauge = lang; 
				await ReplyAsync("User " + userMatch.dHandle + " has been changed as a " + userMatch.langauge + " speaker.");
			}
			else
			{
				try
				{
					userData.userList.Add(new UserData
					{
						dHandle = Context.User.Username + "#" + Context.User.Discriminator,
						discordID = Context.User.Id,
						langauge = lang
					});
					await ReplyAsync("User " + Context.User.Username + "#" + Context.User.Discriminator + " has been registered as a " + lang + " speaker.");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);

					throw;
				}
			}

			File.WriteAllText(@"users.json", JsonConvert.SerializeObject(userData, Formatting.Indented));
		}
	}
}

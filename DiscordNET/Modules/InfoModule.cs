using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private LiteDatabase _database;
		private LiteCollection<userData> _userCollection;
		private LiteCollection<InsultCollection> _insultColection;

		public InfoModule ( DiscordSocketClient client, CommandService commands, LiteDatabase database )
		{
			_client = client;
			_commands = commands;
			_database = database;
			_userCollection = database.GetCollection<userData>("UserCollection");
			_insultColection = database.GetCollection<InsultCollection>("InsultCollection");
		}

		[Command("ping")]
		public async Task Ping ()
		{	
			await ReplyAsync("Pong");
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

		[Command("showuser")]
		public async Task ShowUser()
		{
			UserCollection userData = JsonConvert.DeserializeObject<UserCollection>(File.ReadAllText("users.json"));

			var user = userData.userList.FirstOrDefault(x => x.discordID == Context.User.Id);
			if (user == default(userData))
			{
				await ReplyAsync("User is not registered to a language");
				return;
			}
			else
			{
				var infoEmbed = new EmbedBuilder()
				{
					Title = $"User: {Context.User}",
					Color = Color.Orange
				}
			.AddField("Handle", user.dHandle, true)
			.AddField("Language", user.langauge.ToString(), true)
			.Build();

				await Context.Channel.SendMessageAsync(embed: infoEmbed);
			}
		}
		[Command("registerlang")]
		public async Task RegisterLang(string lang)
		{
			//_database.Engine
			//UserCollection userData = JsonConvert.DeserializeObject<UserCollection>(File.ReadAllText("users.json"));
			var userMatch = _userCollection.FindOne(x => x.discordID == Context.User.Id);
			if (!_insultColection.Exists(x => x.Language == lang))
			{
				await ReplyAsync("Language " + lang + " not supported.");
			}
			else if (_userCollection != null)
			{
				userMatch.langauge = lang;
				await ReplyAsync("User " + userMatch.dHandle + " has been changed as a " + userMatch.langauge + " speaker.");
			}
			else
			{
				try
				{
					_userCollection.Insert(new userData
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
			//userData userMatch = userData.userList.FirstOrDefault(x => x.discordID == Context.User.Id);
			InsultCollection insults = JsonConvert.DeserializeObject<InsultCollection>(File.ReadAllText("insults.json"));
			//try
			//{
			//	RegionInfo myRI1 = new RegionInfo("anan");
			//	await ctx.Channel.SendMessageAsync(myRI1.TwoLetterISORegionName);
			//}
			//catch (ArgumentException e)
			//{
			//	await ctx.Channel.SendMessageAsync(e.ToString());
			//}
			
			
			//File.WriteAllText(@"users.json", JsonConvert.SerializeObject(userData, Formatting.Indented));
		}
		[Command("yaraklariolustur")]
		public async Task createDB()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("yes");

			_insultColection.Insert(new InsultCollection
			{
				Language = "TR",
				Insults = new List<string>
				{
					"yarak", "zenci", "annesiz"
				}
			});
			Console.WriteLine("yes1");

			_insultColection.Insert(new InsultCollection
			{
				Language = "EN",
				Insults = new List<string>
				{
					"motherfucker", "dick", "retard"
				}
			});
			Console.WriteLine("yes2");
			Console.ResetColor();
		}
	}
}

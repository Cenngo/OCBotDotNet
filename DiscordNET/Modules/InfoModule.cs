using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class InfoModule : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commands;
		private LiteDatabase _database;
		private LiteCollection<userData> _userCollection;
		private LiteCollection<InsultCollection> _insultColection;

		public InfoModule ( DiscordShardedClient client, CommandService commands )
		{
			_client = client;
			_commands = commands;
			_database = database;
			_userCollection = _database.GetCollection<userData>("UserCollection");
			_insultColection = _database.GetCollection<InsultCollection>("InsultCollection");
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
		[Command("insult create")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task createInsult ( string language, string loc )
		{
			var json = JsonConvert.DeserializeObject<InsultJSON>(File.ReadAllText(@loc));
			var insults = json.Insults;

			if(_insultColection.Exists(x => x.Language == language))
			{
				await ReplyAsync("Language library with the same name already exists.");
				return;
			}

			_insultColection.Insert(new InsultCollection
			{
				Language = language,
				Insults = insults
			});

			Console.WriteLine(new LogMessage(LogSeverity.Info, "Database", "Successfully Imported Insult Library").ToString());
		}

		[Command("insult add")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task addInsult (string language, string loc )
		{
			var json = JsonConvert.DeserializeObject<InsultJSON>(File.ReadAllText(@loc));
			var insults = json.Insults;

			var chosenLang = _insultColection.FindOne(x => x.Language == language);
			if (chosenLang == null)
			{
				await ReplyAsync("Language Not Found");
				return;
			}

			chosenLang.Insults.AddRange(insults);
			_insultColection.Update(chosenLang);

			Console.WriteLine(new LogMessage(LogSeverity.Info, "Database", "Successfully Imported and Updated Insult Library").ToString());
		}
	}
}

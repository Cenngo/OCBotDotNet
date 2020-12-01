using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using LiteDB;
using Newtonsoft.Json;
using Raven.Client.Documents.Smuggler;
using Raven.Client.Exceptions.Database;
using Raven.Client.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class UtilityModule : ModuleBase<ShardedCommandContext>
	{
		private readonly DiscordShardedClient _client;
		private readonly LiteDatabase _database;
		private readonly LiteCollection<GuildConfig> _guildConfig;

		public UtilityModule ( DiscordShardedClient client, LiteDatabase database, LiteCollection<GuildConfig> guildConfig )
		{
			_client = client;
			_database = database;
			_guildConfig = guildConfig;
		}

		[Command("delete")]
		[RequireUserPermission(GuildPermission.ManageMessages)]
		[Summary("Deletes Messages")]
		public async Task Delete ( int count = 1 )
		{
			if (count > 100)
			{
				await Context.Channel.SendMessageAsync(":exclamation: You can't delete more than 100 messages at a time");
				return;
			}

			var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
			SocketTextChannel channel = Context.Channel as SocketTextChannel;

			await channel.DeleteMessagesAsync(messages);
		}

		[Command("dice")]
		[Summary("Random Number Generator")]
		[Alias("random")]
		public async Task Dice ( int maxValue = 6 )
		{
			Random random = new Random(DateTime.Now.Second);

			int randomNumber = random.Next(1, maxValue);

			await ReplyAsync(randomNumber.ToString());
		}

		[Command("prefix list")]
		[Summary("List all of the prefixes that are resgistered under the current guild")]
		public async Task ListPrefixes()
		{
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
			List<string> prefixes = currentConfig.Prefix;

			StringBuilder replyString = new StringBuilder();

			foreach(string item in prefixes)
			{
				replyString.Append(string.Concat("`", item, "`\t"));
			}

			await ReplyAsync(replyString.ToString());
		}

		[Command("prefix add")]
		[Summary("Register a new prefix under the current guild")]
		public async Task AddPrefix ([Summary("Prefix to Add")]string prefix)
		{
			const int bufferSize = 5;

			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			currentConfig.Prefix.Add(prefix);
            if (_guildConfig.Update(currentConfig))
            {
				await ReplyAsync($"Successfully Added `{prefix}` prefix");
			}
		}

		[Command("prefix remove")]
		[Summary("Remove a prefix from the valid prefix list registered under the current guild")]
		public async Task RemovePrefix ( [Summary("Prefix to Remove")]string prefix )
		{
			if(_guildConfig.FindOne(x => x.GuildId == Context.Guild.Id).Prefix.Count <= 1)
			{
				await ReplyAsync("You cannot have less than 1 prefix registered to the guild.");
				return;
			}

			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			if (!currentConfig.Prefix.Remove(prefix))
			{
				await ReplyAsync("An Error Has Occured During Removel Process!");
				return;
			}
			_guildConfig.Update(currentConfig);
			await ReplyAsync($"Successfully Removed `{prefix}` prefix");
		}

		[Command("irritate set")]
		[Summary("Set the Irritation Mode")]
		public async Task SetIrritate ([Summary("True / False")]bool state)
		{
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			currentConfig.Irritate = state;
			_guildConfig.Update(currentConfig);
			await ReplyAsync($"Successfully Changed Irritation Mode to `{state}`");
		}

		[Command("curse add")]
		public async Task CurseAdd ([Remainder]string curse)
        {
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			if(currentConfig.Curses.Exists(x => x == curse))
            {
				await ReplyAsync("Overlapping Curse Word");
				return;
            }

			currentConfig.Curses.Add(curse);

			if (_guildConfig.Update(currentConfig))
				await ReplyAsync("Successfully Added Swear Word");
		}

		[Command("curse remove")]
		public async Task CurseRemove ( [Remainder]string curse )
        {
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
			if(currentConfig.Curses.Remove(curse))
            {
				if(_guildConfig.Update(currentConfig))
					await ReplyAsync("Successfully Removed Swear Word");
			}
		}

		[Command("curse list")]
		public async Task CurseList()
        {
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
			var curses = currentConfig.Curses;

			StringBuilder replyString = new StringBuilder();

			foreach (string curse in curses)
			{
				replyString.Append(curse + "\n");
			}

			await ReplyAsync($"**CURSES:**\n{replyString}");
		}

		[Command("checklist add")]
		[Summary("Add a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
		public async Task AddWhitelist([Summary("List to perform the operation on: blacklist/whitelist")] string list, [Summary("Mention the users to be effected by the change")]params string[] mentions)
		{
			StringBuilder successString = new StringBuilder();
			StringBuilder conflictString = new StringBuilder();

			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			if(list != "blacklist" && list != "whitelist")
            {
				await ReplyAsync("Invalid List Selection Parameter.");
				return;
            }
			List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList; 

            void AppendUsers(SocketUser user)
            {
				string userId = string.Join(" ", user.Username, user.Discriminator);

				if (checklist.Exists(x => x == userId))
				{
					conflictString.Append($"`{user.Username}`\t");
				}
				else
				{
					successString.Append($"`{user.Username}`\t");
					checklist.Add(userId);
					_guildConfig.Update(currentConfig);
				}
			}

			IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
			IReadOnlyCollection<SocketRole> mentionedRoles = Context.Message.MentionedRoles;

			foreach (SocketUser user in mentionedUsers)
			{
				AppendUsers(user);
			}

			foreach (SocketRole role in mentionedRoles)
            {
				foreach (SocketUser user in role.Members)
                {
					AppendUsers(user);
                }
            }

			if(successString.Length != 0) await ReplyAsync($"{successString} Whitelisted");
			if(conflictString.Length != 0)await ReplyAsync($"{conflictString} Already Whitelisted");
		}

		[Command("checklist list")]
		[Summary("List the Current guild whitelist")]
		public async Task ListWhitelist([Summary("List to perform the operation on: blacklist/whitelist")] string list)
		{
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			if (list != "blacklist" && list != "whitelist")
			{
				await ReplyAsync("Invalid List Selection Parameter.");
				return;
			}
			List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList;

			StringBuilder replyString = new StringBuilder();

			foreach (string user in checklist)
			{
				replyString.Append(string.Concat("`", user, "`\n"));
			}

			await ReplyAsync($"**{list.ToUpper()}:**\n{replyString}");
		}

		[Command("checklist remove")]
		[Summary("Remove a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
		public async Task RemoveWhitelist ([Summary("List to perform the operation on: blacklist/whitelist")]string list, [Summary("Mention the users to be effected by the change")]params string[] mentions )
		{
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;

			StringBuilder successString = new StringBuilder();
			StringBuilder conflictString = new StringBuilder();

			if (list != "blacklist" && list != "whitelist")
			{
				await ReplyAsync("Invalid List Selection Parameter.");
				return;
			}
			List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList;

			foreach (SocketUser user in mentionedUsers)
			{
				string userId = string.Join(" ", user.Username, user.Discriminator);

				if (!checklist.Exists(x => x == userId))
				{
					conflictString.Append($"`{user.Username}`\t");
				}
				else
				{
					successString.Append($"`{user.Username}`\t");
					checklist.Remove(userId);
					_guildConfig.Update(currentConfig);
				}
			}

			if (successString.Length != 0) await ReplyAsync($"{successString} Removed from **{list.ToUpper()}**");
		}

		[Command("checklist op")]
		public async Task WhitelistOP ([Summary("blacklist/whitelist")]string mode)
        {
			GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			if (mode != "blacklist" && mode != "whitelist")
			{
				await ReplyAsync("Invalid List Selection Parameter.");
				return;
			}
			currentConfig.useWhitelist = ( "blacklist" == mode ) ? false : true;
			if (_guildConfig.Update(currentConfig))
				await ReplyAsync("Successfully Updated Operation Mode");
        }
	}
}

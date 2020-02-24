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
			var channel = Context.Channel as SocketTextChannel;

			var messageList = messages as IEnumerable<IMessage>;

			await channel.DeleteMessagesAsync(messageList);
		}

		[Command("dice")]
		[Summary("Random Number Generator")]
		[Alias("random")]
		public async Task Dice ( int maxValue = 6 )
		{
			var random = new Random(Convert.ToInt32(DateTime.UnixEpoch));

			await Context.Channel.SendMessageAsync(random.Next().ToString());
		}

		[Command("prefix list")]
		[Summary("List all of the prefixes that are resgistered under the current guild")]
		public async Task ListPrefixes()
		{
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
			var prefixes = currentConfig.Prefix;

			var replyString = new StringBuilder();

			foreach(var item in prefixes)
			{
				replyString.Append(string.Concat("`", item, "`\t"));
			}

			await ReplyAsync(replyString.ToString());
		}

		[Command("prefix add")]
		[Summary("Register a new prefix under the current guild")]
		public async Task AddPrefix ([Summary("Prefix to Add")]string prefix)
		{
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			currentConfig.Prefix.Add(prefix);
			_guildConfig.Update(currentConfig);
			await ReplyAsync($"Successfully Added `{prefix}` prefix");
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

			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

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
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			currentConfig.Irritate = state;
			_guildConfig.Update(currentConfig);
			await ReplyAsync($"Successfully Changed Irritation Mode to `{state}`");
		}

		[Command("whitelist add")]
		[Summary("Add a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
		public async Task AddWhitelist([Summary("Mention the users to be effected by the change")]params string[] mentions)
		{
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			var mentionedUsers = Context.Message.MentionedUsers;

			var successString = new StringBuilder();
			var conflictString = new StringBuilder();

			foreach (var user in mentionedUsers)
			{
				var userId = string.Join(" ", user.Username, user.Discriminator);

				if (currentConfig.WhiteList.Exists(x => x == userId))
				{
					conflictString.Append($"`{user.Username}`\t");
				}
				else
				{
					successString.Append($"`{user.Username}`\t");
					currentConfig.WhiteList.Add(userId);
					_guildConfig.Update(currentConfig);
				}
			}

			if(successString.Length != 0) await ReplyAsync($"{successString} Whitelisted");
			if(conflictString.Length != 0)await ReplyAsync($"{conflictString} Already Whitelisted");
		}

		[Command("whitelist list")]
		[Summary("List the Current guild whitelist")]
		public async Task ListWhitelist()
		{
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
			var whitelist = currentConfig.WhiteList;

			var replyString = new StringBuilder();

			foreach (var user in whitelist)
			{
				replyString.Append(string.Concat("`", user, "`\n"));
			}

			await ReplyAsync(replyString.ToString());
		}

		[Command("whitelist remove")]
		[Summary("Remove a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
		public async Task RemoveWhitelist ( [Summary("Mention the users to be effected by the change")]params string[] mentions )
		{
			var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

			var mentionedUsers = Context.Message.MentionedUsers;

			var successString = new StringBuilder();
			var conflictString = new StringBuilder();

			foreach (var user in mentionedUsers)
			{
				var userId = string.Join(" ", user.Username, user.Discriminator);

				if (!currentConfig.WhiteList.Exists(x => x == userId))
				{
					conflictString.Append($"`{user.Username}`\t");
				}
				else
				{
					successString.Append($"`{user.Username}`\t");
					currentConfig.WhiteList.Remove(userId);
					_guildConfig.Update(currentConfig);
				}
			}

			if (successString.Length != 0) await ReplyAsync($"{successString} Removed from Whitelist");
			if (conflictString.Length != 0) await ReplyAsync($"{conflictString} Already Blacklisted");
		}
	}
}

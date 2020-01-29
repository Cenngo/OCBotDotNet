using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
	public class UtilityModule : ModuleBase<SocketCommandContext>
	{
		private readonly DiscordSocketClient _client;

		public UtilityModule ( DiscordSocketClient client )
		{
			_client = client;
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

			//FIX BUG: No Method found for bulk deletion
			//await Context.Channel.DeleteMessagesAsync(messages);
		}

		[Command("dice")]
		[Summary("Random Number Generator")]
		[Alias("random")]
		public async Task Dice ( int maxValue = 6 )
		{
			var random = new Random(Convert.ToInt32(DateTime.UnixEpoch));

			await Context.Channel.SendMessageAsync(random.Next().ToString());
		}
		[Command("insult")]
		public async Task InsultMention()
		{
		//	//Feature: insult multiple users
		//	if (Context.Message.MentionedUsers.Count > 1)
		//		throw new InvalidOperationException("You should mention only one user");

		//	UserCollection userData = JsonConvert.DeserializeObject<UserCollection>(File.ReadAllText("users.json"));

		//	IReadOnlyCollection<SocketUser> mentioned = Context.Message.MentionedUsers;

		//	var insults = JsonConvert.DeserializeObject<InsultCollection>(File.ReadAllText("insults.json"));

		//	var InsultLanguage = new Dictionary<string, List<String>>
		//	{
		//		{"tr", insults.TR_insults},
		//		{"en", insults.EN_insults }
		//	};

		//	foreach (SocketUser user in mentioned)
		//	{
		//		var userMatch = userData.userList.FirstOrDefault(x => x.discordID == user.Id);
		//		var randomN = new Random();

		//		string userLang = userMatch.langauge;
		//		List<string> listOfInsults = InsultLanguage[userLang];
		//		string anan = InsultLanguage[userMatch.langauge.ToLower()][randomN.Next(0, insults.TR_insults.Count)];
		//		await ReplyAsync(anan+" "+user.Mention);
		//	}
		//	await Context.Message.DeleteAsync();
		}
	}
}

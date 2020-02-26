using DiscordNET.Data.Rainbow6;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;

namespace DiscordNET.Handlers
{
	public class R6Handler
	{
		public async Task<R6NameSearch> ParseByName(string username, string platform)
		{
			string html = string.Empty;
			string url = @$"https://r6tab.com/api/search.php?platform={platform}&search={username}";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			request.ContentType = "application/json";
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader sr = new StreamReader(stream))
				html = sr.ReadToEnd();

			R6NameSearch result = JsonConvert.DeserializeObject<R6NameSearch>(html);
			return result;
		}

		public async Task<R6IdSearch> ParseById(string userId)
		{
			string html = string.Empty;
			string urlById = @$"https://r6tab.com/api/player.php?p_id={userId}";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlById);
			request.Method = "GET";
			using (HttpWebResponse responseById = (HttpWebResponse)request.GetResponse())
			using (Stream stream = responseById.GetResponseStream())
			using (StreamReader sr = new StreamReader(stream))
				html = sr.ReadToEnd();
			R6IdSearch result = JsonConvert.DeserializeObject<R6IdSearch>(html);
			return result;
		}

		public async Task<R6UserStats> ParseComplete(string username, string platform)
		{
			R6NameSearch nameSearch = await ParseByName(username, platform);
			R6IdSearch idSearch = await ParseById(nameSearch.results[0].pId);

			string avatarUrl = $"https://ubisoft-avatars.akamaized.net/{idSearch.userId}/default_146_146.png";
			R6Rank rank = await RankHandle(Convert.ToInt32(idSearch.currentMMR));

			R6UserStats result = new R6UserStats
			{
				stats = idSearch,
				rank = rank.rank,
				rankColor = rank.rankColor,
				avatarUrl = avatarUrl
			};

			return result;
		}

		public async Task<R6Rank> RankHandle(int mmr)
		{
			string rank = string.Empty;
			Discord.Color color;
			int tier = -1;

			List<string> CBSNumber = new List<string>
			{
				"V",
				"IV",
				"III",
				"II",
				"I"
			};

			List<string> GPNumber = new List<string>
			{
				"III",
				"II",
				"I"
			};

			switch (mmr)
			{
				case int n when (n < 1600):
					color = new Color(0x90040b);

					if (mmr - 1100 < 0)
					{
						tier = 0;
					}
					else
					{
						tier = (mmr - 1100) / 100;
					}
					rank = "Copper " + CBSNumber[tier];
					break;
				case int n when (n >= 1600 && n < 2100):
					color = new Color(0x744a1d);

					tier = (mmr - 1600) / 100;
					rank = "Bronze " + CBSNumber[tier];
					break;
				case int n when (n >= 2100 && n < 2600):
					color = new Color(0xa1a1a1);

					tier = (mmr - 2100) / 100;
					rank = "Silver " + CBSNumber[tier];
					break;
				case int n when (n >= 2600 && n < 3200):
					color = new Color(0xe3c61e);

					tier = (mmr - 2600) / 200;
					rank = "Gold " + GPNumber[tier];
					break;
				case int n when (n >= 3200 && n < 4400):
					color = new Color(0x25a9a2);

					int number = (mmr - 3200) / 400;
					rank = "Platinum " + GPNumber[tier];
					break;
				case int n when (n >= 4400 && n < 5000):
					color = new Color(0x9a7cf4);

					rank = "Diamond";
					break;
				default:
					color = new Color(0xc00f59);

					rank = "Champion";
					break;
			}

			R6Rank result = new R6Rank
			{
				rank = rank,
				rankColor = color
			};

			return result;
		}

		public async Task<string> DecodeOperators(string op)
		{
			Dictionary<string, string> operators = new Dictionary<string, string>
			{
				{"2:1", "Smoke" },
				{"2:2", "Castle" },
				{"2:3", "Doc" },
				{"2:4", "Glaz" },
				{"2:5", "Blitz" },
				{"2:6", "Buck" },
				{"2:7", "Blackbeard" },
				{"2:8", "Capitao" },
				{"2:9", "Hibana" },
				{"2:A", "Jackal" },
				{"2:B", "Ying" },
				{"2:C", "Ela" },
				{"2:D", "Dokkaebi" },
				{"2:F", "Maestro" },
				{"3:1", "Mute" },
				{"3:2", "Ash" },
				{"3:3", "Rook" },
				{"3:4", "Fuze" },
				{"3:5", "IQ" },
				{"3:6", "Frost" },
				{"3:7", "Valkyrie" },
				{"3:8", "Caveira" },
				{"3:9", "Echo" },
				{"3:A", "Mira" },
				{"3:B", "Lesion" },
				{"3:C", "Zofia" },
				{"3:D", "Vigil" },
				{"3:E", "Lion" },
				{"3:F", "Alibi" },
				{"4:1", "Sledge" },
				{"4:2", "Pulse" },
				{"4:3", "Twitch" },
				{"4:4", "Kapkan" },
				{"4:5", "Jager" },
				{"4:E", "Finka" },
				{"5:1", "Thatcher" },
				{"5:2", "Thermite" },
				{"5:3", "Montagne" },
				{"5:4", "Tachanka" },
				{"5:5", "Bandit" },
				{"1:5", "GSG9 Recruit" },
				{"1:4", "Spetsnaz Recruit" },
				{"1:3", "GIGN Recruit" },
				{"1:2", "FBI Recruit" },
				{"1:1", "SAS Recruit" },
				{"2:11", "Nomad" },
				{"3:11", "Kaid" },
				{"3:10", "Clash" },
				{"2:10", "Maverick" },
				{"2:12", "Gridlock" },
				{"3:12", "Mozzie" }
			};

			if (operators.TryGetValue(op, out string opName)) return opName;
			else return "NaN";
 
		}
	}
}

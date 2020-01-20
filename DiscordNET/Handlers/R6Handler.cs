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
			var html = string.Empty;
			var url = @$"https://r6tab.com/api/search.php?platform={platform}&search={username}";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			request.ContentType = "application/json";
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (var stream = response.GetResponseStream())
			using (var sr = new StreamReader(stream))
				html = sr.ReadToEnd();

			var result = JsonConvert.DeserializeObject<R6NameSearch>(html);
			return result;
		}

		public async Task<R6IdSearch> ParseById(string userId)
		{
			var html = string.Empty;
			var urlById = @$"https://r6tab.com/api/player.php?p_id={userId}";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlById);
			request.Method = "GET";
			using (HttpWebResponse responseById = (HttpWebResponse)request.GetResponse())
			using (var stream = responseById.GetResponseStream())
			using (var sr = new StreamReader(stream))
				html = sr.ReadToEnd();
			var result = JsonConvert.DeserializeObject<R6IdSearch>(html);
			return result;
		}

		public async Task<R6UserStats> ParseComplete(string username, string platform)
		{
			var nameSearch = await ParseByName(username, platform);
			var idSearch = await ParseById(nameSearch.results[0].pId);

			var avatarUrl = $"https://ubisoft-avatars.akamaized.net/{idSearch.userId}/default_146_146.png";
			var rank = await RankHandle(Convert.ToInt32(idSearch.currentMMR));

			var result = new R6UserStats
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
			var rank = string.Empty;
			Discord.Color color;

			var CBSNumber = new List<string>
			{
				"V",
				"IV",
				"III",
				"II",
				"I"
			};

			var GPNumber = new List<string>
			{
				"III",
				"II",
				"I"
			};
			
			if (mmr < 1600)
			{
				color = Color.DarkRed;
				int number;

				if(mmr - 1100 < 0)
				{
					number = 0;
				}
				else
				{
					number = (mmr - 1100) / 100;
				}
				rank = "Copper " + CBSNumber[number];
			}
			else if (mmr < 2100)
			{
				color = Color.DarkOrange;

				var number = (mmr - 1600) / 100;
				rank = "Bronze " + CBSNumber[number];
			}
			else if (mmr < 2600)
			{
				color = Color.LightGrey;

				var number = (mmr - 2100) / 100;
				rank = "Silver " + CBSNumber[number];
			}
			else if (mmr < 3200)
			{
				color = Color.Gold;

				var number = (mmr - 2600) / 200;
				rank = "Gold " + GPNumber[number];
			}
			else if (mmr < 4400)
			{
				color = Color.Teal;

				var number = (mmr - 3200) / 400;
				rank = "Platinum " + GPNumber[number];
			}
			else if (mmr < 5000)
			{
				color = new Color(0x9a7cf4);

				rank = "Diamond";
			}
			else
			{
				color = Color.DarkMagenta;

				rank = "Champion";
			}

			var result = new R6Rank
			{
				rank = rank,
				rankColor = color
			};

			return result;
		}
	}
}

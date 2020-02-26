using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
    public class Overwatch
    {
        public async Task<OWInfo> RetrieveUserStats(string battleTag, string region, string platform)
        {
            string userName = battleTag.Replace("#", "-");
            string html = string.Empty;
            string url = $"https://ow-api.com/v1/stats/{platform}/{region}/{userName}/complete";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
                html = sr.ReadToEnd();
            OWInfo stats = JsonConvert.DeserializeObject<OWInfo>(html);
            return stats;
        }
        public async Task<string> SortHero(Dictionary<string, OwHero> AllHeroes)
        {
            int mostValue = 0;
            string best = string.Empty;
            foreach (KeyValuePair<string, OwHero> i in AllHeroes)
            {
                int avg = i.Value.GamesWon * i.Value.WinPercentage;
                if (avg > mostValue)
                {
                    mostValue = avg;
                    best = i.Key;
                }
            }
            return best;
        }
    }
}

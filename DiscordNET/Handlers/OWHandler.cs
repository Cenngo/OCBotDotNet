using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DiscordNET.Handlers
{
    public class Overwatch
    {
        public async Task<OWInfo> RetrieveUserStats(string battleTag, string region, string platform)
        {
            var userName = battleTag.Replace("#", "-");
            var html = string.Empty;
            var url = $"https://ow-api.com/v1/stats/{platform}/{region}/{userName}/complete";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
                html = sr.ReadToEnd();
            System.Console.WriteLine(html);
            var stats = JsonConvert.DeserializeObject<OWInfo>(html);
            return stats;
        }
    }
}

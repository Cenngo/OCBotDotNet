using DiscordNET.Data.Genius;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;

namespace DiscordNET.Extensions
{
    public static class VictoriaCustomExtensions
    {
        public static Task<string> GeniusLyrics ( string query, string token ) =>
            FetchLyrics(query, token);

        public static GeniusSearchResponse SearchGenius ( this LavaTrack lavaTrack, string token ) =>
            Search(lavaTrack.Title, token);

        public static GeniusSearchResponse SearchGenius ( string query, string token ) =>
            Search(query, token);

        public static async Task<string> GeniusLyrics ( this LavaTrack lavaTrack, string token ) =>
            await FetchLyrics(lavaTrack.Title, token);

        private static async Task<string> FetchLyrics (string title, string token )
        {
            GeniusSearchResponse searchResponse = Search(title, token);

            if (searchResponse.Response.Hits.Count == 0)
                return null;

            string url = searchResponse.Response.Hits.First().Result.URL;

            using HttpClient httpClient = new HttpClient();
            using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage)
                .ConfigureAwait(false);
            using HttpContent content = responseMessage.Content;
            byte[] bytes = await content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            MemoryStream stream = new MemoryStream(bytes);

            HtmlDocument html = new HtmlDocument();
            html.Load(stream);

            HtmlNode lyricsNode = html.DocumentNode.SelectSingleNode("//body/routable-page/ng-non-bindable/div[3]/div[1]/div/div[1]/div/p");
            string lyrics = lyricsNode.InnerText;
            return lyrics;
        }

        private static GeniusSearchResponse Search (string query, string token)
        {
            RestClient client = new RestClient($"https://api.genius.com/search")
            {
                Timeout = -1
            };
            RestRequest request = new RestRequest(Method.GET);

            query = Regex.Replace(query, @"\(.*?\)", "");
            query = Regex.Replace(query, @"\s{2,}", " ");

            request.AddParameter("q", query);
            request.AddHeader("User-Agent", "");
            request.AddHeader("Content-Type", "");
            request.AddHeader("Authorization", $"Bearer {token}");
            IRestResponse response = client.Execute(request);

            GeniusSearchResponse result = JsonConvert.DeserializeObject<GeniusSearchResponse>(response.Content);

            return result;
        }

        public static string GetArtwork(this LavaTrack track )
        {
            var videoId = track.Url.Substring(track.Url.Length - 11);
            return $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";
        }
    }
}

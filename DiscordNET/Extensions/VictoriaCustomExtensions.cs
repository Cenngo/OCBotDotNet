using DiscordNET.Data.Genius;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;

namespace DiscordNET.Extensions
{
	public static class VictoriaCustomExtensions
	{
		public static GeniusSearchResponse SearchGenius(this LavaTrack lavaTrack)
		{
			var client = new RestClient($"https://api.genius.com/search");
			client.Timeout = -1;
			var request = new RestRequest(Method.GET);

			var token = Environment.GetEnvironmentVariable("GeniusToken");

			var query = lavaTrack.Title;
			query = Regex.Replace(query, @"\(.*?\)", "");
			query = Regex.Replace(query, @"\s{2,}", " ");

			request.AddParameter("q", query);
			request.AddHeader("User-Agent", "");
			request.AddHeader("Content-Type", "");
			request.AddHeader("Authorization", $"Bearer {token}");
			IRestResponse response = client.Execute(request);

			var result = JsonConvert.DeserializeObject<GeniusSearchResponse>(response.Content);

			return result;
		}

		public static async Task<string> GeniusLyrics(this LavaTrack lavaTrack )
		{
			var searchResponse = lavaTrack.SearchGenius();

			if (searchResponse.Response.Hits.Count == 0)
				return null;

			var url = searchResponse.Response.Hits.First().Result.URL;

			using var httpClient = new HttpClient();
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
			using var responseMessage = await httpClient.SendAsync(requestMessage)
				.ConfigureAwait(false);
			using var content = responseMessage.Content;
			var bytes = await content.ReadAsByteArrayAsync()
				.ConfigureAwait(false);

			var stream = new MemoryStream(bytes);

			var html = new HtmlDocument();
			html.Load(stream);

			var lyricsNode = html.DocumentNode.SelectSingleNode("//body/routable-page/ng-non-bindable/div[3]/div[1]/div/div[1]/div/p");
			var lyrics = lyricsNode.InnerText;
			return lyrics;
		}
	}
}

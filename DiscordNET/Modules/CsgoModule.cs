using CSGO_Tracker.NET;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("CS")]
    public class CsgoModule : ModuleBase<ShardedCommandContext>
    {
        private CsgoClient _csgo;
        private DiscordShardedClient _client;
        public CsgoModule ( DiscordShardedClient client )
        {
            _client = client;
            _csgo = new CsgoClient();
        }
        [Command("profile")]
        public async Task Profile(string username)
        {
            var searchResponse = _csgo.SearchPlayer(username);
            if(searchResponse.Status != 200)
            {
                await ReplyAsync("Cannot Access the Database");
                return;
            }
            var playerProfile = searchResponse.Players.First();
            var player = _csgo.GetProfileStats(playerProfile.UserId);

            var msg = new EmbedBuilder
            {
                Title = "CSGO Stats",
                Description = player.PlatformInfo.UserHandle,
                ThumbnailUrl = player.PlatformInfo.AvatarUrl,
                Color = Color.Blue
            };
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace DiscordNET.Modules
{
    [RequireOwner]
    public class AdminModule : ModuleBase<ShardedCommandContext>
    {
        
        public DiscordShardedClient _client;
        public AdminModule(DiscordShardedClient client)
        {
            _client = client;
        }

        [Command("log")]
        public async Task Log()
        {
            if (!Context.IsPrivate)
                return;

            var client = Context.Client;
            if(true)
            {
                var startTime = Process.GetCurrentProcess().StartTime;
                var uptime = DateTime.Now - startTime;

                var builder = new EmbedBuilder()
                {
                    Title = "Bot Log",
                    Color = Color.Red
                }.AddField("Status", client.Status.ToString(), true)
                .AddField("Latency", client.Latency + "ms", true)
                .AddField("Shard Count", client.Shards.Count, true)
                .AddField("Up Time", uptime.ToString(@"dd\.hh\:mm\:ss"))
                .AddField("Active Guilds", client.Guilds.Count, true);

                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("latency")]
        public async Task Latency ( )
        {
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Title = "Latency",
                Description = ":clock1: " + _client.Latency.ToString() + "ms",
                Color = Discord.Color.DarkMagenta
            }.Build());
        }
    }
}

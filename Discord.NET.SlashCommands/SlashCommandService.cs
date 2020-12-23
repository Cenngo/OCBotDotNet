using Discord.NET.SlashCommands.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Discord.NET.SlashCommands
{
    public class SlashCommandService
    {
        private readonly SlashCommandConfig _config;
        private IDictionary<string, SlashCommand> _guildCommands;
        private List<SlashCommand> _globalCommands;

        public SlashCommandService(SlashCommandConfig config )
        {
            _config = config;
            _guildCommands = new Dictionary<string, SlashCommand>();
        }

        public void AddGuildCommand( SlashCommand command, string guildId )
        {
            _guildCommands.Add(guildId, command);
        }

        public void AddGlobalCommand(SlashCommand command)
        {
            _globalCommands.Add(command);
        }

        public void RegisterGuildCommands ( )
        {
            using(var client = new HttpClient())
            {
                foreach(var guildCommand in _guildCommands)
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, _config.GuildUrl(guildCommand.Key));
                    req.Headers.Add("Authorization", "Bot " + _config.BotToken);
                    var content = JsonConvert.SerializeObject(guildCommand.Value);
                    req.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    var response = client.SendAsync(req).GetAwaiter().GetResult();
                }
            }
        }
        
        public void RegisterGlobalCommands()
        {
            using (var client = new HttpClient())
            {
                foreach (var globalCommand in _globalCommands)
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, _config.GlobalUrl());
                    req.Headers.Add("Authorization", "Bot " + _config.BotToken);
                    var content = JsonConvert.SerializeObject(globalCommand);
                    req.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    var response = client.SendAsync(req).GetAwaiter().GetResult();
                }
            }
        }
    }
}

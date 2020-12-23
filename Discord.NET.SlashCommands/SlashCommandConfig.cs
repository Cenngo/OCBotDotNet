using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.NET.SlashCommands
{
    public class SlashCommandConfig
    {
        public string BotToken { get; }
        public string ClientId { get; }
        public SlashCommandConfig(string token, string clientId )
        {
            BotToken = token;
            ClientId = clientId;
        }

        public string GlobalUrl ( )
        {
            return $"https://discord.com/api/v8/applications/{ClientId}/commands";
        }

        public string GuildUrl ( string guildId )
        {
            return $"https://discord.com/api/v8/applications/{ClientId}/guilds/{guildId}/commands";   
        }
    }
}

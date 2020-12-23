using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.NET.SlashCommands.Data
{
    public class SlashCommand
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("options")]
        public Option[] Options { get; set; }

        public SlashCommand ( )
        {
            Options = new Option[] { };
        }
    }

    public class Option
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("default")]
        public bool Default { get; set; } = false;
        [JsonProperty("required")]
        public bool Required { get; set; } = false;
        [JsonProperty("choices")]
        public Choice[] Choices { get; set; }
        [JsonProperty("options")]
        public Option[] Options { get; set; }

        public Option ( )
        {
            Choices = new Choice[] { };
            Options = new Option[] { };
        }
    }

    public class Choice
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
    }
}

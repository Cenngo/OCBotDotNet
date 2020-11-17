using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET
{
    public class Auth
    {
        public string DiscordToken { get; set; }
        public string GeniusToken { get; set; }
        public string R6Token { get; set; }
        public ushort LavalinkPort { get; set; }
    }
}

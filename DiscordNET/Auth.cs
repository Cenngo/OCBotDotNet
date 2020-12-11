using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET
{
    public class Auth
    {
        public string DiscordToken { get; set; } = default;
        public string GeniusToken { get; set; } = default;
        public string R6Token { get; set; } = default;
        public ushort LavalinkPort { get; set; } = 2333;
        public ConsoleColor VictoriaLogColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor DiscordLogColor { get; set; } = ConsoleColor.Cyan;
    }
}

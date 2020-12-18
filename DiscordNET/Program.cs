using LiteDB;
using System;
using System.Runtime.CompilerServices;

namespace DiscordNET
{
	class Program
	{
		static void Main ( )
		{
			Bot bot = new Bot();
			bot.MainAsync().GetAwaiter().GetResult();
		}
	}
}

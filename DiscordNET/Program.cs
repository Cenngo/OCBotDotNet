using LiteDB;
using System;
using System.Runtime.CompilerServices;

namespace DiscordNET
{
	class Program
	{
		static void Main ( string[] args )
		{
			Bot bot = new Bot();
			bot.MainAsync().GetAwaiter().GetResult();
		}
	}
}

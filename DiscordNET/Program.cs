using LiteDB;

namespace DiscordNET
{
	class Program
	{
		static void Main ( string[] args )
		{
			var bot = new Bot();
			bot.MainAsync().GetAwaiter().GetResult();
		}
	}
}

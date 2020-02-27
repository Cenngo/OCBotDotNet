using Discord;
using DiscordNET.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace DiscordNET.Extensions
{
	public class LavaPlayerBundle : LavaPlayer
	{
		private CancellationTokenSource _source;
		public  QueueHandler _queue { get; private set; }
		public LavaPlayer _player { get; private set; }

		public LavaPlayerBundle(LavaSocket lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel) : base( lavaSocket,  voiceChannel,  textChannel)
		{
			_player = new LavaPlayer(lavaSocket, voiceChannel, textChannel);
			_queue = new QueueHandler();
		}
	}
}

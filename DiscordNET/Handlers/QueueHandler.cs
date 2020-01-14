﻿using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace DiscordNET.Handlers
{
	public class QueueHandler
	{
		private List<QueueTrack> _queue { get; }
		private readonly LavaNode _node;
		private readonly DiscordSocketClient _client;

		public QueueHandler ( LavaNode node, DiscordSocketClient client )
		{
			_client = client;
			_node = node;
			_queue = new List<QueueTrack>();
		}

		public async Task Enqueue ( LavaTrack track, SocketCommandContext context )
		{
			var queueItem = new QueueTrack
			{
				Track = track,
				User = context.Message.Author,
				Channel = context.Channel as SocketTextChannel
			};
			_queue.Add(queueItem);
		}

		public async Task<bool> TryDequeue ( LavaTrack track )
		{
			QueueTrack dequeue;
			try
			{
				dequeue = _queue.First(x => x.Track == track);
			}
			catch (System.Exception)
			{
				return false;
			}

			_queue.Remove(dequeue);
			return true;
		}

		public async Task<List<QueueTrack>> GetItems ()
		{
			return _queue;
		}

		public async Task<int> GetQueueCount ()
		{
			return _queue.Count;
		}

		public async Task Dispose ()
		{
			_queue.Clear();
		}

		public async Task Remove ( int count = 1 )
		{
			_queue.RemoveRange(0, count);
		}
	}

	public struct QueueTrack
	{
		public LavaTrack Track { get; set; }
		public SocketUser User { get; set; }
		public SocketTextChannel Channel { get; set; }
	}
}
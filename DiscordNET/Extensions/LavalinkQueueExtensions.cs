using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Victoria;
using Victoria.Interfaces;

namespace DiscordNET.Extensions
{
    public static class LavalinkQueueExtensions
    {
        public static void Swap(this DefaultQueue<IQueueable> queue, int first, int second)
        {
            var items = queue.Items.ToList();
            var tmp = items[first];
            items[first] = items[second];
            items[second] = tmp;

            EnqueueBulk(items, queue);
        }

        public static void Move(this DefaultQueue<IQueueable> queue, int selectorIndex, int newPosition)
        {
            var items = queue.Items.ToList();
            var selection = items[selectorIndex];
            items.Remove(selection);
            items.Insert(newPosition, selection);

            EnqueueBulk(items, queue);
        }

        private static void EnqueueBulk(IEnumerable<IQueueable> items, DefaultQueue<IQueueable> queue)
        {
            queue.Clear();
            foreach (var queueable in items)
            {
                queue.Enqueue(queueable);
            }
        }
    }
}

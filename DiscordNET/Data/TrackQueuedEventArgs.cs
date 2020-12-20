using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNET.Data
{
    public class TrackQueuedEventArgs : EventArgs
    {
        public LavaTrackWithUser LavaTrackWithUser { get; }
        public QueueableType QueueableType { get; }
        public TrackQueuedEventArgs (LavaTrackWithUser lavaTrack, QueueableType queueableType )
        {
            LavaTrackWithUser = lavaTrack;
            QueueableType = queueableType;
        }
    }
}

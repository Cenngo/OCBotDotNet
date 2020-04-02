using Discord;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using Victoria;
using Victoria.Interfaces;

namespace DiscordNET.Data
{
	public class LavaTrackWithUser : IQueueable
	{
		public IUser User { get; private set; }
		public LavaTrack Track { get; private set; }
		public IChannel Channel { get; private set; }

		public LavaTrackWithUser(LavaTrack lavaTrack, IUser user, IChannel textChannel)
		{
			User = user;
			Track = lavaTrack;
			Channel = textChannel;
		}
	}
}

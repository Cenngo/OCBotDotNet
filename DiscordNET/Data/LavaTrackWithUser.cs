using Discord;
using Victoria;

namespace DiscordNET.Data
{
    public class LavaTrackWithUser : LavaTrack
    {
        public IUser User { get; }
        public IChannel TextChannel { get; }

        public LavaTrackWithUser ( LavaTrack lavaTrack, IUser user, IChannel textChannel ) : base(lavaTrack)
        {
            User = user;
            TextChannel = textChannel;
        }
    }
}

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.Data.Genius;
using DiscordNET.Extensions;
using DiscordNET.Managers;
using LiteDB;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DiscordNET.Modules
{
    public class MusicModule : CommandModule<ShardedCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly MusicManager _musicManager;
        private readonly Auth _auth;
        private event EventHandler<TrackQueuedEventArgs> TrackQueued;

        public MusicModule ( LavaNode lavaNode, MusicManager musicManager, Auth auth )
        {
            _musicManager = musicManager;
            _lavaNode = lavaNode;
            _auth = auth;
            TrackQueued += HandlePlaylist;

            this.EmbedColor = Color.DarkPurple;
        }

        [Command("Join")]
        [Summary("Create a music player instance and summon it to your current voice channel")]
        public async Task Join ( )
        {
            await JoinAsync();
        }

        private async Task<bool> JoinAsync ( )
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "",
                    Description = ":stop_sign: I'm already connected to a voice channel",
                    Color = EmbedColor,
                }.AddField("Hint", "To change the voice channel, use the `MOVE` command.", true)
                .Build());
                return false;
            }

            IVoiceState voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: You need to be connected to a voice channel.",
                    Color = EmbedColor
                }.Build());
                return false;
            }

            try
            {
                var player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                if (player != null)
                {
                    await ReplyAsync(embed: new EmbedBuilder()
                    {
                        Title = "",
                        Description = $":loud_sound: Joined {voiceState.VoiceChannel.Name}!",
                        Color = EmbedColor
                    }.Build());
                    return true;
                }
                else
                    return false;
            }
            catch (Exception exception)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Something went wrong. :bangbang:",
                    Color = EmbedColor
                }.AddField("Error", exception.Message, true)
                .Build());
                return false;
            }
        }

        [Command("Leave")]
        [Summary("Make the bot leave its current voice channel, consequently stopping the currently playing track")]
        public async Task LeaveAsync ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":bangbang: I'm not connected to any voice channels.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            IVoiceChannel voiceChannel = ( Context.User as IVoiceState ).VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":bangbang: Oh no!",
                    Color = EmbedColor
                }.Build());
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":white_check_mark: I've left {voiceChannel.Name}!",
                    Color = EmbedColor
                }.Build());
            }
            catch (Exception exception)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Something went wrong. :bangbang:",
                    Color = EmbedColor
                }.AddField("Error", exception.Message, true)
                .Build());
            }
        }

        [Command("move")]
        [Summary("Use to move the active instance of the bot to a different voice channel")]
        public async Task Move ( )
        {
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: You need to be connected to a voice channel.",
                    Color = EmbedColor
                }.Build());
                return;
            }
            await _lavaNode.MoveChannelAsync(voiceState.VoiceChannel);
        }

        [Command("play")]
        public async Task Play ( [Remainder] string query )
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                if (!await JoinAsync()) return;
            }

            if (Uri.TryCreate(query, UriKind.Absolute, out var url) && url.Host == "open.spotify.com")
            {
                var spQuery = url.AbsolutePath.Split('/');
                await ParseSpotify(spQuery[1], spQuery[2]);
            }
            else
            {
                await ParseYoutube(query);
            }

        }

        private async Task ParseSpotify ( string type, string id )
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            switch (type)
            {
                case "track":
                    var track = await _musicManager.GetSpotifyTrackAsync(id);
                    var search = await _lavaNode.SearchYouTubeAsync($"{track.Name} {track.Artists[0].Name} description:(\"Auto - generated by YouTube.\")");
                    if(search.LoadStatus == LoadStatus.NoMatches)
                    {
                        await PrintText(":x: Wasn't able to parse this item.");
                        return;
                    }

                    var trackwUser = new LavaTrackWithUser(search.Tracks[0], Context.User, Context.Channel);

                    player.Queue.Enqueue(trackwUser);

                    this.TrackQueued.Invoke(this,
                        new TrackQueuedEventArgs(trackwUser, QueueableType.Standalone));
                    break;
                case "playlist":
                    var playlist = await _musicManager.GetSpotifyPlaylistAsync(id);
                    var searchQueries = playlist.Tracks.Items
                        .FindAll(x => x.Track is FullTrack)
                        .Select(x => x.Track as FullTrack)
                        .Select(x => $"{x.Name} {x.Artists[0].Name} description:(\"Auto - generated by YouTube.\")");

                    var duration = TimeSpan.Zero;

                    foreach (var item in playlist.Tracks.Items)
                    {
                        if (item.Track is FullTrack spTrack)
                        {
                            duration += TimeSpan.FromMilliseconds(spTrack.DurationMs);
                        }
                    }

                    await ReplyAsync(embed: new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Name = "Playlist Added"
                        },
                        Title = ":scroll: " + playlist.Name,
                        Color = EmbedColor,
                        ThumbnailUrl = playlist.Images[0].Url
                    }.AddField("Number of Tracks :musical_note:", playlist.Tracks.Items.Count, true)
                    .AddField("Duration :hourglass:", duration.ToString(), true)
                    .WithFooter("Playlist is still being loaded, queue length might change over time.")
                    .Build());

                    foreach (var query in searchQueries)
                    {
                        var response = await _lavaNode.SearchYouTubeAsync(query);
                        if (response.LoadStatus == LoadStatus.NoMatches)
                            continue;
                        var lTrackwUser = new LavaTrackWithUser(response.Tracks[0], Context.User, Context.Channel);
                        player.Queue.Enqueue(lTrackwUser);
                        this.TrackQueued.Invoke(this, 
                            new TrackQueuedEventArgs(lTrackwUser, QueueableType.PlaylistItem));
                    }
                    break;
                default:
                    await PrintText(":stop_sign: Invalid spotify url.");
                    break;
            }
        }

        private async void HandlePlaylist (object sender, TrackQueuedEventArgs args)
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
            {
                if (player.Queue.TryDequeue(out var track))
                {
                    await player.PlayAsync(track);
                    await _musicManager.MusicEmbed(track as LavaTrackWithUser);
                }
            }
            else
            {
                if (args.QueueableType == QueueableType.PlaylistItem)
                    return;

                await _musicManager.QueueEmbed(args.LavaTrackWithUser, player.Queue.Count);
            }
        }

        private async Task ParseYoutube(string query)
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            var response = await _lavaNode.SearchAsync(query);
            switch (response.LoadStatus)
            {
                case LoadStatus.TrackLoaded:
                    var loadedTrackwUser = new LavaTrackWithUser(response.Tracks[0], Context.User, Context.Channel);
                    player.Queue.Enqueue(loadedTrackwUser);

                    this.TrackQueued.Invoke(this,
                        new TrackQueuedEventArgs(loadedTrackwUser, QueueableType.Standalone));
                    break;
                case LoadStatus.PlaylistLoaded:
                    await _musicManager.PlaylistEmbed(query, Context);
                    foreach(var track in response.Tracks)
                    {
                        var loadedPlaylistwUser = new LavaTrackWithUser(track, Context.User, Context.Channel);
                        player.Queue.Enqueue(loadedPlaylistwUser);

                        this.TrackQueued.Invoke(this,
                        new TrackQueuedEventArgs(loadedPlaylistwUser, QueueableType.PlaylistItem));
                    }
                    break;
                case LoadStatus.NoMatches:
                    var searchResponse = await _lavaNode.SearchYouTubeAsync(query);

                    if(searchResponse.LoadStatus == LoadStatus.NoMatches)
                    {
                        await PrintText($":question: No matches we found for {query}");
                        return;
                    }

                    var eb = new EmbedBuilder()
                    {
                        Title = "Found Tracks",
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = "Respond with the index of the track. Ex:`4`"
                        },
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "Type `cancel` to cancel the search query."
                        },
                        Color = EmbedColor
                    };

                    var description = new StringBuilder();

                    for (int i = 0; i < 5; i++)
                    {
                        var curr = searchResponse.Tracks[i];
                        description.AppendLine($"{i + 1}. {curr.Title}");
                    }
                    var message = await ReplyAsync(embed: eb
                        .WithDescription(string.Join('\n', description.ToString()))
                        .Build());

                    var interactive = new InteractiveEventManager(Context, true, true);
                    var userResponse = await interactive.NextMessage(TimeSpan.FromMinutes(1));

                    if (userResponse == null)
                    {
                        await PrintText(":hourglass: Search query timed out.");
                        await message.DeleteAsync();
                        return;
                    }

                    if (int.TryParse(userResponse.Content, out var index))
                    {
                        if (0 > index || index > 5)
                        {
                            await PrintText(":stop_sign: Your response must be an integer between 0 and 5");
                            await message.DeleteAsync();
                            return;
                        }
                        var searchResultwUser = new LavaTrackWithUser(searchResponse.Tracks[index - 1], Context.User, Context.Channel);

                        player.Queue.Enqueue(searchResultwUser);

                        this.TrackQueued.Invoke(this,
                            new TrackQueuedEventArgs(searchResultwUser, QueueableType.Standalone));

                        await message.DeleteAsync();
                        return;
                    }
                    else
                    {
                        await PrintText(":stop_sign: Your response must be an integer between 0 and 5");
                        await message.DeleteAsync();
                        return;
                    }
                case LoadStatus.LoadFailed:
                    await PrintText(":bangbang: Unable to load this track");
                    break;
                default:
                    break;
            }
        }

        [Command("pause")]
        [Summary("Pause the currently playing song to be resumed later")]
        public async Task Pause ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Nothing is currently playing.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            await player.PauseAsync();
            await PrintText($":pause_button: Paused **{player.Track.Title}**");
        }

        [Command("skip")]
        [Summary("Skip the  desired number of tracks from the queue")]
        public async Task Skip ( [Summary("Number of tracks to be skipped")] int count = 1 )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player is found.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (count < 0)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Skip count must be a positive number.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (count == 1 && player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
            {
                await player.StopAsync();
                await PrintText(":stop_button: That's the end of the queue");
            }

            if (count > player.Queue.Count)
            {
                await PrintText(":bangbang: You can't skip past the end of the queue");
                return;
            }

            if (count == 1)
            {
                await player.PlayAsync( player.Queue.First());
                await _musicManager.MusicEmbed(player.Queue.ElementAt(0) as LavaTrackWithUser);
                player.Queue.RemoveAt(0);
            }
            else
            {
                await player.PlayAsync(( player.Queue.ElementAt(count - 1) as LavaTrackWithUser ));
                player.Queue.RemoveRange(0, count - 1);
                await _musicManager.MusicEmbed(player.Queue.First() as LavaTrackWithUser);
            }
        }

        [Command("resume")]
        [Summary("Resume the paused track")]
        public async Task Resume ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await PrintText(":bangbang: No active music player found.");
                return;
            }


            await player.ResumeAsync();
        }

        [Command("volume")]
        [Summary("Set the volume of the music player or get the current volume value")]
        public async Task Volume ( [Summary("Value to be set as the volume gain(Leave blank to get the current value)")] ushort? volume = null )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }
            if (volume > 100 || volume < 0)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Volume must be set to a value between 0 and 100",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (volume == null)
            {
                int? currentVolume = player?.Volume;
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":loud_sound: **Current Volume Setting:** {currentVolume}",
                    Color = EmbedColor
                }.Build());
                return;
            }

            await player.UpdateVolumeAsync(Convert.ToUInt16(volume));
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Title = "",
                Description = $":loud_sound: **Volume Set To:** {volume}",
                Color = EmbedColor
            }.Build());
        }

        [Command("Stop")]
        [Summary("Stop music playback for the current track")]
        public async Task Stop ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Nothing is currently playing.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            await player.StopAsync();
        }

        [Command("fastforward")]
        [Alias("forward")]
        [Summary("Fast forward the current track a desired amount of time")]
        public async Task FastForward ( [Summary("Format: `xx`H`xx`M`xx`S")] TimeSpan time )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }
            LavaTrack currentTrack = player.Track;

            if (currentTrack.Position + time > currentTrack.Duration)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Cannot fastforward past the end of the track.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            await player.SeekAsync(currentTrack.Position + time);
            await PrintText($":fast_forward: Fast forwarded to {currentTrack.Position:hh\\:mm\\:ss}");
        }

        [Command("rewind")]
        [Alias("back")]
        [Summary("Rewind the current track a desired amount of time")]
        public async Task Rewind ( [Summary("Format: `xx`H`xx`M`xx`S")] TimeSpan time )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }
            LavaTrack currentTrack = player.Track;

            if (currentTrack.Position - time < TimeSpan.Zero)
            {
                await player.SeekAsync(TimeSpan.Zero);
                return;
            }

            await player.SeekAsync(currentTrack.Position - time);
            await PrintText($":rewind: Rewinded to {currentTrack.Position:hh\\:mm\\:ss}");
        }

        [Command("dispose")]
        [Alias("clear")]
        [Summary("Stop the  music playback and clear the queue")]
        public async Task Dispose ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await PrintText(":bangbang: No active music player found.");
                return;
            }
            await player.StopAsync();
            player.Queue.Clear();
            await PrintText(":wastebasket: Disposed the music player.");
        }

        [Command("lyrics")]
        [Summary("Get the lyrics for the currently playing  song from Genius")]
        public async Task Lyrics ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player)) return;

            LavaTrack track = player.Track;
            string query = track.Title;
            string lyrics = await track.GeniusLyrics(_auth.GeniusToken);

            if (lyrics == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":bangbang: No lyrics were found for {query}",
                    Color = EmbedColor
                }.Build());
                return;
            }

            GSResult result = track.SearchGenius(_auth.GeniusToken).Response.Hits.First().Result;

            string message = $":small_blue_diamond:**{result.FullTitle.ToUpper()}**:small_blue_diamond:\n```ini\n{lyrics}\n```";

            await ReplyAsync(message);
        }

        [Command("lyrics")]
        public async Task Lyrics( string query)
        {
            string lyrics = await VictoriaCustomExtensions.GeniusLyrics(query, _auth.GeniusToken);

            if (lyrics == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = $":bangbang: No lyrics were found for {query}",
                    Color = EmbedColor
                }.Build());
                return;
            }

            GSResult result = VictoriaCustomExtensions.SearchGenius(query, _auth.GeniusToken).Response.Hits.First().Result;

            string message = $":small_blue_diamond:**{result.FullTitle.ToUpper()}**:small_blue_diamond:\n```ini\n{lyrics}\n```";

            await ReplyAsync(message);
        }

        [Command("now playing")]
        [Alias("now")]
        [Summary("Get the info card for the currently playing song")]
        public async Task NowPlaying ( )
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: No active music player found.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                {
                    Title = "",
                    Description = ":bangbang: Woaaah there, I'm not playing any tracks.",
                    Color = EmbedColor
                }.Build());
                return;
            }

            LavaTrack track = player.Track;
            await _musicManager.MusicEmbed(track, Context);
        }

        [Command("queue")]
        [Summary("Get a listing of the queue")]
        public async Task Queue ( string scope = "top10" )
        {
            if (_lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player))
            {

                var Queue = player.Queue.Cast<LavaTrackWithUser>();
                var duration = new TimeSpan();

                foreach (var item in Queue)
                {
                    duration += item.Duration;
                }

                List<string> description = new List<string>();

                if (Queue.Count() < 1)
                {
                    await PrintText(":zero: Nothing is queued.");
                    return;
                }

                for (int i = 0; i < Queue.Count() && ( scope == "all" || i < 10 ); i++)
                {
                    var track = Queue.ElementAt(i);
                    var index = i + 1;
                    var title = track.Title;
                    title = title.Length > 40 ? title.Substring(0, 38) + ".." : title;
                    var dur = track.Duration;
                    var user = track.User.Username;

                    description.Add(string.Format("**{0, -2}.**:dvd:`{1, -40}` [{2, 8}] {3, 16}", index, title, dur, ":memo:" + user));
                }

                TimeSpan estimatedDuration = TimeSpan.Zero;
                foreach(var track in Queue)
                {
                    estimatedDuration += track.Duration;
                }

                Embed queueEmbed = new EmbedBuilder
                {
                    Title = $"Queue for {Context.Guild.Name}",
                    Description = string.Join("\n", description),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Number of Tracks: {Queue.Count()} \t Estimated Duration: {estimatedDuration.Hours}:{estimatedDuration.Minutes}:{estimatedDuration.Seconds}"
                    },
                    Color = Color.DarkPurple
                }.Build();

                await Context.Channel.SendMessageAsync(embed: queueEmbed);
            }
        }

        [Command("rickroll")]
        [Alias("rr")]
        public async Task Rickroll ( ulong channelId, string customTrack = null )
        {
            var channel = Context.Guild.GetVoiceChannel(channelId);
            if (channel == null)
                return;

            await PlayRickroll(channel, customTrack);
        }

        [Command("rickroll")]
        [Alias("rr")]
        public async Task Rickroll ( [Name("mentions")]string _, string customTrack = null )
        {
            var user = Context.Message.MentionedUsers.First();
            var channels = Context.Guild.VoiceChannels;

            foreach (var channel in channels)
            {
                if (channel.Users.Contains(user))
                {
                    await PlayRickroll(channel, customTrack);
                    return;
                }
            }
            await ReplyAsync(":question: Couldn't Find the User");
        }

        [Command("stfu")]
        public async Task Stfu (string mention)
        {
            await Rickroll(mention, "https://www.youtube.com/watch?v=OLpeX4RRo28");
        }

        private async Task PlayRickroll ( IVoiceChannel channel, string customTrack = null )
        {
            if (!Uri.TryCreate(customTrack, UriKind.Absolute, out _) && customTrack != null)
            {
                await ReplyAsync(":bangbang: Custom track URL is invalid.");
                return;
            }
            var url = customTrack ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            var search = await _lavaNode.SearchYouTubeAsync(url);

            if (search.Tracks.Count > 0)
            {
                if (!_lavaNode.HasPlayer(Context.Guild))
                {
                    var player = await _lavaNode.JoinAsync(channel);
                    await player.PlayAsync(search.Tracks[0]);
                }
                else
                {
                    await _lavaNode.MoveChannelAsync(channel);
                    if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
                    {
                        if (player.PlayerState == PlayerState.Playing)
                            await player.StopAsync();
                        await player.PlayAsync(search.Tracks[0]);
                    }
                }
            }
        }
    }
}
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNET.Data;
using DiscordNET.GeneralUtility;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    public class UtilityModule : CommandModule<ShardedCommandContext>
    {
        private readonly LiteCollection<GuildConfig> _guildConfig;
        private readonly Random _random;

        public UtilityModule (  LiteCollection<GuildConfig> guildConfig, Random random )
        {
            _guildConfig = guildConfig;
            _random = random;

            this.EmbedColor = Color.Magenta;
        }

        [Command("delete")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes Messages")]
        public async Task Delete ( int count = 1 )
        {
            if (count > 100)
            {
                await PrintText(":exclamation: You can't delete more than 100 messages at a time");
                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            SocketTextChannel channel = Context.Channel as SocketTextChannel;

            await channel.DeleteMessagesAsync(messages);
        }

        [Command("dice")]
        [Summary("Random Number Generator")]
        [Alias("random")]
        public async Task Dice ( int maxValue = 6 )
        {
            int randomNumber = _random.Next(1, maxValue);

            await PrintText("Random roll returned: " + randomNumber.ToString());
        }

        [Command("prefix list")]
        [Summary("List all of the prefixes that are resgistered under the current guild")]
        public async Task ListPrefixes ( )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
            List<string> prefixes = currentConfig.Prefix;

            StringBuilder replyString = new StringBuilder();

            foreach (string item in prefixes)
            {
                replyString.Append(string.Concat("`", item, "`\t"));
            }

            await PrintText(":link: Active Prefixes for this guild.", replyString.ToString());
        }

        [Command("prefix add")]
        [Summary("Register a new prefix under the current guild")]
        public async Task AddPrefix ( [Summary("Prefix to Add")] string prefix )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            currentConfig.Prefix.Add(prefix);
            if (_guildConfig.Update(currentConfig))
            {
                await PrintText($":white_check_mark: Successfully added `{prefix}` to guild prefixes");
            }
        }

        [Command("prefix remove")]
        [Summary("Remove a prefix from the valid prefix list registered under the current guild")]
        public async Task RemovePrefix ( [Summary("Prefix to Remove")] string prefix )
        {
            if (_guildConfig.FindOne(x => x.GuildId == Context.Guild.Id).Prefix.Count <= 1)
            {
                await PrintText(":stop_sign: You cannot have less than 1 prefix registered to the guild.");
                return;
            }

            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            if (!currentConfig.Prefix.Remove(prefix))
            {
                await PrintText(":bangbang: An error occured during the removal process.");
                return;
            }
            if(_guildConfig.Update(currentConfig))
                await PrintText($":white_check_mark: Successfully removed `{prefix}` from guild prefixes");
        }

        [Command("irritate set")]
        [Summary("Set the Irritation Mode")]
        public async Task SetIrritate ( [Summary("True / False")] bool state )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            currentConfig.Irritate = state;
            _guildConfig.Update(currentConfig);
            await PrintText($":white_check_mark: Successfully updated the `Irritate Setting` to {state}");
        }

        [Command("curse add")]
        public async Task CurseAdd ( [Remainder] string curse )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            if (currentConfig.Curses.Exists(x => x == curse))
            {
                await PrintText(":stop_sign: This curse word is already registered.");
                return;
            }

            currentConfig.Curses.Add(curse);

            if (_guildConfig.Update(currentConfig))
                await PrintText(":white_check_mark: Succeessfully added the curse word.");
        }

        [Command("curse remove")]
        public async Task CurseRemove ( [Remainder] string curse )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
            if (currentConfig.Curses.Remove(curse))
            {
                if (_guildConfig.Update(currentConfig))
                    await PrintText(":white_check_mark: Succeessfully removed the curse word.");
            }
        }

        [Command("curse list")]
        public async Task CurseList ( )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);
            var curses = currentConfig.Curses;

            StringBuilder replyString = new StringBuilder();

            foreach (string curse in curses)
            {
                replyString.Append(curse + "\n");
            }

            await PrintText(":anger: Guild Curse Words", replyString.ToString());
        }

        [Command("checklist add")]
        [Summary("Add a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
        public async Task AddWhitelist ( [Summary("List to perform the operation on: blacklist/whitelist")] string list, [Summary("Mention the users to be effected by the change")] params string[] _ )
        {
            StringBuilder successString = new StringBuilder();
            StringBuilder conflictString = new StringBuilder();

            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            if (list != "blacklist" && list != "whitelist")
            {
                await PrintText(":bangbang: You need select either the `whitelist` or the `blacklist` in order to use this command.");
                return;
            }
            List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList;

            void AppendUsers ( SocketUser user )
            {
                string userId = string.Join(" ", user.Username, user.Discriminator);

                if (checklist.Exists(x => x == userId))
                {
                    conflictString.Append($"`{user.Username}`\t");
                }
                else
                {
                    successString.Append($"`{user.Username}`\t");
                    checklist.Add(userId);
                    _guildConfig.Update(currentConfig);
                }
            }

            IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
            IReadOnlyCollection<SocketRole> mentionedRoles = Context.Message.MentionedRoles;

            foreach (SocketUser user in mentionedUsers)
            {
                AppendUsers(user);
            }

            foreach (SocketRole role in mentionedRoles)
            {
                foreach (SocketUser user in role.Members)
                {
                    AppendUsers(user);
                }
            }

            if (!string.IsNullOrEmpty(successString.ToString()))
                await PrintText(":white_circle: Whitelisted", successString.ToString());

            if (!string.IsNullOrEmpty(conflictString.ToString()))
                await PrintText(":x: Was already whitelisted", conflictString.ToString());
        }

        [Command("checklist list")]
        [Summary("List the Current guild whitelist")]
        public async Task ListWhitelist ( [Summary("List to perform the operation on: blacklist/whitelist")] string list )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            if (list != "blacklist" && list != "whitelist")
            {
                await PrintText(":bangbang: You need select either the `whitelist` or the `blacklist` in order to use this command.");
                return;
            }
            List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList;

            StringBuilder replyString = new StringBuilder();

            foreach (string user in checklist)
            {
                replyString.Append(string.Concat("`", user, "`\n"));
            }
            await PrintText($"Guild {list.CaptFirst()}", replyString.ToString());
        }

        [Command("checklist remove")]
        [Summary("Remove a Person to the whitelist to be excluded from bot activities that are meant to irritate people")]
        public async Task RemoveWhitelist ( [Summary("List to perform the operation on: blacklist/whitelist")] string list, [Summary("Mention the users to be effected by the change")] params string[] _ )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;

            StringBuilder successString = new StringBuilder();
            StringBuilder conflictString = new StringBuilder();

            if (list != "blacklist" && list != "whitelist")
            {
                await PrintText(":bangbang: You need select either the `whitelist` or the `blacklist` in order to use this command.");
                return;
            }
            List<string> checklist = ( list == "blacklist" ) ? currentConfig.BlackList : currentConfig.WhiteList;

            foreach (SocketUser user in mentionedUsers)
            {
                string userId = string.Join(" ", user.Username, user.Discriminator);

                if (!checklist.Exists(x => x == userId))
                {
                    conflictString.Append($"`{user.Username}`\t");
                }
                else
                {
                    successString.Append($"`{user.Username}`\t");
                    checklist.Remove(userId);
                    _guildConfig.Update(currentConfig);
                }
            }
            if (!string.IsNullOrEmpty(successString.ToString()))
                await PrintText($"Removed from **{ list.ToUpper()}**", successString.ToString());
        }

        [Command("checklist op")]
        public async Task WhitelistOP ( [Summary("blacklist/whitelist")] string mode )
        {
            GuildConfig currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            if (mode != "blacklist" && mode != "whitelist")
            {
                await ReplyAsync("Invalid List Selection Parameter.");
                return;
            }
            currentConfig.UseWhitelist = "blacklist" != mode;
            if (_guildConfig.Update(currentConfig))
                await PrintText($":white_check_mark: Successfully updated operation mode to {mode}");
        }

        [Command("randomrr")]
        public async Task RandomRickroll (bool state)
        {
            var currentConfig = _guildConfig.FindOne(x => x.GuildId == Context.Guild.Id);

            currentConfig.RandomRickroll = state;
            if (_guildConfig.Update(currentConfig))
                await PrintText(":white_check_mark: Successfully Updated the Database");
        }
    }
}

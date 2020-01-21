using Discord;
using Discord.Commands;
using DiscordNET.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNET.Modules
{
    [Group("OW")]
    public class OWCommands : ModuleBase<SocketCommandContext>
    {
        [Command("profile")]
        public async Task GetOwProfile(string battleTag, string role, string platform = "pc", string region = "eu")
        {
            var OW = new Overwatch();
            var stats = await OW.RetrieveUserStats(battleTag, region, platform);
            List<OWRole> roles = stats.OW_RoleList;
            var roleMatch = roles.FirstOrDefault(x => x.role == role);

            if (roleMatch == default(OWRole))
            {
                await ReplyAsync("No SR for current role...");
                return;
            }
            else
            {
                var infoEmbed = new EmbedBuilder()
                {
                    Title = $"Name: {battleTag}",
                    Color = Color.Orange
                }
                .AddField("Role", roleMatch.role, true)
                .AddField("Rating", roleMatch.skillRating + " SR", true)
                .Build(); 
                await ReplyAsync(embed: infoEmbed);
            }
        }
    }
}


using Disqord;
using Imposter.services;
using Mummybot.Attributes.Checks;
using Mummybot.Commands;
using Mummybot.Extentions;
using Qmmands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imposter.Commands.Modules
{
    [Group("imposter", "i")]
    public class ImposterWhitelistModule : MummyModule
    {
        public DataService WhitelistService { get; set; }
        public RoleService RoleService { get; set; }

        [Command("add", "a"), RequirePermissions(Mummybot.Enums.PermissionTarget.User, channelPerms: Permission.ManageRoles)]
        public async Task TaskAsync(params CachedUser[] users)
        {
            var ids = users.Select(x => x.Id);
            foreach (var id in ids)
            {
                if (WhitelistService.WhitelistedIds.Contains(id))
                {
                    await ReplyAsync($"{users.First(users => users.Id == id).Mention} is already whitelisted.", allowedMentions: LocalMentions.None);
                    LogService.LogError($"user is already whitelisted");

                    continue;
                }
                WhitelistService.WhitelistedIds.Add(id);
            }
            LogService.LogInformation($"added {ids.Count()} new ids to imoster whitelist");
            await Context.Message.AddOkAsync();
        }

        [Command("new"), RequireOwner]
        public async Task NewAsync(bool skiproles = false)
        {

            if (skiproles)
            {
                await RoleService.UpdateKingQueenAsync();
            }
            else
            {
                RoleService.CallbackAsync(null);
            }
            await Context.Message.AddOkAsync();

        }

        [Command(), Description("lists all whitelisted imposters")]
        public async Task ListImposters()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var id in WhitelistService.WhitelistedIds)
            {
                sb.AppendLine(Context.Guild.GetMember(id)?.Mention ?? id.ToString());
            }
            sb.AppendLine("safeguard");
            await ReplyAsync($"Currently there are {WhitelistService.WhitelistedIds.Count} whitelisted imposters\n {sb}", allowedMentions: LocalMentions.None);

        }


    }
}

using Disqord;
using Mummybot.Commands;
using Mummybot.Enums;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Imposter.Commands.Modules
{

    [Group("nick", "nickname")]
    public class NicknameModsModule : MummyModule
    {
        [Group("prefix")]
        public class NicknameModsPrefix : MummyModule
        {
            [Command("add"), RunMode(RunMode.Parallel)]
            public async Task AddPrefix(string prefix)
            {
                foreach (var role in Context.Guild.Roles.Where(role => Context.Guild.DefaultRole.Id != role.Key && role.Key != 760248318948802610))//salty imposter role
                {
                    var newname = $"{prefix} {role.Value.Name}";
                    LogService.LogInformation($"setting role name {newname}", LogSource.Commands, Context.GuildId);
                    await role.Value.ModifyAsync(x => x.Name = newname);
                }
                await Context.Message.AddReactionAsync(new LocalEmoji("✅"));
            }

            [Command("remove"), RunMode(RunMode.Parallel)]
            public async Task RemovePrefix(string prefix)
            {
                foreach (var role in Context.Guild.Roles.Where(role => Context.Guild.DefaultRole.Id != role.Key && role.Key != 760248318948802610))//salty imposter role
                {
                    var newname = role.Value.Name.Remove(0, prefix.Length);
                    LogService.LogInformation($"setting role name {newname}", LogSource.Commands, Context.GuildId);
                    await role.Value.ModifyAsync(x => x.Name = newname);
                }
                await Context.Message.AddReactionAsync(new LocalEmoji("✅"));

            }
        }
    }
}


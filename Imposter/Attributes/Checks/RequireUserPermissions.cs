
using Disqord;
using Mummybot.Commands;
using Qmmands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionTarget = Mummybot.Enums.PermissionTarget;

namespace Mummybot.Attributes.Checks
{
    [Name("Require Permissions")]

    public class RequirePermissions : MummyCheckBase
    {
        public override string Name => "RequirePermissions";
        private readonly PermissionTarget _target;
        private readonly GuildPermissions[] _guildPerms;
        private readonly Permission[] _channelPerms;


        public RequirePermissions(PermissionTarget target, params GuildPermissions[] guildPerms)
        {
            _target = target;
            _guildPerms = guildPerms;
            _channelPerms = new Permission[0];
        }

        public RequirePermissions(PermissionTarget target, params Permission[] channelPerms)
        {
            _target = target;
            _channelPerms = channelPerms;
            _guildPerms = new GuildPermissions[0];
        }

        public override ValueTask<CheckResult> CheckAsync(MummyContext context)
        {
            CachedMember user = null;

            switch (_target)
            {
                case PermissionTarget.User:
                    user = context.User;
                    break;

                case PermissionTarget.Bot:
                    user = context.Guild.CurrentMember;
                    break;
            }

            var failedGuildPerms = _guildPerms.Where(guildPerm => !user.Permissions.Has(guildPerm)).ToArray();

            var channelPerms = context.User.GetPermissionsFor(context.Channel);

            var failedChannelPerms = _channelPerms.Where(channelPerm => !channelPerms.Has(channelPerm)).ToArray();

            if (failedGuildPerms.Length == 0 && failedChannelPerms.Length == 0)
                return CheckResult.Successful;

            var sb = new StringBuilder();

            foreach (var guildPerm in failedGuildPerms)
                sb.AppendLine(guildPerm.ToString());

            foreach (var channelPerm in failedChannelPerms)
                sb.AppendLine(channelPerm.ToString());

            var u = context.User;

            var target = _target == PermissionTarget.User ? "You" : "I";

            return CheckResult.Unsuccessful($"{target} lack permissions for {sb}");
        }
    }
}

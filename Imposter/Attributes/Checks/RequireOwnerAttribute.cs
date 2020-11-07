using Mummybot.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Mummybot.Attributes.Checks
{
    [Name("Require Owner")]
    public class RequireOwnerAttribute : MummyCheckBase
    {

        public override string Name { get => "RequireOwner"; }
        public override async ValueTask<CheckResult> CheckAsync(MummyContext context)
        {
            if ((await context.Client.GetCurrentApplicationAsync()).Owner.Id == context.User.Id)
                return CheckResult.Successful;
            else
                return CheckResult.Unsuccessful("Only my owner can run this command");
        }
    }
}

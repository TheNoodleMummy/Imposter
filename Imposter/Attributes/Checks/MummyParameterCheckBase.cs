using Mummybot.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Imposter.Attributes.Checks
{
    public abstract class MummyParameterCheckBase : ParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        => CheckAsync(argument, (MummyContext)context);
        public abstract ValueTask<CheckResult> CheckAsync(object argument, MummyContext context);
    }
}

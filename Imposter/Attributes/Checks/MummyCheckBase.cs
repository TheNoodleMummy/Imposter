using Mummybot.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Mummybot.Attributes.Checks
{
    public abstract class MummyCheckBase : CheckAttribute
    {
        public virtual string Name { get; }

        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
      => CheckAsync((MummyContext)context);
        public abstract ValueTask<CheckResult> CheckAsync(MummyContext context);

    }
}

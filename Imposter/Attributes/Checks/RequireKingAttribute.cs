using Mummybot.Attributes.Checks;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Imposter.Attributes.Checks
{
    public class RequireKingAttribute : MummyCheckBase
    {
        public override ValueTask<CheckResult> CheckAsync(MummyContext context)
        {
            throw new NotImplementedException();
        }
    }
}

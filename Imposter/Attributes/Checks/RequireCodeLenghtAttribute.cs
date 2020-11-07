using Mummybot.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Imposter.Attributes.Checks
{
    public class RequireCodeLenghtAttribute : MummyParameterCheckBase
    {
        public int Lenght { get; }
        public RequireCodeLenghtAttribute(int lenght)
        {
            Lenght = lenght;
        }

        public override ValueTask<CheckResult> CheckAsync(object argument, MummyContext context)
        {
            var lenght = argument.ToString().Length;
            if (lenght == 6)
            {
                return CheckResult.Successful;
            }
            else
            {
                if (lenght < 6)
                {
                    return CheckResult.Unsuccessful($"the code provide was to SHORT and it required to be 6 chars long exactly. (code provided was {lenght} char long)");
                }
                else
                {
                    return CheckResult.Unsuccessful($"the code provide was to LONG and it required to be 6 chars long exactly. (code provided was {lenght} char long)");
                }

            }

        }
    }
}

using Disqord;
using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    internal class EnsureIsIntegerCriterion : ICriterion<CachedUserMessage>
    {
        public Task<bool> JudgeAsync(MummyContext sourceContext, CachedUserMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}

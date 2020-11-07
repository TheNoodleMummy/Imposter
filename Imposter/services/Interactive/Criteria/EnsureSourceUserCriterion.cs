using Disqord;
using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(MummyContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}

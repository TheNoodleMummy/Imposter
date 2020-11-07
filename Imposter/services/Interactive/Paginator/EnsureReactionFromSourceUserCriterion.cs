using Disqord;
using Mummybot.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<ReactionData>
    {
        public async Task<bool> JudgeAsync(MummyContext sourceContext, ReactionData parameter)
        {
            var reactions = await sourceContext.Message.GetReactionsAsync(parameter.Emoji);
            bool ok = reactions.Any(x=>x.Id == sourceContext.User.Id);
            return ok;
        }
    }
}

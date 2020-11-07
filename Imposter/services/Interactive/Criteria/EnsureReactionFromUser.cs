using Discord.Addons.Interactive;
using Disqord;
using Mummybot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imposter.services.Interactive.Criteria
{
    class EnsureReactionFromUser : ICriterion<ReactionData>
    {
        private readonly ulong _id;

        public EnsureReactionFromUser(IUser user)
            => _id = user.Id;

        public EnsureReactionFromUser(ulong id)
            => _id = id;
        public async Task<bool> JudgeAsync(MummyContext sourceContext, ReactionData parameter)
        {
            var reactions = await sourceContext.Message.GetReactionsAsync(parameter.Emoji);
            bool ok = reactions.Any(x => x.Id == _id);
            return ok;
        }
    }
}

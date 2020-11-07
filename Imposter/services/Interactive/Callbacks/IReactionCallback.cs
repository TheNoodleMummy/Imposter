using Disqord;
using Disqord.Events;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<ReactionData> Criterion { get; }
        TimeSpan? Timeout { get; }
        MummyContext Context { get; }

        Task<bool> HandleCallbackAsync(ReactionAddedEventArgs reaction);
    }
}

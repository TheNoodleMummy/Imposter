using Disqord;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public abstract class InteractiveBase : InteractiveBase<MummyContext>
    {
    }

    public abstract class InteractiveBase<T> : ModuleBase<MummyContext>
    {
        public InteractiveService Interactive { get; set; }

        public Task<CachedUserMessage> NextMessageAsync(ICriterion<CachedUserMessage> criterion, TimeSpan? timeout = null, CancellationToken token = default)
            => Interactive.NextMessageAsync(Context, criterion, timeout, token);
        public Task<CachedUserMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null, CancellationToken token = default)
            => Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout, token);

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, LocalEmbed embed = null, TimeSpan? timeout = null)
            => Interactive.ReplyAndDeleteAsync(Context, content, isTTS, embed, timeout);


        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criterion = new Criteria<ReactionData>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            return PagedReplyAsync(pager, criterion);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<ReactionData> criterion)
            => Interactive.SendPaginatedMessageAsync(Context, pager, criterion);

    }
}

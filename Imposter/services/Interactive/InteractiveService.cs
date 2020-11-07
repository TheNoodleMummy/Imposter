using Disqord;
using Disqord.Events;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class InteractiveService : IDisposable
    {
        public DiscordClient Discord { get; }

        private readonly Dictionary<ulong, IReactionCallback> _callbacks;
        private readonly TimeSpan _defaultTimeout;

        public InteractiveService(DiscordClient discord, InteractiveServiceConfig config = null)
        {
            Discord = discord;
            Discord.ReactionAdded += HandleReactionAsync;

            config ??= new InteractiveServiceConfig();
            _defaultTimeout = config.DefaultTimeout;

            _callbacks = new Dictionary<ulong, IReactionCallback>();
        }

        public Task<CachedUserMessage> NextMessageAsync(MummyContext context,
            bool fromSourceUser = true,
            bool inSourceChannel = true,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            var criterion = new Criteria<CachedUserMessage>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            if (inSourceChannel)
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            return NextMessageAsync(context, criterion, timeout, token);
        }

        public async Task<CachedUserMessage> NextMessageAsync(MummyContext context,
            ICriterion<CachedUserMessage> criterion,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            timeout ??= _defaultTimeout;

            var eventTrigger = new TaskCompletionSource<CachedUserMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            async Task Handler(MessageReceivedEventArgs e)
            {
                var message = e.Message as CachedUserMessage;
                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
                if (result)
                    eventTrigger.SetResult(message);
            }

            context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var cancel = cancelTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

            context.Client.MessageReceived -= Handler;

            if (task == trigger)
                return await trigger.ConfigureAwait(false);
            else
                return null;
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(MummyContext context,
            string content, bool isTTS = false,
            LocalEmbed embed = null,
            TimeSpan? timeout = null
           )
        {
            timeout ??= _defaultTimeout;
            var message = await context.Channel.SendMessageAsync(content, isTTS, embed).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }

        public async Task<IUserMessage> SendPaginatedMessageAsync(MummyContext context,
            PaginatedMessage pager,
            ICriterion<ReactionData> criterion = null)
        {
            var callback = new PaginatedMessageCallback(this, context, pager, criterion);
            await callback.DisplayAsync().ConfigureAwait(false);
            return callback.Message;
        }

        public void AddReactionCallback(IMessage message, IReactionCallback callback)
            => _callbacks[message.Id] = callback;
        public void RemoveReactionCallback(IMessage message)
            => RemoveReactionCallback(message.Id);
        public void RemoveReactionCallback(ulong id)
            => _callbacks.Remove(id);
        public void ClearReactionCallbacks()
            => _callbacks.Clear();

        private async Task HandleReactionAsync(ReactionAddedEventArgs e)
        {
            var reaction = e.Reaction;
            var message = e.Message;

            if (e.User.Id == Discord.CurrentUser.Id) return;
            if (!(_callbacks.TryGetValue(message.Id, out var callback))) return;
            if (!await callback.Criterion.JudgeAsync(callback.Context, reaction.Value).ConfigureAwait(false))
                return;
            switch (callback.RunMode)
            {
                case RunMode.Parallel:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(e).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(e).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }

        public void Dispose()
        {
            Discord.ReactionAdded -= HandleReactionAsync;
        }
    }
}

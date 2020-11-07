using Disqord;
using Disqord.Events;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public MummyContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Parallel;
        public ICriterion<ReactionData> Criterion { get; }
        public TimeSpan? Timeout => _options.Timeout;

        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions _options => _pager.Options;
        private readonly int pages;
        private int page = 1;


        public PaginatedMessageCallback(InteractiveService interactive, MummyContext sourceContext,PaginatedMessage pager, ICriterion<ReactionData> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<ReactionData>();
            _pager = pager;
            pages = _pager.Pages.Count;
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(_options.First);
                await message.AddReactionAsync(_options.Back);
                await message.AddReactionAsync(_options.Next);
                await message.AddReactionAsync(_options.Last);

                var manageMessages = (Context.Channel is IGuildChannel guildChannel)&& Context.User.GetPermissionsFor(guildChannel).ManageMessages;

                if (_options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (_options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    await message.AddReactionAsync(_options.Jump);

                await message.AddReactionAsync(_options.Stop);

                if (_options.DisplayInformationIcon)
                    await message.AddReactionAsync(_options.Info);
            });
            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(ReactionAddedEventArgs e)
        {
            var emote = e.Emoji;

            if (emote.Equals(_options.First))
                page = 1;
            else if (emote.Equals(_options.Next))
            {
                if (page >= pages)
                    return false;
                ++page;
            }
            else if (emote.Equals(_options.Back))
            {
                if (page <= 1)
                    return false;
                --page;
            }
            else if (emote.Equals(_options.Last))
                page = pages;
            else if (emote.Equals(_options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(_options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<CachedUserMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(e.User.Id))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, _options.Stop.Name);
                        return;
                    }
                    page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(_options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, _options.InformationText, timeout: _options.InfoTimeout);
                return false;
            }
            await Message.RemoveMemberReactionAsync(e.User.Id,emote);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }

        protected LocalEmbed BuildEmbed()
        {
            var current = _pager.Pages[page];
            current.WithFooter($"Page {page}/{_pager.Pages.Count}");
            return current.Build();
        }
        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}

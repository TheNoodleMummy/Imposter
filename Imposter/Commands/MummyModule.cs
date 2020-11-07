using Disqord;
using Disqord.Rest;
using Mummybot.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Mummybot.Commands
{
    public abstract class MummyModule : ModuleBase<MummyContext>
    {
        public IServiceProvider Services { get; set; }
        public LogService LogService { get; set; }

        protected Task<RestUserMessage> ReplyAsync(string content = "", LocalEmbedBuilder embed = null, LocalMentions allowedMentions = null)
            => Context.Channel.SendMessageAsync(content, false, embed?.Build(), mentions: allowedMentions);
    }
}

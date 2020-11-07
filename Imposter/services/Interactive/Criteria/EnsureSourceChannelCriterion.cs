using Disqord;
using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(MummyContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.ChannelId;
            return Task.FromResult(ok);
        }
    }
}

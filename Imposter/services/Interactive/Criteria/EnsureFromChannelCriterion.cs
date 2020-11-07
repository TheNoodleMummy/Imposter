using Disqord;
using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class EnsureFromChannelCriterion : ICriterion<IMessage>
    {
        private readonly ulong _channelId;

        public EnsureFromChannelCriterion(IMessageChannel channel)
            => _channelId = channel.Id;

        public Task<bool> JudgeAsync(MummyContext sourceContext, IMessage parameter)
        {
            bool ok = _channelId == parameter.ChannelId;
            return Task.FromResult(ok);
        }
    }
}

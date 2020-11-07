using Disqord;
using Mummybot.Commands;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class EnsureFromUserCriterion : ICriterion<IMessage>
    {
        private readonly ulong _id;

        public EnsureFromUserCriterion(IUser user)
            => _id = user.Id;

        
        public EnsureFromUserCriterion(ulong id)
            => _id = id;

        public Task<bool> JudgeAsync(MummyContext sourceContext, IMessage parameter)
        {
            bool ok = _id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}

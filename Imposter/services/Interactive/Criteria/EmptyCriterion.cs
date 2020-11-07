using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(MummyContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}

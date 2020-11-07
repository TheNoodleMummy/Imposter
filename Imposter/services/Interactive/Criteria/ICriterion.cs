using Mummybot.Commands;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(MummyContext sourceContext, T parameter);
    }
}

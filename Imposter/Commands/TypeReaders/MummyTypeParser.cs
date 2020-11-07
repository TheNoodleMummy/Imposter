using Mummybot.Attributes;
using Qmmands;
using System.Threading.Tasks;

namespace Mummybot.Commands.TypeReaders
{
    [DontOverride]
    public abstract class MummyTypeParser<T> : TypeParser<T>
    {
        public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context)
        => ParseAsync(parameter, value, (MummyContext)context);

        public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, MummyContext context);
    }
}

using Discord;
using Disqord;
using System.Threading.Tasks;

namespace Mummybot.Extentions
{
    public partial class Extentions
    {
        public static Task AddOkAsync(this IUserMessage message)
            => message.AddReactionAsync(new LocalEmoji("👌"));

        public static Task AddNotOkAsync(this IUserMessage message)
            => message.AddReactionAsync(new LocalEmoji("⛔"));

    }
}

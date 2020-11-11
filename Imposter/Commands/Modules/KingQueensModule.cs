using Disqord;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Imposter.Commands.Modules
{
    [Group("top")]
    public class KingQueensModule : MummyModule
    {
        public DataService Data { get; set; }


        [Command("kings")]
        public async Task Kings()
        {
            var lenght = Math.Min(Data.ImposterKings.Count, 25);
            var order = Data.ImposterKings.Take(lenght).OrderByDescending(x => x.count);
            var sb = new StringBuilder();
            var eb = new LocalEmbedBuilder().WithTitle($"Top {lenght} Kings");
            foreach (var (id, count) in order)
            {
                var user = Context.Guild.GetMember(id);
                sb.AppendLine($"{user.Nick ?? user.Name} {(Data.WhitelistedIds.Any(x => x == user.Id) ? $"has betrayed trust: {count} times" : "user is not whitelisted as imposter")}");
            }
            eb.WithDescription(sb.ToString());
            await ReplyAsync("", eb, LocalMentions.None);
        }

        [Command("queens")]
        public async Task Queens()
        {
            var lenght = Math.Min(Data.ImposterQueens.Count, 25);
            var order = Data.ImposterQueens.Take(lenght).OrderByDescending(x => x.count);
            var sb = new StringBuilder();
            var eb = new LocalEmbedBuilder().WithTitle($"Top {lenght} Queens");
            foreach (var (id, count) in order)
            {
                var user = Context.Guild.GetMember(id);
                sb.AppendLine($"{user.DisplayName}  {(Data.WhitelistedIds.Any(x => x == user.Id) ? $"has betrayed trust: {count} times" : "user is not whitelisted as imposter")}");
            }
            eb.WithDescription(sb.ToString());
            await ReplyAsync("", eb, LocalMentions.None);
        }

        [Command("all")]
        public async Task Top()
        {
            var list = Data.ImposterKings.ToList();
            foreach (var queen in Data.ImposterQueens)
            {
                list.Add(queen);
            }
            var lenght = Math.Min(list.Count, 25);

            var order = list.Take(lenght).OrderByDescending(x => x.count);
            var sb = new StringBuilder();
            var eb = new LocalEmbedBuilder().WithTitle($"Top {lenght}");
            foreach (var (id, count) in order)
            {
                var user = Context.Guild.GetMember(id);
                sb.AppendLine($"{user.DisplayName} {(Data.WhitelistedIds.Any(x => x == user.Id) ? $"has betrayed trust: {count} times " : "user is not whitelisted as imposter")}");
            }
            eb.WithDescription(sb.ToString());
            await ReplyAsync("", eb, LocalMentions.None);
        }

        [Command()]
        public async Task TopB()
        {
            var klenght = Math.Min(Data.ImposterKings.Count, 25);
            var qlenght = Math.Min(Data.ImposterQueens.Count, 25);
            var kingorder = Data.ImposterKings.Take(klenght).OrderByDescending(x => x.count);
            var queensorder = Data.ImposterQueens.Take(qlenght).OrderByDescending(x => x.count);
            var eb = new LocalEmbedBuilder();
            var sb = new StringBuilder();
            foreach (var (id, count) in kingorder)
            {
                var user = Context.Guild.GetMember(id);
                sb.AppendLine($"{user.DisplayName}  {(Data.WhitelistedIds.Any(x => x == user.Id) ? $"has betrayed trust: {count} times " : "user is not whitelisted as imposter")}");
            }
            eb.AddField("Top Kings", sb.ToString(), true);
            sb.Clear();
            foreach (var (id, count) in queensorder)
            {
                var user = Context.Guild.GetMember(id);
                sb.AppendLine($"{user.DisplayName}  {(Data.WhitelistedIds.Any(x => x == user.Id) ? $"has betrayed trust: {count} times " : "user is not whitelisted as imposter")}");
            }
            eb.AddField("Top Queens", sb.ToString(), true);
            await ReplyAsync("", eb, LocalMentions.None);

        }

    }

    [Group("add")]
    public class KingQueensConfigModule : MummyModule
    {
        public DataService Data { get; set; }

        [Command("queen")]
        public async Task Queen(CachedUser user)
        {
            if (Data.ImposterQueens.Any(x => x.id == user.Id))
            {
                await ReplyAsync($"{user} is already a potential Queen", null, LocalMentions.None);
            }
            else
            {
                Data.ImposterQueens.Add((user.Id, 0));
                await ReplyAsync($"{user} is now a potential Queen", null,  LocalMentions.None);
            }
        }

        [Command("king")]
        public async Task King(CachedUser user)
        {
            if (Data.ImposterKings.Any(x => x.id == user.Id))
            {
                await ReplyAsync($"{user} is already a potential King", null, LocalMentions.None);
            }
            else
            {
                Data.ImposterKings.Add((user.Id, 0));
                await ReplyAsync($"{user} is now a potential King", null, LocalMentions.None);
            }
        }
    }


    public class UnlistedModule : MummyModule
    {
        public DataService Data { get; set; }

        [Command("unlisted")]
        public async Task Unlisted()
        {
            var sb = new StringBuilder();
            var count = 0;
            foreach (var user in Context.Guild.Members)
            {
                var b1 = Data.ImposterKings.Any(x => x.id != user.Key);
                var b2 = Data.ImposterQueens.Any(x => x.id != user.Key);
                if (!b1 && !b2)
                {
                    sb.AppendLine($"{user.Value.DisplayName} has not been asign to Ponetial Kings/Queens pools");
                    count++;
                }
            }
            if (count == 0)
                sb.Append("everybody is listed as potential King/Queen.");
            await ReplyAsync(sb.ToString(), allowedMentions: LocalMentions.None);
        }

        [Command("imposters")]
        public async Task Imposters()
        {
            var sb = new StringBuilder();
            foreach (var id in Data.WhitelistedIds)
            {
                var user = Context.Guild.GetMember(id);
                    sb.AppendLine($"{user.DisplayName} can be crowned imposter");
               
            }
            
            await ReplyAsync(sb.ToString(), allowedMentions: LocalMentions.None);
        }

    }
}


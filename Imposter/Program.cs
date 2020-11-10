
using Discord.Addons.Interactive;
using Disqord;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mummybot;
using Mummybot.Commands;
using Mummybot.Enums;
using Mummybot.Exceptions;
using Mummybot.Extentions;
using Mummybot.Services;
using Newtonsoft.Json;
using Qmmands;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Imposter
{
    internal class Heart
    {
        private static void Main(string[] args)
            => new Heart().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var assembly = Assembly.GetEntryAssembly();
            var types = assembly?.GetTypes()
                .Where(x => typeof(BaseService).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var services = new ServiceCollection()
                .AddServices(types)
                .AddSingleton(assembly)

                 //.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                 //{
                 //    ExclusiveBulkDelete = true,
                 //    AlwaysDownloadUsers = true,
                 //    LogLevel = LogSeverity.Info,
                 //    MessageCacheSize = 100
                 //}))

                 .AddSingleton(new DiscordClient(TokenType.Bot, config.Token, new DiscordClientConfiguration()
                 {
#if DEBUG
                     Activity = new Optional<LocalActivity>(new LocalActivity("being debuged", ActivityType.Playing)),
#else
                     Activity = new Optional<LocalActivity>(new LocalActivity("imposter", ActivityType.Playing)),
#endif
                     
                     Logger = new Optional<ILogger>(new LogService()),
                     Status = UserStatus.DoNotDisturb
                 }))

                 .AddSingleton<InteractiveService>()

                 .AddSingleton(new CommandService(new CommandServiceConfiguration()
                 {
                     StringComparison = StringComparison.CurrentCultureIgnoreCase,
                     CooldownBucketKeyGenerator = CoolDownBucketGenerator
                 })
                 .AddTypeParsers(assembly))
                 .AddSingleton<Random>()
                .BuildServiceProvider();


            var imposter = new BotStartup(services);
            await imposter.StartAsync(types);
        }

        public object CoolDownBucketGenerator(object bucketType, CommandContext context)
        {
            if (!(context is MummyContext ctx))
                throw new InvalidContextException(context.GetType());

            if (bucketType is CooldownBucketType CBT)
                return CBT switch
                {
                    CooldownBucketType.Guild => (object)ctx.GuildId,
                    CooldownBucketType.User => ctx.UserId,
                    CooldownBucketType.Channel => ctx.ChannelId,
                    CooldownBucketType.Global => ctx.Command,
                    _ => throw new InvalidOperationException("got unexpected cooldownbuckettype"),
                };
            throw new InvalidOperationException($"cooldownbuckettype failed to parse as {typeof(CooldownBucketType)}");
        }
    }
}

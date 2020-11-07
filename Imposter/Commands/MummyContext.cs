using Disqord;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mummybot.Enums;
using Mummybot.Services;
using Qmmands;
using System;

namespace Mummybot.Commands
{
    public class MummyContext : CommandContext
    {

        public MummyContext(DiscordClient client, CachedUserMessage message, IServiceProvider services,string prefix) : base(services)
        {
            Client = client;
            Message = message;
            LogService = services.GetRequiredService<LogService>();
            Services = services;
            PrefixUsed = prefix;
        }

        public IServiceProvider Services { get; private set; }
        public CachedMember User => Message.Author as CachedMember;
        public CachedTextChannel Channel => Message.Channel as CachedTextChannel;

        public DiscordClient Client { get; set; }
        public CachedUserMessage Message { get; private set; }

        public CachedGuild Guild => User.Guild;

        public LogService LogService { get; }
        public string PrefixUsed { get; set; }

        public bool IsEdit { get; set; }


        public ulong ChannelId => Channel.Id;
        public ulong UserId => User.Id;
        public ulong GuildId => Guild.Id;

        internal void LogDebug(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
       => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Debug, source.ToString(), Message, exception, Guild));

        internal void LogWarning(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
        => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Warning, source.ToString(), Message, exception, Guild));

        internal void LogVerbose(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
        => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Trace, source.ToString(), Message, exception, Guild));

        internal void LogCritical(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
        => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Critical, source.ToString(), Message, exception, Guild));

        internal void LogError(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
        => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Error, source.ToString(), Message, exception, Guild));


        internal void LogInformation(string Message, LogSource source = LogSource.Unkown, Exception exception = null)
        => LogService.LogEventCustomAsync(new Structs.LogMessage(LogSeverity.Information, source.ToString(), Message, exception, Guild));

        internal static MummyContext Create(DiscordClient client, CachedUserMessage message, IServiceProvider services,string prefix)
        {
            return new MummyContext(client, message, services, prefix)
            {
                Client = client,
                Message = message,
                PrefixUsed = prefix,
                Services = services,
            };
        }
    }
}

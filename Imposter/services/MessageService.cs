
using Disqord;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Mummybot.Attributes.Checks;
using Mummybot.Commands;
using Mummybot.Enums;
using Mummybot.Services;
using Qmmands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Imposter.services
{
    public class MessageService : BaseService
    {
        public DiscordClient Client { get; set; }
        public IServiceProvider Services { get; set; }
        public CommandService Commands { get; set; }
        public LogService LogService { get; set; }
        public IEnumerable<string> Prefixes { get; set; }

        private readonly ConcurrentDictionary<ulong, Stopwatch> ActiveTimings = new ConcurrentDictionary<ulong, Stopwatch>();


        public override Task InitialiseAsync(IServiceProvider services)
        {
#if DEBUG
            Prefixes = new List<string>() { "*", "*" };
#else   
            Prefixes = new List<string>() { "!", "!" };
#endif

            Services = services;
            var client = services.GetRequiredService<DiscordClient>();
            Commands = services.GetRequiredService<CommandService>();
            LogService = services.GetRequiredService<LogService>();
            client.MessageReceived += e =>
                           e.Message is CachedUserMessage message
                               ? HandleReceivedMessageAsync(message)
                               : Task.CompletedTask;
            client.MessageUpdated += e =>
                           e.NewMessage is CachedUserMessage message
                               ? HandleReceivedMessageAsync(message)
                               : Task.CompletedTask;
            Client = client;
            return base.InitialiseAsync(services);
        }


        private async Task HandleReceivedMessageAsync(CachedUserMessage message)
        {
            if (message.Author.IsBot)
                return;

            if (!(message.Channel is CachedTextChannel textChannel) ||
                !textChannel.Guild.CurrentMember.GetPermissionsFor(textChannel).Has(Permission.SendMessages))
                return;
            if (CommandUtilities.HasAnyPrefix(message.Content, Prefixes, StringComparison.CurrentCulture,out var prefix, out var output) ||
                CommandUtilities.HasPrefix(message.Content, $"<@{Client.CurrentUser.Id}>", StringComparison.Ordinal, out output) ||
                CommandUtilities.HasPrefix(message.Content, $"<@!{Client.CurrentUser.Id}>", StringComparison.Ordinal, out output))
            {
                if (string.IsNullOrWhiteSpace(output))
                    return;

                try
                {
                    if (prefix is null)
                        prefix = Client.CurrentUser.Mention;
                    var ctx = MummyContext.Create(Client, message, Services, prefix);


                    ActiveTimings[ctx.UserId] = Stopwatch.StartNew();
                    var r = await Commands.ExecuteAsync(output, ctx);
                    ActiveTimings[ctx.UserId].Stop();
                    ActiveTimings.Remove(ctx.UserId, out var st);
                    if (r.IsSuccessful)
                    {
                        LogService.LogInformation($"command: {ctx.Command.Name} has successful finished execution in {st.ElapsedMilliseconds}ms.", LogSource.MessagesService, ctx.GuildId);
                    }
                    else
                    {
                        switch (r)
                        {

                            case ExecutionFailedResult executionfailed:
                                {
                                    LogService.LogError(executionfailed.ToString(), LogSource.MessagesService, ctx.GuildId, executionfailed.Exception);

                                    break;
                                }
                            case ArgumentParseFailedResult parsefailed:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithTitle("ArmgumentParseFailure");
                                    eb.AddField(parsefailed.Reason, parsefailed.RawArguments);
                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case ArgumentParserResult parseresult:
                                {
                                    LogService.LogCritical("dunno argumentparse", LogSource.MessagesService, ctx.GuildId);
                                    await ctx.Channel.SendMessageAsync("a error has occoured and not been handled this error will be resolved shortly. (hopefully)");
                                    break;
                                }
                            case ChecksFailedResult checksfailed:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithTitle($"{checksfailed.FailedChecks.Count} checks have failed");
                                    foreach (var (Check, Result) in checksfailed.FailedChecks)
                                    {
                                        eb.AddField((Check as MummyCheckBase).Name, Result.Reason, true);
                                    }
                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case CommandDisabledResult disabled:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    await ctx.Channel.SendMessageAsync();
                                    eb.WithTitle($"{disabled.Command} is currently diabled");

                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case CommandNotFoundResult notfound:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithTitle($"command with name {notfound.Reason}");
                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case CommandOnCooldownResult oncooldown:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithTitle($"{oncooldown.Command.Name} is currently on cooldown");
                                    foreach (var (cooldown, retryafter) in oncooldown.Cooldowns)
                                    {
                                        int index = cooldown.ToString().LastIndexOf('.');
                                        var bucketype = (CooldownBucketType)cooldown.BucketType;
                                        eb.AddField(cooldown.ToString().Substring(index + 1), $"is only allowed to be run {cooldown.Amount} per {cooldown.Per.Humanize()} and it forced per {bucketype}, currently locked for {retryafter.TotalMinutes} Minutes", true);
                                    }
                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case OverloadsFailedResult overloadsFailed:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithDescription(overloadsFailed.Reason);
                                    foreach (var (command, overloadResult) in overloadsFailed.FailedOverloads)
                                        eb.AddField($"{command.Name} {string.Join(' ', command.Parameters.Select(x => x.Name))}", overloadResult.Reason);

                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                            case ParameterChecksFailedResult paramcheckfailed:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.WithTitle($"checks on {paramcheckfailed.Parameter} have failed with provided argument: {paramcheckfailed.Argument}");
                                    foreach (var (Check, Result) in paramcheckfailed.FailedChecks)
                                    {
                                        var index = Check.ToString().LastIndexOf('.');
                                        var name = Check.ToString().Substring(index + 1);
                                        eb.AddField(name, Result.Reason);

                                    }
                                    break;
                                }
                            case TypeParseFailedResult typeParseFailed:
                                {
                                    var eb = new LocalEmbedBuilder();
                                    eb.WithAuthor(ctx.User);
                                    eb.AddField(typeParseFailed.Parameter.Name, typeParseFailed.Reason);
                                    await ctx.Channel.SendMessageAsync(embed: eb.Build());
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError("Issue with message service", LogSource.MessagesService, exception: ex);
                }
            }
        }
    }
}

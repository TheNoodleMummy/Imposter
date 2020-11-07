using Disqord;
using Disqord.Events;
using Microsoft.Extensions.DependencyInjection;
using Mummybot.Extentions;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Mummybot
{
    public class BotStartup
    {
        private readonly DiscordClient DiscordClient;
        private readonly IServiceProvider Services;
        private readonly CommandService CommandService;
        private IEnumerable<Type> Types;

        public BotStartup(IServiceProvider services)
        {
            Services = services;
            DiscordClient = services.GetRequiredService<DiscordClient>();
            CommandService = services.GetRequiredService<CommandService>();

            CommandService.AddModules(services.GetRequiredService<Assembly>());
            DiscordClient.MemberJoined += DiscordClient_UserJoined;
        }

        private async Task DiscordClient_UserJoined(MemberJoinedEventArgs e)
        {
            if (e.Member.Guild.Id == 759143648339558412)
            {
                await e.Member.GrantRoleAsync(759199482537050164);
            }
            await Task.CompletedTask;
        }

        public async Task StartAsync(IEnumerable<Type> types)
        {

            Types = types;
            DiscordClient.Ready += DiscordClient_ReadyAsync;
            await DiscordClient.RunAsync();//seems to be never returning
            await Task.Delay(-1);
        }

        private async Task DiscordClient_ReadyAsync(ReadyEventArgs e)
        {
            Console.Title = DiscordClient.CurrentUser.Name;
            await Services.RunInitialisersAsync(Types);
            DiscordClient.Ready -= DiscordClient_ReadyAsync;
        }
    }
}

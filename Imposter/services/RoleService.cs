using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Mummybot.Enums;
using Mummybot.Extentions;
using Mummybot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Imposter.services
{
    public class RoleService : BaseService
    {
        public DiscordClient Client { get; set; }
        public LogService LogService { get; set; }
        public Timer Timer { get; set; }
        public IServiceProvider Services { get; set; }

        public Random Random { get; set; }

        private CachedMember CurrentKing, CurrentQueen;
        private readonly ulong KingId = 764921111750115360;
        private readonly ulong QueenId = 764921076919959583;

        public override Task InitialiseAsync(IServiceProvider services)
        {
            Services = services;
            Client = services.GetRequiredService<DiscordClient>();
            LogService = services.GetRequiredService<LogService>();
            Random = services.GetRequiredService<Random>();


            var nowutc = DateTime.Today.ToUniversalTime();
            var hours = 23 - DateTime.UtcNow.Hour;
            var min = 59 - DateTime.UtcNow.Minute;
            var span = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(min);
            //var span = TimeSpan.FromSeconds(5);

            Timer = new Timer(CallbackAsync, new object(), span, TimeSpan.FromHours(24));
            LogService.LogInformation($"timer started and will fire in {span}");

            CurrentQueen = Client.GetGuild(759143648339558412).GetRole(QueenId).Members.FirstOrDefault().Value;
            CurrentKing = Client.GetGuild(759143648339558412).GetRole(KingId).Members.FirstOrDefault().Value;
            return base.InitialiseAsync(services);
        }


        internal async void CallbackAsync(object state)
        {
            var guild = Client.GetGuild(759143648339558412);
            var role = guild.GetRole(759199236545577021);
            try
            {
                var data = Services.GetRequiredService<DataService>();
               
                LogService.LogInformation("Starting to Determin new imposters", LogSource.RoleService, guild.Id);
                LogService.LogInformation($"removing role form current: {role.Members.Count()} imposters", LogSource.RoleService, guild.Id);

                foreach (var user in role.Members)
                {
                     await user.Value.RevokeRoleAsync(role.Id);
                }

                var maximposters = Math.Min((int)Math.Floor(guild.MemberCount / 3.0), data.WhitelistedIds.Count);
                LogService.LogInformation($"Choosing new imposters with a max of: {maximposters} imposters", LogSource.RoleService, guild.Id);
                var guildusers = guild.Members.ToArray();
                var currentnewimposters = new List<ulong>();
                for (int i = 0; i < maximposters; i++)
                {
                    var choosenuser = guildusers[Random.Next(0, guildusers.Count())];
                    if (choosenuser.Value.Roles.Any(r => r.Key == role.Id)||currentnewimposters.Any(x => x == choosenuser.Key))
                    {
                        LogService.LogInformation($"rechoosing user, {choosenuser} cannot be crowend imposter (already is imposter)", LogSource.RoleService, guild.Id);
                        i--;
                        continue;
                    }
                    if (!data.WhitelistedIds.Contains(choosenuser.Key))
                    {
                        LogService.LogInformation($"rechoosing user, {choosenuser} cannot be crowend imposter (not whitelisted)", LogSource.RoleService, guild.Id);
                        i--;
                        continue;
                    }

                    LogService.LogInformation($"assigning {choosenuser}", LogSource.RoleService, guild.Id);
                    if (data.ImposterKings.Any(x => x.id == choosenuser.Key))
                    {
                        var (id, count) = data.ImposterKings.FirstOrDefault(x => x.id == choosenuser.Key);
                        data.ImposterKings.Remove((id, count));
                        count++;
                        data.ImposterKings.Add((id, count));
                    }
                    
                    if (data.ImposterQueens.Any(x => x.id == choosenuser.Key))
                    {
                        var (id2, count2) = data.ImposterQueens.FirstOrDefault(x => x.id == choosenuser.Key);
                        data.ImposterQueens.Remove((id2, count2));
                        count2++;
                        data.ImposterQueens.Add((id2, count2));
                    }
                    currentnewimposters.Add(choosenuser.Key);
                    await choosenuser.Value.GrantRoleAsync(role.Id);
                }
                await UpdateKingQueenAsync();
            }
            catch (Exception e)
            {
                LogService.LogError("something went wrong...", LogSource.RoleService, guild.Id, e);
            }

        }

        public async Task UpdateKingQueenAsync()
        {
            try
            {
                LogService.LogDebug("Update has been requested", LogSource.RoleService);
                var data = Services.GetRequiredService<DataService>();
                var guild = Client.GetGuild(759143648339558412);
                var KingR = guild.GetRole(KingId);
                var QueenR = guild.GetRole(QueenId);
                var tmpking = data.ImposterKings.OrderByDescending(x => x.count).FirstOrDefault();
                var tmpqueen = data.ImposterQueens.OrderByDescending(x => x.count).FirstOrDefault();

                if (CurrentKing?.Id != tmpking.id)
                {
                    LogService.LogInformation($"{CurrentKing} has been outranked.=> decrowning", LogSource.RoleService, guild.Id);
                    if (CurrentKing != null)
                        await CurrentKing.RevokeRoleAsync(KingR.Id);
                    CurrentKing = guild.GetMember(tmpking.id);
                    await CurrentKing.GrantRoleAsync(KingId);
                    LogService.LogInformation($"{CurrentKing} has crowned king", LogSource.RoleService, guild.Id);
                }

                if (CurrentQueen?.Id != tmpqueen.id)
                {
                    LogService.LogInformation($"{CurrentQueen} has been outranked.=> decrowning", LogSource.RoleService, guild.Id);
                    if (CurrentQueen != null)
                        await CurrentQueen.RevokeRoleAsync(QueenR.Id);
                    CurrentQueen = guild.GetMember(tmpqueen.id);
                    await CurrentQueen.GrantRoleAsync(QueenId);
                    LogService.LogInformation($"{CurrentQueen} has crowned queen", LogSource.RoleService, guild.Id);
                }
            }
            catch (Exception e)
            {
                LogService.LogError($"uh oh shit broke", LogSource.RoleService, 0, e);
            }
        }
    }
}

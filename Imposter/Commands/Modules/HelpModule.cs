using Discord.Addons.Interactive;
using Disqord;
using Imposter.services.Interactive.Criteria;
using Mummybot.Commands;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imposter.Commands.Modules
{
    [Name("Help Commands"), Description("This Module helps with commands as in what they need and if its required or optional")]
    public class HelpModule : MummyModule
    {
        public CommandService Commands { get; set; }
        public InteractiveService Iservice { get; set; }

        [Command("commands", "help"), Description("All Command for this bot")]
        public async Task HelpAsync()
        {
            var options = PaginatedAppearanceOptions.Default;
            var msg = new PaginatedMessage { Options = options };
            var infoemb = new LocalEmbedBuilder();
            infoemb.AddField("optional", $"*value*", true);
            infoemb.AddField("remainder", "__value__", true);
            infoemb.AddField("required", "**value**", true);
            infoemb.WithDescription("for example: !command __*cookies this can contain spaces*__ this  means the command take a optional remainder");

            msg.Pages.Add(infoemb);

            foreach (var module in Commands.GetAllModules())
            {
                try
                {
                    var modulecheck = await module.RunChecksAsync(Context);
                    if (modulecheck.IsSuccessful)
                    {
                        if (module.Commands.Count == 0)
                            continue; //skip module if commands are 0
                        if (module.Parent != null)
                            continue;

                        var emb = new LocalEmbedBuilder();
                        emb.WithTitle(module.Name);
                        emb.WithAuthor(Context.User.DisplayName, Context.User.GetAvatarUrl());
                        var sb = new StringBuilder();
                        var commands = CommandUtilities.EnumerateAllCommands(module);
                        foreach (var command in commands)
                        {
                            var checks = await command.RunChecksAsync(Context);
                            if (checks.IsSuccessful)
                            {
                                sb.Append(Context.PrefixUsed).Append(command.Name).Append(" ");
                                foreach (var parameter in command.Parameters)
                                {
                                    if (parameter.IsOptional && parameter.IsRemainder)//optional remiander
                                    {
                                        sb.Append($"__*{parameter.Name}*__ ");
                                    }
                                    else if (parameter.IsOptional && !parameter.IsRemainder)//optional
                                    {
                                        sb.Append($"*{parameter.Name}* ");
                                    }
                                    else if (!parameter.IsOptional && parameter.IsRemainder) //required remainder
                                    {
                                        sb.Append($"__**{parameter.Name}**__ ");
                                    }
                                    else if (!parameter.IsOptional && !parameter.IsRemainder)//required
                                    {
                                        sb.Append($"**{parameter.Name}** ");
                                    }
                                }
                                sb.AppendLine();
                            }
                        }
                        emb.WithDescription(sb.ToString());

                        msg.Pages.Add(emb);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            await new PaginatedMessageCallback(Iservice, Context, msg, new EnsureReactionFromUser(Context.User)).DisplayAsync();
        }

        //[Command("help"), Description("Specific help for a module")]
        //public async Task HelpAsync([Description("Command you want the help for"),Remainder]Module module)
        //{

        //    var builder = new EmbedBuilder()
        //    {
        //        Color = Context.User.Roles.FirstOrDefault(x => x.IsHoisted)?.Color ?? Color.DarkRed,
        //        Description = $"Here are some commands in the  **{module.Name}**"
        //    };
        //    builder.WithAuthor(Context.User.GetDisplayName(), Context.User.GetAvatarUrl());
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var cmd in module.Commands)
        //    {
        //        if (!(await cmd.RunChecksAsync(Context)).IsSuccessful) return;
        //        sb.Append(Context.PrefixUsed).AppendLine(cmd.Name);
        //    }
        //    builder.AddField("\u200B", sb.ToString());
        //    await ReplyAsync(embed: builder);
        //}

        [Command("help"), Description("Specific help for a command")]
        public async Task HelpAsync([Description("Command you want the help for"), Remainder] string command)
        {
            var result = Commands.FindCommands(command).ToArray();
            if (result.Length == 0)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new LocalEmbedBuilder()
                .WithAuthor(Context.User)
                .WithColor(Context.User.Roles.FirstOrDefault(x => x.Value.IsHoisted).Value?.Color ?? Color.DarkRed);

            foreach (var match in result.Take(4))
            {
                var sb = new StringBuilder();

                if (Context.PrefixUsed.Length == 1)
                    sb.Append(Context.PrefixUsed).Append(match.Command.Name).Append(" ");
                else
                    sb.Append(Context.PrefixUsed).Append(" ").Append(match.Command.Name).Append(" ");

                foreach (var parameter in match.Command.Parameters)
                {
                    if (parameter.IsOptional && parameter.IsRemainder)//optional remiander
                    {
                        sb.Append($"__*{parameter.Name}*__ ");
                    }
                    else if (parameter.IsOptional && !parameter.IsRemainder)//optional
                    {
                        sb.Append($"*{parameter.Name}* ");
                    }
                    else if (!parameter.IsOptional && parameter.IsRemainder) //required remainder
                    {
                        sb.Append($"__**{parameter.Name}**__ ");
                    }
                    else if (!parameter.IsOptional && !parameter.IsRemainder)//required
                    {
                        sb.Append($"**{parameter.Name}** ");
                    }
                }

                sb.AppendLine();
                builder.AddField("Command", sb.ToString());

                foreach (var parameter in match.Command.Parameters)
                {
                    builder.AddField(parameter.Name, parameter.Description ?? parameter.Remarks ?? "missing info", true);
                }
            }
            builder.AddField("\u200B", "info on parameter formation");

            builder.AddField("optional", $"*value*", true);
            builder.AddField("remainder", "__value__", true);
            builder.AddField("required", "**value**", true);

            await ReplyAsync(embed: builder);
        }
    }
}

using Disqord;
using Humanizer;
using Imposter.extensions;
using Microsoft.CodeAnalysis;
using Mummybot.Attributes.Checks;
using Mummybot.Extentions;
using Mummybot.Services;
using Qmmands;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mummybot.Commands.Modules
{
    [RequireOwner]
    public class OwnerModule : MummyModule
    {
        public EvalService EvalService { get; set; }


        [Command("eval")]
        public async Task Eval([Remainder] string code)
        {
               
            var builder = new LocalEmbedBuilder
            {
                Title = "Evaluating Code...",
                Color = Color.DarkRed,
                Description = "Waiting for completion...",
                Author = new LocalEmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Name = Context.User.DisplayName
                },
                Timestamp = DateTimeOffset.UtcNow,
                ThumbnailUrl = Context.Guild.CurrentMember.GetAvatarUrl()
            };
            var msg = await ReplyAsync(embed: builder);
            var sw = Stopwatch.StartNew();
            var script = EvalService.Build(code);

            string snippet = string.Join(Environment.NewLine, script.Code.Split(Environment.NewLine.ToCharArray()).Where(line => !line.StartsWith("using")));

            var diagnostics = script.Compile();
            var compilationTime = sw.ElapsedMilliseconds;

            if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                builder.WithDescription($"Compilation finished in: {compilationTime}ms");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");

                builder.AddField("Code", $"```cs{Environment.NewLine}{snippet}```");
                builder.AddField("Compilation Errors", string.Join('\n', diagnostics.Select(x => $"{x}")));

                await msg.ModifyAsync(x => x.Embed = builder.Build());

                return;
            }
            sw.Restart();

            var context = new RoslynContext(Context, Services);

            try
            {
                var result = await script.RunAsync(context);

                sw.Stop();
                builder.WithColor(Color.Green);

                builder.WithDescription($"Code compiled in {compilationTime}ms and ran in {sw.ElapsedMilliseconds}ms");
                builder.WithTitle("Code Evaluated");
                builder.AddField("Code", $"```cs{Environment.NewLine}{snippet}```");

                if (!(result.ReturnValue is null))
                {
                    var sb = new StringBuilder();
                    var type = result.ReturnValue.GetType();
                    var rValue = result.ReturnValue;

                    switch (rValue)
                    {
                        case Color col:
                            builder.WithColor(col);
                            builder.AddField("Colour", $"{col.RawValue}");
                            break;

                        case string str:
                            builder.AddField($"{type}", $"\"{str}\"");
                            break;

                        case IEnumerable enumerable:

                            var list = enumerable.Cast<object>().ToList();
                            var enumType = enumerable.GetType();

                            if (list.Count > 25)
                            {
                                builder.AddField($"{enumType}", $"Enumerable has more than 10 elements ({list.Count})");
                                break;
                            }

                            if (list.Count > 0)
                            {
                                sb.AppendLine("```css");

                                foreach (var element in list)
                                    sb.Append('[').Append(element).AppendLine("]");

                                sb.AppendLine("```");
                            }
                            else
                            {
                                sb.AppendLine("Collection is empty");
                            }

                            builder.AddField($"{enumType}", sb.ToString());

                            break;

                        case Enum @enum:

                            builder.AddField($"{@enum.GetType()}", $"```\n{@enum.Humanize()}\n```");

                            break;

                        default:

                            var messages = rValue.Inspect();

                            if (type.IsValueType && messages.Count == 0)
                            {
                                builder.AddField($"{type}", rValue);
                            }

                            foreach (var message in messages)
                                await ReplyAsync($"```css\n{message}\n```");

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();


                builder.WithDescription($"Code evaluated in {sw.ElapsedMilliseconds}ms but there was a issue tho");
                builder.WithColor(Color.Red);
                builder.WithTitle("Failed Evaluation");
                builder.AddField("Code", $"```cs{Environment.NewLine}{snippet}```");

                var str = ex.ToString();

                builder.AddField("Exception", Format.Sanitize(str.Length >= 1000 ? str.Substring(0, 1000) : str));
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            await msg.ModifyAsync(x => x.Embed = builder.Build());
        }

        [Command("addusing")]
        public async Task AddUsingsAsync(string ns)
        {
            if (ns.Contains(" "))
            {
                await ReplyAsync($"the namespace {ns} contains a space => no beuno");
                return;
            }
            EvalService.usings.Add(ns);
            EvalService.SaveUsings();
            await Context.Message.AddOkAsync();
        }

        [Command("removeusing")]
        public async Task RemoveUsingsAsync(string ns)
        {
            if (EvalService.usings.Contains(ns))
            {
                EvalService.usings.Remove(ns);
                EvalService.SaveUsings();
            }
            await Context.Message.AddOkAsync();
        }

        [Command("usings")]
        public Task UsingsAsync()
        => ReplyAsync(string.Join('\n', EvalService.usings));

    }
}



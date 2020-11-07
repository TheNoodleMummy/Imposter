using Qmmands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mummybot.Commands.TypeReaders
{
    public class TimeSpanTypeparser : MummyTypeParser<TimeSpan>
    {
        private const string Regex = @"(\d+)\s?(w(?:eeks|eek?)?|d(?:ays|ay?)?|h(?:ours|rs|r?)|m(?:inutes|ins|in?)?|s(?:econds|econd|ecs|ec?)?)";

        public static readonly Regex TimeSpanRegex = new Regex(Regex, RegexOptions.Compiled);

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string value, MummyContext context)
        {

            MatchCollection matches = TimeSpanRegex.Matches(value);

            if (matches.Count == 0)
            {

                return TypeParserResult<TimeSpan>.Unsuccessful("no matches idk");
            }

            var result = new TimeSpan();
            bool weeks = false, days = false, hours = false, minutes = false, seconds = false;

            for (var m = 0; m < matches.Count; m++)
            {
                Match match = matches[m];

                if (!uint.TryParse(match.Groups[1].Value, out uint amount))
                {
                    continue;
                }

                switch (match.Groups[2].Value[0])
                {
                    case 'w':
                        if (!weeks)
                        {
                            result = result.Add(TimeSpan.FromDays(amount * 7));
                            weeks = true;
                        }

                        continue;

                    case 'd':
                        if (!days)
                        {
                            result = result.Add(TimeSpan.FromDays(amount));
                            days = true;
                        }

                        continue;

                    case 'h':
                        if (!hours)
                        {
                            result = result.Add(TimeSpan.FromHours(amount));
                            hours = true;
                        }

                        continue;

                    case 'm':
                        if (!minutes)
                        {
                            result = result.Add(TimeSpan.FromMinutes(amount));
                            minutes = true;
                        }

                        continue;

                    case 's':
                        if (!seconds)
                        {
                            result = result.Add(TimeSpan.FromSeconds(amount));
                            seconds = true;
                        }

                        continue;
                }
            }

            return TypeParserResult<TimeSpan>.Successful(result);
        }
    }
}
using Qmmands;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Mummybot.Commands.TypeReaders
{
    /// <summary>
    /// always use with RemainderAttribute as it requires a date followed by format example: "1/7/2019 dmy" or "7/1/2019 mdy"
    /// </summary>
    public class DateTimeOffsetTypeReader : MummyTypeParser<DateTimeOffset>
    {
        public override ValueTask<TypeParserResult<DateTimeOffset>> ParseAsync(Parameter parameter, string value, MummyContext context)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);
            if (!value.EndsWith("dmy", StringComparison.CurrentCultureIgnoreCase) && !value.EndsWith("mdy", StringComparison.CurrentCultureIgnoreCase))
            {
                return TypeParserResult<DateTimeOffset>.Unsuccessful("failed to parse expected mdy or dmy");
            }
            if (value.EndsWith("dmy", StringComparison.CurrentCultureIgnoreCase))
            {
                var time = value.Substring(0, value.IndexOf("dmy"));
                if (!DateTimeOffset.TryParse(time, CultureInfo.CurrentCulture, styles: DateTimeStyles.AssumeUniversal, out dateTimeOffset))
                    return TypeParserResult<DateTimeOffset>.Unsuccessful("failed to parse time (something doesnt look right in the time part)");
            }
            if (value.EndsWith("mdy", StringComparison.CurrentCultureIgnoreCase))
            {
                var time = value.Substring(0, value.IndexOf("mdy"));
                if (!DateTimeOffset.TryParse(time, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.AssumeUniversal, out dateTimeOffset))
                    return TypeParserResult<DateTimeOffset>.Unsuccessful("failed to parse time (something doesnt look right in the time part)");
            }
            if (dateTimeOffset != new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero))
                return TypeParserResult<DateTimeOffset>.Successful(dateTimeOffset);

            return TypeParserResult<DateTimeOffset>.Unsuccessful("something went badly wrong and i entirely failed to parse your time");
        }
    }
}


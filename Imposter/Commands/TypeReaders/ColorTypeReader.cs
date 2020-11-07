using Discord;
using Disqord;
using Qmmands;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Mummybot.Commands.TypeReaders
{
    class ColorTypeReader : MummyTypeParser<Color>
    {
        public override ValueTask<TypeParserResult<Color>> ParseAsync(Parameter parameter, string value, MummyContext context)
        {
            if (value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) ||
                value.StartsWith("&H", StringComparison.CurrentCultureIgnoreCase))
            {
                value = value.Substring(2);
            }
            if (value.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
            {
                value = value.Substring(1);
            }

            Color color;
            if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var hex))
            {
                color = new Color(hex);
                return TypeParserResult<Color>.Successful(color);

            }
            else if (IsRGB(value, out (int red, int green, int blue) rgb))
            {
                color = new Color(rgb.red, rgb.green, rgb.blue);
                return TypeParserResult<Color>.Successful(color);
            }
            else
                return TypeParserResult<Color>.Unsuccessful("Could not parse color");
        }

        private bool IsRGB(string value, out (int red, int green, int blue) rgb)
        {
            if (value.Contains(',') || value.Contains(' '))
            {
                var tmp = value.Split(',', ' ');
                if (tmp.Length == 3)
                {
                    rgb.red = int.Parse(tmp[0]);
                    rgb.green = int.Parse(tmp[1]);
                    rgb.blue = int.Parse(tmp[2]);
                    return true;
                }
                rgb.red = 0;
                rgb.green = 0;
                rgb.blue = 0;
                return false;
            }
            else
            {
                rgb.red = 0;
                rgb.green = 0;
                rgb.blue = 0;
                return false;
            }
        }
    }
}

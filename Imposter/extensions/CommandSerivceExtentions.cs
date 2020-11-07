using Mummybot.Attributes;
using Mummybot.Exceptions;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mummybot.Extentions
{
    public partial class Extentions
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static CommandService AddTypeParsers(this CommandService commands, Assembly assembly)
        {
            const string addParserName = "AddTypeParserInternal";

            var parsers = FindTypeParsers(commands, assembly);

            var internalAddParser = commands.GetType().GetMethod(addParserName, Flags);

            if (internalAddParser is null)
                throw new QuahuRenamedException(addParserName);

            foreach (var parser in parsers)
            {
                var @override = parser.GetCustomAttribute<DontOverrideAttribute>() is null;
                var autoadd = parser.GetCustomAttribute<DontAutoAddAttribute>() is null;
                var targetType = parser.BaseType?.GetGenericArguments().First();

                if (autoadd)
                {
                    internalAddParser.Invoke(commands, new[] { targetType, Activator.CreateInstance(parser), @override });
                }
            }

            return commands;
        }


        public static IReadOnlyCollection<Type> FindTypeParsers(this CommandService commands, Assembly assembly)
        {
            const string parserInterface = "ITypeParser";

            var typeParserInterface = commands.GetType().Assembly.GetTypes()
                .FirstOrDefault(x => x.Name == parserInterface)?.GetTypeInfo();

            if (typeParserInterface is null)
                throw new QuahuRenamedException(parserInterface);

            var parsers = assembly.GetTypes().Where(x => typeParserInterface.IsAssignableFrom(x) && !x.IsAbstract);

            return parsers.ToArray();
        }
    }
}

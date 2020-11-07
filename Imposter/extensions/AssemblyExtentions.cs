using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mummybot.Extentions
{
    public partial class Extentions
    {
        private static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        }
    }
}

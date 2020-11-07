using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Mummybot.Extentions
{
    public partial class Extentions
    {
        public static IServiceCollection AddServices(this IServiceCollection collection, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                collection.AddSingleton(type);
            }

            return collection;
        }
    }
}

using Mummybot.Attributes;
using Mummybot.Exceptions;
using Mummybot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mummybot.Extentions
{
    public partial class Extentions
    {
        public static async Task RunInitialisersAsync(this IServiceProvider services, IEnumerable<Type> types)
        {
            var myServices = new Dictionary<int, Type> { };
            foreach (var item in types)
            {
                var attribute = item.GetCustomAttribute(typeof(InitilizerPriorityAttribute), false);
                var prio = attribute as InitilizerPriorityAttribute;

                if (prio != null && myServices.ContainsKey(prio.value))
                    throw new InvalidPriorityException(prio.value, item);

                myServices.Add(prio?.value ?? myServices.Count + 100, item);
            }

            var ordered = myServices.OrderBy(i => i.Key);

            foreach (var type in ordered)
            {
                var service = services.GetService(type.Value);

                if (!(service is BaseService validService))
                    throw new InvalidServiceException(nameof(type));

                await validService.InitialiseAsync(services);
            }
        }
    }
}

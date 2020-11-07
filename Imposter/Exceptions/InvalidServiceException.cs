using Mummybot.Services;
using System;

namespace Mummybot.Exceptions
{
    public class InvalidServiceException : Exception
    {
        public InvalidServiceException(string type) : base($"{type} does not inherit {nameof(BaseService)}")
        {
        }

    }
}

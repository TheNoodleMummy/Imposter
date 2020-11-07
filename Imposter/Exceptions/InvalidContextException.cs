using Mummybot.Commands;
using System;

namespace Mummybot.Exceptions
{
    class InvalidContextException : Exception
    {
        public InvalidContextException(Type type) : base($"Expected {typeof(MummyContext)}, got: {type}")
        {

        }
    }
}

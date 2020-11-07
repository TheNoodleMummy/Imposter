using System;

namespace Mummybot.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DoNotAutoAddAttribute : Attribute
    {
    }
}

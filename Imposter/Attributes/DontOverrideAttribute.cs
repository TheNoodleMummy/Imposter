using System;

namespace Mummybot.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DontOverrideAttribute : Attribute
    {
    }
}

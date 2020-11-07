using System;

namespace Mummybot.Attributes
{
    class NameAttribute : Attribute
    {
        public string Name { get; }
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}

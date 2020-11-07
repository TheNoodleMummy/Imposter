using System;

namespace Mummybot.Attributes
{
    public class InitilizerPriorityAttribute : Attribute
    {
        public int value;
        public InitilizerPriorityAttribute(int weight)
        {
            value = weight;
        }
    }
}

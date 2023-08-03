using System;

namespace BehaviorDesigner
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public readonly string name;

        public ButtonAttribute(string name = "")
        {
            this.name = name;
        }
    }
}
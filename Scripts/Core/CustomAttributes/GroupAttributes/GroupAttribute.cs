using System;

namespace Scuti
{
    public abstract class GroupAttribute : ScutiAttribute
    {
        public string Name { get; private set; }

        public GroupAttribute(string name)
        {
            this.Name = name;
        }
    }
}

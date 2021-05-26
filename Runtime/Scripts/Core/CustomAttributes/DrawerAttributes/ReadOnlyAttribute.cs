using System;

namespace Scuti
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : DrawerAttribute
    {
    }
}

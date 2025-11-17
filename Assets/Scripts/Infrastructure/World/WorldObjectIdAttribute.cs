using System;

namespace Infrastructure.World
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class WorldObjectIdAttribute : Attribute
    {
        public bool ShowWarning { get; }

        public WorldObjectIdAttribute(bool showWarning = false)
        {
            ShowWarning = showWarning;
        }
    }
}
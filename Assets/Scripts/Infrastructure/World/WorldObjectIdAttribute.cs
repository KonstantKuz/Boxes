using System;

namespace Infrastructure.World
{
    /// <summary>
    /// Marks a string field to accept WorldObject drag-and-drop and automatically extract its ID.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class WorldObjectIdAttribute : Attribute
    {
    }
}
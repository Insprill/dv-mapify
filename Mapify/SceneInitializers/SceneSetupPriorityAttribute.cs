using System;

namespace Mapify.SceneInitializers
{
    /// <summary>
    ///     Defines the priority of a scene setup.
    ///     Lower priority scene setups will be run first.
    ///     Defaults to 0.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SceneSetupPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public SceneSetupPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}

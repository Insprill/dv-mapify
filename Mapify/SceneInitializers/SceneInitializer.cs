using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DV.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mapify.SceneInitializers
{
    public abstract class SceneInitializer
    {
        protected Scene scene { get; private set; }
        private readonly bool vanilla;

        protected SceneInitializer(Scene scene, bool vanilla = false)
        {
            this.scene = scene;
            this.vanilla = vanilla;
        }

        public virtual void Run()
        {
            foreach (SceneSetup setup in CreateSetups())
                setup.Run();
            if (!vanilla)
                new GameObject(nameof(SingletonInstanceFinder)).AddComponent<SingletonInstanceFinder>();
        }

        private List<SceneSetup> CreateSetups()
        {
            List<SceneSetup> initializers = new List<SceneSetup>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                         .Where(type => type.IsSubclassOf(typeof(SceneSetup)) && type.Namespace == GetType().Namespace && type != GetType())
                         .OrderBy(type => type.GetCustomAttribute<SceneSetupPriorityAttribute>()?.Priority ?? 0)
                    )
                if (type.GetConstructor(Type.EmptyTypes)?.Invoke(null) is SceneSetup setup)
                    initializers.Add(setup);
                else
                    Mapify.LogError($"Failed to create {nameof(SceneSetup)} {type.FullName}");

            return initializers;
        }
    }
}

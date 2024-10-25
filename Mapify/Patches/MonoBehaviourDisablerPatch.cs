using System;
using System.Collections.Generic;
using System.Reflection;
using DV.Booklets.Testing;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    /// <summary>
    ///     A utility for disabling MonoBehaviours with setup logic in the GameContent scene.
    ///     This allows us to load it to copy assets from, without worrying about invalid states being created.
    /// </summary>
    public static class MonoBehaviourDisablerPatch
    {
        private static HashSet<MethodInfo> patchedMethods;
        private static readonly string[] methodNames = {
            "Awake",
            "OnEnable",
            "Start",
            "FixedUpdate",
            "Update",
            "LateUpdate",
            "OnDisable",
            "OnDestroy"
        };
        private static readonly MethodInfo prefix = AccessTools.DeclaredMethod(typeof(MonoBehaviourDisablerPatch), nameof(Prefix));
        private static readonly HarmonyMethod harmonyPrefix = new HarmonyMethod(prefix);

        public static void DisableAll()
        {
            Mapify.LogDebug(() => "Disabling MonoBehaviours");

            patchedMethods = new HashSet<MethodInfo>();
            foreach (Type type in Assembly.GetAssembly(typeof(TrainCar)).GetTypes())
            {
                if (!type.IsSubclassOf(typeof(MonoBehaviour)) || (type.Namespace != null && !type.Namespace.StartsWith("DV.")))
                    continue;
                if (type == typeof(ALicenseSpawner<>))
                    continue; // Causes an error while trying to patch

                Mapify.LogDebugExtreme(() => $"Disabling {type.FullName}");

                foreach (string methodName in methodNames)
                {
                    // AccessTools#DeclaredMethod logs a warning if the method isn't found :|
                    MethodInfo method = type.GetMethod(methodName, AccessTools.allDeclared);
                    if (method == null) continue;
                    try
                    {
                        Mapify.Harmony.Patch(method, harmonyPrefix);
                        patchedMethods.Add(method);
                    }
                    catch (Exception e)
                    {
                        Mapify.LogException($"Failed to patch {type.FullName}#{methodName}", e);
                    }
                }
            }
        }

        public static void EnableAll()
        {
            Mapify.LogDebug(() => "Enabling disabled MonoBehaviours");
            foreach (MethodInfo method in patchedMethods)
                Mapify.Harmony.Unpatch(method, prefix);
            patchedMethods = null;
        }

        private static bool Prefix()
        {
            return false;
        }
    }
}

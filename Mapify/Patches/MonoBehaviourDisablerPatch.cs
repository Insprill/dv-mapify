using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DV;
using DV.CabControls;
using DV.CabControls.Spec;
using DV.CashRegister;
using DV.RenderTextureSystem;
using DV.Shops;
using HarmonyLib;
using UnityEngine;

namespace Mapify.Patches
{
    public static class MonoBehaviourPatch
    {
        private static readonly Type[] disableTypes = {
            typeof(RenderTextureSystem),
            typeof(CarSpawner),
            typeof(CarSpawnerOriginShiftHandler),
            typeof(SaveLoadController),
            typeof(LogicController),
            typeof(StorageController),
            typeof(StorageBase),
            typeof(ItemBase),
            typeof(ItemDisablerGrid),
            typeof(GlobalShopController),
            typeof(DerailAndDamageObserver),
            typeof(TutorialEnabler),
            typeof(StationController),
            typeof(StationLocoSpawner),
            typeof(WarehouseMachineController),
            typeof(PitStop),
            typeof(StorageAccessPoint),
            typeof(Shop),
            typeof(CashRegisterBase),
            typeof(ResourceModule),
            typeof(LocoResourceModule),
            typeof(ScanItemResourceModule),
            typeof(CashRegisterResourceModules),
            typeof(GarageLogic),
            typeof(GarageCarSpawner),
            typeof(ControlSpec)
        };
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
        private static readonly MethodInfo prefix = AccessTools.DeclaredMethod(typeof(MonoBehaviourPatch), nameof(Prefix));
        private static readonly HarmonyMethod harmonyPrefix = new HarmonyMethod(prefix);

        public static void DisableAll()
        {
            patchedMethods = new HashSet<MethodInfo>();
            foreach (Type type in disableTypes)
            {
                if (!type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    Main.Logger.Error($"Tried to patch non-MonoBehaviour type {type}");
                    continue;
                }

                foreach (string methodName in methodNames)
                {
                    MethodInfo method = AccessTools.DeclaredMethod(type, methodName);
                    if (method == null) continue;
                    Main.Harmony.Patch(method, harmonyPrefix);
                    patchedMethods.Add(method);
                }
            }
        }

        public static void EnableAllLater()
        {
            SingletonBehaviour<CoroutineManager>.Instance.StartCoroutine(EnableLater());
        }

        private static IEnumerator EnableLater()
        {
            yield return null;
            foreach (MethodInfo method in patchedMethods)
                Main.Harmony.Unpatch(method, prefix);
            patchedMethods = null;
        }

        private static bool Prefix()
        {
            return false;
        }
    }
}

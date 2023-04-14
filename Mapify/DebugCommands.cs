using System;
using System.Collections.Generic;
using System.Reflection;
using CommandTerminal;
using DV.Logic.Job;
using HarmonyLib;
using Mapify.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mapify
{
    public static class DebugCommands
    {
        public static void RegisterCommands()
        {
            try
            {
                MethodInfo[] methods = typeof(DebugCommands).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    if (!(Attribute.GetCustomAttribute(method, typeof(RegisterCommandAttribute)) is RegisterCommandAttribute commandAttribute))
                        return;
                    Action<CommandArg[]> proc = (Action<CommandArg[]>)Delegate.CreateDelegate(typeof(Action<CommandArg[]>), method);
                    Terminal.Shell.AddCommand(commandAttribute.Name, proc, commandAttribute.MinArgCount, commandAttribute.MaxArgCount, commandAttribute.Help, commandAttribute.Hint, commandAttribute.Secret);
                    Terminal.Autocomplete.Register(commandAttribute.Name);
                    Main.Logger.Log($"Registered command {commandAttribute.Name}");
                }
            }
            catch (Exception e)
            {
                Main.Logger.LogException("Failed to register debug commands", e);
            }
        }

        [RegisterCommand("mapify.license", Help = "Modifies a licenses", MinArgCount = 0, MaxArgCount = 2)]
        private static void ModifyLicense(CommandArg[] args)
        {
            if (args.Length == 0)
            {
                foreach (GeneralLicenseType gl in Enum.GetValues(typeof(GeneralLicenseType)))
                    LicenseManager.AcquireGeneralLicense(gl);
                foreach (JobLicenses jl in Enum.GetValues(typeof(JobLicenses)))
                    LicenseManager.AcquireJobLicense(jl);
                Debug.Log("Granted all licenses");
                return;
            }

            string licenseName = args[0].String;
            bool isGeneral = Enum.TryParse(licenseName, out GeneralLicenseType generalLicense);
            bool isJob = Enum.TryParse(licenseName, out JobLicenses jobLicense);
            if (!isGeneral && !isJob)
            {
                Debug.LogError($"No license found with name '{licenseName}'");
                return;
            }

            bool addLicense = args.Length == 2
                ? args[1].Bool
                : (isGeneral && !LicenseManager.IsGeneralLicenseAcquired(generalLicense)) || (isJob && !LicenseManager.IsJobLicenseAcquired(jobLicense));
            if (isGeneral)
            {
                if (addLicense)
                    LicenseManager.AcquireGeneralLicense(generalLicense);
                else
                    LicenseManager.RemoveGeneralLicense(generalLicense);
            }
            else
            {
                if (addLicense)
                    LicenseManager.AcquireJobLicense(jobLicense);
                else
                    LicenseManager.RemoveJobLicense(jobLicense);
            }

            LicenseManager.SaveData();
            Debug.Log($"{(addLicense ? "Granted" : "Revoked")} {(isGeneral ? "general" : "job")} license {licenseName}");
        }

        [RegisterCommand("mapify.money", Help = "Sets the amount of money you have", MinArgCount = 0, MaxArgCount = 1)]
        private static void SetMoney(CommandArg[] args)
        {
            SingletonBehaviour<Inventory>.Instance.SetMoney(args.Length == 0 ? double.MaxValue : args[0].Double());
        }

        [RegisterCommand("mapify.stationInfo", Help = "Prints information about all stations", MaxArgCount = 0)]
        private static void PrintStationInfo(CommandArg[] args)
        {
            foreach (StationController station in Object.FindObjectsOfType<StationController>())
            {
                StationInfo stationInfo = station.stationInfo;
                string str = "\n";
                str += $"{stationInfo.Name}\n";
                str += $"+-- Type: '{stationInfo.Type}'\n";
                str += $"+-- Yard ID: '{stationInfo.YardID}'\n";
                str += $"+-- Color: '{ColorUtility.ToHtmlStringRGBA(stationInfo.StationColor)}'";
                Debug.Log(str);
            }
        }

        [RegisterCommand("mapify.streamerInfo", Help = "Prints information about all streamers", MaxArgCount = 0)]
        private static void PrintStreamerInfo(CommandArg[] args)
        {
            foreach (Streamer streamer in Object.FindObjectsOfType<Streamer>())
            {
                Debug.Log($"--------------- {streamer.name} ---------------");
                streamer.gameObject.PrintHierarchy();
                Debug.Log("Scene GO's");
                foreach (KeyValuePair<int[], SceneSplit> data in streamer.scenesArray)
                {
                    if (data.Value.sceneGo == null) continue;
                    Debug.Log($"{data.Key[0]} {data.Key[1]} {data.Key[2]}");
                    data.Value.sceneGo.PrintHierarchy();
                }

                Debug.Log("Scene Collection");
                Debug.Log(JsonUtility.ToJson(streamer.sceneCollection, true));
            }
        }

        [RegisterCommand("mapify.spawnItem", Help = "Spawns an item at your feet", MaxArgCount = 1)]
        private static void GiveItem(CommandArg[] args)
        {
            Object obj = Resources.Load(args[0].String);
            GameObject go = obj as GameObject;
            if (go == null)
            {
                Debug.LogError($"Failed to find item {args[0].String}");
                return;
            }

            GameObject instantiated = GameObject.Instantiate(go);
            instantiated.transform.position = PlayerManager.PlayerTransform.position;
        }

        #region we do a little trolling

        [RegisterCommand("mapify.toggleSaving", Help = "Toggles saving", MaxArgCount = 1)]
        private static void ToggleSaving(CommandArg[] args)
        {
            bool on = args.Length == 0 ? SingletonBehaviour<SaveGameManager>.Instance.disableAutosave : args[0].Bool;
            SingletonBehaviour<SaveGameManager>.Instance.disableAutosave = !on;
            Debug.Log($"{(on ? "Enabled" : "Disabled")} saving");
        }

        [RegisterCommand("mapify.damageCar", Help = "Damages a car", MaxArgCount = 1)]
        private static void DamageCar(CommandArg[] args)
        {
            TrainCar trainCar = PlayerManager.Car;
            if (trainCar == null)
            {
                Debug.LogError("You must be on a car to damage it!");
                return;
            }

            float amount = args.Length == 0 ? float.MaxValue : args[0].Float;
            if (amount < 0)
                trainCar.CarDamage.RepairCar(amount);
            else
                trainCar.CarDamage.DamageCar(amount);
        }

        private static readonly MethodInfo CargoDamageModel_Method_ApplyDamageToCargo = AccessTools.DeclaredMethod(typeof(CargoDamageModel), "ApplyDamageToCargo", new[] { typeof(float) });

        [RegisterCommand("mapify.damageCargo", Help = "Damages a car's cargo", MaxArgCount = 1)]
        private static void DamageCargo(CommandArg[] args)
        {
            TrainCar trainCar = PlayerManager.Car;
            if (trainCar == null)
            {
                Debug.LogError("You must be on a car to damage it's cargo!");
                return;
            }

            CargoDamageModel_Method_ApplyDamageToCargo.Invoke(trainCar.CargoDamage, new object[] { args.Length == 0 ? float.MaxValue : args[0].Float });
        }

        [RegisterCommand("mapify.loadCar", Help = "Loads a car with the specified cargo", MinArgCount = 1, MaxArgCount = 2)]
        private static void LoadCar(CommandArg[] args)
        {
            TrainCar trainCar = PlayerManager.Car;
            if (trainCar == null)
            {
                Debug.LogError("You must be on a car to load it!");
                return;
            }

            if (!Enum.TryParse(args[0].String, out CargoType cargoType))
            {
                Debug.LogError($"Failed to find cargo type {args[0].String}, Please choose from the following list: {string.Join(", ", Enum.GetNames(typeof(CargoType)))}");
                return;
            }

            trainCar.logicCar.LoadCargo(args.Length == 1 ? trainCar.logicCar.capacity : args[1].Float, cargoType);
        }

        [RegisterCommand("mapify.unloadCar", Help = "Unloads a car", MinArgCount = 1, MaxArgCount = 2)]
        private static void UnloadCar(CommandArg[] args)
        {
            TrainCar trainCar = PlayerManager.Car;
            if (trainCar == null)
            {
                Debug.LogError("You must be on a car to unload it!");
                return;
            }

            if (!Enum.TryParse(args[0].String, out CargoType cargoType))
            {
                Debug.LogError($"Failed to find cargo type {args[0].String}, Please choose from the following list: {string.Join(", ", Enum.GetNames(typeof(CargoType)))}");
                return;
            }

            trainCar.logicCar.UnloadCargo(args.Length == 1 ? trainCar.logicCar.capacity : args[1].Float, cargoType);
        }

        #endregion
    }
}

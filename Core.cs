using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using MelonLoader;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

#if Mono
using ScheduleOne.Quests;
using ScheduleOne.Vehicles;
#elif IL2CPP
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.Vehicles;
#endif

[assembly: MelonInfo(typeof(Small_Corner_Map.Core), Constants.ModName, Constants.ModVersion, Constants.ModAuthor, null)]
[assembly: MelonGame(Constants.GameDeveloper, Constants.GameName)]

namespace Small_Corner_Map
{
    public class Core : MelonMod
    {
        private MinimapUI minimapUI;
        private MapPreferences mapPreferences;

        private static Core Instance { get; set; }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            mapPreferences = new MapPreferences();
            mapPreferences.LoadPreferences();
            Instance = this;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            MelonLogger.Msg(string.Format("Scene loaded: {0} (Build Index: {1})", sceneName, buildIndex));

            if (sceneName == "Main")
            {
                MelonLogger.Msg("GameplayScene loaded, initializing Small Corner Map...");
                minimapUI = new MinimapUI(mapPreferences);
                minimapUI.Initialize();
            }
            else
            {
                minimapUI?.Dispose();
                minimapUI = null;
            }
        }

        [HarmonyPatch(typeof(VehicleManager), "SpawnAndReturnVehicle")]
        class Patch_VehicleManagerSpawnAndReturnVehicle
        {
            static void Postfix(VehicleManager __instance, LandVehicle __result)
            {
                var vehicle = __instance.GetVehiclePrefab(__result.VehicleCode);
                var trackVehicles = Instance.mapPreferences?.TrackVehicles;
                if (trackVehicles == null || !trackVehicles.Value) return;
                Instance.minimapUI?.OnOwnedVehiclesAdded();
            }
        }

        [HarmonyPatch(typeof(Quest), "Start")]
        class Patch_QuestStart
        {
            static void Postfix(Quest __instance)
            {
                if (!QuestManager.InstanceExists) return;
                if (__instance == null || __instance.State != EQuestState.Active || !__instance.IsTracked) return;
                if (__instance is Contract || __instance.name.StartsWith("Deal for")) return;
                MelonLogger.Msg(string.Format("[Small Corner Map] Started Quest: {0} - {1}", __instance.GUID, __instance.name));
                Instance.minimapUI?.OnQuestStarted(__instance);
            }
        }

        [HarmonyPatch(typeof(Quest), "End")]
        class Patch_QuestEnd
        {
            static void Postfix(Quest __instance)
            {
                if (!QuestManager.InstanceExists) return;
                if (__instance == null || !__instance.IsTracked) return;
                if (__instance is Contract || __instance.name.StartsWith("Deal for")) return;
                MelonLogger.Msg(string.Format("[Small Corner Map] End Quest: {0} - {1}", __instance.GUID, __instance.name));
                if (__instance is Contract) return;
                Instance.minimapUI?.OnQuestCompleted(__instance);
            }
        }



        [HarmonyPatch(typeof(Contract), "Start")]
        class Patch_ContractStart
        {
            static void Postfix(Contract __instance)
            {
                var trackContracts = Instance.mapPreferences?.TrackContracts;
                if (trackContracts == null || !trackContracts.Value) return;
                if (__instance == null || __instance.State != EQuestState.Active || !__instance.IsTracked) return;

                Instance.minimapUI?.OnContractAccepted(__instance);
            }
        }

        [HarmonyPatch(typeof(Contract), "End")]
        class Patch_ContractEnd
        {
            static void Postfix(Contract __instance)
            {
                var trackContracts = Instance.mapPreferences?.TrackContracts;
                if (trackContracts == null || !trackContracts.Value) return;
                if (__instance == null || !__instance.IsTracked) return;
                Instance.minimapUI?.OnContractCompleted(__instance);
            }
        }
    }
}
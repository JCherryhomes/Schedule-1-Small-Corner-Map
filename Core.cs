using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using MelonLoader;
using HarmonyLib;

#if Mono
using ScheduleOne.Quests;
#elif IL2CPP
using Il2CppScheduleOne.Quests;
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

        [HarmonyPatch(typeof(Contract), "Start")]
        class Patch_ContractStart
        {
            static void Postfix(Contract __instance)
            {
                if (__instance == null || __instance.State != EQuestState.Active || !__instance.IsTracked) return;

                Instance.minimapUI?.OnContractAccepted(__instance);
            }
        }

        [HarmonyPatch(typeof(Contract), "End")]
        class Patch_ContractEnd
        {
            static void Postfix(Contract __instance)
            {
                if (__instance == null || !__instance.IsTracked) return;
                Instance.minimapUI?.OnContractCompleted(__instance);
            }
        }
    }
}
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using MelonLoader;
using UnityEngine;


#if Mono
using ScheduleOne.Economy;
using ScheduleOne.Quests;
using ScheduleOne.Vehicles;
#elif IL2CPP
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.Vehicles;
#endif

[assembly: MelonInfo(typeof(Small_Corner_Map.Core), Constants.ModName, Constants.ModVersion, Constants.ModAuthor, null)]
[assembly: MelonGame(Constants.GameDeveloper, Constants.GameName)]

namespace Small_Corner_Map
{
    public class Core : MelonMod
    {
        private static Core Instance { get; set; }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            Instance = this;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            MelonLogger.Msg(string.Format("Scene loaded: {0} (Build Index: {1})", sceneName, buildIndex));

            if (sceneName == "Main")
            {
                MelonLogger.Msg("GameplayScene loaded, initializing Small Corner Map...");
                MinimapManager.Instance.Initialize();
            }
            else
            {
                // Dispose of the minimap when leaving the main scene
                var baseGameObject = GameObject.Find("MinimapCanvas");

                if (baseGameObject != null)
                {
                    UnityEngine.Object.Destroy(baseGameObject);
                    MelonLogger.Msg("Small Corner Map disposed.");
                }
            }
        }
    }
}
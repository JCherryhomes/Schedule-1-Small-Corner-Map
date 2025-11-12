using MelonLoader;
using Small_Corner_Map.Helpers;

[assembly: MelonInfo(typeof(Small_Corner_Map.Core), Constants.ModName, Constants.ModVersion, Constants.ModAuthor, null)]
[assembly: MelonGame(Constants.GameDeveloper, Constants.GameName)]

namespace Small_Corner_Map
{
    public class Core : MelonMod
    {
        private MinimapUI minimapUI;
        private MapPreferences mapPreferences;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            mapPreferences = new MapPreferences();
            mapPreferences.loadPreferences();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (sceneName == "Main")
            {
                MelonLogger.Msg("GameplayScene loaded, initializing minimap...");
                minimapUI = new MinimapUI(mapPreferences);
                minimapUI.Initialize();
            }
            else
            {
                minimapUI?.Dispose();
                minimapUI = null;
            }
        }
    }
}
using MelonLoader;

[assembly: MelonInfo(typeof(Small_Corner_Map.Core), "Small Corner Map", "1.1.0", "winzaar", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

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
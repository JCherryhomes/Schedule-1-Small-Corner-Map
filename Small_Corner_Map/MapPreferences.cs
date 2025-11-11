using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small_Corner_Map
{
    public class MapPreferences
    {
        private const string CATEGORY_IDENTIFIER = "SmallCornerMapSettings";
        private const string MINIMAP_ENABLED_KEY = "MinimapEnabled";
        private const string DOUBLE_SIZE_MINIMAP_KEY = "DoubleSizeMinimap";
        private const string SHOW_GAME_TIME_KEY = "ShowGameTime";
        private const string MINIMAP_ENABLED_DISPLAY_NAME = "Enable Minimap";
        private const string DOUBLE_SIZE_MINIMAP_DISPLAY_NAME = "Double Size Minimap";
        private const string SHOW_GAME_TIME_DISPLAY_NAME = "Show Current Game Time";
        private const string CATEGORY_DISPLAY_NAME = "Small Corner Map Settings";
        private const bool MINIMAP_ENABLED_DEFAULT = true;
        private const bool DOUBLE_SIZE_MINIMAP_DEFAULT = false;
        private const bool SHOW_GAME_TIME_DEFAULT = true;

        private MelonPreferences_Category settingsCategory { get; set; }
        public MelonPreferences_Entry<bool> minimapEnabled { get; private set; }
        public MelonPreferences_Entry<bool> doubleSizeMinimap { get; private set; }
        public MelonPreferences_Entry<bool> showGameTime { get; private set; }

        public Action OnChange { get; set; }

        public void loadPreferences()
        {
            if (!MelonPreferences.HasEntry(CATEGORY_IDENTIFIER, MINIMAP_ENABLED_KEY))
            {
                MelonLogger.Msg("MapPreferences: No existing preferences found, creating default entries.");
                createDefaultEntries();

            }
            else
            {
                settingsCategory = MelonPreferences.GetCategory(CATEGORY_IDENTIFIER);
                minimapEnabled = MelonPreferences.GetEntry<bool>(CATEGORY_IDENTIFIER, MINIMAP_ENABLED_KEY);
                doubleSizeMinimap = MelonPreferences.GetEntry<bool>(CATEGORY_IDENTIFIER, DOUBLE_SIZE_MINIMAP_KEY);
                showGameTime = MelonPreferences.GetEntry<bool>(CATEGORY_IDENTIFIER, SHOW_GAME_TIME_KEY);
                MelonLogger.Msg("MapPreferences: Loaded existing preferences.");
            }
        }

        public void createDefaultEntries()
        {
            settingsCategory = MelonPreferences.CreateCategory(CATEGORY_IDENTIFIER, CATEGORY_DISPLAY_NAME);

            minimapEnabled = settingsCategory.CreateEntry(
                MINIMAP_ENABLED_KEY, 
                MINIMAP_ENABLED_DEFAULT, 
                MINIMAP_ENABLED_DISPLAY_NAME);

            doubleSizeMinimap = settingsCategory.CreateEntry(
                DOUBLE_SIZE_MINIMAP_KEY, 
                DOUBLE_SIZE_MINIMAP_DEFAULT, 
                DOUBLE_SIZE_MINIMAP_DISPLAY_NAME);

            showGameTime = settingsCategory.CreateEntry(
                SHOW_GAME_TIME_KEY, 
                SHOW_GAME_TIME_DEFAULT, 
                SHOW_GAME_TIME_DISPLAY_NAME);
        }
    }
}

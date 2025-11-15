using MelonLoader;

namespace Small_Corner_Map.Helpers  
{
    public class MapPreferences
    {
        // Preference Category and Keys
        private const string CategoryIdentifier = "SmallCornerMapSettings";
        private const string CategoryDisplayName = "Small Corner Map Settings";
        
        // Preference Keys, Display Names, and Default Values
        private const string MinimapEnabledKey = "MinimapEnabled";
        private const string MinimapEnabledDisplayName = "Enable Minimap";
        private const bool MinimapEnabledDefault = true;
        
        private const string ShowGameTimeKey = "ShowGameTime";
        private const string ShowGameTimeDisplayName = "Show Current Game Time";
        private const bool ShowGameTimeDefault = true;
        
        private const string IncreaseSizeKey = "IncreaseMapSize";
        private const string IncreaseSizeDisplayName = "Increase Minimap Size";
        private const bool IncreaseSizeDefault = false;

        // Preference Entries
        private MelonPreferences_Category SettingsCategory { get; set; }
        public MelonPreferences_Entry<bool> MinimapEnabled { get; private set; }
        public MelonPreferences_Entry<bool> ShowGameTime { get; private set; }
        public MelonPreferences_Entry<bool> IncreaseSize { get; private set; }
        
        private readonly float defaultScaleFactor = 1.0f;
        private readonly float increasedScaleFactor = 1.5f;

        public float MinimapScaleFactor => (IncreaseSize.Value ? increasedScaleFactor : defaultScaleFactor);

        public void LoadPreferences()
        {
            if (!MelonPreferences.HasEntry(CategoryIdentifier, MinimapEnabledKey))
            {
                MelonLogger.Msg("MapPreferences: No existing preferences found, creating default entries.");
                CreateDefaultEntries();

            }
            else
            {
                SettingsCategory = MelonPreferences.GetCategory(CategoryIdentifier);
                MinimapEnabled = MelonPreferences.GetEntry<bool>(CategoryIdentifier, MinimapEnabledKey);
                ShowGameTime = MelonPreferences.GetEntry<bool>(CategoryIdentifier, ShowGameTimeKey);
                IncreaseSize = MelonPreferences.GetEntry<bool>(CategoryIdentifier, IncreaseSizeKey);
                MelonLogger.Msg("MapPreferences: Loaded existing preferences.");
            }
        }

        private void CreateDefaultEntries()
        {
            SettingsCategory = MelonPreferences.CreateCategory(CategoryIdentifier, CategoryDisplayName);

            MinimapEnabled = SettingsCategory.CreateEntry(
                MinimapEnabledKey, 
                MinimapEnabledDefault, 
                MinimapEnabledDisplayName);

            ShowGameTime = SettingsCategory.CreateEntry(
                ShowGameTimeKey, 
                ShowGameTimeDefault, 
                ShowGameTimeDisplayName);
            
            IncreaseSize = SettingsCategory.CreateEntry(
                IncreaseSizeKey, 
                IncreaseSizeDefault, 
                IncreaseSizeDisplayName);
        }
    }
}

using MelonLoader;
using UnityEngine;

namespace Small_Corner_Map.Helpers  
{
    public class MapPreferences
    {
        // Preference Category and Keys
        private const string CategoryDisplayName = "Small Corner Map Settings";
        
        // Preference Keys, Display Names, and Default Values
        private const string MinimapEnabledKey = "01_MinimapEnabled";
        private const string MinimapEnabledDisplayName = "Enable Minimap";
        private const bool MinimapEnabledDefault = true;
        
        private const string ShowGameTimeKey = "03_ShowGameTime";
        private const string ShowGameTimeDisplayName = "Show Current Game Time";
        private const bool ShowGameTimeDefault = true;
        
        private const string IncreaseSizeKey = "02_IncreaseMapSize";
        private const string IncreaseSizeDisplayName = "Increase Minimap Size";
        private const bool IncreaseSizeDefault = false;

        private const string ContractTrackingKey = "04_TrackContracts";
        private const string ContractTrackingDisplayName = "Track Active Contracts on Minimap";
        private const bool ContractTrackingDefault = true;
        
        private const string PropertyTrackingKey = "05_TrackProperties";
        private const string PropertyTrackingDisplayName = "Track Owned Properties on Minimap";
        private const bool PropertyTrackingDefault = true;
        
        private const string VehicleTrackingKey = "06_TrackVehicles";
        private const string VehicleTrackingDisplayName = "Track Owned Vehicles on Minimap";
        private const bool VehicleTrackingDefault = true;
        
        private const string MinimapPositionXKey = "07_MinimapPositionX";
        private const string MinimapPositionXDisplayName = "Change Minimap Position: X Coordinate";
        private const float MinimapPositionXDefault = -20f;
        
        private const string MinimapPositionYKey = "08_MinimapPositionY";
        private const string MinimapPositionYDisplayName = "Change Minimap Position: Y Coordinate";
        private const float MinimapPositionYDefault = -20f;

        
        // Preference Entries
        public MelonPreferences_Category SettingsCategory { get; set; }
        public MelonPreferences_Entry<bool> MinimapEnabled { get; private set; }
        public MelonPreferences_Entry<bool> ShowGameTime { get; private set; }
        public MelonPreferences_Entry<bool> IncreaseSize { get; private set; }
        public MelonPreferences_Entry<bool> TrackContracts { get; private set; }
        public MelonPreferences_Entry<bool> TrackProperties { get; private set; }
        public MelonPreferences_Entry<bool> TrackVehicles { get; private set; }
        public MelonPreferences_Entry<float> MinimapPositionX { get; private set; }
        public MelonPreferences_Entry<float> MinimapPositionY { get; private set; }
        public MelonPreferences_Entry<bool> ShowSquareMinimap { get; private set; }
        
        private readonly float defaultScaleFactor = 1.0f;
        private readonly float increasedScaleFactor = 1.5f;

        public float MinimapScaleFactor => (IncreaseSize.Value ? increasedScaleFactor : defaultScaleFactor);

        public void LoadPreferences()
        {
            // Always create default entries first to ensure they exist for retrieval
            CreateDefaultEntries();
            
            SettingsCategory = MelonPreferences.GetCategory(Constants.MapPreferencesCategoryIdentifier);
            MinimapEnabled = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, MinimapEnabledKey);
            ShowGameTime = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, ShowGameTimeKey);
            IncreaseSize = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, IncreaseSizeKey);
            
            TrackContracts = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, ContractTrackingKey);
            TrackProperties = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, PropertyTrackingKey);
            TrackVehicles = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, VehicleTrackingKey);
            ShowSquareMinimap = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, "ShowSquareMinimap");
            MinimapPositionX = MelonPreferences.GetEntry<float>(Constants.MapPreferencesCategoryIdentifier, MinimapPositionXKey);
            MinimapPositionY = MelonPreferences.GetEntry<float>(Constants.MapPreferencesCategoryIdentifier, MinimapPositionYKey);
        }

        private void CreateDefaultEntries()
        {
            if (MelonPreferences.GetCategory(Constants.MapPreferencesCategoryIdentifier) != null)
            {
                // Category already exists, no need to recreate entries
                return;
            }
            
            SettingsCategory = MelonPreferences.CreateCategory(Constants.MapPreferencesCategoryIdentifier, CategoryDisplayName);

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

            TrackContracts = SettingsCategory.CreateEntry(
                ContractTrackingKey, 
                ContractTrackingDefault, 
                ContractTrackingDisplayName);
            
            TrackProperties = SettingsCategory.CreateEntry(
                PropertyTrackingKey, 
                PropertyTrackingDefault, 
                PropertyTrackingDisplayName);
            
            TrackVehicles = SettingsCategory.CreateEntry(
                VehicleTrackingKey, 
                VehicleTrackingDefault, 
                VehicleTrackingDisplayName);

            ShowSquareMinimap = SettingsCategory.CreateEntry(
                "ShowSquareMinimap",
                false,
                "Enable Square Minimap");
        
            MinimapPositionX = SettingsCategory.CreateEntry(
                MinimapPositionXKey,
                MinimapPositionXDefault,
                MinimapPositionXDisplayName);
            
            MinimapPositionY = SettingsCategory.CreateEntry(
                MinimapPositionYKey,
                MinimapPositionYDefault,
                MinimapPositionYDisplayName);
        }
    }
}
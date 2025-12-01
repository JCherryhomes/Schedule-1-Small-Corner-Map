using MelonLoader;
using UnityEngine;

namespace Small_Corner_Map.Helpers  
{
    public class MapPreferences
    {
        // Preference Category and Keys
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

        // Advanced tuning preferences (always defined with defaults if not loaded from file)
        private const string MapZoomLevelKey = "MapZoomLevel";
        private const string MapZoomLevelDisplayName = "Map Movement Scale (Zoom)";
        
        private const string MinimapPlayerOffsetXKey = "MinimapPlayerOffsetX";
        private const string MinimapPlayerOffsetXDisplayName = "Minimap Player X Offset";
        
        private const string MinimapPlayerOffsetYKey = "MinimapPlayerOffsetY";
        private const string MinimapPlayerOffsetYDisplayName = "Minimap Player Y Offset";

        private const string ContractTrackingKey = "TrackContracts";
        private const string ContractTrackingDisplayName = "Track Active Contracts on Minimap";
        private const bool ContractTrackingDefault = true;
        
        private const string PropertyTrackingKey = "TrackProperties";
        private const string PropertyTrackingDisplayName = "Track Owned Properties on Minimap";
        private const bool PropertyTrackingDefault = true;
        
        private const string VehicleTrackingKey = "TrackVehicles";
        private const string VehicleTrackingDisplayName = "Track Owned Vehicles on Minimap";
        private const bool VehicleTrackingDefault = true;
        
        private const string ShowCompassKey = Constants.CompassPreferenceKey;
        private const string ShowCompassDisplayName = "Show Compass Ring";
        private const bool ShowCompassDefault = true;
        
        // Preference Entries
        public MelonPreferences_Category SettingsCategory { get; set; }
        public MelonPreferences_Entry<bool> MinimapEnabled { get; private set; }
        public MelonPreferences_Entry<bool> ShowGameTime { get; private set; }
        public MelonPreferences_Entry<bool> IncreaseSize { get; private set; }
        public MelonPreferences_Entry<float> MapZoomLevel { get; private set; }
        public MelonPreferences_Entry<float> MinimapPlayerOffsetX { get; private set; }
        public MelonPreferences_Entry<float> MinimapPlayerOffsetY { get; private set; }
        public MelonPreferences_Entry<bool> TrackContracts { get; private set; }
        public MelonPreferences_Entry<bool> TrackProperties { get; private set; }
        public MelonPreferences_Entry<bool> TrackVehicles { get; private set; }
        public MelonPreferences_Entry<bool> ShowCompass { get; private set; }
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
            
            // These preferences are always loaded/created, but only become visible/tunable if EnableAdvancedMinimapTuning is true.
            MapZoomLevel = MelonPreferences.GetEntry<float>(Constants.MapPreferencesCategoryIdentifier, MapZoomLevelKey);
            MinimapPlayerOffsetX = MelonPreferences.GetEntry<float>(Constants.MapPreferencesCategoryIdentifier, MinimapPlayerOffsetXKey);
            MinimapPlayerOffsetY = MelonPreferences.GetEntry<float>(Constants.MapPreferencesCategoryIdentifier, MinimapPlayerOffsetYKey);
            
            TrackContracts = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, ContractTrackingKey);
            TrackProperties = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, PropertyTrackingKey);
            TrackVehicles = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, VehicleTrackingKey);
            ShowCompass = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, ShowCompassKey);
            ShowSquareMinimap = MelonPreferences.GetEntry<bool>(Constants.MapPreferencesCategoryIdentifier, "ShowSquareMinimap");
        }

        private void CreateDefaultEntries()
        {
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
            
            ShowCompass = SettingsCategory.CreateEntry(
                ShowCompassKey,
                ShowCompassDefault,
                ShowCompassDisplayName);

            ShowSquareMinimap = SettingsCategory.CreateEntry(
                "ShowSquareMinimap",
                false,
                "Enable Square Minimap");

            // These tuning entries are always created as preferences, but their visibility in UI is tied to DisableDebugging.
            // Their default values come from Constants.cs.
            MapZoomLevel = SettingsCategory.CreateEntry<float>(
                MapZoomLevelKey,
                Constants.MinimapDefaultMapMovementScale,
                MapZoomLevelDisplayName);
            
            MapZoomLevel.IsHidden = Constants.DisableDebug;
            
            MinimapPlayerOffsetX = SettingsCategory.CreateEntry(
                MinimapPlayerOffsetXKey,
                Constants.MinimapDefaultPlayerOffsetX,
                MinimapPlayerOffsetXDisplayName);
            
            MinimapPlayerOffsetX.IsHidden = Constants.DisableDebug;
            
            MinimapPlayerOffsetY = SettingsCategory.CreateEntry(
                MinimapPlayerOffsetYKey,
                Constants.MinimapDefaultPlayerOffsetY,
                MinimapPlayerOffsetYDisplayName);
            
            MinimapPlayerOffsetY.IsHidden = Constants.DisableDebug;
        }
    }
}
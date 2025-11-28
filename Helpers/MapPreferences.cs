using MelonLoader;
using UnityEngine;

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
        private MelonPreferences_Category SettingsCategory { get; set; }
        public MelonPreferences_Entry<bool> MinimapEnabled { get; private set; }
        public MelonPreferences_Entry<bool> ShowGameTime { get; private set; }
        public MelonPreferences_Entry<bool> IncreaseSize { get; private set; }
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
            if (!MelonPreferences.HasEntry(CategoryIdentifier, MinimapEnabledKey))
            {
                CreateDefaultEntries();
            }
            else
            {
                SettingsCategory = MelonPreferences.GetCategory(CategoryIdentifier);
                MinimapEnabled = MelonPreferences.GetEntry<bool>(CategoryIdentifier, MinimapEnabledKey);
                ShowGameTime = MelonPreferences.GetEntry<bool>(CategoryIdentifier, ShowGameTimeKey);
                IncreaseSize = MelonPreferences.GetEntry<bool>(CategoryIdentifier, IncreaseSizeKey);
                TrackContracts = MelonPreferences.GetEntry<bool>(CategoryIdentifier, ContractTrackingKey);
                TrackProperties = MelonPreferences.GetEntry<bool>(CategoryIdentifier, PropertyTrackingKey);
                TrackVehicles = MelonPreferences.GetEntry<bool>(CategoryIdentifier, VehicleTrackingKey);
                ShowCompass = MelonPreferences.GetEntry<bool>(CategoryIdentifier, ShowCompassKey);
                ShowSquareMinimap = MelonPreferences.GetEntry<bool>(CategoryIdentifier, "ShowSquareMinimap");
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
        }
    }
}

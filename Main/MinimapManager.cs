using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;


#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
#else
using ScheduleOne.PlayerScripts;
using Il2CppIEnumerator = System.Collections.IEnumerator;
#endif

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class MinimapManager : MonoBehaviour
    {
        private static MinimapManager _instance;
        public static MinimapManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("MinimapManager").AddComponent<MinimapManager>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        private MinimapView _minimapView;
        private MapPreferences _mapPreferences;

        public void Initialize()
        {
            MelonLogger.Msg("MinimapManager initializing.");
            
            _mapPreferences = new MapPreferences();
            _mapPreferences.LoadPreferences();

            // Create the MinimapView
            _minimapView = new GameObject("MinimapView").AddComponent<MinimapView>();
            _minimapView.transform.SetParent(transform);

            // Start a coroutine to wait for the player object
            MelonCoroutines.Start(InitializeWhenReady());
        }

        private System.Collections.IEnumerator InitializeWhenReady()
        {
            // Wait for Player.Local to be available
            while (Player.Local == null)
            {
                yield return null;
            }

            MelonLogger.Msg("Player object found, initializing MinimapView.");

            _minimapView.Initialize(
                Player.Local, 
                _mapPreferences.MinimapEnabled.Value, 
                _mapPreferences.MinimapScaleFactor, 
                _mapPreferences.ShowSquareMinimap.Value, 
                _mapPreferences.ShowGameTime.Value,
                Constants.BaseWorldToUIScaleFactor,
                _mapPreferences.MapZoomLevel.Value,
                _mapPreferences.MinimapPlayerOffsetX.Value,
                _mapPreferences.MinimapPlayerOffsetY.Value,
                _mapPreferences.TrackProperties.Value,
                _mapPreferences.TrackContracts.Value,
                _mapPreferences.TrackVehicles.Value
            );
            
            // Subscribe to preference changes
            _mapPreferences.MinimapEnabled.OnEntryValueChanged.Subscribe(OnMinimapEnabledChanged);
            _mapPreferences.IncreaseSize.OnEntryValueChanged.Subscribe(OnIncreaseSizeChanged);
            _mapPreferences.ShowSquareMinimap.OnEntryValueChanged.Subscribe(OnShowSquareMinimapChanged);
            
            // Only subscribe to tuning preference changes if advanced tuning is enabled
            // _mapPreferences.MapZoomLevel.OnEntryValueChanged.Subscribe(OnMapZoomLevelChanged);
            _mapPreferences.MinimapPlayerOffsetX.OnEntryValueChanged.Subscribe(OnMinimapPlayerOffsetXChanged);
            _mapPreferences.MinimapPlayerOffsetY.OnEntryValueChanged.Subscribe(OnMinimapPlayerOffsetYChanged);
            
            _mapPreferences.ShowGameTime.OnEntryValueChanged.Subscribe(OnShowGameTimeChanged);
            _mapPreferences.TrackProperties.OnEntryValueChanged.Subscribe(OnTrackPropertiesChanged);
            _mapPreferences.TrackContracts.OnEntryValueChanged.Subscribe(OnTrackContractsChanged);
            _mapPreferences.TrackVehicles.OnEntryValueChanged.Subscribe(OnTrackVehiclesChanged);
        }

        private void OnDestroy()
        {
            // Unsubscribe from preference changes
            if (_mapPreferences != null)
            {
                _mapPreferences.MinimapEnabled.OnEntryValueChanged.Unsubscribe(OnMinimapEnabledChanged);
                _mapPreferences.IncreaseSize.OnEntryValueChanged.Unsubscribe(OnIncreaseSizeChanged);
                _mapPreferences.ShowSquareMinimap.OnEntryValueChanged.Unsubscribe(OnShowSquareMinimapChanged);
                
                // Only unsubscribe tuning preference changes if they were subscribed
                // _mapPreferences.MapZoomLevel.OnEntryValueChanged.Unsubscribe(OnMapZoomLevelChanged);
                _mapPreferences.MinimapPlayerOffsetX.OnEntryValueChanged.Unsubscribe(OnMinimapPlayerOffsetXChanged);
                _mapPreferences.MinimapPlayerOffsetY.OnEntryValueChanged.Unsubscribe(OnMinimapPlayerOffsetYChanged);

                _mapPreferences.ShowGameTime.OnEntryValueChanged.Unsubscribe(OnShowGameTimeChanged);
                _mapPreferences.TrackProperties.OnEntryValueChanged.Unsubscribe(OnTrackPropertiesChanged);
                _mapPreferences.TrackContracts.OnEntryValueChanged.Unsubscribe(OnTrackContractsChanged);
                _mapPreferences.TrackVehicles.OnEntryValueChanged.Unsubscribe(OnTrackVehiclesChanged);
            }
        }

        private void OnMinimapEnabledChanged(bool oldValue, bool newValue)
        {
            _minimapView.ToggleMinimapVisibility(newValue);
        }

        private void OnIncreaseSizeChanged(bool oldValue, bool newValue)
        {
            // Recalculate scale factor and update view
            _minimapView.UpdateMinimapUISize(_mapPreferences.MinimapScaleFactor);
        }

        private void OnShowSquareMinimapChanged(bool oldValue, bool newValue)
        {
            _minimapView.SetStyle(!newValue); // true for circle, false for square
        }

        // private void OnMapZoomLevelChanged(float oldValue, float newValue)
        // {
        //     // This event handler should only be subscribed if tuning is enabled, so no need for an extra check here.
        //     _minimapView.UpdateMapMovementScale(newValue);
        // }

        private void OnMinimapPlayerOffsetXChanged(float oldValue, float newValue)
        {
            // This event handler should only be subscribed if tuning is enabled, so no need for an extra check here.
            _minimapView.UpdateMinimapPlayerCenterXOffset(newValue);
        }

        private void OnMinimapPlayerOffsetYChanged(float oldValue, float newValue)
        {
            // This event handler should only be subscribed if tuning is enabled, so no need for an extra check here.
            _minimapView.UpdateMinimapPlayerCenterYOffset(newValue);
        }

        private void OnShowGameTimeChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdateTimeDisplayVisibility(newValue);
        }
        
        private void OnTrackPropertiesChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdatePropertyTracking(newValue);
        }
        
        private void OnTrackContractsChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdateContractTracking(newValue);
        }

        private void OnTrackVehiclesChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdateVehicleTracking(newValue);
        }
    }
}

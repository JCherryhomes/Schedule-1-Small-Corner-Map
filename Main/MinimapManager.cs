using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;
using System.Collections;
using System;


#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
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

        private IEnumerator InitializeWhenReady()
        {
            // Wait for Player.Local to be available
            while (Player.Local == null)
            {
                yield return null;
            }

            MelonLogger.Msg("Player object found, initializing MinimapView.");

            // Determine initial values for map movement scale and offsets
            float initialMapZoomLevel = Constants.MinimapDefaultMapMovementScale;
            float initialMinimapPlayerOffsetX = Constants.MinimapDefaultPlayerOffsetX;
            float initialMinimapPlayerOffsetY = Constants.MinimapDefaultPlayerOffsetY;

            if (_mapPreferences.EnableAdvancedMinimapTuning.Value)
            {
                initialMapZoomLevel = _mapPreferences.MapZoomLevel.Value;
                initialMinimapPlayerOffsetX = _mapPreferences.MinimapPlayerOffsetX.Value;
                initialMinimapPlayerOffsetY = _mapPreferences.MinimapPlayerOffsetY.Value;
            }

            _minimapView.Initialize(
                Player.Local, 
                _mapPreferences.MinimapEnabled.Value, 
                _mapPreferences.MinimapScaleFactor, 
                _mapPreferences.ShowSquareMinimap.Value, 
                _mapPreferences.ShowGameTime,
                Constants.BaseWorldToUIScaleFactor, // Use the renamed constant
                initialMapZoomLevel, 
                initialMinimapPlayerOffsetX,
                initialMinimapPlayerOffsetY
            );
            
            // Subscribe to preference changes
            _mapPreferences.MinimapEnabled.OnEntryValueChanged.Subscribe(OnMinimapEnabledChanged);
            _mapPreferences.IncreaseSize.OnEntryValueChanged.Subscribe(OnIncreaseSizeChanged);
            _mapPreferences.ShowSquareMinimap.OnEntryValueChanged.Subscribe(OnShowSquareMinimapChanged);
            _mapPreferences.MapZoomLevel.OnEntryValueChanged.Subscribe(OnMapZoomLevelChanged);
            _mapPreferences.MinimapPlayerOffsetX.OnEntryValueChanged.Subscribe(OnMinimapPlayerOffsetXChanged);
            _mapPreferences.MinimapPlayerOffsetY.OnEntryValueChanged.Subscribe(OnMinimapPlayerOffsetYChanged);
            _mapPreferences.ShowGameTime.OnEntryValueChanged.Subscribe(OnShowGameTimeChanged);
            _mapPreferences.ShowCompass.OnEntryValueChanged.Subscribe(OnShowCompassChanged);
            _mapPreferences.EnableAdvancedMinimapTuning.OnEntryValueChanged.Subscribe(OnEnableAdvancedMinimapTuningChanged);
        }

        private void OnDestroy()
        {
            // Unsubscribe from preference changes
            if (_mapPreferences != null)
            {
                _mapPreferences.MinimapEnabled.OnEntryValueChanged.Unsubscribe(OnMinimapEnabledChanged);
                _mapPreferences.IncreaseSize.OnEntryValueChanged.Unsubscribe(OnIncreaseSizeChanged);
                _mapPreferences.ShowSquareMinimap.OnEntryValueChanged.Unsubscribe(OnShowSquareMinimapChanged);
                _mapPreferences.MapZoomLevel.OnEntryValueChanged.Unsubscribe(OnMapZoomLevelChanged);
                _mapPreferences.MinimapPlayerOffsetX.OnEntryValueChanged.Unsubscribe(OnMinimapPlayerOffsetXChanged);
                _mapPreferences.MinimapPlayerOffsetY.OnEntryValueChanged.Unsubscribe(OnMinimapPlayerOffsetYChanged);
                _mapPreferences.ShowGameTime.OnEntryValueChanged.Unsubscribe(OnShowGameTimeChanged);
                _mapPreferences.ShowCompass.OnEntryValueChanged.Unsubscribe(OnShowCompassChanged);
                _mapPreferences.EnableAdvancedMinimapTuning.OnEntryValueChanged.Unsubscribe(OnEnableAdvancedMinimapTuningChanged);
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

        private void OnMapZoomLevelChanged(float oldValue, float newValue)
        {
            // Only update if advanced tuning is enabled
            if (_mapPreferences.EnableAdvancedMinimapTuning.Value)
            {
                _minimapView.UpdateMapMovementScale(newValue);
            }
        }

        private void OnMinimapPlayerOffsetXChanged(float oldValue, float newValue)
        {
            // Only update if advanced tuning is enabled
            if (_mapPreferences.EnableAdvancedMinimapTuning.Value)
            {
                _minimapView.UpdateMinimapPlayerCenterXOffset(newValue);
            }
        }

        private void OnMinimapPlayerOffsetYChanged(float oldValue, float newValue)
        {
            // Only update if advanced tuning is enabled
            if (_mapPreferences.EnableAdvancedMinimapTuning.Value)
            {
                _minimapView.UpdateMinimapPlayerCenterYOffset(newValue);
            }
        }
        
        private void OnEnableAdvancedMinimapTuningChanged(bool oldValue, bool newValue)
        {
            // When this preference changes, update all related settings to either defaults or preference values
            float mapZoomLevel = newValue ? _mapPreferences.MapZoomLevel.Value : Constants.MinimapDefaultMapMovementScale;
            float minimapPlayerOffsetX = newValue ? _mapPreferences.MinimapPlayerOffsetX.Value : Constants.MinimapDefaultPlayerOffsetX;
            float minimapPlayerOffsetY = newValue ? _mapPreferences.MinimapPlayerOffsetY.Value : Constants.MinimapDefaultPlayerOffsetY;

            _minimapView.UpdateMapMovementScale(mapZoomLevel);
            _minimapView.UpdateMinimapPlayerCenterXOffset(minimapPlayerOffsetX);
            _minimapView.UpdateMinimapPlayerCenterYOffset(minimapPlayerOffsetY);
        }

        private void OnShowGameTimeChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdateTimeDisplayVisibility(newValue);
        }

        private void OnShowCompassChanged(bool oldValue, bool newValue)
        {
            _minimapView.UpdateCompassVisibility(newValue);
        }
    }
}


using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

using Small_Corner_Map.Helpers;
using Small_Corner_Map.UI;
using System.Collections;
using Small_Corner_Map.PoIManagers;

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class MinimapCoordinator : MonoBehaviour
    {
        private PlayerMarkerManager _playerMarkerManager;

        void Start()
        {
            MelonLogger.Msg("MinimapCoordinator started.");
            var mapPreferences = new MapPreferences();
            _playerMarkerManager = new PlayerMarkerManager();
            mapPreferences.LoadPreferences();

            MelonLogger.Msg("Preferences loaded. Minimap Enabled: " + mapPreferences.MinimapEnabled.Value);
            if (!mapPreferences.MinimapEnabled.Value)
            {
                return; // Minimap is disabled, do nothing
            }

            var minimapContainer = new GameObject("MinimapContainer");
            
            var builder = minimapContainer.AddComponent<UIBuilder>();
            
            var minimapSize = Constants.BaseMinimapSize * mapPreferences.MinimapScaleFactor;
            builder.InitializeMinimapUI(mapPreferences.ShowSquareMinimap.Value, minimapSize);

            var contentManager = builder.ContentManager;
            if (contentManager != null)
            {
                MelonCoroutines.Start(contentManager.Initialize(builder.MinimapContent.GetComponent<RectTransform>(), Player.Local.transform));
            }
            
            MelonLogger.Msg("MinimapUI: UIBuilder initialized");

            // Create the player marker, centering it within the minimap's root (mask)
            _playerMarkerManager.CreatePlayerMarker(builder.MinimapRoot);
            
            MelonLogger.Msg("MinimapCoordinator: Player marker created.");

            MelonCoroutines.Start(builder.IntegrateWithScene());
            MelonCoroutines.Start(_playerMarkerManager.InitializePlayerMarkerIcon(builder.CachedMapContent));
            
            MelonLogger.Msg("MinimapCoordinator: Coroutines started.");
        }
    }
}

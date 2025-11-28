using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

using Small_Corner_Map.Helpers;
using Small_Corner_Map.UI;
using System.Collections;

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


            var minimapCanvas = MinimapUIFactory.CreateCanvas(minimapContainer);
            var minimapSize = Constants.BaseMinimapSize * mapPreferences.MinimapScaleFactor;
            var (frameObject, _) = MinimapUIFactory.CreateFrame(minimapCanvas, minimapSize);

            var builder = new UIBuilder();
            builder
                .SetParentContainer(frameObject)
                .InitializeMinimapUI(mapPreferences.ShowSquareMinimap.Value);

            MelonLogger.Msg("MinimapUI: UIBuilder initialized");

            MelonCoroutines.Start(builder.IntegrateWithScene());
            MelonCoroutines.Start(_playerMarkerManager.InitializePlayerMarkerIcon());
        }

        void Dispose()
        {
            var minimapObject = GameObject.Find("MinimapContainer");
            minimapObject = null;
        }
    }
}

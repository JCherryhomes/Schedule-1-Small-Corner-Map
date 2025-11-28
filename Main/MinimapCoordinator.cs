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
            // The UIBuilder will now create its own canvas and frame.
            // MinimapContainer will be the parent of the UIBuilder's internally created canvas.

            // Correctly instantiate UIBuilder as a MonoBehaviour
            var uiBuilderGameObject = new GameObject("MinimapUIBuilder"); // GameObject for the UIBuilder component
            uiBuilderGameObject.transform.SetParent(minimapContainer.transform); // Parent it to the main container
            var builder = uiBuilderGameObject.AddComponent<UIBuilder>();
            
            // Set minimapContainer as the parent for UIBuilder's internal canvas
            builder.SetParentContainer(minimapContainer);

            builder.InitializeMinimapUI(mapPreferences.ShowSquareMinimap.Value);

            MelonLogger.Msg("MinimapUI: UIBuilder initialized");

            // Create the player marker, centering it within the minimap's root (mask)
            _playerMarkerManager.CreatePlayerMarker(builder.MinimapRoot);

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

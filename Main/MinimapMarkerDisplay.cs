using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    /// <summary>
    /// Handles the creation and visual display of markers on the minimap.
    /// Subscribes to MarkerRegistry events to instantiate/remove marker UI elements.
    /// </summary>
    internal class MinimapMarkerDisplay
    {
        private readonly MarkerRegistry markerRegistry;
        private readonly Transform minimapParent;
        private readonly MinimapContent minimapContent;
        private bool subscribed;

        public MinimapMarkerDisplay(
            MarkerRegistry registry,
            Transform minimapParentTransform,
            MinimapContent content)
        {
            markerRegistry = registry;
            minimapParent = minimapParentTransform;
            minimapContent = content;
        }

        public void Subscribe()
        {
            if (subscribed) return;
            markerRegistry.MarkerAdded += OnMarkerAdded;
            markerRegistry.MarkerRemoved += OnMarkerRemoved;
            markerRegistry.MarkerUpdated += OnMarkerUpdated;
            subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!subscribed) return;
            markerRegistry.MarkerAdded -= OnMarkerAdded;
            markerRegistry.MarkerRemoved -= OnMarkerRemoved;
            markerRegistry.MarkerUpdated -= OnMarkerUpdated;
            subscribed = false;
        }

        private void OnMarkerAdded(MarkerRegistry.MarkerData data)
        {
            if (!data.IsVisibleOnMinimap) return;
            CreateMarkerUI(data);
        }

        private void OnMarkerUpdated(MarkerRegistry.MarkerData data)
        {
            if (!data.IsVisibleOnMinimap) return;
            // Find existing marker and update position
            var existing = minimapParent.Find(data.Id);
            if (existing != null)
            {
                UpdateMarkerPosition(data, existing.GetComponent<RectTransform>());
            }
            else
            {
                // Marker doesn't exist yet, create it
                CreateMarkerUI(data);
            }
        }

        private void OnMarkerRemoved(string markerId)
        {
            var markerTransform = minimapParent.Find(markerId);
            if (markerTransform != null)
            {
                UnityEngine.Object.Destroy(markerTransform.gameObject);
            }
        }

        private void CreateMarkerUI(MarkerRegistry.MarkerData data)
        {
            var iconPrefab = data.IconPrefab;
            if (iconPrefab == null)
            {
                MelonLogger.Warning($"[MinimapMarkerDisplay] IconPrefab is null for marker {data.Id}");
                return;
            }

            try
            {
                var markerObject = UnityEngine.Object.Instantiate(iconPrefab, minimapParent, false);
                markerObject.name = data.Id;
                var rect = markerObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    UpdateMarkerPosition(data, rect);
                }
                else
                {
                    MelonLogger.Warning($"[MinimapMarkerDisplay] Marker {data.Id} has no RectTransform");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[MinimapMarkerDisplay] Error creating marker {data.Id}: {ex.Message}");
            }
        }

        private void UpdateMarkerPosition(MarkerRegistry.MarkerData data, RectTransform markerRect)
        {
            if (minimapParent == null || markerRect == null) return;

            var minimapScale = minimapContent.CurrentMapScale;
            var minimapRadius = Constants.BaseMinimapSize / 2f;
            var playerPos = Vector3.zero; // Player is at center of minimap

            MarkerRegistry.UpdateMarkerPosition(
                data,
                markerRect,
                minimapScale,
                minimapRadius,
                minimapRadius,
                playerPos,
                minimapParent);
        }
    }
}

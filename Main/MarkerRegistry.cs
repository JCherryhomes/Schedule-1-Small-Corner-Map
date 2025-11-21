using System;
using System.Collections.Generic;
using UnityEngine;
using Small_Corner_Map.Helpers;
using MelonLoader;

namespace Small_Corner_Map.Main
{
    public class MarkerRegistry
    {
        public class MarkerData
        {
            public string Id;
            public Vector3 WorldPos;
            public GameObject IconPrefab;
            public Sprite Sprite; // Optional, for UI-only markers
            public MarkerType Type;
            public string DisplayName;
            public Color? Color; // Nullable, use if you want to override default color
            public bool IsTracked;
            public float XOffset;
            public float ZOffset;
            public bool IsVisibleOnMinimap = true;
            public bool IsVisibleOnCompass = true;
        }

        private readonly Dictionary<string, MarkerData> markers = new();
        public event Action<MarkerData> MarkerAdded;
        public event Action<string> MarkerRemoved;
        public event Action<MarkerData> MarkerUpdated;

        public void AddOrUpdateMarker(MarkerData data)
        {
            markers[data.Id] = data;
            MarkerAdded?.Invoke(data);
        }

        public void RemoveMarker(string id)
        {
            if (markers.Remove(id))
                MarkerRemoved?.Invoke(id);
        }

        public IEnumerable<MarkerData> GetAllMarkers() => markers.Values;
        public MarkerData GetMarker(string id) => markers.TryGetValue(id, out var data) ? data : null;
        
        public static void UpdateMarkerPosition(MarkerData data, RectTransform markerRect, float minimapScale, float minimapRadius, float compassRadius, Vector3 playerPos, Transform minimapParent)
        {
            // Ensure marker is parented to minimap content (only needs to happen once)
            if (markerRect.parent != minimapParent)
            {
                MelonLoader.MelonLogger.Msg($"[UpdateMarkerPosition] Reparenting {data.Id} to minimap");
                markerRect.SetParent(minimapParent, false);
                // Ensure RectTransform is set up correctly
                markerRect.anchorMin = Vector2.one * 0.5f;
                markerRect.anchorMax = Vector2.one * 0.5f;
                markerRect.pivot = Vector2.one * 0.5f;
                
                // Disable mask interaction on all Image components so marker can render outside minimap boundary
                var image = markerRect.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.maskable = false;
                }
                // Check all child images too
                foreach (var childImage in markerRect.GetComponentsInChildren<UnityEngine.UI.Image>())
                {
                    childImage.maskable = false;
                }
            }

            // Always apply size and scale to ensure consistency (handles property markers with pre-existing sizes)
            // For property markers with complex internal structures, use localScale to resize everything uniformly
            if (data.Id.Contains("Contract_Marker") || data.Id.Contains("Regular_Quest") || data.Id.Contains("DeadDrop_Marker"))
            {
                markerRect.localScale = Vector3.one * .33f;
            }
            else
            {
                markerRect.localScale = Vector3.one * .75f;
            }

            Vector2 playerMarkerUIPos = new Vector2(
                (playerPos.x * minimapScale) + Constants.MinimapImageOffsetX,
                (playerPos.z * minimapScale) + Constants.MinimapImageOffsetY
            );

            // 2. Vector from player marker to marker in minimap space
            Vector2 playerToMarker = new Vector2(
                ((data.WorldPos.x - playerPos.x) * minimapScale) - Constants.MarkerXOffset,
                ((data.WorldPos.z - playerPos.z) * minimapScale) - Constants.MarkerZOffset
            );

            // 3. Clamp if needed (use the correct radius, e.g., compassCenterRadius)
            float clampRadius = compassRadius + 3f; // This should be the same as compassCenterRadius from CompassManager


            if (playerToMarker.magnitude >= compassRadius)
            {
                playerToMarker = playerToMarker.normalized * clampRadius;
                playerToMarker.x -= Constants.MarkerXOffset * 1.8f;
            }

            // 4. Final position is player marker UI position + (possibly clamped) vector
            Vector2 finalPos = playerMarkerUIPos + playerToMarker;

            markerRect.anchoredPosition = finalPos;
            markerRect.gameObject.SetActive(true);
            markerRect.SetAsLastSibling();
        }
    }

    public enum MarkerType
    {
        RegularQuest,
        DeadDrop,
        Contract,
        Property,
        Vehicle
    }
}

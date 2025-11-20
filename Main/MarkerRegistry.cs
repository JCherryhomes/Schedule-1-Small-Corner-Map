using System;
using System.Collections.Generic;
using UnityEngine;
using Small_Corner_Map.Helpers;

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
            markerRect.localScale = Vector3.one * .75f;
            
            // Calculate marker position relative to player (for distance check)
            var relativeToPlayer = new Vector2(
                (data.WorldPos.x + data.XOffset - playerPos.x) * minimapScale,
                (data.WorldPos.z + data.ZOffset - playerPos.z) * minimapScale
            );

            bool isWest = relativeToPlayer.x < 0;
            const float WEST_CLAMP_OFFSET = 25f; // Use ALL_CAPS for constants
            float clampThreshold = compassRadius + (isWest ? WEST_CLAMP_OFFSET : -WEST_CLAMP_OFFSET);
            var uiDistance = relativeToPlayer.magnitude;
            
            Vector2 finalPos;
            if (uiDistance <= clampThreshold)
            {
            
                // Calculate absolute world position (minimap content at -playerPos * scale will offset this correctly)
                var absoluteWorldPos = new Vector2(
                    (data.WorldPos.x + data.XOffset) * minimapScale + 6f,
                    (data.WorldPos.z + data.ZOffset) * minimapScale
                );
                // Within compass radius - use absolute world position
                finalPos = absoluteWorldPos;
            }
            else
            {
                // Beyond compass radius - clamp to compass edge
                // Maintain same coordinate system as non-clamped markers
                // Clamp the relative position to compassRadius, then convert back to absolute world position
                var clampedRelative = relativeToPlayer.normalized * compassRadius;

                // Move the clamped position further to the left (west) by subtracting an offset from X
                const float westClampOffset = 25f; // Adjust this value as needed for your UI
                clampedRelative.x -= westClampOffset;

                finalPos = new Vector2(
                    (playerPos.x * minimapScale) + clampedRelative.x + 6f,
                    (playerPos.z * minimapScale) + clampedRelative.y - 2f
                );
            }
            
            markerRect.anchoredPosition = finalPos;
            markerRect.gameObject.SetActive(true);
            
            // Move marker to front (render above compass) when outside minimap radius
            if (uiDistance > minimapRadius)
            {
                markerRect.SetAsLastSibling(); // Render on top
            }
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

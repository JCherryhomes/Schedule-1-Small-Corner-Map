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
            var isUpdate = markers.ContainsKey(data.Id);
            markers[data.Id] = data;

            if (isUpdate)
                MarkerUpdated?.Invoke(data);
            else
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
                MelonLogger.Msg($"[UpdateMarkerPosition] Reparenting {data.Id} to minimap");
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

            // Unclamped child anchored position in MapContent-local (world -> content coordinates)
            Vector2 unclampedChildPos = new Vector2(
                (data.WorldPos.x + data.XOffset) * minimapScale,
                (data.WorldPos.z + data.ZOffset) * minimapScale
            );

            // Content's current anchoredPosition (MapContent) -- this is the translation applied to all children
            var contentRect = minimapParent as RectTransform;
            Vector2 contentAnchored = contentRect != null ? contentRect.anchoredPosition : Vector2.zero;

            // Compute the marker position in UI/mask space (contentAnchored + child's anchoredPosition)
            // This is the coordinate space where the mask/compass center lives (mask center at (0,0) in its parent).
            Vector2 markerUiPos = contentAnchored + unclampedChildPos;
            markerUiPos.y += Constants.MarkerZOffset; // slight vertical offset to prevent clipping with compass ring
            markerUiPos.x += Constants.MarkerXOffset; // slight horizontal offset for better alignment

            // Vector from player to marker in UI space (player UI pos is contentAnchored + playerPos*scale)
            Vector2 playerUiPos = contentAnchored + new Vector2(playerPos.x * minimapScale, playerPos.z * minimapScale);
            Vector2 uiVectorFromPlayer = markerUiPos - playerUiPos;

            // Clamp radius is provided in UI units (compass center/outer radius from CompassManager)
            float clampRadius = compassRadius;

            Vector2 finalUiPos;
            uiVectorFromPlayer.x += Constants.MarkerXOffset; // adjust back for marker offset before clamping
            if (uiVectorFromPlayer.magnitude >= clampRadius)
            {
                // Clamp in UI space around the player UI position so clamped points lie exactly on the visible ring
                var clampedUiRelative = uiVectorFromPlayer.normalized * (clampRadius - 3f);
                finalUiPos = playerUiPos + clampedUiRelative;
                finalUiPos.x -= Constants.MarkerXOffset * 1.2f; // adjust for marker offset
            }
            else
            {
                finalUiPos = markerUiPos;
            }

            // Convert final UI position back to MapContent-local child anchoredPosition:
            Vector2 finalChildAnchoredPos = finalUiPos - contentAnchored;

            markerRect.anchoredPosition = finalChildAnchoredPos;
            markerRect.gameObject.SetActive(true);
            markerRect.SetAsLastSibling(); // Render on top
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
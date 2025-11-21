using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    internal class CompassManager
    {
        private GameObject root;
        private RectTransform rootRect;
        private readonly MapPreferences mapPreferences;
        private const float HideDistanceThreshold = 0.01f;
        private bool subscribed;
        private float maskDiameterUI;
        private float lastWorldScale;
        private CompassUIFactory uiFactory;
        private static MarkerRegistry _markerRegistry;
        private readonly Dictionary<string, GameObject> compassMarkers = new();
        private Transform playerTransform;
        private Vector3 playerPosition;

        public CompassManager(MapPreferences prefs) => mapPreferences = prefs;

        public void Initialize(MarkerRegistry registry)
        {
            _markerRegistry = registry;
            uiFactory = new CompassUIFactory(_markerRegistry, Constants.CompassDefaultIconSize);
            _markerRegistry.MarkerAdded += OnMarkerAdded;
            _markerRegistry.MarkerRemoved += OnMarkerRemoved;
            foreach (var marker in _markerRegistry.GetAllMarkers())
                OnMarkerAdded(marker);
        }

        public void Create(GameObject frameObject, float maskDiameter)
        {
            maskDiameterUI = maskDiameter;
            var (r, rRect, _, _) = MinimapUIFactory.CreateCompass(frameObject, maskDiameter);
            root = r; rootRect = rRect;
            ApplyVisibility();
        }

        public void SetWorldScale(float worldScale) => lastWorldScale = worldScale;

        public void UpdateLayout(float newMaskDiameter)
        {
            if (root == null) return;
            maskDiameterUI = newMaskDiameter;
            var parent = rootRect?.parent?.gameObject;
            UnityEngine.Object.Destroy(root);
            var (r, rRect, _, _) = MinimapUIFactory.CreateCompass(parent, newMaskDiameter);
            root = r; rootRect = rRect;
            foreach (var marker in compassMarkers.Values)
            {
                if (marker != null) UnityEngine.Object.Destroy(marker);
            }
            compassMarkers.Clear();
            foreach (var marker in _markerRegistry.GetAllMarkers())
                OnMarkerAdded(marker);
        }

        private void OnMarkerAdded(MarkerRegistry.MarkerData data)
        {
            if (!data.IsVisibleOnCompass) return;
            if (compassMarkers.ContainsKey(data.Id) || root == null) return;
            var iconPrefab = data.IconPrefab;
            if (iconPrefab == null) {
                MelonLogger.Warning($"[CompassManager] IconPrefab is null for marker {data.Id}");
                return;
            }
            var markerObject = UnityEngine.Object.Instantiate(iconPrefab, root.transform, false);
            markerObject.name = data.Id;
            var rect = markerObject.GetComponent<RectTransform>();

            // Set color if specified
            var image = markerObject.GetComponentInChildren<Image>();
            if (image != null && data.Color.HasValue)
                image.color = data.Color.Value;
            compassMarkers[data.Id] = markerObject;
        }

        private void OnMarkerRemoved(string id)
        {
            if (compassMarkers.TryGetValue(id, out var marker))
            {
                UnityEngine.Object.Destroy(marker);
                compassMarkers.Remove(id);
            }
        }

        public void Dispose()
        {
            if (_markerRegistry != null)
            {
                _markerRegistry.MarkerAdded -= OnMarkerAdded;
                _markerRegistry.MarkerRemoved -= OnMarkerRemoved;
            }
            foreach (var marker in compassMarkers.Values)
                UnityEngine.Object.Destroy(marker);
            compassMarkers.Clear();
        }

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }
        private void ApplyVisibility() => SetVisible(mapPreferences.ShowCompass.Value);
        public void Subscribe()
        {
            if (subscribed) return;
            mapPreferences.ShowCompass.OnEntryValueChanged.Subscribe(OnShowCompassChanged);
            subscribed = true;
        }
        private void OnShowCompassChanged(bool oldVal, bool newVal) => ApplyVisibility();

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }
        
        public float MinimapWorldRadius { get; set; } // Set this from MinimapUI or coordinator
        public MinimapContent MinimapContent { get; set; } // Set this from coordinator or UI factory

        public void UpdateCompassMarkers()
        {
            if (playerTransform == null || rootRect == null) return;
            playerPosition = playerTransform.position;

            // Calculate the correct visible compass ring radius
            var maskRadius = maskDiameterUI / 2f;
            const float tickHalfHeightMajor = (Constants.CompassTickHeight * Constants.CompassTickMajorScale) / 2f;
            var ringThickness = tickHalfHeightMajor + Constants.CompassRingExtraThickness + Constants.CompassRingPadding;

            // Use the outer ring radius (maskRadius + ringThickness) so markers clamp to the visible edge
            var ringRadius = maskRadius + ringThickness;

            var minimapScale = lastWorldScale > 0 ? lastWorldScale : 1f;
            var minimapMaskRadiusUI = maskDiameterUI / 2f;

            // Get parent transform for markers (minimap content so they move with the map)
            var minimapParent = MinimapContent?.MapContentObject?.transform;

            if (minimapParent == null)
            {
                MelonLogger.Warning($"[CompassManager] Missing minimap parent transform!");
                return;
            }

            // Use unified update method
            foreach (var kvp in compassMarkers)
            {
                var markerId = kvp.Key;
                var markerObj = kvp.Value;
                var markerData = _markerRegistry.GetMarker(markerId);
                if (markerData == null) continue;
                var markerRect = markerObj.GetComponent<RectTransform>();
                if (markerRect == null) continue;

                // pass ringRadius (UI units) so UpdateMarkerPosition clamps to the visible ring edge
                MarkerRegistry.UpdateMarkerPosition(
                    markerData, markerRect, minimapScale, minimapMaskRadiusUI, ringRadius, playerPosition, minimapParent);
            }
        }
    }
}

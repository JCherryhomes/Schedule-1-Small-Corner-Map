using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    internal class CompassManager
    {
        private GameObject root;
        private RectTransform rootRect;
        private RectTransform[] letters;
        private Image[] ticks;
        private MapPreferences mapPreferences;
        private float currentMinimapDiameter;
        public CompassManager(MapPreferences prefs)
        {
            mapPreferences = prefs;
        }
        public void Create(GameObject frameObject, float minimapDiameter)
        {
            currentMinimapDiameter = minimapDiameter;
            var (r,rRect,l,t) = MinimapUIFactory.CreateCompass(frameObject, minimapDiameter);
            root = r; rootRect = rRect; letters = l; ticks = t;
            ApplyVisibility();
        }
        public void UpdateLayout(float minimapDiameter)
        {
            currentMinimapDiameter = minimapDiameter;
            if (root == null) return;
            // Rebuild by destroying and recreating for simplicity (minimal overhead)
            UnityEngine.Object.Destroy(root);
            Create(rootRect.parent.gameObject, minimapDiameter);
        }
        public void Rotate(float yawDeg)
        {
            if (rootRect == null) return;
            // Rotate with player facing direction; player forward (world +Z) assumed
            rootRect.localRotation = Quaternion.Euler(0f,0f,-yawDeg);
        }
        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }
        private void ApplyVisibility()
        {
            SetVisible(mapPreferences.ShowCompass.Value);
        }
        public void Subscribe()
        {
            mapPreferences.ShowCompass.OnEntryValueChanged.Subscribe(OnShowCompassChanged);
        }
        private void OnShowCompassChanged(bool oldVal, bool newVal)
        {
            ApplyVisibility();
        }
        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            root = null; rootRect = null; letters = null; ticks = null;
            mapPreferences.ShowCompass.OnEntryValueChanged.Unsubscribe(OnShowCompassChanged);
        }
    }
}


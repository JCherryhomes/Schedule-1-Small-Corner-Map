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
        private MapPreferences mapPreferences;
        private readonly Dictionary<string, CompassTarget> targets = new();
        private const float HideDistanceThreshold = 0.01f;
        private bool subscribed;
        private float maskDiameterUI;
        private float lastWorldScale;
        private CompassUIFactory uiFactory = new(Constants.CompassDefaultIconSize);
        
        private class CompassTarget
        {
            public string Id;
            public Func<Vector3> WorldPositionProvider;
            public GameObject MarkerGo;
            public RectTransform Rect;
            public Sprite Sprite;
            public bool Normalized;
        }
        
        private CompassUIFactory.CompassMarkerCategory DetermineCategory(string name) => uiFactory.DetermineCategory(name);
        
        public CompassManager(MapPreferences prefs) => mapPreferences = prefs;
        
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
            foreach (var kv in targets)
            {
                var target = kv.Value;
                RecreateTargetVisual(target);
            }
        }
        
        private void RecreateTargetVisual(CompassTarget target)
        {
            if (target == null) return;
            var category = DetermineCategory(target.Id);
            var proto = uiFactory.AcquirePrototype(category, target.Id);
            var clone = UnityEngine.Object.Instantiate(proto, root.transform, false);
            if (target.MarkerGo != null) UnityEngine.Object.Destroy(target.MarkerGo);
            target.MarkerGo = clone;
            target.Rect = clone.GetComponent<RectTransform>() ?? clone.AddComponent<RectTransform>();
            target.Sprite = clone.GetComponentInChildren<Image>()?.sprite;
            target.Normalized = false;
            uiFactory.NormalizeOnce(target.MarkerGo, category);
            target.Normalized = true;
        }
        
        private void TryUpgradeTarget(CompassTarget target)
        {
            if (target == null || target.MarkerGo == null || target.Sprite != null) return;
            var category = DetermineCategory(target.Id);
            var live = MinimapPoIHelper.TryGetMarker(target.Id);
            if (live == null && category == CompassUIFactory.CompassMarkerCategory.Vehicle && OwnedVehiclesManager.VehicleIconPrototype != null)
                live = OwnedVehiclesManager.VehicleIconPrototype;
            if (live == null) return;
            var upgraded = uiFactory.CloneSourceMarker(live);
            upgraded.transform.SetParent(root.transform, false);
            UnityEngine.Object.Destroy(target.MarkerGo);
            target.MarkerGo = upgraded;
            target.Rect = upgraded.GetComponent<RectTransform>() ?? upgraded.AddComponent<RectTransform>();
            target.Sprite = upgraded.GetComponentInChildren<Image>()?.sprite;
            target.Normalized = false;
            uiFactory.NormalizeOnce(target.MarkerGo, category);
            target.Normalized = true;
        }
        
        public void RegisterTarget(string id, Func<Vector3> worldPositionProvider, GameObject iconPrefab = null, RectTransform iconContainer = null, float? sizeOverride = null)
        {
            if (targets.ContainsKey(id) || root == null) return;
            var category = DetermineCategory(id);
            GameObject prototype = iconPrefab != null ? uiFactory.CloneSourceMarker(iconPrefab) : iconContainer != null ? uiFactory.CloneSourceMarker(iconContainer.gameObject) : uiFactory.AcquirePrototype(category, id);
            var clone = UnityEngine.Object.Instantiate(prototype, root.transform, false);
            var rect = clone.GetComponent<RectTransform>() ?? clone.AddComponent<RectTransform>();
            var target = new CompassTarget
            {
                Id = id,
                WorldPositionProvider = worldPositionProvider,
                MarkerGo = clone,
                Rect = rect,
                Sprite = clone.GetComponentInChildren<Image>()?.sprite,
                Normalized = false
            };
            targets[id] = target;
            uiFactory.NormalizeOnce(target.MarkerGo, category);
            target.Normalized = true;
        }
        
        public void UnregisterTarget(string id)
        {
            if (!targets.TryGetValue(id, out var t)) return;
            if (t.MarkerGo != null) UnityEngine.Object.Destroy(t.MarkerGo);
            targets.Remove(id);
        }
        
        public void UpdateTargets(Vector3 playerPos)
        {
            if (rootRect == null || lastWorldScale <= 0f) return;
            var maskRadiusUI = maskDiameterUI / 2f;
            var hideRadiusUI = maskDiameterUI / 2f + Constants.CompassVisibilityBuffer;
            foreach (var t in targets.Values)
            {
                if (t.MarkerGo == null) continue;
                TryUpgradeTarget(t);
                var worldPos = t.WorldPositionProvider?.Invoke() ?? Vector3.zero;
                var dir = worldPos - playerPos;
                var planarDistWorld = new Vector2(dir.x, dir.z).magnitude;
                var distUI = planarDistWorld * lastWorldScale;
                if (distUI < hideRadiusUI || planarDistWorld < HideDistanceThreshold)
                {
                    t.MarkerGo.SetActive(false);
                    continue;
                }
                t.MarkerGo.SetActive(true);
                var angleRad = Mathf.Atan2(dir.x, dir.z);
                var uiAngleRad = Mathf.Deg2Rad * (90f - angleRad * Mathf.Rad2Deg);
                var ringRadiusUI = maskDiameterUI / 2f + Constants.CompassRingPadding;
                t.Rect.anchoredPosition = new Vector2(ringRadiusUI * Mathf.Cos(uiAngleRad), ringRadiusUI * Mathf.Sin(uiAngleRad));
            }
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
        public void Dispose()
        {
            foreach (var t in targets.Values)
                if (t.MarkerGo != null) UnityEngine.Object.Destroy(t.MarkerGo);
            targets.Clear();
            if (subscribed)
            {
                mapPreferences.ShowCompass.OnEntryValueChanged.Unsubscribe(OnShowCompassChanged);
                subscribed = false;
            }
            if (root != null) UnityEngine.Object.Destroy(root);
            root = null; rootRect = null;
        }
        public bool HasTarget(string id) => targets.ContainsKey(id);
        
        public void SyncFromPoIMarkers(Vector3 playerPos, float worldScale)
        {
            if (worldScale <= 0f) return;
            lastWorldScale = worldScale;
            var maskRadiusUI = maskDiameterUI / 2f;
            var hideRadiusUI = maskRadiusUI + Constants.CompassVisibilityBuffer;
            var activeNames = new HashSet<string>();
            foreach (var (name, worldPos, _, _) in MinimapPoIHelper.EnumerateWorldPositionsWithOffsets())
            {
                var planarDistWorld = new Vector2(worldPos.x - playerPos.x, worldPos.z - playerPos.z).magnitude;
                var distUI = planarDistWorld * worldScale;
                if (distUI <= hideRadiusUI)
                {
                    if (HasTarget(name)) UnregisterTarget(name);
                    continue;
                }
                activeNames.Add(name);
                
                if (!HasTarget(name)) RegisterTarget(name, () => worldPos);
            }
            var toRemove = targets.Keys.Where(k => !activeNames.Contains(k)).ToList();
            foreach (var rem in toRemove) UnregisterTarget(rem);
        }
    }
}

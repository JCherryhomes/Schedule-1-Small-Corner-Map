using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Small_Corner_Map.Main;

namespace Small_Corner_Map.Helpers
{
    /// <summary>
    /// Factory responsible for UI-centric operations for compass markers: cloning, prototype acquisition, normalization.
    /// </summary>
    internal class CompassUIFactory
    {
        private readonly Dictionary<CompassMarkerCategory, GameObject> prototypeCache = new();
        private readonly float unifiedSize;
        private readonly MarkerRegistry markerRegistry;

        internal CompassUIFactory(MarkerRegistry registry, float unifiedSize)
        {
            this.unifiedSize = unifiedSize;
            markerRegistry = registry;
        }

        internal enum CompassMarkerCategory { Contract, DeadDrop, Property, Quest, Vehicle, White, Other }

        internal CompassMarkerCategory DetermineCategory(string name)
        {
            MelonLogger.Msg($"[CompassUIFactory] Determining category for '{name}'");
            if (string.IsNullOrEmpty(name)) return CompassMarkerCategory.Other;
            if (name.StartsWith("StaticMarker_White")) return CompassMarkerCategory.White;
            if (name.Contains("Vehicle")) return CompassMarkerCategory.Vehicle;
            if (name.Contains("Contract")) return CompassMarkerCategory.Contract;
            if (name.Contains("Regular_Quest")) return CompassMarkerCategory.Quest;
            if (name.Contains("DeadDrop")) return CompassMarkerCategory.DeadDrop;
            return name.Contains("Property") ? CompassMarkerCategory.Property : CompassMarkerCategory.Other;
        }

        internal GameObject CloneSourceMarker(GameObject source)
        {
            if (source == null) return null;
            var clone = UnityEngine.Object.Instantiate(source);
            foreach (var img in clone.GetComponentsInChildren<Image>(true))
            {
                img.raycastTarget = false;
                img.preserveAspect = true;
                var c = img.color; 
                if (c.a < 0.9f) { c.a = 0.9f; img.color = c; }
            }
            var rect = clone.GetComponent<RectTransform>() ?? clone.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            return clone;
        }

        private GameObject CreateFallback(CompassMarkerCategory category)
        {
            var fallback = new GameObject("CompassFallback_" + category);
            var rect = fallback.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(unifiedSize, unifiedSize);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f,0.5f);
            rect.pivot = new Vector2(0.5f,0.5f);
            var img = fallback.AddComponent<Image>();
            img.color = category switch
            {
                CompassMarkerCategory.Vehicle => new Color(1f,0.6f,0.2f,0.9f),
                CompassMarkerCategory.Contract => new Color(0.9f,0.3f,0.3f,0.9f),
                CompassMarkerCategory.Property => new Color(0.3f,0.9f,0.3f,0.9f),
                CompassMarkerCategory.White => new Color(1f,1f,1f,0.9f),
                _ => new Color(0.7f,0.7f,0.7f,0.9f)
            };
            img.raycastTarget = false; img.preserveAspect = true;
            return fallback;
        }

        internal GameObject AcquirePrototype(CompassMarkerCategory category, string markerName)
        {
            if (prototypeCache.TryGetValue(category, out var cached) && cached != null)
            {
                MelonLogger.Msg($"[CompassUIFactory] Using cached prototype for category {category} (marker '{markerName}')");
                return cached;
            }
            var marker = markerRegistry.GetMarker(markerName);
            var live = marker.IconPrefab;
            if (live == null && category == CompassMarkerCategory.Vehicle && OwnedVehiclesManager.VehicleIconPrototype != null)
                live = OwnedVehiclesManager.VehicleIconPrototype;
            var proto = live != null ? CloneSourceMarker(live) : CreateFallback(category);
            prototypeCache[category] = proto;
            return proto;
        }

        private static bool IsBackgroundOrOverlay(RectTransform r)
        {
            var n = r.gameObject.name;
            return n == "Shade" || n == "Mask" || n == "Background";
        }

        private static bool HasSprite(RectTransform r)
        {
            var img = r.GetComponent<Image>();
            return img != null && img.sprite != null;
        }

        private static bool IsStructuralContainer(RectTransform r)
        {
            var n = r.gameObject.name;
            return n.Contains("Marker_") || n.Contains("Container") || n == "Owned" || n == "Unowned";
        }

        private struct DimensionInfo { public float Dim; public bool HadSprite; }

        private DimensionInfo ComputeDimensionInfo(GameObject icon)
        {
            if (icon == null) return new DimensionInfo { Dim = 0f, HadSprite = false };
            var rects = icon.GetComponentsInChildren<RectTransform>(true);
            var candidateDims = new List<float>();
            foreach (var r in rects)
            {
                var size = r.sizeDelta;
                var w = Mathf.Abs(size.x * r.lossyScale.x);
                var h = Mathf.Abs(size.y * r.lossyScale.y);
                var localMax = Mathf.Max(w, h);
                bool spriteCandidate = HasSprite(r) && !IsBackgroundOrOverlay(r) && localMax > 0.01f;
                if (spriteCandidate) candidateDims.Add(localMax);
            }
            if (candidateDims.Count > 0)
            {
                candidateDims.Sort();
                var chosen = candidateDims[candidateDims.Count / 2];
                if (chosen < unifiedSize * 0.25f)
                {
                    var largest = candidateDims[^1];
                    chosen = largest;
                }
                return new DimensionInfo { Dim = chosen, HadSprite = true };
            }
            // Fallback no sprite candidates
            float maxDim = 0f;
            foreach (var r in rects)
            {
                var size = r.sizeDelta;
                var w = Mathf.Abs(size.x * r.lossyScale.x);
                var h = Mathf.Abs(size.y * r.lossyScale.y);
                var localMax = Mathf.Max(w, h);
                if (localMax > maxDim) maxDim = localMax;
            }
            return new DimensionInfo { Dim = maxDim, HadSprite = false };
        }

        // Replace ComputeMaxDimension with filtered logic
        internal float ComputeMaxDimension(GameObject icon)
        {
            if (icon == null) return 0f;
            var info = ComputeDimensionInfo(icon);
            return info.Dim;
        }

        internal void NormalizeOnce(GameObject iconRoot, CompassMarkerCategory category)
        {
            if (iconRoot == null) return;
            var info = ComputeDimensionInfo(iconRoot);
            var effectiveDim = info.Dim <= 0.001f ? unifiedSize : info.Dim;
            var rootRect = iconRoot.GetComponent<RectTransform>() ?? iconRoot.AddComponent<RectTransform>();
            var baseScaleFactor = unifiedSize / effectiveDim;
            // Category-specific sprite boost factors (inner icon emphasis)
            var spriteBoost = category switch
            {
                CompassMarkerCategory.Vehicle => 1.21f,
                CompassMarkerCategory.Property => 1.20f,
                CompassMarkerCategory.Contract => 0.85f, // slightly smaller
                _ => 1.00f
            };
            iconRoot.transform.localScale = Vector3.one;
            foreach (var r in iconRoot.GetComponentsInChildren<RectTransform>(true))
            {
                if (r == rootRect) continue;
                var img = r.GetComponent<Image>();
                if (img != null && img.sprite != null && !IsBackgroundOrOverlay(r))
                {
                    r.localScale = Vector3.one * (baseScaleFactor * spriteBoost);
                }
                else
                {
                    r.localScale = Vector3.one * baseScaleFactor;
                }
            }
            rootRect.sizeDelta = new Vector2(unifiedSize, unifiedSize);
            foreach (var img in iconRoot.GetComponentsInChildren<Image>(true))
            {
                img.preserveAspect = true; img.raycastTarget = false;
            }
        }
    }
}

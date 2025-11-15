using UnityEngine;
using Small_Corner_Map.Main;

namespace Small_Corner_Map.Helpers
{
    public static class MinimapPoIHelper
    {
        // Single storage: name -> entry containing GameObject + world data (IL2CPP-safe)
        private struct MarkerEntry
        {
            public GameObject Marker;
            public Vector3 WorldPos;
            public float XOffset;
            public float ZOffset;
        }

        private static readonly Dictionary<string, MarkerEntry> MarkerStore = new();

        public static void AddWhitePoIMarker(MinimapContent minimapContent, Vector3 worldPos, GameObject iconPrefab)
        {
            var whiteMarker = minimapContent.AddWhiteStaticMarker(worldPos, iconPrefab);
            if (whiteMarker == null) return;

            var baseName = "StaticMarker_White";
            var uniqueName = baseName + "_" + worldPos.x.ToString("F2") + "_" + worldPos.z.ToString("F2");

            RemoveMarker(uniqueName);

            whiteMarker.name = uniqueName;
            var rect = whiteMarker.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Apply calibration offsets on initial placement
                rect.anchoredPosition += new Vector2(Constants.PropertyMarkerXOffset, Constants.PropertyMarkerZOffset);

                MarkerStore[uniqueName] = new MarkerEntry
                {
                    Marker = whiteMarker,
                    WorldPos = worldPos,
                    XOffset = Constants.PropertyMarkerXOffset,
                    ZOffset = Constants.PropertyMarkerZOffset
                };
                whiteMarker.transform.SetAsLastSibling();
            }
            else
            {
                UnityEngine.Object.Destroy(whiteMarker);
            }
        }

        public static void AddRedPoIMarker(MinimapContent minimapContent, Vector3 worldPos)
        {
            minimapContent.AddRedStaticMarker(worldPos);
        }

        public static void UpdateMarkerPosition(string name, Vector2 mappedPosition)
        {
            if (!MarkerStore.TryGetValue(name, out var entry)) return;
            if (entry.Marker == null)
            {
                MarkerStore.Remove(name);
                return;
            }
            var rect = entry.Marker.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = mappedPosition;
        }

        public static void UpdateAllMarkerPositions(float mapScale)
        {
            // Recalculate positions for all stored markers
            foreach (var (name, entry) in MarkerStore.ToList())
            {
                if (entry.Marker == null)
                {
                    MarkerStore.Remove(name);
                    continue;
                }
                var x = entry.WorldPos.x * mapScale + entry.XOffset;
                var z = entry.WorldPos.z * mapScale + entry.ZOffset;
                var mapped = new Vector2(x, z);
                var rect = entry.Marker.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = mapped;
            }
        }

        public static void AddMarkersToMap(
            GameObject markerPrefab,
            GameObject mapContentObject,
            string name,
            Vector2 mappedPosition,
            Vector3 worldPos,
            float xOffset = 0f,
            float zOffset = 0f)
        {
            // Remove existing marker with same name
            RemoveMarker(name);
            var markerObject = UnityEngine.Object.Instantiate(markerPrefab, mapContentObject.transform, false);
            markerObject.name = name;
            var rect = markerObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                UnityEngine.Object.Destroy(markerObject);
                return;
            }
            var finalPos = mappedPosition + new Vector2(xOffset, zOffset);
            rect.sizeDelta = new Vector2(Constants.ContractMarkerSize, Constants.ContractMarkerSize);
            rect.anchoredPosition = finalPos;
            markerObject.transform.localScale = new Vector3(Constants.ContractMarkerScale, Constants.ContractMarkerScale, Constants.ContractMarkerScale);
            markerObject.transform.SetAsLastSibling();

            MarkerStore[name] = new MarkerEntry
            {
                Marker = markerObject,
                WorldPos = worldPos,
                XOffset = xOffset,
                ZOffset = zOffset
            };
        }

        public static void RemoveMarker(string name)
        {
            if (!MarkerStore.TryGetValue(name, out var entry)) return;
            if (entry.Marker != null)
                UnityEngine.Object.Destroy(entry.Marker);
            MarkerStore.Remove(name);
        }

        public static void RemoveAllByKey(string key)
        {
            var toRemove = MarkerStore.Keys.Where(k => k.StartsWith(key)).ToList();
            foreach (var k in toRemove)
            {
                var entry = MarkerStore[k];
                if (entry.Marker != null)
                    UnityEngine.Object.Destroy(entry.Marker);
                MarkerStore.Remove(k);
            }
        }
    }
}

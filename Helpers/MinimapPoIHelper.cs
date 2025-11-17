using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Main;
using UnityEngine.UI;

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

        public static void AddWhitePoIMarker(
            MinimapContent minimapContent, 
            Vector3 worldPos, 
            GameObject iconPrefab, 
            string keyPrefix = "StaticMarker_White")
        {
            var whiteMarker = minimapContent.AddWhiteStaticMarker(worldPos, iconPrefab);
            if (whiteMarker == null) return;
            
            var uniqueName = keyPrefix + "_" + worldPos.x.ToString("F2") + "_" + worldPos.z.ToString("F2");

            RemoveMarker(uniqueName);

            whiteMarker.name = uniqueName;
            var rect = whiteMarker.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Apply calibration offsets on initial placement
                rect.anchoredPosition += new Vector2(-Constants.MarkerXOffset, Constants.PropertyMarkerZOffset);

                MarkerStore[uniqueName] = new MarkerEntry
                {
                    Marker = whiteMarker,
                    WorldPos = worldPos,
                    XOffset = -Constants.MarkerXOffset,
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
            var mapContentObject = minimapContent.MapContentObject;
            if (mapContentObject == null)
            {
                MelonLogger.Warning("MinimapContent: Cannot add marker, missing map content.");
                return;
            }

            var markerObject = new GameObject("StaticMarker_Red");
            markerObject.transform.SetParent(mapContentObject.transform, false);
            var markerRect = markerObject.AddComponent<RectTransform>();
            markerRect.sizeDelta = new Vector2(Constants.RedMarkerSize, Constants.RedMarkerSize);
            var mappedX = worldPos.x * Constants.DefaultMapScale;
            var mappedZ = worldPos.z * Constants.DefaultMapScale;
            markerRect.anchoredPosition = new Vector2(mappedX, mappedZ);
            var markerImage = markerObject.AddComponent<Image>();
            markerImage.color = Color.red;
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

        public static bool MarkerExists(string name, string baseKey = "StaticMarker_White")
        {
            MelonLogger.Msg("MinimapPoIHelper: Checking for markers with base key: " + baseKey);
            foreach (var (key, value) in MarkerStore.ToList())
            {
                if (key.StartsWith(baseKey))
                {
                    MelonLogger.Msg("MinimapPoIHelper: Objects matching base key: " + key);
                }
            }
            MelonLogger.Msg("Looking for key: " + name);
            return MarkerStore.ContainsKey(name);
        }

        public static void AddMarkerToMap(
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

        public static IEnumerable<string> GetAllMarkerNames() => MarkerStore.Keys.ToList();
        public static GameObject TryGetMarker(string name)
        {
            return MarkerStore.TryGetValue(name, out var entry) ? entry.Marker : null;
        }

        public static bool RenameMarker(string oldName, string newName)
        {
            if (oldName == newName) return true;
            if (!MarkerStore.TryGetValue(oldName, out var entry)) return false;
            if (MarkerStore.ContainsKey(newName)) return false; // avoid collision
            if (entry.Marker == null)
            {
                MarkerStore.Remove(oldName);
                return false;
            }
            entry.Marker.name = newName;
            MarkerStore.Remove(oldName);
            MarkerStore[newName] = entry;
            return true;
        }

        public static IEnumerable<(string Name, Vector3 WorldPos)> EnumerateWorldPositions()
        {
            foreach (var kv in MarkerStore)
            {
                yield return (kv.Key, kv.Value.WorldPos);
            }
        }

        public static IEnumerable<(string Name, Vector3 WorldPos, float XOffset, float ZOffset)> EnumerateWorldPositionsWithOffsets()
        {
            foreach (var kv in MarkerStore)
            {
                yield return (kv.Key, kv.Value.WorldPos, kv.Value.XOffset, kv.Value.ZOffset);
            }
        }
    }
}

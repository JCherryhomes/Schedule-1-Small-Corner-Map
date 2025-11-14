using UnityEngine;
using Small_Corner_Map.Main;

namespace Small_Corner_Map.Helpers
{
    public static class MinimapPoIHelper
    {
        public static List<GameObject> PoIMarkers { get; private set; } = new List<GameObject>();

        public static void AddWhitePoIMarker(MinimapContent minimapContent, Vector3 worldPos, GameObject iconPrefab)
        {
            minimapContent.AddWhiteStaticMarker(worldPos, iconPrefab);
        }

        public static void AddRedPoIMarker(MinimapContent minimapContent, Vector3 worldPos)
        {
            minimapContent.AddRedStaticMarker(worldPos);
        }

        public static void UpdateMarkerPosition(string name, Vector2 mappedPosition)
        {
            var marker = PoIMarkers.Find(m => m.name == name);
            if (marker != null)
            {
                RectTransform markerRect = marker.GetComponent<RectTransform>();
                if (markerRect != null)
                {
                    markerRect.anchoredPosition = mappedPosition;
                }
            }
        }

        public static void addMarkersToMap(GameObject markerPrefab, GameObject mapContentObject, string name, Vector2 mappedPosition)
        {
            GameObject markerObject = UnityEngine.Object.Instantiate(markerPrefab);
            markerObject.transform.SetParent(mapContentObject.transform, false);
            markerObject.name = name;
            RectTransform markerRect = markerObject.GetComponent<RectTransform>();

            if (markerRect != null)
            {
                markerRect.sizeDelta = new Vector2(15f, 15f);
                markerRect.anchoredPosition = mappedPosition;
                markerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                PoIMarkers.Add(markerObject);
            }
        }

        public static void RemoveMarker(string name)
        {
            
            var marker = PoIMarkers.Find(m => m.name == name);
            if (marker != null)
            {
                UnityEngine.Object.Destroy(marker);
                PoIMarkers.Remove(marker);
            }
        }

        public static void RemoveAllByKey(string key)
        {
            var markersToRemove = PoIMarkers.Where(m => m.name.StartsWith(key)).ToList();
            foreach (var marker in markersToRemove)
            {
                UnityEngine.Object.Destroy(marker);
                PoIMarkers.Remove(marker);
            }
        }
    }
}

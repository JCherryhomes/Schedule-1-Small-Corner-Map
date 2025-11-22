using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    public class MinimapContent
    {
        public GameObject MapContentObject { get; private set; }
        public GameObject MapImageObject { get; private set; }
        public RectTransform GridContainer { get; private set; }
        public float CurrentMapScale { get; private set; }

        private readonly float mapContentSize;

        public MinimapContent(float mapContentSize = 500f, float mapScale = 1.2487098f)
        {
            this.mapContentSize = mapContentSize;
            this.CurrentMapScale = mapScale;
        }

        public void Create(GameObject parent)
        {
            MapContentObject = new GameObject("MapContent");
            MapContentObject.transform.SetParent(parent.transform, false);
            var mapContentRect = MapContentObject.AddComponent<RectTransform>();
            mapContentRect.sizeDelta = new Vector2(mapContentSize, mapContentSize);
            mapContentRect.anchorMin = new Vector2(0.5f, 0.5f);
            mapContentRect.anchorMax = new Vector2(0.5f, 0.5f);
            mapContentRect.pivot = new Vector2(0.5f, 0.5f);
            mapContentRect.anchoredPosition = new Vector2(20f, 20f);

            // Create the image as a child
            MapImageObject = new GameObject("MapImage");
            MapImageObject.transform.SetParent(MapContentObject.transform, false);
            var imageRect = MapImageObject.AddComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.sizeDelta = mapContentRect.sizeDelta;
            imageRect.anchoredPosition = new Vector2(4f, 1.5f);
        }

        public GameObject AddWhiteStaticMarker(Vector3 worldPos, GameObject iconPrefab)
        {
            if (MapContentObject == null || iconPrefab == null)
            {
                MelonLogger.Warning("MinimapContent: Cannot add marker, missing map content or icon prefab.");
                return null;
            }

            var markerObject = UnityEngine.Object.Instantiate(iconPrefab, MapContentObject.transform, false);
            markerObject.name = "StaticMarker_White"; // base name; helper may override for uniqueness
            var markerRect = markerObject.GetComponent<RectTransform>();

            if (markerRect == null) return markerObject; // ensure a return even if markerRect was null
            
            markerRect.sizeDelta = new Vector2(10f, 10f);
            var mappedX = worldPos.x * CurrentMapScale;
            var mappedZ = worldPos.z * CurrentMapScale;
            markerRect.anchoredPosition = new Vector2(mappedX, mappedZ);
            markerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            return markerObject; // ensure a return even if markerRect was null
        }

        public void UpdateMapScale(float newScale)
        {
            CurrentMapScale = newScale;
        }

    }
}
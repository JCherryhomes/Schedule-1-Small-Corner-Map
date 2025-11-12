using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

namespace Small_Corner_Map
{
    public class MinimapContent
    {
        public GameObject MapContentObject { get; private set; }
        public RectTransform GridContainer { get; private set; }

        private readonly float mapContentSize;
        private readonly int gridSize;
        private readonly Color gridColor;
        private readonly float mapScale;

        public MinimapContent(float mapContentSize = 500f, int gridSize = 20, float mapScale = 1.2487098f, Color? gridColor = null)
        {
            this.mapContentSize = mapContentSize;
            this.gridSize = gridSize;
            this.mapScale = mapScale;
            this.gridColor = gridColor ?? new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        public void Create(GameObject parent)
        {
            MapContentObject = new GameObject("MapContent");
            MapContentObject.transform.SetParent(parent.transform, false);
            RectTransform mapContentRect = MapContentObject.AddComponent<RectTransform>();
            mapContentRect.sizeDelta = new Vector2(mapContentSize, mapContentSize);
            mapContentRect.anchorMin = new Vector2(0.5f, 0.5f);
            mapContentRect.anchorMax = new Vector2(0.5f, 0.5f);
            mapContentRect.pivot = new Vector2(0.5f, 0.5f);
            mapContentRect.anchoredPosition = Vector2.zero;

            // Create grid container
            GameObject gridObject = new GameObject("GridContainer");
            gridObject.transform.SetParent(MapContentObject.transform, false);
            GridContainer = gridObject.AddComponent<RectTransform>();
            GridContainer.sizeDelta = new Vector2(mapContentSize, mapContentSize);
            GridContainer.anchorMin = new Vector2(0.5f, 0.5f);
            GridContainer.anchorMax = new Vector2(0.5f, 0.5f);
            GridContainer.pivot = new Vector2(0.5f, 0.5f);
            GridContainer.anchoredPosition = Vector2.zero;
        }

        public void AddWhiteStaticMarker(Vector3 worldPos, GameObject iconPrefab)
        {
            if (MapContentObject == null || iconPrefab == null)
            {
                MelonLogger.Warning("MinimapContent: Cannot add white marker, missing map content or icon prefab.");
                return;
            }

            GameObject markerObject = UnityEngine.Object.Instantiate(iconPrefab);
            markerObject.name = "StaticMarker_White";
            markerObject.transform.SetParent(MapContentObject.transform, false);
            RectTransform markerRect = markerObject.GetComponent<RectTransform>();

            if (markerRect != null)
            {
                markerRect.sizeDelta = new Vector2(10f, 10f);
                float mappedX = worldPos.x * mapScale;
                float mappedZ = worldPos.z * mapScale;
                markerRect.anchoredPosition = new Vector2(mappedX, mappedZ);
                markerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        public void AddRedStaticMarker(Vector3 worldPos)
        {
            if (MapContentObject == null)
            {
                MelonLogger.Warning("MinimapContent: Cannot add red marker, missing map content.");
                return;
            }

            GameObject markerObject = new GameObject("StaticMarker_Red");
            markerObject.transform.SetParent(MapContentObject.transform, false);
            RectTransform markerRect = markerObject.AddComponent<RectTransform>();
            markerRect.sizeDelta = new Vector2(5f, 5f);
            float mappedX = worldPos.x * mapScale;
            float mappedZ = worldPos.z * mapScale;
            markerRect.anchoredPosition = new Vector2(mappedX, mappedZ);
            Image markerImage = markerObject.AddComponent<Image>();
            markerImage.color = Color.red;
            MelonLogger.Msg("Red static marker added at mapped position: " + markerRect.anchoredPosition);
        }
    }
}
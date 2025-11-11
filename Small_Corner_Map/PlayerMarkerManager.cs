using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map
{
    public class PlayerMarkerManager
    {
        private GameObject playerMarker;
        private RectTransform directionIndicator;
        private readonly Color markerColor = new Color(0.2f, 0.6f, 1f, 1f);

        public GameObject Marker => playerMarker;

        public void CreatePlayerMarker(GameObject parent)
        {
            playerMarker = new GameObject("PlayerMarker");
            playerMarker.transform.SetParent(parent.transform, false);
            RectTransform rect = playerMarker.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(10f, 10f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            Image image = playerMarker.AddComponent<Image>();
            image.color = markerColor;

            // Ensure player marker is drawn on top
            playerMarker.transform.SetAsLastSibling();
        }

        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
            if (playerMarker == null || realIconPrefab == null)
                return;

            GameObject newMarker = UnityEngine.Object.Instantiate(realIconPrefab, playerMarker.transform.parent, false);
            newMarker.name = "PlayerMarker";
            RectTransform newRect = newMarker.GetComponent<RectTransform>();
            if (newRect != null)
            {
                newRect.anchoredPosition = Vector2.zero;
                newRect.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            // Remove arrow if present
            Transform arrowImage = newMarker.transform.Find("Image");
            if (arrowImage != null)
            {
                UnityEngine.Object.Destroy(arrowImage.gameObject);
            }

            UnityEngine.Object.Destroy(playerMarker);
            playerMarker = newMarker;
            playerMarker.transform.SetAsLastSibling();
        }

        public void UpdateDirectionIndicator(Transform playerTransform)
        {
            if (playerMarker == null || playerTransform == null)
                return;

            if (directionIndicator == null)
            {
                Transform indicatorTransform = playerMarker.transform.Find("DirectionIndicator");
                if (indicatorTransform != null)
                {
                    directionIndicator = (RectTransform)indicatorTransform;
                }
                else
                {
                    GameObject indicatorObject = new GameObject("DirectionIndicator");
                    indicatorObject.transform.SetParent(playerMarker.transform, false);
                    directionIndicator = indicatorObject.AddComponent<RectTransform>();
                    directionIndicator.sizeDelta = new Vector2(6f, 6f);
                    Image image = indicatorObject.AddComponent<Image>();
                    image.color = Color.white;
                }
            }

            directionIndicator.pivot = new Vector2(0.5f, 0.5f);
            float indicatorDistance = 15f;
            Quaternion rotation = playerTransform.rotation;
            float yRotation = rotation.eulerAngles.y;
            float angleRad = (90f - yRotation) * Mathf.Deg2Rad;

            Vector2 newPosition = new Vector2(
                indicatorDistance * Mathf.Cos(angleRad),
                indicatorDistance * Mathf.Sin(angleRad)
            );
            directionIndicator.anchoredPosition = newPosition;
        }

        public void SetMarkerScale(float scale)
        {
            if (playerMarker != null)
                playerMarker.transform.localScale = Vector3.one * scale;
        }
    }
}
using UnityEngine;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    public class PlayerMarkerManager
    {
        private RectTransform directionIndicator;
        private readonly Color markerColor = new Color(0.2f, 0.6f, 1f, 1f);

        private GameObject Marker { get; set; }

        public void CreatePlayerMarker(GameObject parent)
        {
            Marker = MinimapUIFactory.CreatePlayerMarker(parent, markerColor);
        }

        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
            if (Marker == null || realIconPrefab == null)
                return;

            var newMarker = UnityEngine.Object.Instantiate(realIconPrefab, Marker.transform.parent, false);
            newMarker.name = "PlayerMarker";
            var newRect = newMarker.GetComponent<RectTransform>();
            if (newRect != null)
            {
                newRect.anchoredPosition = Vector2.zero;
                newRect.localScale = new Vector3(
                    Constants.PlayerIconReplacementScale, 
                    Constants.PlayerIconReplacementScale, 
                    Constants.PlayerIconReplacementScale);
            }

            // Remove arrow if present
            var arrowImage = newMarker.transform.Find("Image");
            if (arrowImage != null)
            {
                UnityEngine.Object.Destroy(arrowImage.gameObject);
            }

            UnityEngine.Object.Destroy(Marker);
            Marker = newMarker;
            Marker.transform.SetAsLastSibling();
        }

        public void UpdateDirectionIndicator(Transform playerTransform)
        {
            if (Marker == null || playerTransform == null)
                return;

            if (directionIndicator == null)
            {
                var indicatorTransform = Marker.transform.Find("DirectionIndicator");
                if (indicatorTransform != null)
                {
                    directionIndicator = (RectTransform)indicatorTransform;
                }
                else
                {
                    directionIndicator = MinimapUIFactory.CreateDirectionIndicator(Marker, Color.white);
                }
            }

            directionIndicator.pivot = new Vector2(0.5f, 0.5f);
            var indicatorDistance = Constants.DirectionIndicatorDistance;
            var rotation = playerTransform.rotation;
            var yRotation = rotation.eulerAngles.y;
            var angleRad = (90f - yRotation) * Mathf.Deg2Rad;

            var newPosition = new Vector2(
                indicatorDistance * Mathf.Cos(angleRad),
                indicatorDistance * Mathf.Sin(angleRad)
            );
            directionIndicator.anchoredPosition = newPosition;
        }
    }
}
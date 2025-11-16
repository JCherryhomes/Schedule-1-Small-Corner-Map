using UnityEngine;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    public class PlayerMarkerManager
    {
        private RectTransform directionIndicator;
        private readonly Color markerColor = new Color(0.2f, 0.6f, 1f, 1f);

        private GameObject Marker { get; set; }
        private GameObject originalPlayerIconPrefab;
        private bool showingVehicleIcon;

        public void CreatePlayerMarker(GameObject parent)
        {
            Marker = MinimapUIFactory.CreatePlayerMarker(parent, markerColor);
        }

        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
            if (realIconPrefab == null) return;
            originalPlayerIconPrefab = realIconPrefab; // cache for restore
            ReplaceWithIcon(realIconPrefab, Constants.PlayerIconReplacementScale, isVehicle:false);
        }
        
        public void ReplaceWithVehicleIcon(GameObject vehicleIconPrefab)
        {
            if (vehicleIconPrefab == null) return;
            ReplaceWithIcon(vehicleIconPrefab, Constants.PlayerIconReplacementScale, isVehicle:true);
        }
        
        private void ReplaceWithIcon(GameObject iconPrefab, float scale, bool isVehicle)
        {
            if (Marker == null)
            {
                MelonLoader.MelonLogger.Warning("PlayerMarkerManager: Cannot replace icon, Marker is null!");
                return;
            }
            
            var parent = Marker.transform.parent;
            var newMarker = UnityEngine.Object.Instantiate(iconPrefab, parent, false);
            newMarker.name = isVehicle ? "PlayerVehicleMarker" : "PlayerMarker";
            var newRect = newMarker.GetComponent<RectTransform>();
            if (newRect != null)
            {
                newRect.anchoredPosition = Vector2.zero;
                newRect.localScale = new Vector3(scale, scale, scale);
            }
            // Remove arrow if present on prefab instance
            var arrowImage = newMarker.transform.Find("Image");
            if (arrowImage != null)
                UnityEngine.Object.Destroy(arrowImage.gameObject);
            
            // Preserve existing directionIndicator by reparenting if it exists
            if (directionIndicator != null)
            {
                directionIndicator.SetParent(newMarker.transform, false);
                directionIndicator.SetAsLastSibling();
            }
            UnityEngine.Object.Destroy(Marker);
            Marker = newMarker;
            Marker.transform.SetAsLastSibling();
            showingVehicleIcon = isVehicle;
        }
        
        public void RestoreOriginalPlayerIcon()
        {
            if (originalPlayerIconPrefab == null) return;
            ReplaceWithIcon(originalPlayerIconPrefab, Constants.PlayerIconReplacementScale, isVehicle:false);
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
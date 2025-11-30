using MelonLoader;
using Small_Corner_Map.Helpers; // Added for Utils.CreateCircleSprite
using Small_Corner_Map.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map.Main
{
    public class PlayerMarkerManager
    {
        private RectTransform directionIndicator;
        private readonly Color markerColor = new Color(0.2f, 0.6f, 1f, 1f);

        private GameObject Marker { get; set; }
        private GameObject originalPlayerIconPrefab;
        private bool showingVehicleIcon;

        public GameObject CreatePlayerMarker(GameObject parent)
        {
            Marker = new GameObject("PlayerMarker");
            Marker.transform.SetParent(parent.transform, false);
            
            var rect = Marker.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Constants.PlayerMarkerSize, Constants.PlayerMarkerSize);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            
            var image = Marker.AddComponent<Image>();
            // Use the circular sprite
            image.sprite = Small_Corner_Map.Helpers.Utils.CreateCircleSprite(
                (int)Constants.PlayerMarkerSize, // Diameter
                markerColor,                     // Color
                Constants.MinimapCircleResolutionMultiplier, // Resolution
                Constants.MinimapMaskFeather,    // Feather
                true                             // Feather inside
            );
            image.color = markerColor;

            // Ensure player marker is drawn on top
            Marker.transform.SetAsLastSibling();

            return Marker;
        }

        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
            MelonLogger.Msg("PlayerMarkerManager: Replacing player marker with real icon: " + realIconPrefab?.name ?? "null");
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
                    directionIndicator = CreateDirectionIndicator(Marker, Color.white);
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

        public IEnumerator InitializePlayerMarkerIcon(GameObject mapObject)
        {
            if (mapObject == null) yield return new WaitForSeconds(2.0f);

            var playerPoI = mapObject.transform.Find("PlayerPoI(Clone)");
            var realIcon = playerPoI?.Find("IconContainer");
            if (realIcon != null)
            {
                ReplaceWithRealPlayerIcon(realIcon.gameObject);
                MelonLogger.Msg("MinimapUI: Replaced fallback player marker with real player icon.");
            }
        }

        /// <summary>
        /// Creates the direction indicator for the player marker.
        /// </summary>
        /// <param name="parent">Parent GameObject (player marker) to attach the indicator to.</param>
        /// <param name="indicatorColor">Color for the indicator.</param>
        /// <param name="size">Size of the indicator.</param>
        /// <returns>The created direction indicator RectTransform.</returns>
        public RectTransform CreateDirectionIndicator(GameObject parent, Color indicatorColor, float size = Constants.DirectionIndicatorSize)
        {
            var indicatorObject = new GameObject("DirectionIndicator");
            indicatorObject.transform.SetParent(parent.transform, false);
            
            var directionIndicator = indicatorObject.AddComponent<RectTransform>();
            directionIndicator.sizeDelta = new Vector2(size, size);
            directionIndicator.pivot = new Vector2(0.5f, 0.5f);
            
            var image = indicatorObject.AddComponent<Image>();
            image.color = indicatorColor;

            return directionIndicator;
        }
    }
}
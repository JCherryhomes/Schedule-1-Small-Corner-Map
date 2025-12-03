using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;
using MelonLoader;
using System.Collections;

namespace Small_Corner_Map.PoIManagers
{
    [RegisterTypeInIl2Cpp]
    public class PlayerMarkerView : MonoBehaviour
    {
        private Image _playerMarkerImage;
        private RectTransform _playerMarkerRect;
        private Transform _playerTransform;
        
        private RectTransform _directionIndicator;
        private GameObject _markerGO;
        private GameObject _originalPlayerIconPrefab;
        private bool _showingVehicleIcon;
        private readonly Color _markerColor = new Color(0.2f, 0.6f, 1f, 1f); // Original blue color

        // Store coordinate system primitives
        private float _worldScaleFactor;
        private float _minimapPlayerCenterXOffset;
        private float _minimapPlayerCenterYOffset;
        private float _currentZoomLevel;

        public void UpdateZoomLevel(float newZoomLevel)
        {
            _currentZoomLevel = newZoomLevel;
        }

        public void UpdateMinimapPlayerCenterXOffset(float newOffsetX)
        {
            _minimapPlayerCenterXOffset = newOffsetX;
        }

        public void UpdateMinimapPlayerCenterYOffset(float newOffsetY)
        {
            _minimapPlayerCenterYOffset = newOffsetY;
        }

        public void Initialize(Transform playerTransform, Transform parent, float worldScaleFactor, float currentZoomLevel, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
        {
            _playerTransform = playerTransform;
            _worldScaleFactor = worldScaleFactor;
            _minimapPlayerCenterXOffset = minimapPlayerCenterXOffset;
            _minimapPlayerCenterYOffset = minimapPlayerCenterYOffset;
            _currentZoomLevel = currentZoomLevel;

            _markerGO = new GameObject("PlayerMarker");
            _markerGO.transform.SetParent(parent, false);
            
            _playerMarkerRect = _markerGO.AddComponent<RectTransform>();
            _playerMarkerRect.sizeDelta = new Vector2(Constants.PlayerMarkerSize, Constants.PlayerMarkerSize);
            _playerMarkerRect.anchorMin = new Vector2(0.5f, 0.5f);
            _playerMarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
            _playerMarkerRect.pivot = new Vector2(0.5f, 0.5f);
            _playerMarkerRect.anchoredPosition = Vector2.zero; // Marker fixed in center of parent (MinimapContainer)
            
            _playerMarkerImage = _markerGO.AddComponent<Image>();
            _playerMarkerImage.sprite = Utils.CreateCircleSprite(
                Constants.PlayerMarkerCircleDrawingResolution, // Use higher resolution for drawing
                _markerColor, 
                Constants.MinimapCircleResolutionMultiplier, 
                Constants.MinimapMaskFeather
            );
            _playerMarkerImage.color = _markerColor;

            _markerGO.transform.SetAsLastSibling();
            _markerGO.SetActive(true); // Ensure marker is active

            // Re-enable asynchronous replacement with the real player icon
            MelonCoroutines.Start(InitializePlayerMarkerIcon());
        }

        void Update()
        {
            if (_playerTransform != null)
            {
                UpdateDirectionIndicator();
            }
        }
        
        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
            MelonLogger.Msg("PlayerMarkerView: Replacing player marker with real icon: " + (realIconPrefab?.name ?? "null"));
            if (realIconPrefab == null) return;
            _originalPlayerIconPrefab = realIconPrefab; // cache for restore
            ReplaceWithIcon(realIconPrefab, Constants.PlayerIconReplacementScale, isVehicle:false);
        }
        
        private void ReplaceWithIcon(GameObject iconPrefab, float scale, bool isVehicle)
        {
            if (_markerGO == null) return;
            
            var parent = _markerGO.transform.parent;
            var newMarker = Instantiate(iconPrefab, parent, false);
            newMarker.name = isVehicle ? "PlayerVehicleMarker" : "PlayerMarker";
            var newRect = newMarker.GetComponent<RectTransform>();
            if (newRect != null)
            {
                newRect.anchoredPosition = _playerMarkerRect.anchoredPosition; // Preserve position
                newRect.localScale = new Vector3(scale, scale, scale);
            }
            
            // Remove arrow if present on prefab instance
            var arrowImage = newMarker.transform.Find("Image");
            if (arrowImage != null)
                Destroy(arrowImage.gameObject);
            
            if (_directionIndicator != null)
            {
                _directionIndicator.SetParent(newMarker.transform, false);
                _directionIndicator.SetAsLastSibling();
            }
            
            Destroy(_markerGO);
            _markerGO = newMarker;
            _playerMarkerRect = newRect;
            _markerGO.transform.SetAsLastSibling();
            _showingVehicleIcon = isVehicle;
        }

        private void UpdateDirectionIndicator()
        {
            if (_markerGO == null || _playerTransform == null) return;

            if (_directionIndicator == null)
            {
                var indicatorTransform = _markerGO.transform.Find("DirectionIndicator");
                if (indicatorTransform != null)
                {
                    _directionIndicator = (RectTransform)indicatorTransform;
                }
                else
                {
                    _directionIndicator = CreateDirectionIndicator(_markerGO, Color.white);
                }
            }

            _directionIndicator.pivot = new Vector2(0.5f, 0.5f);
            var indicatorDistance = Constants.DirectionIndicatorDistance;
            var yRotation = _playerTransform.rotation.eulerAngles.y;
            var angleRad = (90f - yRotation) * Mathf.Deg2Rad;

            // Apply scaling to the indicator distance and reduce to half
            float scaledIndicatorDistance = indicatorDistance * MinimapCoordinateSystem.WorldToUIScale(_worldScaleFactor, _currentZoomLevel);

            var newPosition = new Vector2(
                scaledIndicatorDistance * Mathf.Cos(angleRad),
                scaledIndicatorDistance * Mathf.Sin(angleRad)
            );
            _directionIndicator.anchoredPosition = newPosition;
            
            // Rotate the indicator to point away from the marker (outward)
            // The angle is calculated so the chevron points in the direction of player facing
            var angleDeg = 90f - yRotation;
            _directionIndicator.localRotation = Quaternion.Euler(0, 0, angleDeg);
        }

        private IEnumerator InitializePlayerMarkerIcon()
        {
            // Wait for the map object to be available
            GameObject mapObject = null;
            while (mapObject == null)
            {
                mapObject = GameObject.Find(Constants.MapAppPath);
                yield return new WaitForSeconds(1.0f);
            }

            var playerPoI = mapObject.transform.Find("PlayerPoI(Clone)");
            var realIcon = playerPoI?.Find("IconContainer");
            if (realIcon != null)
            {
                ReplaceWithRealPlayerIcon(realIcon.gameObject);
                MelonLogger.Msg("PlayerMarkerView: Replaced fallback player marker with real player icon.");
                
                // Get the direction indicator sprite from MapApp
                var mapAppDirectionIndicator = realIcon.Find("Image");
                if (mapAppDirectionIndicator != null)
                {
                    var mapAppImage = mapAppDirectionIndicator.GetComponent<Image>();
                    if (mapAppImage != null && mapAppImage.sprite != null && _directionIndicator != null)
                    {
                        var ourDirectionImage = _directionIndicator.GetComponent<Image>();
                        if (ourDirectionImage != null)
                        {
                            ourDirectionImage.sprite = mapAppImage.sprite;
                            ourDirectionImage.color = Color.white; // Use white to show the sprite as-is
                            
                            // Match the size from MapApp
                            var mapAppRect = mapAppDirectionIndicator.GetComponent<RectTransform>();
                            if (mapAppRect != null)
                            {
                                _directionIndicator.sizeDelta = mapAppRect.sizeDelta;
                            }
                            
                            MelonLogger.Msg("PlayerMarkerView: Applied MapApp direction indicator sprite and size.");
                        }
                    }
                }
            }
        }
        
        private RectTransform CreateDirectionIndicator(GameObject parent, Color indicatorColor, float size = Constants.DirectionIndicatorSize)
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
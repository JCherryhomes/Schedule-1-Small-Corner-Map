using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;
using MelonLoader;



#if IL2CPP
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
using Il2CppScheduleOne.PlayerScripts;
#else
using Il2CppIEnumerator = System.Collections.IEnumerator;
using ScheduleOne.PlayerScripts;
#endif

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

        public void Initialize(Transform parent, float worldScaleFactor, float currentZoomLevel, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
        {
            _playerTransform = Player.Local.transform;
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
            // Add safety checks to prevent null reference exceptions during cleanup
            if (Player.Local == null || this == null || gameObject == null || !gameObject.activeInHierarchy)
            {
                return;
            }
            
            try
            {
                if (Player.Local.IsInVehicle)
                {
                    _playerTransform = Player.Local.CurrentVehicle?.transform;     
                }
                else
                {
                    _playerTransform = Player.Local.transform;
                }
                
                if (_playerTransform != null)
                {
                    UpdateDirectionIndicator();
                }
            }
            catch (System.Exception ex)
            {
                // Log exception but don't crash - this can happen during scene transitions
                MelonLogger.Warning($"[PlayerMarkerView] Exception in Update: {ex.Message}");
            }
        }
        
        public void ReplaceWithRealPlayerIcon(GameObject realIconPrefab)
        {
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

        private System.Collections.IEnumerator InitializePlayerMarkerIcon()
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using Small_Corner_Map.Helpers;

#if IL2CPP
using Il2CppScheduleOne.Vehicles;
#else
using ScheduleOne.Vehicles;
#endif

namespace Small_Corner_Map.PoIManagers
{
    [RegisterTypeInIl2Cpp]
    public class PoIMarkerView : MonoBehaviour
    {
        private RectTransform _thisRect;
        private RectTransform _poiMarkerRect;
        private RectTransform _sourceRect;
        private Vector2 _phoneMapPosition;
        private string poiName;
        private string _markerType;
        private float worldScaleFactor;
        private float currentZoomLevel;
        private bool _isDynamic;
        private bool _isCircleMode;

        private RectTransform _mapImageRT;
        private Vector2 _mapPosition;

        private static readonly HashSet<string> clampedMarkerTypes = new HashSet<string>
        {
            "QuestPoI(Clone)",
            "DeaddropPoI_Red(Clone)",
            "ContractPoI(Clone)"
        };

        public void Initialize(RectTransform poiTransform, Vector2 phoneMapPosition, float worldScale, float zoom, bool isDynamic, string markerType, RectTransform mapImageRT, bool isCircle)
        {
            _sourceRect = poiTransform;
            _phoneMapPosition = phoneMapPosition;
            poiName = poiTransform.name;
            _markerType = markerType;
            worldScaleFactor = worldScale;
            currentZoomLevel = zoom;
            _isDynamic = isDynamic;
            _mapImageRT = mapImageRT;
            _isCircleMode = isCircle;

            // Get the RectTransform (should already exist since it was added before this component)
            _thisRect = gameObject.GetComponent<RectTransform>();
            if (_thisRect == null)
            {
                MelonLogger.Error($"PoIMarkerView ({poiName}): RectTransform not found! This should not happen.");
                return;
            }
            
            // Set up this RectTransform with center anchoring
            _thisRect.anchorMin = new Vector2(0.5f, 0.5f);
            _thisRect.anchorMax = new Vector2(0.5f, 0.5f);
            _thisRect.pivot = new Vector2(0.5f, 0.5f);

            var markerGO = Instantiate(poiTransform.gameObject, transform, false);
            markerGO.name = "PoIContent_" + poiName;
            
            var layoutElement = markerGO.GetComponent<LayoutElement>();
            if (layoutElement != null) Destroy(layoutElement);

            _poiMarkerRect = markerGO.GetComponent<RectTransform>();
            if (_poiMarkerRect == null)
            {
                MelonLogger.Error($"PoIMarkerView ({poiName}): Cloned GameObject does not have a RectTransform!");
                Destroy(gameObject);
                return;
            }
            
            // Content should be centered at (0,0) relative to parent
            _poiMarkerRect.anchorMin = new Vector2(0.5f, 0.5f);
            _poiMarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
            _poiMarkerRect.pivot = new Vector2(0.5f, 0.5f);
            _poiMarkerRect.anchoredPosition = Vector2.zero;
            _poiMarkerRect.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            markerGO.SetActive(true);
            
            // Store the marker's true position on the map
            Vector2 scaledPosition = _phoneMapPosition * 0.25f;
            _mapPosition = new Vector2(scaledPosition.x + 2f, scaledPosition.y - 3f);
            
            // Set initial position
            _thisRect.anchoredPosition = _mapPosition;
        }
        
        public void SetShape(bool isCircle)
        {
            _isCircleMode = isCircle;
        }

        public void UpdateZoomLevel(float newZoomLevel)
        {
            currentZoomLevel = newZoomLevel;
        }

        public void UpdatePosition(Vector2 newPhoneMapPosition)
        {
            if (_thisRect == null)
                return;

            _phoneMapPosition = newPhoneMapPosition;
            Vector2 scaledPosition = _phoneMapPosition * 0.25f;
            _mapPosition = new Vector2(scaledPosition.x + 2f, scaledPosition.y  - 3f);
        }

        public void UpdatePositionFromWorld(Vector3 worldPos)
        {
            if (_thisRect == null) return;
            // Convert world position to the minimap's internal _mapPosition
            var positionWithOffset = worldPos + new Vector3(-10f, 0f, 6f); // Adjust if needed
            _mapPosition = MinimapCoordinateSystem.WorldToMapSpace(positionWithOffset, worldScaleFactor, currentZoomLevel);
        }

        private void Update()
        {
            if (_thisRect == null || _mapImageRT == null) return;
            
            Vector2 newAnchoredPosition = _mapPosition;

            if (clampedMarkerTypes.Contains(_markerType))
            {
                Vector2 positionRelativeToPlayer = _mapPosition + _mapImageRT.anchoredPosition;

                if (_isCircleMode)
                {
                    float distance = positionRelativeToPlayer.magnitude;
                    if (distance > MinimapState.MinimapRadius)
                    {
                        Vector2 clampedPlayerRelativePosition = positionRelativeToPlayer.normalized * MinimapState.MinimapRadius;
                        newAnchoredPosition = clampedPlayerRelativePosition - _mapImageRT.anchoredPosition;
                    }
                }
                else // Square mode
                {
                    float halfSize = MinimapState.MinimapRadius;
                    Vector2 clampedPlayerRelativePosition = new Vector2(
                        Mathf.Clamp(positionRelativeToPlayer.x, -halfSize, halfSize),
                        Mathf.Clamp(positionRelativeToPlayer.y, -halfSize, halfSize)
                    );

                    if (clampedPlayerRelativePosition != positionRelativeToPlayer)
                    {
                        newAnchoredPosition = clampedPlayerRelativePosition - _mapImageRT.anchoredPosition;
                    }
                }
            }
            
            _thisRect.anchoredPosition = newAnchoredPosition;
        }
    }
}
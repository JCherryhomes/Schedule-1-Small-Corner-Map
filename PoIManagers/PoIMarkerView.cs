using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;
using MelonLoader;

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
        private float worldScaleFactor;
        private float currentZoomLevel;
        private bool _isDynamic;

        public void Initialize(RectTransform poiTransform, Vector2 phoneMapPosition, float worldScale, float zoom, bool isDynamic)
        {
            _sourceRect = poiTransform;
            _phoneMapPosition = phoneMapPosition;
            poiName = poiTransform.name;
            worldScaleFactor = worldScale;
            currentZoomLevel = zoom;
            _isDynamic = isDynamic;

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
            
            // Set the position AFTER everything else is set up
            // Scale the phone map position to match the minimap size (approx 2000->500 => 0.25)
            Vector2 scaledPosition = _phoneMapPosition * 0.25f;
            scaledPosition = new Vector2(scaledPosition.x + 2f, scaledPosition.y - 3f);
            _thisRect.anchoredPosition = scaledPosition;
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
            _thisRect.anchoredPosition = scaledPosition;
        }
    }
}
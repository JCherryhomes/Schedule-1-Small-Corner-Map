using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class TimeDisplayView : MonoBehaviour
    {
        private Text _timeText;
        private GameObject _timeDisplayGO;
        private MelonPreferences_Entry<bool> _showGameTimePreference;

        public void ToggleVisibility(bool isVisible)
        {
            if (_timeDisplayGO != null)
            {
                _timeDisplayGO.SetActive(isVisible);
            }
        }

        public void Initialize(RectTransform parent, MelonPreferences_Entry<bool> showGameTimePreference)
        {
            _showGameTimePreference = showGameTimePreference;

            // Create the time display UI element
            _timeDisplayGO = new GameObject("TimeDisplay");
            _timeDisplayGO.transform.SetParent(parent, false);
            RectTransform timeDisplayRect = _timeDisplayGO.AddComponent<RectTransform>();

            // Add a background image
            Image backgroundImage = _timeDisplayGO.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.5f);
            
            // Set position and size
            timeDisplayRect.anchorMin = new Vector2(0.5f, 0);
            timeDisplayRect.anchorMax = new Vector2(0.5f, 0);
            timeDisplayRect.pivot = new Vector2(0.5f, 1);
            timeDisplayRect.anchoredPosition = new Vector2(0, -10); // 10 pixels below the parent
            timeDisplayRect.sizeDelta = new Vector2(100, 30);

            // Add the text component
            GameObject textGO = new GameObject("TimeText");
            textGO.transform.SetParent(timeDisplayRect, false);
            _timeText = textGO.AddComponent<Text>();
            _timeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _timeText.fontSize = 18;
            _timeText.alignment = TextAnchor.MiddleCenter;
            _timeText.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Set initial visibility
            _timeDisplayGO.SetActive(_showGameTimePreference.Value);
        }

        void Update()
        {
            if (_timeDisplayGO != null)
            {
                _timeDisplayGO.SetActive(_showGameTimePreference.Value);
            }

            // Update the time text
            if (_timeText != null && _showGameTimePreference.Value)
            {
                // Placeholder: Replace with actual in-game time when available
                _timeText.text = System.DateTime.Now.ToString("HH:mm");
            }
        }
    }
}

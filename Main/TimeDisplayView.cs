using Il2CppScheduleOne.GameTime;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class TimeDisplayView : MonoBehaviour
    {
        private Text timeText;
        private GameObject timeDisplayGo;
        private MelonPreferences_Entry<bool> showGameTimePreference;
        private const int Width = 110;
        private const int Height = 55;

        public void ToggleVisibility(bool isVisible)
        {
            if (timeDisplayGo != null)
            {
                timeDisplayGo.SetActive(isVisible);
            }
        }

        public void Initialize(RectTransform parent, MelonPreferences_Entry<bool> gameTimePreference)
        {
            showGameTimePreference = gameTimePreference;

            // Create the time display UI element
            timeDisplayGo = new GameObject("TimeDisplay");
            timeDisplayGo.transform.SetParent(parent, false);
            var timeDisplayRect = timeDisplayGo.AddComponent<RectTransform>();

            // Add a background image
            var backgroundImage = timeDisplayGo.AddComponent<Image>();
            backgroundImage.sprite = Utils.CreateRoundedRectSprite(Width, Height, 10, new Color(0, 0, 0, 0.5f));
            backgroundImage.type = Image.Type.Sliced;

            // Set position and size
            timeDisplayRect.anchorMin = new Vector2(0.5f, 0);
            timeDisplayRect.anchorMax = new Vector2(0.5f, 0);
            timeDisplayRect.pivot = new Vector2(0.5f, 1);
            timeDisplayRect.anchoredPosition = new Vector2(0, -10); // 10 pixels below the parent
            timeDisplayRect.sizeDelta = new Vector2(Width, Height);

            // Add the text component
            var textGo = new GameObject("TimeText");
            textGo.transform.SetParent(timeDisplayRect, false);
            timeText = textGo.AddComponent<Text>();
            timeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            timeText.fontSize = 14;
            timeText.alignment = TextAnchor.MiddleCenter;
            timeText.color = Color.white;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Set initial visibility
            timeDisplayGo.SetActive(this.showGameTimePreference.Value);
        }

        void Update()
        {
            if (timeDisplayGo)
            {
                timeDisplayGo.SetActive(showGameTimePreference.Value);
            }

            if (!showGameTimePreference.Value || !TimeManager.InstanceExists)
            {
                return;
            }

            // Update the time text
            if (!timeText) return;
            
            var currentTime = TimeManager.Instance.CurrentTime;
            var currentDay = TimeManager.Instance.CurrentDay;
            if (timeText)
            {
                timeText.text = currentDay + "\n" + TimeManager.Get12HourTime(currentTime);
            }
        }
    }
}
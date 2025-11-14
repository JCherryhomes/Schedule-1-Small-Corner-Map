using MelonLoader;
using S1API.GameTime;
using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map.Main
{
    public class MinimapTimeDisplay
    {
        private RectTransform minimapTimeContainer;
        private Text minimapTimeText;
        private MelonPreferences_Entry<bool> timeBarEnabled;

        public RectTransform Container => minimapTimeContainer;
        public Text TimeText => minimapTimeText;

        private bool enabled { get { return timeBarEnabled.Value; } }

        public void Create(Transform parent, MelonPreferences_Entry<bool> enabled)
        {
            timeBarEnabled = enabled;
            GameObject containerObject = new GameObject("MinimapTimeContainer");
            containerObject.transform.SetParent(parent, false);

            minimapTimeContainer = containerObject.AddComponent<RectTransform>();
            minimapTimeContainer.sizeDelta = new Vector2(100f, 50f);
            minimapTimeContainer.anchorMin = new Vector2(0.5f, 0f);
            minimapTimeContainer.anchorMax = new Vector2(0.5f, 0f);
            minimapTimeContainer.pivot = new Vector2(0.5f, 1f);

            minimapTimeContainer.anchoredPosition = new Vector2(0f, 20f);

            Image backgroundImage = containerObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            GameObject timeTextObject = new GameObject("MinimapTime");
            timeTextObject.transform.SetParent(containerObject.transform, false);

            RectTransform timeTextRect = timeTextObject.AddComponent<RectTransform>();
            timeTextRect.anchorMin = new Vector2(0f, 0f);
            timeTextRect.anchorMax = new Vector2(1f, 1f);
            timeTextRect.anchoredPosition = Vector2.zero;

            minimapTimeText = timeTextObject.AddComponent<Text>();
            minimapTimeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            minimapTimeText.fontSize = 16;
            minimapTimeText.color = Color.white;
            minimapTimeText.alignment = TextAnchor.MiddleCenter;
            minimapTimeText.text = minimapTimeText.text ?? "00:00:00";
            minimapTimeText.raycastTarget = false;

            if (!this.enabled)
            {
                containerObject.SetActive(false);
            }
        }

        public void SetTimeBarEnabled(bool enabled)
        {
            timeBarEnabled.Value = enabled;
            if (minimapTimeContainer != null)
                minimapTimeContainer.gameObject.SetActive(enabled);
        }

        public void UpdateMinimapTime()
        {
            try
            {
                var currentTime = TimeManager.GetFormatted12HourTime();
                var currentDay = TimeManager.CurrentDay;
                if (minimapTimeText != null && currentTime != null)
                {
                    minimapTimeText.text = currentDay.ToString() + "\n" + currentTime;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error("Error updating minimap time: " + ex.Message);
            }
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map
{
    public class MinimapTimeDisplay
    {
        private RectTransform minimapTimeContainer;
        private Text cachedTimeText;
        private Text minimapTimeText;
        private bool timeBarEnabled = true;

        public RectTransform Container => minimapTimeContainer;
        public Text TimeText => minimapTimeText;

        public void Create(Transform parent, bool doubleSizeEnabled, bool timeBarEnabled)
        {
            this.timeBarEnabled = timeBarEnabled;

            GameObject containerObject = new GameObject("MinimapTimeContainer");
            containerObject.transform.SetParent(parent, false);

            minimapTimeContainer = containerObject.AddComponent<RectTransform>();
            minimapTimeContainer.sizeDelta = new Vector2(100f, 50f);
            minimapTimeContainer.anchorMin = new Vector2(0.5f, 0f);
            minimapTimeContainer.anchorMax = new Vector2(0.5f, 0f);
            minimapTimeContainer.pivot = new Vector2(0.5f, 1f);

            minimapTimeContainer.anchoredPosition = doubleSizeEnabled
                ? new Vector2(0f, 40f)
                : new Vector2(0f, 10f);

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
            minimapTimeText.fontSize = 18;
            minimapTimeText.color = Color.white;
            minimapTimeText.alignment = TextAnchor.MiddleCenter;
            minimapTimeText.text = minimapTimeText.text ?? "00:00:00";
            minimapTimeText.raycastTarget = false;

            if (!timeBarEnabled)
            {
                containerObject.SetActive(false);
            }
        }

        public void SetTimeBarEnabled(bool enabled)
        {
            timeBarEnabled = enabled;
            if (minimapTimeContainer != null)
                minimapTimeContainer.gameObject.SetActive(enabled);
        }

        public void UpdatePosition(bool doubleSizeEnabled)
        {
            if (minimapTimeContainer != null)
            {
                minimapTimeContainer.anchoredPosition = doubleSizeEnabled
                    ? new Vector2(0f, 40f)
                    : new Vector2(0f, 10f);
            }
        }

        public void UpdateMinimapTime()
        {
            if (cachedTimeText == null)
            {
                GameObject val = GameObject.Find("GameplayMenu/Phone/phone/HomeScreen/InfoBar/Time");
                if (val != null)
                {
                    cachedTimeText = val.GetComponent<Text>();
                }
            }
            if (minimapTimeText != null && cachedTimeText != null)
            {
                string text = cachedTimeText.text;
                string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (array.Length >= 3)
                {
                    string text2 = array[0] + " " + array[1];
                    string text3 = array[^1];
                    minimapTimeText.text = text3 + "\n" + text2;
                }
                else
                {
                    minimapTimeText.text = text;
                }
            }
        }
    }
}
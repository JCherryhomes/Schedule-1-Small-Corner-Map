using MelonLoader;
using S1API.GameTime;
using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main;
    
public class MinimapTimeDisplay
{
    private RectTransform minimapTimeContainer;
    private Text minimapTimeText;
    private MelonPreferences_Entry<bool> timeBarEnabled;

    public RectTransform Container => minimapTimeContainer;
    public Text TimeText => minimapTimeText;

    private bool Enabled => timeBarEnabled.Value;

    public void Create(Transform parent, MelonPreferences_Entry<bool> timeEnabled)
    {
        timeBarEnabled = timeEnabled;
        var containerObject = new GameObject("MinimapTimeContainer");
        containerObject.transform.SetParent(parent, false);

        minimapTimeContainer = containerObject.AddComponent<RectTransform>();
        minimapTimeContainer.sizeDelta = new Vector2(Constants.TimeDisplayWidth, Constants.TimeDisplayHeight);
        minimapTimeContainer.anchorMin = new Vector2(0.5f, 0f);
        minimapTimeContainer.anchorMax = new Vector2(0.5f, 0f);
        minimapTimeContainer.pivot = new Vector2(0.5f, 1f);

        minimapTimeContainer.anchoredPosition = new Vector2(0f, Constants.TimeDisplayOffsetY);

        var backgroundImage = containerObject.AddComponent<Image>();
        backgroundImage.color = new Color(
            Constants.TimeBackgroundR, 
            Constants.TimeBackgroundG, 
            Constants.TimeBackgroundB, 
            Constants.TimeBackgroundA);

        var timeTextObject = new GameObject("MinimapTime");
        timeTextObject.transform.SetParent(containerObject.transform, false);

        var timeTextRect = timeTextObject.AddComponent<RectTransform>();
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

        if (!this.Enabled)
        {
            containerObject.SetActive(false);
        }
    }

    public void SetTimeBarEnabled(bool timeEnabled)
    {
        timeBarEnabled.Value = timeEnabled;
        if (minimapTimeContainer != null)
            minimapTimeContainer.gameObject.SetActive(timeEnabled);
    }

    public void UpdateMinimapTime()
    {
        try
        {
            var currentTime = TimeManager.GetFormatted12HourTime();
            var currentDay = TimeManager.CurrentDay;
            if (minimapTimeText != null)
            {
                minimapTimeText.text = currentDay.ToString() + "\n" + currentTime;
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error("Error updating minimap time: " + ex.Message);
        }
    }
}

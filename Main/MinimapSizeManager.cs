using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;
namespace Small_Corner_Map.Main;
/// <summary>
/// Manages minimap size calculations and dynamic resizing.
/// </summary>
internal class MinimapSizeManager
{
    private readonly MapPreferences mapPreferences;
    private RectTransform minimapFrameRect;
    private GameObject minimapDisplayObject;
    private GameObject minimapBorderObject;
    private Image minimapMaskImage;
    private Image minimapBorderImage;
    private MinimapContent minimapContent;
    private CompassManager compassManager;
    
    public float ScaledMinimapSize { get; private set; }
    public float ScaledMapContentSize { get; private set; }
    public float ScaledFrameSize => ScaledMinimapSize + (Constants.MinimapBorderThickness * 2f) + (Constants.MinimapBorderFeather * 4f);
    public float CurrentWorldScale => Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor;
    
    public MinimapSizeManager(MapPreferences preferences)
    {
        mapPreferences = preferences;
        RecalculateScaledSizes();
    }
    
    public void SetUIReferences(
        RectTransform frameRect,
        GameObject displayObject,
        GameObject borderObject,
        Image maskImage,
        Image borderImage,
        MinimapContent content)
    {
        minimapFrameRect = frameRect;
        minimapDisplayObject = displayObject;
        minimapBorderObject = borderObject;
        minimapMaskImage = maskImage;
        minimapBorderImage = borderImage;
        minimapContent = content;
    }
    public void SetCompassManager(CompassManager manager)
    {
        compassManager = manager;
    }
    
    public void SetMinimapVisible(bool visible)
    {
        if (minimapDisplayObject != null)
            minimapDisplayObject.SetActive(visible);
        
        if (minimapBorderObject != null)
            minimapBorderObject.SetActive(visible);
        
        compassManager?.SetVisible(visible); // rely on preference inside manager for final visibility
    }
    
    public void RecalculateScaledSizes()
    {
        var scale = mapPreferences.MinimapScaleFactor;
        ScaledMinimapSize = Constants.BaseMinimapSize * scale;
        ScaledMapContentSize = Constants.BaseMapContentSize * scale;
    }
    
    public void UpdateMinimapSize(bool regenerateMaskSprite = false)
    {
        RecalculateScaledSizes();
        
        // Frame needs to accommodate the border thickness + feather + extra buffer to prevent clipping
        if (minimapFrameRect != null)
        {
            minimapFrameRect.sizeDelta = new Vector2(ScaledFrameSize, ScaledFrameSize);
        }
        
        if (minimapDisplayObject != null)
        {
            var component = minimapDisplayObject.GetComponent<RectTransform>();
            var adjustedSize = ScaledMinimapSize + Constants.MinimapMaskDiameterOffset;
            component.sizeDelta = new Vector2(adjustedSize, adjustedSize);
            if (regenerateMaskSprite && minimapMaskImage != null)
                minimapMaskImage.sprite = MinimapUIFactory.CreateCircleSprite(
                    (int)adjustedSize,
                    Color.black,
                    Constants.MinimapCircleResolutionMultiplier,
                    Constants.MinimapMaskFeather,
                    featherInside: true);
        }
        // Resize border to remain slightly larger than mask
        if (minimapBorderObject != null)
        {
            var borderRect = minimapBorderObject.GetComponent<RectTransform>();
            var borderDiameter = ScaledMinimapSize + (Constants.MinimapBorderThickness * 2f);
            borderRect.sizeDelta = new Vector2(borderDiameter, borderDiameter);
            if (regenerateMaskSprite && minimapBorderImage != null)
            {
                var borderColor = new Color(
                    Constants.MinimapBorderR,
                    Constants.MinimapBorderG,
                    Constants.MinimapBorderB,
                    Constants.MinimapBorderA);
                minimapBorderImage.sprite = MinimapUIFactory.CreateCircleSprite(
                    (int)borderDiameter,
                    borderColor,
                    Constants.MinimapCircleResolutionMultiplier,
                    Constants.MinimapBorderFeather,
                    featherInside: false);
            }
        }
        if (minimapContent?.MapContentObject == null) return;
        var contentRect = minimapContent.MapContentObject.GetComponent<RectTransform>();
        if (contentRect != null)
            contentRect.sizeDelta = new Vector2(ScaledMapContentSize, ScaledMapContentSize);
        compassManager?.UpdateLayout(ScaledMinimapSize);
    }
}

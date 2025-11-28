using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    /// <summary>
    /// Handles resizing of all minimap UI elements when the size preference changes.
    /// </summary>
    internal class MinimapResizer
    {
        private readonly MinimapCoordinateSystem coordinateSystem;
        
        // UI element references
        private RectTransform frameRect;
        private RectTransform maskRect;
        private RectTransform borderRect;
        private RectTransform contentRect;
        private Image maskImage;
        private Image borderImage;
        private CompassManager compassManager;

        public MinimapResizer(MinimapCoordinateSystem coordSystem)
        {
            coordinateSystem = coordSystem;
        }

        /// <summary>
        /// Stores references to UI elements that will be resized.
        /// Call this once after creating the UI.
        /// </summary>
        public void SetUIReferences(
            RectTransform frame,
            RectTransform mask,
            RectTransform border,
            RectTransform content,
            Image maskImg,
            Image borderImg,
            CompassManager compass)
        {
            frameRect = frame;
            maskRect = mask;
            borderRect = border;
            contentRect = content;
            maskImage = maskImg;
            borderImage = borderImg;
            compassManager = compass;
        }

        /// <summary>
        /// Resizes all minimap UI elements to the specified size multiplier.
        /// </summary>
        /// <param name="sizeMultiplier">Size multiplier (1.0 = default, 1.5 = increased)</param>
        /// <param name="regenerateSprites">Whether to regenerate circular sprites for better quality</param>
        public void Resize(float sizeMultiplier, bool regenerateSprites = true)
        {
            // Update coordinate system
            coordinateSystem.SetSizeMultiplier(sizeMultiplier);
            
            // Calculate scaled sizes
            var minimapSize = Constants.BaseMinimapSize * sizeMultiplier;
            var frameSize = minimapSize + (Constants.MinimapBorderThickness * 2f) + (Constants.MinimapBorderFeather * 4f);
            var contentSize = Constants.BaseMapContentSize * sizeMultiplier;
            
            // Resize frame
            if (frameRect != null)
            {
                frameRect.sizeDelta = new Vector2(frameSize, frameSize);
            }
            
            // Resize mask
            if (maskRect != null)
            {
                var adjustedSize = minimapSize + Constants.MinimapMaskDiameterOffset;
                maskRect.sizeDelta = new Vector2(adjustedSize, adjustedSize);
                
                if (regenerateSprites && maskImage != null)
                {
                    maskImage.sprite = MinimapUIFactory.CreateCircleSprite(
                        (int)adjustedSize,
                        Color.black,
                        Constants.MinimapCircleResolutionMultiplier,
                        Constants.MinimapMaskFeather,
                        featherInside: true);
                }
            }
            
            // Resize border
            if (borderRect != null)
            {
                var borderDiameter = minimapSize + (Constants.MinimapBorderThickness * 2f);
                borderRect.sizeDelta = new Vector2(borderDiameter, borderDiameter);
                
                if (regenerateSprites && borderImage != null)
                {
                    var borderColor = new Color(
                        Constants.MinimapBorderR,
                        Constants.MinimapBorderG,
                        Constants.MinimapBorderB,
                        Constants.MinimapBorderA);
                        
                    borderImage.sprite = MinimapUIFactory.CreateCircleSprite(
                        (int)borderDiameter,
                        borderColor,
                        Constants.MinimapCircleResolutionMultiplier,
                        Constants.MinimapBorderFeather,
                        featherInside: false);
                }
            }
            
            // Resize map content container
            if (contentRect != null)
            {
                contentRect.sizeDelta = new Vector2(contentSize, contentSize);
            }
            
            // Update compass layout
            if (compassManager != null)
            {
                var maskDiameterWithOffset = minimapSize + Constants.MinimapMaskDiameterOffset;
                compassManager.UpdateLayout(maskDiameterWithOffset);
            }
        }
    }
}

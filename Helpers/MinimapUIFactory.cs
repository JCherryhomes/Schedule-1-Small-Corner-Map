using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map.Helpers
{
    /// <summary>
    /// Factory class for creating minimap UI GameObjects.
    /// </summary>
    public static class MinimapUIFactory
    {
        /// <summary>
        /// Creates the root canvas for the minimap UI.
        /// </summary>
        /// <param name="parent">Parent GameObject to attach the canvas to.</param>
        /// <returns>The created canvas GameObject.</returns>
        public static GameObject CreateCanvas(GameObject parent)
        {
            var canvasObject = new GameObject("MinimapCanvas");
            canvasObject.transform.SetParent(parent.transform, false);
            
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            
            canvasObject.AddComponent<GraphicRaycaster>();

            return canvasObject;
        }

        /// <summary>
        /// Creates the minimap frame (positions minimap in the corner).
        /// </summary>
        /// <param name="parent">Parent GameObject to attach the frame to.</param>
        /// <param name="size">Size of the frame.</param>
        /// <returns>The created frame GameObject and its RectTransform.</returns>
        public static (GameObject frameObject, RectTransform rectTransform) CreateFrame(GameObject parent, float size)
        {
            var frameObject = new GameObject("MinimapFrame");
            frameObject.transform.SetParent(parent.transform, false);
            
            var rectTransform = frameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-20f, -20f);
            rectTransform.sizeDelta = new Vector2(size, size);

            return (frameObject, rectTransform);
        }

        /// <summary>
        /// Creates the minimap mask (circular area for minimap).
        /// </summary>
        /// <param name="parent">Parent GameObject to attach the mask to.</param>
        /// <param name="size">Size of the mask.</param>
        /// <returns>The created mask GameObject and its Image component.</returns>
        public static (GameObject maskObject, Image maskImage) CreateMask(GameObject parent, float size)
        {
            var maskObject = new GameObject("MinimapMask");
            maskObject.transform.SetParent(parent.transform, false);
            
            var maskRect = maskObject.AddComponent<RectTransform>();
            maskRect.sizeDelta = new Vector2(size, size);
            maskRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.anchoredPosition = Vector2.zero;
            
            maskObject.AddComponent<Mask>().showMaskGraphic = false;
            
            var maskImage = maskObject.AddComponent<Image>();
            maskImage.sprite = CreateCircleSprite((int)size, Color.black);
            maskImage.type = Image.Type.Sliced;
            maskImage.color = Color.black;

            return (maskObject, maskImage);
        }

        /// <summary>
        /// Creates a thin border behind the minimap mask.
        /// </summary>
        /// <param name="parent">Parent (frame) object.</param>
        /// <param name="maskDiameter">Diameter of the current minimap mask.</param>
        /// <returns>Border GameObject and Image.</returns>
        public static (GameObject borderObject, Image borderImage) CreateBorder(GameObject parent, float maskDiameter)
        {
            var borderObject = new GameObject("MinimapBorder");
            borderObject.transform.SetParent(parent.transform, false);
            var thickness = Constants.MinimapBorderThickness;
            var borderDiameter = maskDiameter + (thickness * 2f);
            var rect = borderObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(borderDiameter, borderDiameter);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var img = borderObject.AddComponent<Image>();
            var borderColor = new Color(Constants.MinimapBorderR, Constants.MinimapBorderG, Constants.MinimapBorderB, Constants.MinimapBorderA);
            img.sprite = CreateCircleSprite((int)borderDiameter, borderColor, Constants.MinimapCircleResolutionMultiplier, Constants.MinimapBorderFeather, featherInside:false);
            img.type = Image.Type.Simple;
            img.raycastTarget = false;
            // Ensure border rendered below mask
            borderObject.transform.SetAsFirstSibling();
            return (borderObject, img);
        }

        /// <summary>
        /// Creates a filled circle sprite with optional feather (edge anti-aliasing).
        /// </summary>
        /// <param name="diameter">Base diameter in pixels.</param>
        /// <param name="color">Fill color.</param>
        /// <param name="resolutionMultiplier">Resolution multiplier for higher definition.</param>
        /// <param name="featherWidth">Edge feather width in pixels (inside or outside based on featherInside).</param>
        /// <param name="featherInside">If true, feather fades inward; else outward.</param>
        /// <returns>The created circle sprite.</returns>
        public static Sprite CreateCircleSprite(int diameter, Color color, int resolutionMultiplier = 1, int featherWidth = 0, bool featherInside = true)
        {
            var texSize = diameter * resolutionMultiplier;
            var texture = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            var clear = new Color(0f, 0f, 0f, 0f);
            for (var y = 0; y < texSize; y++)
                for (var x = 0; x < texSize; x++)
                    texture.SetPixel(x, y, clear);

            var radius = texSize / 2f;
            var center = new Vector2(radius, radius);
            var maxDist = radius;
            var effectiveFeather = Mathf.Max(0, featherWidth * resolutionMultiplier);

            for (var y = 0; y < texSize; y++)
            {
                for (var x = 0; x < texSize; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= maxDist)
                    {
                        float alpha = color.a;
                        if (effectiveFeather > 0)
                        {
                            if (featherInside)
                            {
                                // Fade near edge inside circle
                                var edgeDist = maxDist - dist;
                                if (edgeDist <= effectiveFeather)
                                {
                                    alpha *= Mathf.Clamp01(edgeDist / effectiveFeather);
                                }
                            }
                            else
                            {
                                // Outward feather: keep solid inner region, add soft outer ring
                                var edgeDist = maxDist - dist;
                                if (edgeDist <= effectiveFeather)
                                {
                                    // Inner edge solid, we will later allow full alpha; outward handled by mask using larger diameter
                                    alpha *= Mathf.Clamp01(edgeDist / effectiveFeather);
                                }
                            }
                        }
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), resolutionMultiplier);
        }

        /// <summary>
        /// Creates the player marker GameObject.
        /// </summary>
        /// <param name="parent">Parent GameObject to attach the marker to.</param>
        /// <param name="markerColor">Color for the marker.</param>
        /// <param name="size">Size of the marker.</param>
        /// <returns>The created player marker GameObject.</returns>
        public static GameObject CreatePlayerMarker(GameObject parent, Color markerColor, float size = 10f)
        {
            var marker = new GameObject("PlayerMarker");
            marker.transform.SetParent(parent.transform, false);
            
            var rect = marker.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            
            var image = marker.AddComponent<Image>();
            image.color = markerColor;

            // Ensure player marker is drawn on top
            marker.transform.SetAsLastSibling();

            return marker;
        }

        /// <summary>
        /// Creates the direction indicator for the player marker.
        /// </summary>
        /// <param name="parent">Parent GameObject (player marker) to attach the indicator to.</param>
        /// <param name="indicatorColor">Color for the indicator.</param>
        /// <param name="size">Size of the indicator.</param>
        /// <returns>The created direction indicator RectTransform.</returns>
        public static RectTransform CreateDirectionIndicator(GameObject parent, Color indicatorColor, float size = Constants.DirectionIndicatorSize)
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

#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.PoIManagers;

namespace Small_Corner_Map.UI
{
    /// <summary>
    /// A builder class for creating and displaying rectangular or circular UI elements on the screen.
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class UIBuilder : MonoBehaviour
    {
        // UI Hierarchy References
        private GameObject _uiContainerParent; // The main parent
        private GameObject _minimapRootGO;     // The main ScrollView GameObject
        private GameObject _canvasGO;          // The Canvas GameObject
        private GameObject _contentGO;         // The Content GameObject inside the ScrollRect
        private GameObject _internalMapImageGO;
        private ScrollRect _scrollRect;        // The core logic component
        private Image _mapImage;               // The Image component used for background/masking
        private Image _internalMapImage;
        private float _currentZoomLevel = 1.0f;
        
        public GameObject MinimapRoot => _minimapRootGO;
        private MinimapContentManager _contentManager;

        public static GameObject CachedMapContent;

        // Sprites needed for switching styles
        private Sprite _rectangleSprite; // A simple white square sprite (default Unity UI sprite)
        private Sprite _circleSprite;    // A dynamically generated circular sprite

        internal Player PlayerObject { get; private set; }

        public UIBuilder SetParentContainer(GameObject parent)
        {
            _uiContainerParent = parent;
            _canvasGO = new GameObject("Minimap_Root_Canvas");
            _canvasGO.transform.SetParent(_uiContainerParent.transform, false);
            var canvas = _canvasGO.AddComponent<Canvas>();

            // 3. Configure the Canvas settings (e.g., Screen Space Overlay is common for HUDs)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 4. Add required helper components for layout and interaction
            _canvasGO.AddComponent<CanvasScaler>();
            _canvasGO.AddComponent<GraphicRaycaster>();

            return this;
        }

        // --- Core Setup Function ---
        public void InitializeMinimapUI(bool useSquare)
        {
            if (_uiContainerParent == null)
            {
                MelonLogger.Error("UIBuilder: Parent container not set!");
                return;
            }

            // 1. Create the root GameObject ("Minimap_ScrollView")
            _minimapRootGO = new GameObject("Minimap_ScrollView_Root");
            var rootRT = _minimapRootGO.AddComponent<RectTransform>();
            rootRT.SetParent(_canvasGO.transform, false);
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = Vector2.zero;
            rootRT.anchoredPosition = Vector2.zero;

            // Add the content manager
            _contentManager = _minimapRootGO.AddComponent<MinimapContentManager>();

            // 2. Add the Visual Components (Image and Mask)
            // This is the component we will swap the sprite on.
            _mapImage = _minimapRootGO.AddComponent<Image>();
            // Add a Mask component to clip content to the shape of the Image sprite
            _minimapRootGO.AddComponent<Mask>().showMaskGraphic = true; // Use the image's shape

            // 3. Add the ScrollRect logic component
            _scrollRect = _minimapRootGO.AddComponent<ScrollRect>();
            _scrollRect.viewport = rootRT; // Viewport is the root object itself
            _scrollRect.movementType = ScrollRect.MovementType.Clamped; // Example setting

            // 4. Create the Content GameObject (Child of root)
            _contentGO = new GameObject("Minimap_Content");
            var contentRT = _contentGO.AddComponent<RectTransform>();
            contentRT.SetParent(rootRT, false);
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            contentRT.sizeDelta = new Vector2(0, 0); // Size dynamically based on content

            // Link Content to ScrollRect
            _scrollRect.content = contentRT;

            // Initialize default sprites
            // You need a default Unity sprite for the rectangle shape
            _rectangleSprite = CreateSquareSprite(512, Color.white);
            _circleSprite = CreateCircleSprite(512, Color.white);

            // Set initial style based on user preference (e.g., preference "UseCircleMap")
            SetStyle(useSquare ? "Rectangle" : "Circle");
        }

        // --- Style Switching Functions ---

        /// <summary>
        /// Switches the visual appearance of the minimap root GameObject
        /// </summary>
        public void SetStyle(string styleName)
        {
            if (_mapImage == null) return;

            if (styleName == "Circle")
            {
                _mapImage.sprite = _circleSprite;
                // Ensure Image Type is Simple for standard masking
                _mapImage.type = Image.Type.Simple;
            }
            else // Default to Rectangle
            {
                _mapImage.sprite = _rectangleSprite;
                _mapImage.type = Image.Type.Sliced; // Sliced usually works better for rectangles
            }
        }

        internal IEnumerator IntegrateWithScene()
        {
            MelonLogger.Msg("UIBuilder: Looking for game objects in the scene.");
            yield return new WaitForSeconds(2f);

            GameObject mapAppObject = null;
            GameObject viewportObject = null;
            var attempts = 0;
            PlayerObject ??= Player.Local;
            while ((mapAppObject == null || PlayerObject == null) && attempts < 30)
            {
                attempts++;
                if (mapAppObject == null) mapAppObject = FindMapApp();
                if (mapAppObject != null && viewportObject == null) viewportObject = FindViewport(mapAppObject);
                if (mapAppObject == null || PlayerObject == null)
                    yield return new WaitForSeconds(Constants.SceneIntegrationRetryDelay);
            }
            LogIntegrationResults(mapAppObject, viewportObject);
            if (viewportObject != null) ApplyMapSprite(viewportObject);
            CachedMapContent = GameObject.Find(Constants.MapAppPath);
            if (CachedMapContent != null)
            {
                // Load Player Icon Here
                MelonLogger.Msg("UIBuilder: Cached map content found.");
            }
            else
            {
                MelonLogger.Warning("UIBuilder: Cached map content not found.");
            }

            // Initialize the content manager
            if (_contentManager != null && PlayerObject != null && _contentGO != null)
            {
                _contentManager.Initialize(_contentGO.GetComponent<RectTransform>(), PlayerObject.transform);
            }

            AdjustZoom(1f);
            yield break;
        }

        /// <summary>
        /// Zooms the minimap by changing the image's size, allowing the ScrollRect to function.
        /// </summary>
        public void AdjustZoom(float zoomMultiplier)
        {
            if (_internalMapImageGO == null) return;

            var imageRT = _internalMapImageGO.GetComponent<RectTransform>();

            // Get the current size and apply the multiplier
            Vector2 currentSize = imageRT.sizeDelta;
            Vector2 newSize = currentSize * zoomMultiplier;

            imageRT.sizeDelta = newSize;
        }

        private Vector2 GetAnchorVector()
        {
            return new Vector2(0.5f, 0.5f);
        }

        private GameObject FindMapApp()
        {
            MelonLogger.Msg("UIBuilder: Searching for MapApp.");
            var gameplayMenu = GameObject.Find("GameplayMenu");
            var mapApp = gameplayMenu?.transform.Find("Phone")?.Find("phone")?.Find("AppsCanvas")?.Find("MapApp");
            if (mapApp != null) MelonLogger.Msg("UIBuilder: Found MapApp.");
            else MelonLogger.Warning("UIBuilder: MapApp not found.");
            return mapApp?.gameObject;
        }

        private GameObject FindViewport(GameObject mapAppObject)
        {
            MelonLogger.Msg("UIBuilder: Searching for Viewport in MapApp.");
            if (mapAppObject == null)
            {
                MelonLogger.Warning("UIBuilder: MapApp object is null.");
                return null;
            }
            var viewport = mapAppObject.transform.Find("Container")?.Find("Scroll View")?.Find("Viewport");
            if (viewport != null) MelonLogger.Msg("UIBuilder: Found Viewport.");
            else MelonLogger.Warning("UIBuilder: Viewport not found.");
            return viewport?.gameObject;
        }

        private void LogIntegrationResults(GameObject mapAppObject, GameObject viewportObject)
        {
            if (viewportObject != null) { MelonLogger.Msg("UIBuilder: Viewport object found."); }
            else { MelonLogger.Warning("UIBuilder: Viewport object not found."); }
            if (PlayerObject == null) { MelonLogger.Warning("UIBuilder: Player object not found."); }
            MelonLogger.Msg("UIBuilder: Game object search completed.");
        }

        private void ApplyMapSprite(GameObject viewportObject)
        {
            try
            {
                if (viewportObject.transform.childCount <= 0) return;
                var contentTransform = viewportObject.transform.GetChild(0);
                MelonLogger.Msg("MinimapUI: Found viewport content: " + contentTransform.name);
                var mapImage = contentTransform.GetComponent<Image>();
                if (mapImage != null && mapImage.sprite != null)
                {
                    ApplySpriteToMinimap(mapImage);
                    return;
                }
                MelonLogger.Msg("MinimapUI: Content doesn't have an Image component or sprite");
                TryApplySpriteFromChildren(contentTransform);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("MinimapUI: Error accessing map content: " + ex.Message);
            }
        }

        private void ApplySpriteToMinimap(Image sourceImage)
        {
            MelonLogger.Msg("UIBuilder: Applying sprite to minimap.");
            GameObject mapImageInContentGO = new GameObject("InternalMapDisplayObject");
            mapImageInContentGO.transform.SetParent(_contentGO.transform, false);

            // Get the component and store it in our NEW field
            _internalMapImage = mapImageInContentGO.AddComponent<Image>();
            _internalMapImage.sprite = sourceImage.sprite;
            _internalMapImage.SetNativeSize(); // Set original size

            MelonLogger.Msg("UIBuilder: Successfully applied map sprite to minimap.");
        }

        /// <summary>
        /// Creates a dynamically generated square sprite of a specified size and color.
        /// </summary>
        /// <param name="size">The width and height in pixels (e.g., 512).</param>
        /// <param name="color">The color to fill the square with.</param>
        /// <returns>The newly created Sprite.</returns>
        private Sprite CreateSquareSprite(int size, Color color)
        {
            // 1. Create a new Texture2D
            // Format RGBA32 allows transparency; false means no mipmaps needed for UI
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // 2. Fill the texture with the specified color
            // Create an array of colors equal to the total number of pixels (size * size)
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply(); // Apply the changes to the texture

            // 3. Create a Sprite from the Texture2D
            // Rect defines the area of the texture to use (the whole thing in this case)
            Rect rect = new Rect(0, 0, size, size);

            // Pivot point (Vector2(0.5f, 0.5f) is center)
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // Pixels Per Unit (standard for UI is 100, but can be adjusted)
            float pixelsPerUnit = 100.0f;

            Sprite newSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
            newSprite.name = "Minimap_SquareSprite";

            return newSprite;
        }

        private Sprite CreateCircleSprite(int diameter, Color color, int resolutionMultiplier = 1, int featherWidth = 0, bool featherInside = true)
        {
            MelonLogger.Msg("UIBuilder: Creating circle sprite.");
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
            var effectiveFeather = Mathf.Max(0, featherWidth * resolutionMultiplier);

            for (var y = 0; y < texSize; y++)
            {
                for (var x = 0; x < texSize; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = 0f;
                    if (featherInside)
                    {
                        if (!(dist <= radius)) continue;
                        alpha = color.a;
                        if (effectiveFeather > 0)
                        {
                            var edgeDist = radius - dist;
                            if (edgeDist <= effectiveFeather)
                            {
                                alpha *= Mathf.Clamp01(edgeDist / effectiveFeather);
                            }
                        }
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        if (dist <= radius)
                        {
                            alpha = color.a;
                            texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                        }
                        else if (dist > radius && dist <= radius + effectiveFeather)
                        {
                            var edgeDist = dist - radius;
                            alpha = color.a * Mathf.Clamp01(1f - (edgeDist / effectiveFeather));
                            texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                        }
                    }
                }
            }

            texture.Apply();
            MelonLogger.Msg("UIBuilder: Circle sprite created.");
            return Sprite.Create(texture, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), resolutionMultiplier);
        }

        private void TryApplySpriteFromChildren(Transform contentTransform)
        {
            for (var i = 0; i < contentTransform.childCount; i++)
            {
                var child = contentTransform.GetChild(i);
                var childImage = child.GetComponent<Image>();
                if (childImage != null && childImage.sprite != null)
                {
                    MelonLogger.Msg("MinimapUI: Found image in content child: " + childImage.name + ", Sprite: " + childImage.sprite.name);
                    ApplySpriteToMinimap(childImage);
                    break;
                }
            }
        }
    }
}
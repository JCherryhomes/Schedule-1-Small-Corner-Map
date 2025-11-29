#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO; // Added for file operations
using Small_Corner_Map.Helpers;
using Small_Corner_Map.PoIManagers;
using Small_Corner_Map.Main;

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
        // private GameObject _cachedMapContent;  // Cached reference to the game's original map content (REMOVED)
        private ScrollRect _scrollRect;        // The core logic component
        private Image _mapImage;               // The Image component used for background/masking
        private Image _internalMapImage;
        private float _currentZoomLevel = 1.0f;

        // Sprites needed for switching styles
        private Sprite _rectangleSprite; // A simple white square sprite (default Unity UI sprite)
        private Sprite _circleSprite;    // A dynamically generated circular sprite
        private MinimapContentManager _contentManager;

        internal Player PlayerObject { get; private set; }
        private PlayerMarkerManager _playerMarkerManager;
        
                public GameObject MinimapRoot => _minimapRootGO;
                public GameObject MinimapContent => _contentGO;
                public MinimapContentManager ContentManager => _contentManager;
                private MinimapCoordinateSystem _sharedCoordinateSystem;
        
                // public GameObject CachedMapContent => _cachedMapContent; // REMOVED
        
                public void SetPlayerMarkerManager(PlayerMarkerManager playerMarkerManager)
                {
                    _playerMarkerManager = playerMarkerManager;
                }
        
                public void SetMinimapCoordinateSystem(MinimapCoordinateSystem system)
                {
                    _sharedCoordinateSystem = system;
                }
                
                // --- Core Setup Function ---
                public void InitializeMinimapUI(bool useSquare, float minimapSize)
                {            // Create Canvas
            _canvasGO = new GameObject("Minimap_Root_Canvas");
            _canvasGO.transform.SetParent(transform, false);
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            var canvasScaler = _canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            _canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create Frame
            var frameObject = new GameObject("MinimapFrame");
            frameObject.transform.SetParent(_canvasGO.transform, false);
            var frameRT = frameObject.AddComponent<RectTransform>();
            frameRT.anchorMin = new Vector2(1f, 1f);
            frameRT.anchorMax = new Vector2(1f, 1f);
            frameRT.pivot = new Vector2(1f, 1f);
            frameRT.anchoredPosition = new Vector2(-20f, -20f);
            frameRT.sizeDelta = new Vector2(minimapSize, minimapSize);

            // 1. Create the root GameObject ("Minimap_ScrollView")
            _minimapRootGO = new GameObject("Minimap_ScrollView_Root");
            var rootRT = _minimapRootGO.AddComponent<RectTransform>();
            rootRT.SetParent(frameObject.transform, false);
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

            // 4. Create the Content GameObject (Child of root)
            _contentGO = new GameObject("Minimap_Content");
            var contentRT = _contentGO.AddComponent<RectTransform>();
            contentRT.SetParent(rootRT, false);
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            contentRT.sizeDelta = new Vector2(Constants.BaseMapContentSize, Constants.BaseMapContentSize); // Set defined size

            // Initialize default sprites
            // You need a default Unity sprite for the rectangle shape
            _rectangleSprite = CreateSquareSprite(512, Color.gray);
            _circleSprite = CreateCircleSprite(512, Color.gray);

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
            MelonLogger.Msg("UIBuilder: Integrating with scene (loading static map).");
            yield return new WaitForSeconds(Constants.SceneIntegrationInitialDelay); // Wait a bit for scene to settle

            LoadStaticMapSprite(); // Call new method to load the static PNG

            // Initialize the content manager now that we have the player object
            if (ContentManager != null && PlayerObject != null && MinimapContent != null && _sharedCoordinateSystem != null && _playerMarkerManager != null)
            {
                MelonCoroutines.Start(ContentManager.Initialize(MinimapContent.GetComponent<RectTransform>(), PlayerObject.transform, _sharedCoordinateSystem, _playerMarkerManager));
            }

            AdjustZoom(1f); // Initial zoom
            yield break;
        }

        /// <summary>
        /// Zooms the minimap by changing the image's size, allowing the ScrollRect to function.
        /// </summary>
        public void AdjustZoom(float zoomMultiplier)
        {
            if (_internalMapImageGO == null || _sharedCoordinateSystem == null || _internalMapImage.sprite == null) return;

            _sharedCoordinateSystem.SetCurrentZoomLevel(zoomMultiplier);

            var imageRT = _internalMapImageGO.GetComponent<RectTransform>();

            // Calculate new size based on sprite's native size and the initial scaling factor and current zoom level
            Vector2 newSize = _internalMapImage.sprite.rect.size * Constants.InitialMapImageScale * zoomMultiplier;

            imageRT.sizeDelta = newSize;
        }

        private Vector2 GetAnchorVector()
        {
            return new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Loads the static minimap PNG image from the specified path and applies it to the minimap UI.
        /// </summary>
        private void LoadStaticMapSprite()
        {
            MelonLogger.Msg($"UIBuilder: Attempting to load static minimap image from {Constants.MinimapImagePath}");
            try
            {
                // Load the image file as a Texture2D
                byte[] fileData = File.ReadAllBytes(Constants.MinimapImagePath);
                Texture2D texture = new Texture2D(2, 2); // Create empty texture
                if (texture.LoadImage(fileData)) // Load image data into the texture
                {
                    // Create a sprite from the texture
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 50f); // PPU 50

                    // Assign the sprite to the internal map image
                    GameObject mapImageInContentGO = new GameObject("InternalMapDisplayObject");
                    _internalMapImageGO = mapImageInContentGO;
                    _internalMapImageGO.transform.SetParent(_contentGO.transform, false);

                    _internalMapImage = mapImageInContentGO.AddComponent<Image>();
                    _internalMapImage.sprite = sprite;
                    _internalMapImage.SetNativeSize(); // Set original size

                    MelonLogger.Msg("UIBuilder: Successfully loaded and applied static map sprite.");
                }
                else
                {
                    MelonLogger.Error($"UIBuilder: Failed to load image from {Constants.MinimapImagePath}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"UIBuilder: Error loading static map sprite: {ex.Message}");
            }
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
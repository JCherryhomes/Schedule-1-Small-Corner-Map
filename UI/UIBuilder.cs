#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
using Il2CppSystem.Security.Cryptography;
#else
using ScheduleOne.PlayerScripts;
using System.Security.Cryptography;
#endif

using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO; // Added for file operations
using System.Reflection; // Added for embedded resources
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
        private GameObject _cachedMapContent;  // Cached reference to the game's original map content
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
        
                public GameObject CachedMapContent => _cachedMapContent; // Restored
        
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
            _mapImage.color = Color.clear; // Make the mask graphic transparent initially
            // Add a Mask component to clip content to the shape of the Image sprite
            // _minimapRootGO.AddComponent<Mask>().showMaskGraphic = true; // Use the image's shape (TEMPORARILY DISABLED)

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
            _rectangleSprite = Helpers.Utils.CreateSquareSprite(512, Color.gray);
            _circleSprite = Helpers.Utils.CreateCircleSprite(512, Color.gray);

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

            // Wait for PlayerObject to be available
            var attempts = 0;
            PlayerObject ??= Player.Local; // Try to assign PlayerObject
            while (PlayerObject == null && attempts < Constants.SceneIntegrationMaxAttempts)
            {
                attempts++;
                MelonLogger.Msg($"UIBuilder: Waiting for PlayerObject (attempt {attempts}/{Constants.SceneIntegrationMaxAttempts})");
                yield return new WaitForSeconds(Constants.SceneIntegrationRetryDelay);
                PlayerObject ??= Player.Local; // Try again
            }

            if (PlayerObject == null)
            {
                MelonLogger.Error("UIBuilder: Failed to find PlayerObject after multiple attempts. Minimap initialization aborted.");
                yield break;
            }

            // --- RESTORE FINDING CACHED MAP CONTENT ---
            _cachedMapContent = GameObject.Find(Constants.MapAppPath);
            if (_cachedMapContent == null)
            {
                MelonLogger.Warning("UIBuilder: Cached map content not found, real player icon might not be available.");
            } else {
                MelonLogger.Msg("UIBuilder: Cached map content found.");
            }
            // --- END RESTORE ---

            // Initialize the content manager now that we have the player object
            if (ContentManager != null && MinimapContent != null && _sharedCoordinateSystem != null && _playerMarkerManager != null)
            {
                MelonCoroutines.Start(ContentManager.Initialize(MinimapContent.GetComponent<RectTransform>(), PlayerObject.transform, _sharedCoordinateSystem, _playerMarkerManager));
            } else {
                MelonLogger.Error($"UIBuilder: ContentManager init skipped. ContentManager: {ContentManager}, MinimapContent: {MinimapContent}, _sharedCoordinateSystem: {_sharedCoordinateSystem}, _playerMarkerManager: {_playerMarkerManager}");
            }

            // --- RESTORE CALL TO INITIALIZE PLAYER MARKER ICON ---
            if (_playerMarkerManager != null)
            {
                MelonCoroutines.Start(_playerMarkerManager.InitializePlayerMarkerIcon(_cachedMapContent));
            }
            // --- END RESTORE ---

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
            MelonLogger.Msg($"UIBuilder: Attempting to load static minimap image from embedded resource: {Constants.MinimapImagePath}");
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(Constants.MinimapImagePath))
                {
                    if (stream == null)
                    {
                        MelonLogger.Error($"UIBuilder: Failed to get manifest resource stream for {Constants.MinimapImagePath}. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
                        return;
                    }

                    byte[] fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, (int)stream.Length);

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
                        _internalMapImage.color = Color.white; // Ensure full opacity

                        // Explicitly set RectTransform properties for proper centering
                        RectTransform imageRT = _internalMapImageGO.GetComponent<RectTransform>();
                        imageRT.anchorMin = new Vector2(0.5f, 0.5f);
                        imageRT.anchorMax = new Vector2(0.5f, 0.5f);
                        imageRT.pivot = new Vector2(0.5f, 0.5f);
                        imageRT.anchoredPosition = Vector2.zero; // Center it

                        MelonLogger.Msg("UIBuilder: Successfully loaded and applied static map sprite.");
                    }
                    else
                    {
                        MelonLogger.Error($"UIBuilder: Failed to load image data into texture from embedded resource {Constants.MinimapImagePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"UIBuilder: Error loading static map sprite from embedded resource: {ex.Message}");
            }
        }
    }
}
using MelonLoader;
using S1API.Entities;
using Small_Corner_Map.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Small_Corner_Map
{
    // The main orchestrator for the minimap UI and its managers.
    public partial class MinimapUI
    {
        // --- Managers and Content ---
        // Handles map content (image, grid, static markers)
        private MinimapContent minimapContent;
        // Handles player marker and direction indicator
        private PlayerMarkerManager playerMarkerManager;
        // Handles contract PoI markers
        private ContractMarkerManager contractMarkerManager;
        // Handles minimap time display
        private MinimapTimeDisplay minimapTimeDisplay;

        // --- UI GameObjects ---
        private GameObject minimapObject;           // Root object for the minimap UI
        private GameObject minimapDisplayObject;    // The mask object for the minimap
        private RectTransform minimapFrameRect;     // The frame (positioned in the corner)

        // --- State ---
        private MapPreferences mapPreferences;
        private bool initialized = false;

        // --- Constants ---
        private const float markerXAdjustment = 12f; // X offset for contract markers
        private const float markerZAdjustment = -3f;  // Y offset for contract markers
        private const float minimapSize = 150f;     // Size of the minimap mask/frame
        private const float mapContentSize = 500f;  // Size of the map content

        // --- Cached Player Reference ---
        private Player playerObject;

        private bool timeBarEnabled {  get { return mapPreferences.showGameTime.Value; } }
        private bool minimapEnabled {  get { return mapPreferences.minimapEnabled.Value; } }

        public MinimapUI(MapPreferences preferences)
        {
            mapPreferences = preferences;
            mapPreferences.minimapEnabled.OnEntryValueChanged.Subscribe(OnMinimapEnableChanged);
            mapPreferences.showGameTime.OnEntryValueChanged.Subscribe(OnTimeBarEnableChanged);
        }

        /// <summary>
        /// Initializes the minimap UI and starts integration and update coroutines.
        /// </summary>
        public void Initialize()
        {
            if (minimapEnabled)
            {
                CreateMinimapDisplay();
                StartSceneIntegration();    // Finds map sprite, player, and sets up markers
                StartMinimapUpdateLoop();   // Continuously updates minimap as player moves
                initialized = true;

                MelonCoroutines.Start(ContractPoICheckerWorld());
            }
        }

        /// <summary>
        /// Cleans up the minimap UI when the scene changes or mod is unloaded.
        /// </summary>
        public void Dispose()
        {
            if (minimapObject != null)
            {
                UnityEngine.Object.Destroy(minimapObject);
                minimapObject = null;
            }
        }

        /// <summary>
        /// Creates the minimap UI hierarchy, including mask, background, content, and managers.
        /// </summary>
        private void CreateMinimapDisplay()
        {
            // Root container for minimap UI
            minimapObject = new GameObject("MinimapContainer");
            UnityEngine.Object.DontDestroyOnLoad(minimapObject);

            // Canvas for UI rendering
            var canvasObject = createCanvasAsset();

            // Frame (positions minimap in the corner)
            var frameObject = createFrameAsset(canvasObject);

            // Mask (circular area for minimap)
            var maskObject = createMaskAsset(frameObject);

            // Map content (holds the map image, grid, and markers)
            minimapContent = new MinimapContent(mapContentSize, 20, Constants.DefaultMapScale);
            minimapContent.Create(maskObject);
            TryApplyMapSprite(); // Try to assign the map image immediately

            // Player marker (centered in the minimap)
            playerMarkerManager = new PlayerMarkerManager();
            playerMarkerManager.CreatePlayerMarker(maskObject);

            // Contract PoI markers
            contractMarkerManager = new ContractMarkerManager(
                minimapContent, Constants.DefaultMapScale, markerXAdjustment, markerZAdjustment);

            // Time display (shows in-game time)
            minimapTimeDisplay = new MinimapTimeDisplay();
            minimapTimeDisplay.Create(minimapFrameRect, mapPreferences.showGameTime);
        }

        public void OnMinimapEnableChanged(bool oldValue, bool newValue)
        {

            if (newValue && !initialized)
            {
                Initialize();
            }

            if (oldValue != newValue)
            {
                if (minimapDisplayObject != null)
                    minimapDisplayObject.SetActive(minimapEnabled);

                if (minimapEnabled)
                {
                    UpdateMinimapSize();
                }

                if (minimapTimeDisplay != null)
                {
                    minimapTimeDisplay.SetTimeBarEnabled(timeBarEnabled);
                }
            }
        }

        public void OnTimeBarEnableChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                minimapTimeDisplay?.SetTimeBarEnabled(timeBarEnabled);
            }
        }

        /// <summary>
        /// Updates the minimap and its elements when the size changes (e.g., 2x toggle).
        /// </summary>
        private void UpdateMinimapSize()
        {
            float sizeMultiplier = 1f;

            if (minimapFrameRect != null)
                minimapFrameRect.sizeDelta = new Vector2(minimapSize, minimapSize) * sizeMultiplier;

            if (minimapDisplayObject != null)
            {
                RectTransform component = minimapDisplayObject.GetComponent<RectTransform>();
                component.sizeDelta = new Vector2(minimapSize, minimapSize) * sizeMultiplier;
            }
        }

        /// <summary>
        /// Utility for creating a filled circle sprite for the minimap mask/background.
        /// </summary>
        private Sprite CreateCircleSprite(int diameter, Color color, int resolutionMultiplier =1)
        {
            int texSize = diameter * resolutionMultiplier;
            Texture2D texture = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            Color clear = new Color(0f,0f,0f,0f);
            for (int i =0; i < texSize; i++)
                for (int j =0; j < texSize; j++)
                    texture.SetPixel(j, i, clear);
            int num = texSize /2;
            Vector2 center = new Vector2(num, num);
            for (int k =0; k < texSize; k++)
                for (int l =0; l < texSize; l++)
                    if (Vector2.Distance(new Vector2(l, k), center) <= num)
                        texture.SetPixel(l, k, color);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f,0f, texSize, texSize), new Vector2(0.5f,0.5f), resolutionMultiplier);
        }

        /// <summary>
        /// Attempts to find and assign the map sprite from the game's UI to the minimap.
        /// </summary>
        private void TryApplyMapSprite()
        {
            
            GameObject contentGO = GameObject.Find("GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content");
            Image contentImage = null;
            if (contentGO != null)
            {
                contentImage = contentGO.GetComponent<Image>();
                if (contentImage == null && contentGO.transform.childCount > 0)
                    contentImage = contentGO.transform.GetChild(0).GetComponent<Image>();
            }

            if (contentImage != null && minimapContent != null)
            {
                var minimapImage = minimapContent.MapContentObject.GetComponent<Image>();
                if (minimapImage == null)
                    minimapImage = minimapContent.MapContentObject.AddComponent<Image>();

                minimapImage.sprite = contentImage.sprite;
                minimapImage.type = Image.Type.Simple;
                minimapImage.preserveAspect = true;
                minimapImage.enabled = true;
                MelonLogger.Msg("MinimapUI: Successfully applied map sprite to minimap.");
            }
            else
            {
                MelonLogger.Warning("MinimapUI: Could not find map sprite to apply to minimap.");
            }
        }

        /// <summary>
        /// Starts the coroutine that integrates the minimap with the scene (finds player, map, markers).
        /// </summary>
        public void StartSceneIntegration()
        {
            MelonCoroutines.Start(SceneIntegrationRoutine());
        }

        /// <summary>
        /// Coroutine that finds the player, map sprite, and sets up markers after the scene loads.
        /// </summary>
        private IEnumerator SceneIntegrationRoutine()
        {
            MelonLogger.Msg("MinimapUI: Looking for game objects...");

            yield return new WaitForSeconds(2f);

            GameObject mapAppObject = null;
            GameObject viewportObject = null;

            int attempts = 0;
            while ((mapAppObject == null || playerObject == null) && attempts < 30)
            {
                attempts++;

                // Find player by looking for a CharacterController not at world origin
                if (playerObject == null)
                {
                    playerObject = Player.Local;
                }

                // Find the MapApp UI object
                if (mapAppObject == null)
                {
                    GameObject gameplayMenu = GameObject.Find("GameplayMenu");
                    if (gameplayMenu != null)
                    {
                        Transform phoneTransform = gameplayMenu.transform.Find("Phone");
                        if (phoneTransform != null)
                        {
                            Transform phoneChildTransform = phoneTransform.Find("phone");
                            if (phoneChildTransform != null)
                            {
                                Transform appsCanvas = phoneChildTransform.Find("AppsCanvas");
                                if (appsCanvas != null)
                                {
                                    Transform mapApp = appsCanvas.Find("MapApp");
                                    if (mapApp != null)
                                    {
                                        mapAppObject = mapApp.gameObject;
                                        MelonLogger.Msg("MinimapUI: Found MapApp");
                                    }
                                }
                            }
                        }
                    }
                }

                // Find the viewport (where the map image is)
                if (mapAppObject != null && viewportObject == null)
                {
                    Transform container = mapAppObject.transform.Find("Container");
                    if (container != null)
                    {
                        Transform scrollView = container.Find("Scroll View");
                        if (scrollView != null)
                        {
                            Transform viewport = scrollView.Find("Viewport");
                            if (viewport != null)
                            {
                                viewportObject = viewport.gameObject;
                                MelonLogger.Msg("MinimapUI: Found Map Viewport");
                            }
                        }
                    }
                }

                if (mapAppObject == null || playerObject == null)
                    yield return new WaitForSeconds(0.5f);
            }

            if (mapAppObject == null)
                MelonLogger.Warning("MinimapUI: Could not find Map App after multiple attempts");
            else if (viewportObject == null)
                MelonLogger.Warning("MinimapUI: Found MapApp but could not find Viewport");
            if (playerObject == null)
                MelonLogger.Warning("MinimapUI: Could not find Player after multiple attempts");

            MelonLogger.Msg("MinimapUI: Game object search completed");

            // Apply map sprite from the viewport's content image
            if (viewportObject != null)
            {
                try
                {
                    if (viewportObject.transform.childCount > 0)
                    {
                        Transform contentTransform = viewportObject.transform.GetChild(0);
                        MelonLogger.Msg("MinimapUI: Found viewport content: " + contentTransform.name);
                        Image contentImage = contentTransform.GetComponent<Image>();

                        if (contentImage != null && contentImage.sprite != null)
                        {
                            MelonLogger.Msg("MinimapUI: Found content image with sprite: " + contentImage.sprite.name);

                            var minimapImage = minimapContent.MapContentObject.GetComponent<Image>();
                            if (minimapImage == null)
                                minimapImage = minimapContent.MapContentObject.AddComponent<Image>();
                            minimapImage.sprite = contentImage.sprite;
                            minimapImage.type = Image.Type.Simple;
                            minimapImage.preserveAspect = true;
                            minimapImage.enabled = true;
                            MelonLogger.Msg("MinimapUI: Successfully applied map sprite to minimap!");

                            if (minimapContent.GridContainer != null)
                                minimapContent.GridContainer.gameObject.SetActive(false);
                        }
                        else
                        {
                            MelonLogger.Msg("MinimapUI: Content doesn't have an Image component or sprite");
                            // Try children for a valid image
                            int childCount = contentTransform.childCount;
                            for (int i = 0; i < childCount; i++)
                            {
                                Transform child = contentTransform.GetChild(i);
                                Image childImage = child.GetComponent<Image>();
                                if (childImage != null && childImage.sprite != null)
                                {
                                    MelonLogger.Msg("MinimapUI: Found image in content child: " + child.name + ", Sprite: " + childImage.sprite.name);

                                    var minimapImage = minimapContent.MapContentObject.GetComponent<Image>();
                                    if (minimapImage == null)
                                        minimapImage = minimapContent.MapContentObject.AddComponent<Image>();
                                    minimapImage.sprite = childImage.sprite;
                                    minimapImage.type = Image.Type.Simple;
                                    minimapImage.preserveAspect = true;
                                    minimapImage.enabled = true;
                                    MelonLogger.Msg("MinimapUI: Successfully applied map sprite to minimap!");

                                    if (minimapContent.GridContainer != null)
                                        minimapContent.GridContainer.gameObject.SetActive(false);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("MinimapUI: Error accessing map content: " + ex.Message);
                }
            }

            // Replace fallback player marker with real icon if available
            GameObject cachedMapContent = GameObject.Find("GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content");
            if (cachedMapContent != null)
            {
                Transform playerPoI = cachedMapContent.transform.Find("PlayerPoI(Clone)");
                if (playerPoI != null)
                {
                    Transform realIcon = playerPoI.Find("IconContainer");
                    if (realIcon != null)
                    {
                        playerMarkerManager.ReplaceWithRealPlayerIcon(realIcon.gameObject);
                        MelonLogger.Msg("MinimapUI: Replaced fallback player marker with real player icon.");
                    }
                }
            }

            // Add default static markers if possible
            if (cachedMapContent != null)
            {
                Transform propertyPoI = cachedMapContent.transform.Find("PropertyPoI(Clone)");
                if (propertyPoI != null)
                {
                    Transform iconContainer = propertyPoI.Find("IconContainer");
                    if (iconContainer != null)
                    {
                        minimapContent.AddWhiteStaticMarker(new Vector3(-67.17f, -3.03f, 138.31f), iconContainer.gameObject);
                        minimapContent.AddWhiteStaticMarker(new Vector3(-79.88f, -2.26f, 85.13f), iconContainer.gameObject);
                        minimapContent.AddWhiteStaticMarker(new Vector3(-179.99f, -3.03f, 113.69f), iconContainer.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Starts the coroutine that updates the minimap every frame.
        /// </summary>
        public void StartMinimapUpdateLoop()
        {
            MelonCoroutines.Start(MinimapUpdateLoopCoroutine());
        }

        /// <summary>
        /// Coroutine that updates the minimap's content position and player marker direction.
        /// </summary>
        private IEnumerator MinimapUpdateLoopCoroutine()
        {
            while (true)
            {
                UpdateMinimap();
                yield return null;
            }
        }

        /// <summary>
        /// Updates the minimap's content position to keep the player centered, and updates the player marker direction.
        /// </summary>
        private void UpdateMinimap()
        {
            if (playerObject == null)
            {
                playerObject = Player.Local;

                if (playerObject == null)
                    return;
            }

            if (minimapContent?.MapContentObject == null)
                return;

            // Move the map content so the player is always centered in the minimap
            Vector3 position = playerObject.Position;
            float mappedX = -position.x * Constants.DefaultMapScale;
            float mappedZ = -position.z * Constants.DefaultMapScale;
            Transform minimapMask = minimapDisplayObject.transform.Find("MinimapMask");
            var zero = Vector2.zero;

            if (minimapMask != null)
            {
                RectTransform contentRect = minimapContent.MapContentObject.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    Rect rect = contentRect.rect;
                    float halfWidth = rect.width * 0.5f;
                    contentRect.anchoredPosition = new Vector2(mappedX, mappedZ);
                    zero = new Vector2(halfWidth, rect.height * 0.5f);
                }
            }

            // Position of the player marker on the minimap
            var sizeVector = new Vector2(11.2f, -2.7f);
            var heightVector = new Vector2(mappedX, mappedZ) + zero + sizeVector;

            var contentObject = minimapContent.MapContentObject.GetComponent<RectTransform>();
            if (contentObject != null)
            {
                contentObject.anchoredPosition = Vector2.Lerp(
                    contentObject.anchoredPosition,
                    heightVector,
                    Time.deltaTime * 10f);

                // Update player marker direction indicator
                playerMarkerManager?.UpdateDirectionIndicator(playerObject.Transform);
            }


            minimapTimeDisplay.UpdateMinimapTime();
        }

        private GameObject createCanvasAsset()
        {
            GameObject canvasObject = new GameObject("MinimapCanvas");
            canvasObject.transform.SetParent(minimapObject.transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            UnityEngine.UI.CanvasScaler canvasScaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            return canvasObject;
        }

        private GameObject createFrameAsset(GameObject canvasObject)
        {
            GameObject frameObject = new GameObject("MinimapFrame");
            frameObject.transform.SetParent(canvasObject.transform, false);
            minimapFrameRect = frameObject.AddComponent<RectTransform>();
            minimapFrameRect.anchorMin = new Vector2(1f, 1f);
            minimapFrameRect.anchorMax = new Vector2(1f, 1f);
            minimapFrameRect.pivot = new Vector2(1f, 1f);
            minimapFrameRect.anchoredPosition = new Vector2(-20f, -20f);
            minimapFrameRect.sizeDelta = new Vector2(minimapSize, minimapSize);

            return frameObject;
        }

        private GameObject createMaskAsset(GameObject frameObject)
        {
            GameObject maskObject = new GameObject("MinimapMask");
            maskObject.transform.SetParent(frameObject.transform, false);
            RectTransform maskRect = maskObject.AddComponent<RectTransform>();
            maskRect.sizeDelta = new Vector2(minimapSize, minimapSize);
            maskRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.anchoredPosition = Vector2.zero;
            maskObject.AddComponent<Mask>().showMaskGraphic = false;
            minimapDisplayObject = maskObject;
            Image maskImage = maskObject.AddComponent<Image>();
            maskImage.sprite = CreateCircleSprite((int)minimapSize, Color.black);
            maskImage.type = Image.Type.Sliced;
            maskImage.color = Color.black;

            return maskObject;
        }

        private IEnumerator ContractPoICheckerWorld()
        {
            return new ContractPoIChecker(0, contractMarkerManager);
        }
    }
}
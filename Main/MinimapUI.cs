#if IL2CPP
using S1Quests = Il2CppScheduleOne.Quests;
#else
using S1Quests = ScheduleOne.Quests;
#endif

using S1API.Entities;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main;

// The main orchestrator for the minimap UI and its managers.
public class MinimapUI
{
    // --- Managers and Content ---
    // Handles map content (image, grid, static markers)
    private MinimapContent minimapContent;
    // Handles player marker and direction indicator
    private PlayerMarkerManager playerMarkerManager;
    // Handles minimap time display
    private MinimapTimeDisplay minimapTimeDisplay;

    // --- UI GameObjects ---
    private GameObject minimapObject;           // Root object for the minimap UI
    private GameObject minimapDisplayObject;    // The mask object for the minimap
    private RectTransform minimapFrameRect;     // The frame (positioned in the corner)
    private GameObject minimapBorderObject;     // NEW: Border object
    private Image minimapBorderImage;           // NEW: Border image

    // --- State ---
    private readonly MapPreferences mapPreferences;
    private bool initialized = false;
    
    // --- Scaled sizes (calculated at runtime) ---
    private float scaledMinimapSize;
    private float scaledMapContentSize;
    private Image minimapMaskImage; // Cache for regenerating mask sprite

    // --- Cached Player Reference ---
    private Player playerObject;

    // Handles contract PoI markers
    private ContractMarkerManager ContractMarkerManager { get; set; }

    private bool TimeBarEnabled => mapPreferences.ShowGameTime.Value;
    private bool MinimapEnabled => mapPreferences.MinimapEnabled.Value;

    // Dynamic world-to-minimap scale reflecting user preference.
    private float CurrentWorldScale => Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor;

    public MinimapUI(MapPreferences preferences)
    {
        mapPreferences = preferences;
        mapPreferences.MinimapEnabled.OnEntryValueChanged.Subscribe(OnMinimapEnableChanged);
        mapPreferences.ShowGameTime.OnEntryValueChanged.Subscribe(OnTimeBarEnableChanged);
        mapPreferences.IncreaseSize.OnEntryValueChanged.Subscribe(OnIncreaseSizeChanged);
        RecalculateScaledSizes();
    }
    
    private void RecalculateScaledSizes()
    {
        var scale = mapPreferences.MinimapScaleFactor;
        scaledMinimapSize = Constants.BaseMinimapSize * scale;
        scaledMapContentSize = Constants.BaseMapContentSize * scale;
    }
    
    private void OnIncreaseSizeChanged(bool oldValue, bool newValue)
    {
        if (!initialized) return;
        RecalculateScaledSizes();
        UpdateMinimapSize(true); // regenerate mask sprite

        // Update scale in ContractMarkerManager
        var currentScale = Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor;
        ContractMarkerManager?.UpdateMapScale(currentScale);
        
        // Reproject all PoI markers using new scale
        MinimapPoIHelper.UpdateAllMarkerPositions(CurrentWorldScale);
        // Immediately recenter content so player marker remains centered after size jump
        UpdateMinimap();
    }

    /// <summary>
    /// Initializes the minimap UI and starts integration and update coroutines.
    /// </summary>
    public void Initialize()
    {
        if (!MinimapEnabled) return;
        CreateMinimapDisplay();
        StartSceneIntegration();    // Finds map sprite, player, and sets up markers
        StartMinimapUpdateLoop();   // Continuously updates minimap as player moves
        initialized = true;
    }

    /// <summary>
    /// Cleans up the minimap UI when the scene changes or mod is unloaded.
    /// </summary>
    public void Dispose()
    {
        if (minimapObject == null) return;
        UnityEngine.Object.Destroy(minimapObject);
        minimapObject = null;
    }

    /// <summary>
    /// Creates the minimap UI hierarchy, including mask, background, content, and managers.
    /// </summary>
    private void CreateMinimapDisplay()
    {
        RecalculateScaledSizes();
        
        // Root container for minimap UI
        minimapObject = new GameObject("MinimapContainer");
        UnityEngine.Object.DontDestroyOnLoad(minimapObject);

        // Canvas for UI rendering
        var canvasObject = MinimapUIFactory.CreateCanvas(minimapObject);

        // Frame (positions minimap in the corner)
        var (frameObject, frameRect) = MinimapUIFactory.CreateFrame(canvasObject, scaledMinimapSize);
        minimapFrameRect = frameRect;

        // Border (slightly larger circle behind mask)
        var (borderObj, borderImg) = MinimapUIFactory.CreateBorder(frameObject, scaledMinimapSize);
        minimapBorderObject = borderObj;
        minimapBorderImage = borderImg;

        // Mask (circular area for minimap)
        var (maskObject, maskImage) = MinimapUIFactory.CreateMask(frameObject, scaledMinimapSize);
        minimapDisplayObject = maskObject;
        minimapMaskImage = maskImage;

        // Map content (holds the map image, grid, and markers)
        minimapContent = new MinimapContent(
            scaledMapContentSize, 
            20, 
            Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor);
        minimapContent.Create(maskObject);
        TryApplyMapSprite(); // Try to assign the map image immediately

        // Player marker (centered in the minimap)
        playerMarkerManager = new PlayerMarkerManager();
        playerMarkerManager.CreatePlayerMarker(maskObject);

        // Contract PoI markers
        ContractMarkerManager = new ContractMarkerManager(
            minimapContent, 
            Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor, 
            Constants.ContractMarkerXOffset, 
            Constants.ContractMarkerZOffset,
            mapPreferences);

        // Time display (shows in-game time)
        minimapTimeDisplay = new MinimapTimeDisplay();
        minimapTimeDisplay.Create(minimapFrameRect, mapPreferences.ShowGameTime);
    }

    private void OnMinimapEnableChanged(bool oldValue, bool newValue)
    {

        if (newValue && !initialized)
        {
            Initialize();
        }

        if (oldValue == newValue) return;
        if (minimapDisplayObject != null)
            minimapDisplayObject.SetActive(MinimapEnabled);

        if (MinimapEnabled)
        {
            UpdateMinimapSize();
        }

        minimapTimeDisplay?.SetTimeBarEnabled(TimeBarEnabled);
    }

    private void OnTimeBarEnableChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue)
        {
            minimapTimeDisplay?.SetTimeBarEnabled(TimeBarEnabled);
        }
    }

    /// <summary>
    /// Updates the minimap and its elements when the size changes (e.g., 2x toggle).
    /// </summary>
    private void UpdateMinimapSize(bool regenerateMaskSprite = false)
    {
        RecalculateScaledSizes();

        if (minimapFrameRect != null)
            minimapFrameRect.sizeDelta = new Vector2(scaledMinimapSize, scaledMinimapSize);

        if (minimapDisplayObject != null)
        {
            var component = minimapDisplayObject.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(scaledMinimapSize, scaledMinimapSize);
            
            if (regenerateMaskSprite && minimapMaskImage != null)
                minimapMaskImage.sprite = MinimapUIFactory.CreateCircleSprite((int)scaledMinimapSize, Color.black, Constants.MinimapCircleResolutionMultiplier, Constants.MinimapMaskFeather, featherInside:true);
        }

        // Resize border to remain slightly larger than mask
        if (minimapBorderObject != null)
        {
            var borderRect = minimapBorderObject.GetComponent<RectTransform>();
            var borderDiameter = scaledMinimapSize + (Constants.MinimapBorderThickness * 2f);
            borderRect.sizeDelta = new Vector2(borderDiameter, borderDiameter);
            if (regenerateMaskSprite && minimapBorderImage != null)
            {
                var borderColor = new Color(Constants.MinimapBorderR, Constants.MinimapBorderG, Constants.MinimapBorderB, Constants.MinimapBorderA);
                minimapBorderImage.sprite = MinimapUIFactory.CreateCircleSprite((int)borderDiameter, borderColor, Constants.MinimapCircleResolutionMultiplier, Constants.MinimapBorderFeather, featherInside:false);
            }
        }

        if (minimapContent?.MapContentObject == null) return;
        var contentRect = minimapContent.MapContentObject.GetComponent<RectTransform>();
        if (contentRect != null)
            contentRect.sizeDelta = new Vector2(scaledMapContentSize, scaledMapContentSize);
    }

    /// <summary>
    /// Attempts to find and assign the map sprite from the game's UI to the minimap.
    /// </summary>
    private void TryApplyMapSprite()
    {
        
        var contentGo = GameObject.Find("GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content");
        Image contentImage = null;
        if (contentGo != null)
        {
            contentImage = contentGo.GetComponent<Image>();
            if (contentImage == null && contentGo.transform.childCount > 0)
                contentImage = contentGo.transform.GetChild(0).GetComponent<Image>();
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
    private void StartSceneIntegration()
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

        var attempts = 0;
        while ((mapAppObject == null || playerObject == null) && attempts < 30)
        {
            attempts++;

            // Find player by looking for a CharacterController not at world origin
            playerObject ??= Player.Local;

            // Find the MapApp UI object
            if (mapAppObject == null)
            {
                var gameplayMenu = GameObject.Find("GameplayMenu");
                if (gameplayMenu != null)
                {
                    var phoneTransform = gameplayMenu.transform.Find("Phone");
                    if (phoneTransform != null)
                    {
                        var phoneChildTransform = phoneTransform.Find("phone");
                        if (phoneChildTransform != null)
                        {
                            var appsCanvas = phoneChildTransform.Find("AppsCanvas");
                            if (appsCanvas != null)
                            {
                                var mapApp = appsCanvas.Find("MapApp");
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
                var container = mapAppObject.transform.Find("Container");
                if (container != null)
                {
                    var scrollView = container.Find("Scroll View");
                    if (scrollView != null)
                    {
                        var viewport = scrollView.Find("Viewport");
                        if (viewport != null)
                        {
                            viewportObject = viewport.gameObject;
                            MelonLogger.Msg("MinimapUI: Found Map Viewport");
                        }
                    }
                }
            }

            if (mapAppObject == null || playerObject == null)
                yield return new WaitForSeconds(Constants.SceneIntegrationRetryDelay);
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
                    var contentTransform = viewportObject.transform.GetChild(0);
                    MelonLogger.Msg("MinimapUI: Found viewport content: " + contentTransform.name);
                    var contentImage = contentTransform.GetComponent<Image>();

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
                        var childCount = contentTransform.childCount;
                        for (var i = 0; i < childCount; i++)
                        {
                            var child = contentTransform.GetChild(i);
                            var childImage = child.GetComponent<Image>();
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
        var cachedMapContent = GameObject.Find("GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content");
        if (cachedMapContent != null)
        {
            var playerPoI = cachedMapContent.transform.Find("PlayerPoI(Clone)");
            if (playerPoI != null)
            {
                var realIcon = playerPoI.Find("IconContainer");
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
            var propertyPoI = cachedMapContent.transform.Find("PropertyPoI(Clone)");
            if (propertyPoI == null) yield break;
            
            var iconContainer = propertyPoI.Find("IconContainer");
            if (iconContainer == null) yield break;
            
            PropertyPoIManager.Initialize(minimapContent, iconContainer);
        }
    }

    /// <summary>
    /// Starts the coroutine that updates the minimap every frame.
    /// </summary>
    private void StartMinimapUpdateLoop()
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
        // ReSharper disable once IteratorNeverReturns
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
        
        // Use dynamic world scale that reflects current minimap size preference
        var worldScale = CurrentWorldScale;
        var position = playerObject.Position;
        var mappedX = -position.x * worldScale;
        var mappedZ = -position.z * worldScale;
        var minimapMask = minimapDisplayObject.transform.Find("MinimapMask");
        var zero = Vector2.zero;

        if (minimapMask != null)
        {
            var contentRect = minimapContent.MapContentObject.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                var rect = contentRect.rect;
                var halfWidth = rect.width * 0.5f;
                contentRect.anchoredPosition = new Vector2(mappedX, mappedZ);
                zero = new Vector2(halfWidth, rect.height * 0.5f);
            }
        }

        // Position of the player marker on the minimap (offset scaled with map)
        var scaleFactor = mapPreferences.MinimapScaleFactor;
        var sizeVector = new Vector2(
            Constants.PlayerMarkerOffsetX * scaleFactor, 
            Constants.PlayerMarkerOffsetZ * scaleFactor);
        var heightVector = new Vector2(mappedX, mappedZ) + zero + sizeVector;

        var contentObject = minimapContent.MapContentObject.GetComponent<RectTransform>();
        if (contentObject != null)
        {
            contentObject.anchoredPosition = Vector2.Lerp(
                contentObject.anchoredPosition,
                heightVector,
                Time.deltaTime * Constants.MapContentLerpSpeed);

            // Update player marker direction indicator
            playerMarkerManager?.UpdateDirectionIndicator(playerObject.Transform);
        }
        
        minimapTimeDisplay.UpdateMinimapTime();
    }


    internal void OnContractAccepted(S1Quests.Contract contract)
    {
        ContractMarkerManager.AddContractPoIMarkerWorld(contract);
    }

    internal void OnContractCompleted(S1Quests.Contract contract)
    {
        ContractMarkerManager.RemoveContractPoIMarkers(contract);
    }
}

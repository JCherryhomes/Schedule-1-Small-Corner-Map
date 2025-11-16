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

/// <summary>
/// Main orchestrator for the minimap UI. Delegates responsibilities to specialized helper classes.
/// </summary>
public class MinimapUI
{
    // --- Core Components ---
    private readonly MapPreferences mapPreferences;
    private MinimapSizeManager sizeManager;
    private MinimapSceneIntegration sceneIntegration;
    private MinimapMarkerCoordinator markerCoordinator;
    
    // --- Content and Managers ---
    private MinimapContent minimapContent;
    private PlayerMarkerManager playerMarkerManager;
    private MinimapTimeDisplay minimapTimeDisplay;
    private ContractMarkerManager contractMarkerManager;

    // --- UI GameObjects ---
    private GameObject minimapObject;
    private GameObject minimapDisplayObject;

    // --- State ---
    private bool initialized;

    private bool TimeBarEnabled => mapPreferences.ShowGameTime.Value;
    private bool MinimapEnabled => mapPreferences.MinimapEnabled.Value;


    public MinimapUI(MapPreferences preferences)
    {
        mapPreferences = preferences;
        sizeManager = new MinimapSizeManager(mapPreferences);
        
        mapPreferences.MinimapEnabled.OnEntryValueChanged.Subscribe(OnMinimapEnableChanged);
        mapPreferences.ShowGameTime.OnEntryValueChanged.Subscribe(OnTimeBarEnableChanged);
        mapPreferences.IncreaseSize.OnEntryValueChanged.Subscribe(OnIncreaseSizeChanged);
        mapPreferences.TrackContracts.OnEntryValueChanged.Subscribe(OnContractTrackingChanged);
        mapPreferences.TrackProperties.OnEntryValueChanged.Subscribe(OnPropertyTrackingChanged);
    }
    
    private void OnIncreaseSizeChanged(bool oldValue, bool newValue)
    {
        if (!initialized) return;
        sizeManager.UpdateMinimapSize(true);
        markerCoordinator.OnSizeChanged();
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
        // Root container for minimap UI
        minimapObject = new GameObject("MinimapContainer");
        UnityEngine.Object.DontDestroyOnLoad(minimapObject);

        // Canvas for UI rendering
        var canvasObject = MinimapUIFactory.CreateCanvas(minimapObject);

        // Frame (positions minimap in the corner) - size managed by sizeManager
        var (frameObject, frameRect) = MinimapUIFactory.CreateFrame(canvasObject, sizeManager.ScaledFrameSize);

        // Border (slightly larger circle behind mask)
        var (borderObj, borderImg) = MinimapUIFactory.CreateBorder(frameObject, sizeManager.ScaledMinimapSize);

        // Mask (circular area for minimap)
        var (maskObject, maskImage) = MinimapUIFactory.CreateMask(frameObject, sizeManager.ScaledMinimapSize);
        minimapDisplayObject = maskObject;

        // Map content (holds the map image, grid, and markers)
        minimapContent = new MinimapContent(
            sizeManager.ScaledMapContentSize, 
            20, 
            sizeManager.CurrentWorldScale);
        minimapContent.Create(maskObject);

        // Player marker (centered in the minimap)
        playerMarkerManager = new PlayerMarkerManager();
        playerMarkerManager.CreatePlayerMarker(maskObject);

        // Contract PoI markers
        contractMarkerManager = new ContractMarkerManager(
            minimapContent,
            Constants.ContractMarkerXOffset, 
            mapPreferences);

        // Time display (shows in-game time)
        minimapTimeDisplay = new MinimapTimeDisplay();
        minimapTimeDisplay.Create(frameRect, mapPreferences.ShowGameTime);
        
        // Set UI references in size manager
        sizeManager.SetUIReferences(frameRect, maskObject, borderObj, maskImage, borderImg, minimapContent);
        
        // Initialize scene integration helper
        sceneIntegration = new MinimapSceneIntegration(minimapContent, playerMarkerManager, mapPreferences);
        
        // Initialize marker coordinator
        markerCoordinator = new MinimapMarkerCoordinator(contractMarkerManager, mapPreferences, minimapContent, sizeManager);
    }

    private void OnMinimapEnableChanged(bool oldValue, bool newValue)
    {
        if (newValue && !initialized)
        {
            Initialize();
        }

        if (oldValue == newValue) return;
        
        sizeManager?.SetMinimapVisible(MinimapEnabled);

        if (MinimapEnabled && initialized)
        {
            sizeManager?.UpdateMinimapSize();
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
    /// Starts the coroutine that integrates the minimap with the scene (finds player, map, markers).
    /// </summary>
    private void StartSceneIntegration()
    {
        MelonCoroutines.Start(SceneIntegrationRoutine());
    }

    /// <summary>
    /// Coroutine that delegates scene integration to the helper class.
    /// </summary>
    private IEnumerator SceneIntegrationRoutine()
    {
        yield return sceneIntegration.IntegrateWithScene();
        markerCoordinator.SetCachedMapContent(sceneIntegration.CachedMapContent);
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
        var playerObject = sceneIntegration?.PlayerObject ?? Player.Local;
        
        if (playerObject == null)
            return;

        if (minimapContent?.MapContentObject == null)
            return;
        
        // Use dynamic world scale that reflects current minimap size preference
        var worldScale = sizeManager.CurrentWorldScale;
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
        markerCoordinator?.OnContractAccepted(contract);
    }

    internal void OnContractCompleted(S1Quests.Contract contract)
    {
        markerCoordinator?.OnContractCompleted(contract);
    }
    
    private void OnContractTrackingChanged(bool previous, bool current)
    {
        markerCoordinator?.OnContractTrackingChanged(previous, current);
    }
    
    private void OnPropertyTrackingChanged(bool previous, bool current)
    {
        markerCoordinator?.OnPropertyTrackingChanged(previous, current);
    }
}

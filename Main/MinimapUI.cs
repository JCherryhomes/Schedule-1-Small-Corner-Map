#if IL2CPP
using S1Player = Il2CppScheduleOne.PlayerScripts;
using S1Quests = Il2CppScheduleOne.Quests;
using S1Vehicles = Il2CppScheduleOne.Vehicles;
#else
using S1Player = ScheduleOne.PlayerScripts;
using S1Quests = ScheduleOne.Quests;
using S1Vehicles = ScheduleOne.Vehicles;
#endif

using MelonLoader;
using UnityEngine;
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
    private OwnedVehiclesManager ownedVehiclesManager;
    private CompassManager compassManager;

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
        mapPreferences.TrackVehicles.OnEntryValueChanged.Subscribe(OnVehicleTrackingChanged);
        mapPreferences.ShowCompass.OnEntryValueChanged.Subscribe(OnShowCompassChanged);
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
        compassManager?.Dispose();
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
        minimapContent = new MinimapContent(sizeManager.ScaledMapContentSize, sizeManager.CurrentWorldScale);
        minimapContent.Create(maskObject);

        // Player marker (centered in the minimap)
        playerMarkerManager = new PlayerMarkerManager();
        playerMarkerManager.CreatePlayerMarker(maskObject);

        // Contract PoI markers
        contractMarkerManager = new ContractMarkerManager(
            minimapContent,
            Constants.MarkerXOffset, 
            mapPreferences);
        
        // Owned vehicles manager
        ownedVehiclesManager = new OwnedVehiclesManager(
            minimapContent,
            mapPreferences);

        // Time display (shows in-game time)
        minimapTimeDisplay = new MinimapTimeDisplay();
        minimapTimeDisplay.Create(frameRect, mapPreferences.ShowGameTime);
        
        // Compass Manager & UI
        var maskDiameterWithOffset = sizeManager.ScaledMinimapSize + Constants.MinimapMaskDiameterOffset;
        compassManager = new CompassManager(mapPreferences);
        compassManager.Create(frameObject, maskDiameterWithOffset);
        compassManager.Subscribe();
        
        // Set UI references in size manager
        sizeManager.SetUIReferences(frameRect, maskObject, borderObj, maskImage, borderImg, minimapContent);
        sizeManager.SetCompassManager(compassManager);
        
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
        compassManager?.SetVisible(MinimapEnabled && mapPreferences.ShowCompass.Value);
    }

    private void OnTimeBarEnableChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue)
        {
            minimapTimeDisplay?.SetTimeBarEnabled(TimeBarEnabled);
        }
    }

    private void OnShowCompassChanged(bool oldValue, bool newValue)
    {
        compassManager?.SetVisible(MinimapEnabled && newValue);
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
    private bool previousInVehicle;
    private S1Vehicles.LandVehicle previousVehicle;
    
    private void UpdateMinimap()
    {
        var playerObject = S1Player.Player.Local;
        if (playerObject == null || minimapContent?.MapContentObject == null) return;

        var currentVehicle = playerObject.CurrentVehicle?.transform?.GetComponentInParent<S1Vehicles.LandVehicle>();
        var isInVehicle = currentVehicle != null;

        // Swap marker icon when entering/exiting vehicle
        if (isInVehicle != previousInVehicle && playerMarkerManager != null)
        {
            if (isInVehicle)
            {
                // Replace player icon with vehicle icon if available, otherwise keep player icon
                if (OwnedVehiclesManager.IconContainer != null)
                {
                    playerMarkerManager.ReplaceWithVehicleIcon(OwnedVehiclesManager.IconContainer.gameObject);
                }

                // Hide the vehicle's original marker on the map (only if vehicle tracking is enabled)
                if (mapPreferences.TrackVehicles.Value)
                {
                    ownedVehiclesManager?.HideVehicleMarker(currentVehicle);
                    previousVehicle = currentVehicle;
                }
            }
            else
            {
                // Only restore if we actually changed to a vehicle icon
                if (OwnedVehiclesManager.IconContainer != null)
                {
                    playerMarkerManager.RestoreOriginalPlayerIcon();
                }

                // Show the vehicle marker again on the map (only if vehicle tracking is enabled)
                if (mapPreferences.TrackVehicles.Value && previousVehicle != null)
                {
                    ownedVehiclesManager?.ShowVehicleMarker(previousVehicle);
                }
                previousVehicle = null;
            }
            previousInVehicle = isInVehicle;
        }

        // Decide what to track: vehicle if occupied, otherwise player
        var trackTransform = isInVehicle ? currentVehicle.transform : playerObject.transform;
        var trackPosition = isInVehicle ? currentVehicle.transform.position : playerObject.PlayerBasePosition;

        // Dynamic world scale reflecting current minimap size preference
        var worldScale = sizeManager.CurrentWorldScale;
        var mappedX = -trackPosition.x * worldScale;
        var mappedZ = -trackPosition.z * worldScale;

        // Legacy logic attempted to find a child named MinimapMask of the mask itself – always null.
        // Retain structure but simplify: zero remains Vector2.zero.
        var zero = Vector2.zero;

        var scaleFactor = mapPreferences.MinimapScaleFactor;
        var sizeVector = new Vector2(
            Constants.PlayerMarkerOffsetX * scaleFactor,
            Constants.PlayerMarkerOffsetZ * scaleFactor);
        var heightVector = new Vector2(mappedX, mappedZ) + zero + sizeVector;

        var contentRect = minimapContent.MapContentObject.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.Lerp(
                contentRect.anchoredPosition,
                heightVector,
                Time.deltaTime * Constants.MapContentLerpSpeed);
            playerMarkerManager?.UpdateDirectionIndicator(trackTransform);
            compassManager?.SetWorldScale(worldScale);
            compassManager?.UpdateTargets(playerObject.PlayerBasePosition);
            compassManager?.SyncFromPoIMarkers(playerObject.PlayerBasePosition, worldScale);
        }

        minimapTimeDisplay.UpdateMinimapTime();
    }

    internal void OnOwnedVehiclesAdded()
    {
        ownedVehiclesManager.AddOwnedVehicleMarkers();
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

    private void OnVehicleTrackingChanged(bool previous, bool current)
    {
        if (current)
        {
            ownedVehiclesManager.AddOwnedVehicleMarkers();
        }
        else
        {
            ownedVehiclesManager.RemoveOwnedVehicleMarkers();
        }
    }
}

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
using Small_Corner_Map.PoIManagers;

namespace Small_Corner_Map.Main;

/// <summary>
/// Main orchestrator for the minimap UI. Delegates responsibilities to specialized helper classes.
/// </summary>
public class MinimapUI
{
    // --- Core Components ---
    private readonly MapPreferences mapPreferences;
    private readonly MinimapSizeManager sizeManager;
    private MinimapSceneIntegration sceneIntegration;
    private MinimapMarkerCoordinator markerCoordinator;
    
    // --- Content and Managers ---
    private MinimapContent minimapContent;
    private PlayerMarkerManager playerMarkerManager;
    private MinimapTimeDisplay minimapTimeDisplay;
    private QuestMarkerManager questMarkerManager;
    private OwnedVehiclesManager ownedVehiclesManager;
    private PropertyPoIManager propertyPoIManager;
    private CompassManager compassManager;

    // --- UI GameObjects ---
    private GameObject minimapObject;

    // --- State ---
    private bool initialized;
    private bool previousInVehicle;
    private S1Vehicles.LandVehicle previousVehicle;

    private bool TimeBarEnabled => mapPreferences.ShowGameTime.Value;
    private bool MinimapEnabled => mapPreferences.MinimapEnabled.Value;

    private MarkerRegistry markerRegistry;


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

    /// <summary>
    /// Initializes the minimap UI and starts integration and update coroutines.
    /// </summary>
    public void Initialize()
    {
        if (!MinimapEnabled) return;
        markerRegistry = new MarkerRegistry();
        CreateMinimapDisplay();
        StartSceneIntegration();    // Finds map sprite, player, and sets up markers
        StartMinimapUpdateLoop();   // Continuously updates minimap as player moves
        StartQuestInitializationLoop(); // Initializes quest markers after scene integration
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

    // --- Internal Methods ---
    internal void OnContractAccepted(S1Quests.Contract contract)
    {
        markerCoordinator?.OnContractAccepted(contract);
    }

    internal void OnContractCompleted(S1Quests.Contract contract)
    {
        markerCoordinator?.OnContractCompleted(contract);
    }
    
    internal void OnOwnedVehiclesAdded()
    {
        ownedVehiclesManager.AddAllMarkers();
    }

    internal void OnQuestCompleted(S1Quests.Quest quest)
    {
        markerCoordinator.OnQuestCompleted(quest);
    }
    
    internal void OnQuestStarted(S1Quests.Quest quest)
    {
        markerCoordinator?.OnQuestStarted(quest);
    }

    // --- Private Methods ---
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

        // Map content (holds the map image, grid, and markers)
        minimapContent = new MinimapContent(sizeManager.ScaledMapContentSize, sizeManager.CurrentWorldScale);
        minimapContent.Create(maskObject);

        // Player marker (centered in the minimap)
        playerMarkerManager = new PlayerMarkerManager();
        playerMarkerManager.CreatePlayerMarker(maskObject);
        
        // Quest PoI markers
        questMarkerManager = new QuestMarkerManager(minimapContent, mapPreferences, markerRegistry);
        
        // Owned vehicles manager
        ownedVehiclesManager = new OwnedVehiclesManager(
            minimapContent,
            mapPreferences,
            markerRegistry);
        
        // Property PoI markers
        propertyPoIManager = new PropertyPoIManager(
            minimapContent,
            mapPreferences,
            markerRegistry);

        // Time display (shows in-game time)
        minimapTimeDisplay = new MinimapTimeDisplay();
        minimapTimeDisplay.Create(frameRect, mapPreferences.ShowGameTime);
        
        // Compass Manager & UI
        var maskDiameterWithOffset = sizeManager.ScaledMinimapSize + Constants.MinimapMaskDiameterOffset;
        compassManager = new CompassManager(mapPreferences);
        compassManager.Create(frameObject, maskDiameterWithOffset);
        compassManager.Initialize(markerRegistry);
        compassManager.MinimapContent = minimapContent;
        compassManager.Subscribe();
        
        // Set UI references in size manager
        sizeManager.SetUIReferences(frameRect, maskObject, borderObj, maskImage, borderImg, minimapContent);
        sizeManager.SetCompassManager(compassManager);
        
        // Initialize scene integration helper
        sceneIntegration = new MinimapSceneIntegration(
            minimapContent, playerMarkerManager, mapPreferences, markerRegistry, propertyPoIManager);
        
        // Initialize marker coordinator
        markerCoordinator = new MinimapMarkerCoordinator(
            questMarkerManager, mapPreferences, minimapContent, sizeManager, markerRegistry, propertyPoIManager);
        markerCoordinator.SetCompassManager(compassManager);
    }

    private IEnumerator InitializeQuestManagerRoutine()
    {
        while (questMarkerManager is { IsInitialized: false })
        {
            questMarkerManager.Initialize();
            yield return new WaitForSeconds(1);
        }
    }

    private void OnContractTrackingChanged(bool previous, bool current)
    {
        markerCoordinator?.OnContractTrackingChanged(previous, current);
    }
    
    private void OnIncreaseSizeChanged(bool oldValue, bool newValue)
    {
        if (!initialized) return;
        sizeManager.UpdateMinimapSize(true);
        markerCoordinator.OnSizeChanged();
        UpdateMinimap();
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

    private void OnPropertyTrackingChanged(bool previous, bool current)
    {
        markerCoordinator?.OnPropertyTrackingChanged(previous, current);
    }

    private void OnShowCompassChanged(bool oldValue, bool newValue)
    {
        compassManager?.SetVisible(MinimapEnabled && newValue);
    }

    private void OnTimeBarEnableChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue)
        {
            minimapTimeDisplay?.SetTimeBarEnabled(TimeBarEnabled);
        }
    }

    private void OnVehicleTrackingChanged(bool previous, bool current)
    {
        if (current)
        {
            ownedVehiclesManager.AddAllMarkers();
        }
        else
        {
            ownedVehiclesManager.RemoveAllMarkers();
        }
    }
    
    private void StartMinimapUpdateLoop()
    {
        MelonCoroutines.Start(MinimapUpdateLoopCoroutine());
    }

    private void StartQuestInitializationLoop()
    {
        MelonCoroutines.Start(InitializeQuestManagerRoutine());
    }

    private void StartSceneIntegration()
    {
        MelonCoroutines.Start(SceneIntegrationRoutine());
    }

    private IEnumerator SceneIntegrationRoutine()
    {
        yield return sceneIntegration.IntegrateWithScene();
        markerCoordinator.SetCachedMapContent(sceneIntegration.CachedMapContent);
    }

    private IEnumerator MinimapUpdateLoopCoroutine()
    {
        while (true)
        {
            UpdateMinimap();
            yield return null;
        }
        // ReSharper disable once IteratorNeverReturns
    }

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
                if (OwnedVehiclesManager.IconPrefab != null)
                {
                    playerMarkerManager.ReplaceWithVehicleIcon(OwnedVehiclesManager.IconPrefab);
                }

                // Hide the vehicle's original marker on the map (only if vehicle tracking is enabled)
                if (mapPreferences.TrackVehicles.Value)
                {
                    ownedVehiclesManager?.RemoveMarker(currentVehicle);
                    previousVehicle = currentVehicle;
                }
            }
            else
            {
                playerMarkerManager.RestoreOriginalPlayerIcon();

                // Show the vehicle marker again on the map (only if vehicle tracking is enabled)
                if (mapPreferences.TrackVehicles.Value && previousVehicle != null)
                {
                    ownedVehiclesManager?.AddAllMarkers();
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
            // Calculate minimap mask radius in world units
            if (compassManager != null)
            {
                var minimapMaskSize = contentRect.sizeDelta.x + Constants.MinimapMaskDiameterOffset;
                var minimapMaskRadiusUI = minimapMaskSize / 2f;
                var minimapWorldRadius = minimapMaskRadiusUI / worldScale;
                compassManager?.SetWorldScale(worldScale);
                compassManager?.SetPlayerTransform(trackTransform);

                compassManager.MinimapWorldRadius = minimapWorldRadius;
                compassManager?.UpdateCompassMarkers();
            }
        }
        minimapTimeDisplay.UpdateMinimapTime();
    }
}

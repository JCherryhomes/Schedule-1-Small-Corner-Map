
using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if IL2CPP
using S1Property = Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Map;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI.Phone.Map;
#else
using S1Property = ScheduleOne.Property;
using ScheduleOne.Map;
using ScheduleOne.UI.Phone.Map;
#endif

namespace Small_Corner_Map.PoIManagers;

[RegisterTypeInIl2Cpp]
public class PropertyPoIManager : MonoBehaviour
{
    private RectTransform parentRT;
    private Dictionary<int, PoIMarkerView> poiMarkers = [];
    private Transform _playerTransform; 
    private RectTransform mapImageRT;
    private float worldScaleFactor;
    private float currentZoomLevel;

    // Throttle POI scanning to reduce per-frame allocations/work
    private int _frameCounter = 0;
    private const int ScanIntervalFrames = 30; // ~0.5s at 60 FPS

    private string[] allowList = 
    [
        "PropertyPoI(Clone)",
        "OwnedVehiclePoI(Clone)",
        "QuestPoI(Clone)",
        "ContractPoI(Clone)",
        "DeaddropPoI_Red(Clone)",
        "PotentialCustomerPoI(Clone)",
        "PotentialDealerPoI(Clone)"
    ];

    private string[] movingMarkers =
    [
        "PotentialCustomerPoI(Clone)",
        "PotentialDealerPoI(Clone)"
    ];

    public void Initialize(Transform player, RectTransform mapImage, float worldScale, float zoom, float offsetX, float offsetY)
    {
        _playerTransform = player; 
        mapImageRT = mapImage;
        worldScaleFactor = worldScale;
        currentZoomLevel = zoom;
        // minimapPlayerCenterXOffset and minimapPlayerCenterYOffset are no longer stored here
        // as they are not passed to PoIMarkerView
    }

    public void UpdateZoomLevel(float newZoomLevel)
    {
        currentZoomLevel = newZoomLevel;
        foreach (var kvp in poiMarkers)
        {
            kvp.Value.UpdateZoomLevel(newZoomLevel);
        }
    }

    private void Start()
    {
        parentRT = transform as RectTransform;
    }
    
    private void Update()
    {
        if (MapApp.Instance == null || MapApp.Instance.PoIContainer == null || _playerTransform == null) return;

        // Only rescan periodically to reduce overhead; dynamic markers update themselves
        _frameCounter++;
        if ((_frameCounter % ScanIntervalFrames) != 0)
            return;

        var currentChildren = MapApp.Instance.PoIContainer
            .GetComponentsInChildren<RectTransform>(false)
            .Where(rt => rt.gameObject.activeSelf && allowList.Any(allowed => rt.gameObject.name == allowed))
            .ToDictionary(rt => rt.gameObject.GetInstanceID(), rt => rt);

        // Remove old markers
        var childrenToRemove = poiMarkers.Where(marker => !currentChildren.ContainsKey(marker.Key));

        foreach (var kvp in childrenToRemove)
        {
            poiMarkers.Remove(kvp.Key);
        }

        // Add new markers
        foreach (var kvp in currentChildren)
        {
            var child = kvp.Value;
            if (poiMarkers.ContainsKey(child.GetInstanceID()) && !movingMarkers.Contains(child.name)) continue;
            
            var poiMarkerViewGO = new GameObject("PoIMarkerView_" + child.name);
            // Add RectTransform BEFORE parenting so it's created as a RectTransform
            var rect = poiMarkerViewGO.AddComponent<RectTransform>();
            rect.SetParent(mapImageRT, false);
            
            var poiMarkerView = poiMarkerViewGO.AddComponent<PoIMarkerView>();
            // Use the anchoredPosition from the phone map - it's already correctly positioned!
            bool isDynamic = movingMarkers.Contains(child.name);
            poiMarkerView.Initialize(child, child.anchoredPosition, worldScaleFactor, currentZoomLevel, isDynamic);
            poiMarkers[kvp.Key] = poiMarkerView;
        }
    }
}
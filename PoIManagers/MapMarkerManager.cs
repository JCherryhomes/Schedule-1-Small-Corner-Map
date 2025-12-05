
using MelonLoader;
using UnityEngine;
using System.Linq;
using Small_Corner_Map.Helpers;
using Il2CppScheduleOne.PlayerScripts;


#if IL2CPP
using Il2CppScheduleOne.UI.Phone.Map;
using Il2CppScheduleOne.Vehicles;
#else
using ScheduleOne.UI.Phone.Map;
using ScheduleOne.Vehicles;
#endif

namespace Small_Corner_Map.PoIManagers;

[RegisterTypeInIl2Cpp]
public class MapMarkerManager : MonoBehaviour
{
    private Dictionary<string, string> poiMarkerKeyMap = [];
    private Dictionary<string, PoIMarkerView> poiMarkers = []; // Use name+position as key for stability
    private RectTransform mapImageRT;
    private float worldScaleFactor;
    private float currentZoomLevel;
    private bool _isCircleMode;
    private bool _isTrackingVehicles;
    private string _vehiclePoIKey;
    private object _updateCoroutine;
    private float minimapPlayerCenterXOffset;
    private float minimapPlayerCenterYOffset;

    private HashSet<string> allowList = new HashSet<string>
    {
        "QuestPoI(Clone)",
        "DeaddropPoI_Red(Clone)"
    };

    public void Initialize(RectTransform mapImage, float worldScale, float zoom, bool trackProperties, bool trackContracts, bool trackVehicles, bool isCircle, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
    {
        mapImageRT = mapImage;
        worldScaleFactor = worldScale;
        currentZoomLevel = zoom;
        _isCircleMode = isCircle;
        _isTrackingVehicles = trackVehicles;
        this.minimapPlayerCenterXOffset = minimapPlayerCenterXOffset;
        this.minimapPlayerCenterYOffset = minimapPlayerCenterYOffset;
        
        // Initialize allowList based on tracking preferences
        if (trackProperties)
        {
            allowList.Add("PropertyPoI(Clone)");
        }
        if (trackContracts)
        {
            allowList.Add("ContractPoI(Clone)");
        }
        if (trackVehicles)
        {
            allowList.Add("OwnedVehiclePoI(Clone)");
            InvokeRepeating("UpdateVehicleMarkers", 0, 1.0f);
        }
    }

    public void UpdateZoomLevel(float newZoomLevel)
    {
        currentZoomLevel = newZoomLevel;
        foreach (var kvp in poiMarkers)
        {
            kvp.Value.UpdateZoomLevel(newZoomLevel);
        }
    }
    
    public void UpdateMinimapShape(bool isCircle)
    {
        _isCircleMode = isCircle;
        foreach (var kvp in poiMarkers)
        {
            kvp.Value.SetShape(isCircle);
        }
    }

    private void OnEnable()
    {
        if (_updateCoroutine == null)
        {
            _updateCoroutine = MelonCoroutines.Start(UpdatePoIMarkersCoroutine());
        }
    }

    private void OnDisable()
    {
        if (_updateCoroutine != null)
        {
            MelonCoroutines.Stop(_updateCoroutine);
            _updateCoroutine = null;
        }
    }
    
    private string GetGeneratedPoIKey(RectTransform rt)
    {
        // Use name + position as stable key (rounded to avoid floating point variations)
        return $"{rt.gameObject.name}_{Mathf.RoundToInt(rt.anchoredPosition.x)}_{Mathf.RoundToInt(rt.anchoredPosition.y)}";
    }

    private System.Collections.IEnumerator UpdatePoIMarkersCoroutine()
    {
        var waitInterval = new WaitForSeconds(1.0f);

        while (true)
        {
            yield return waitInterval;

            if (MapApp.Instance == null || MapApp.Instance.PoIContainer == null)
                continue;

            var allChildren = MapApp.Instance.PoIContainer.GetComponentsInChildren<RectTransform>(false);
            var currentChildren = new Dictionary<string, RectTransform>();
            
            // Only process static markers (skip moving markers like customers/dealers)
            foreach (var rt in allChildren)
            {
                if (rt.gameObject.activeSelf && allowList.Contains(rt.gameObject.name))
                {
                    // Use name + position as stable key (rounded to avoid floating point variations)
                    string key = GetGeneratedPoIKey(rt);
                    currentChildren[key] = rt;
                }
            }

            // Remove old markers that no longer exist
            var childrenToRemove = poiMarkers.Where(marker => 
                !currentChildren.ContainsKey(marker.Key)).ToList();
            
            // Remove markers from childrenToRemove if they exist in poiMarkerKeyMap (to keep track of vehicle PoIs)
            childrenToRemove.RemoveAll(kvp => poiMarkerKeyMap.ContainsValue(kvp.Key));

            foreach (var kvp in childrenToRemove)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
                poiMarkers.Remove(kvp.Key);
            }

            // Add new static markers (skip if already exists)
            foreach (var kvp in currentChildren)
            {
                if (poiMarkers.ContainsKey(kvp.Key) || poiMarkerKeyMap.ContainsValue(kvp.Key))
                    continue;
                
                var child = kvp.Value;
                
                // Create new marker
                var poiMarkerViewGO = new GameObject("PoIMarkerView_" + child.name);
                var rect = poiMarkerViewGO.AddComponent<RectTransform>();
                
                // All markers parented to mapImageRT (move with map)
                rect.SetParent(mapImageRT, false);
                
                var poiMarkerView = poiMarkerViewGO.AddComponent<PoIMarkerView>();
                poiMarkerView.Initialize(child, child.anchoredPosition, worldScaleFactor, currentZoomLevel, false, child.gameObject.name, mapImageRT, _isCircleMode);

                if (kvp.Key.Contains("OwnedVehiclePoI(Clone)"))
                {
                    MelonLogger.Msg("Adding/Updating vehicle PoI to poiMarkerKeyMap");
                    poiMarkerKeyMap[kvp.Key] = kvp.Key;
                }
                
                poiMarkers[kvp.Key] = poiMarkerView;
            }
        }  
    }
        
    public void OnTrackPropertiesChanged(bool isTracking)
    {
        if (isTracking)
        {
            allowList.Add("PropertyPoI(Clone)");
        }
        else
        {
            allowList.Remove("PropertyPoI(Clone)");
        }
    }
    
    public void OnTrackContractsChanged(bool isTracking)
    {
        if (isTracking)
        {
            allowList.Add("ContractPoI(Clone)");
        }
        else
        {
            allowList.Remove("ContractPoI(Clone)");
        }
    }

    public void OnTrackVehiclesChanged(bool isTracking)
    {
        _isTrackingVehicles = isTracking;
        if (isTracking)
        {
            allowList.Add("OwnedVehiclePoI(Clone)");
            foreach(var kvp in poiMarkers.Where(entry => entry.Key.Contains("OwnedVehiclePoI(Clone)")).ToList())
            {
                kvp.Value.gameObject.SetActive(true);
            }
            InvokeRepeating("UpdateVehicleMarkers", 0, 1.0f);
        }
        else
        {
            CancelInvoke("UpdateVehicleMarkers");
            foreach(var kvp in poiMarkers.Where(entry => entry.Key.Contains("OwnedVehiclePoI(Clone)")).ToList())
            {
                kvp.Value.gameObject.SetActive(false);
            }
            allowList.Remove("OwnedVehiclePoI(Clone)");
        }
    }
    
    public void OnPlayerEnterVehicle(LandVehicle vehicle)
    {
        if (!_isTrackingVehicles) return;
        _vehiclePoIKey = GetGeneratedPoIKey(vehicle.POI.UI);        
        poiMarkers[_vehiclePoIKey].gameObject.SetActive(false);
    }
    
    public void OnPlayerExitVehicle(LandVehicle vehicle)
    {
        if (!_isTrackingVehicles) return;
        var marker = poiMarkers[_vehiclePoIKey];
        
        marker.gameObject.SetActive(true);
        _vehiclePoIKey = null;
    }
    
    private void UpdateVehicleMarkers()
    {
        foreach (var vehicle in VehicleManager.Instance.PlayerOwnedVehicles)
        {
            MelonLogger.Msg("Updating vehicle marker for: " + vehicle.VehicleName);
            var key = GetGeneratedPoIKey(vehicle.POI.UI);
            MelonLogger.Msg("Generated key: " + key);
            
            foreach(var existingKey in poiMarkers.Keys)
            {
                if (!existingKey.StartsWith("OwnedVehiclePoI(Clone)_")) continue;
                MelonLogger.Msg("Existing marker key: " + existingKey);
            }
            
            if (poiMarkers.TryGetValue(key, out var marker))
            {
                MelonLogger.Msg("Found marker for key: " + key);
                marker.UpdatePositionFromWorld(vehicle.transform.position);
            }
        }
    }
}
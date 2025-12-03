
using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;
#if IL2CPP
using Il2CppScheduleOne.Map;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI.Phone.Map;
using System.Linq;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
#else
using S1Property = ScheduleOne.Property;
using ScheduleOne.Map;
using ScheduleOne.UI.Phone.Map;
using System.Linq;
using Il2CppIEnumerator = System.Collections.IEnumerator;
#endif

namespace Small_Corner_Map.PoIManagers;

[RegisterTypeInIl2Cpp]
public class MapMarkerManager : MonoBehaviour
{
    private Dictionary<string, PoIMarkerView> poiMarkers = []; // Use name+position as key for stability
    private Transform _playerTransform;
    private RectTransform mapImageRT;
    private float worldScaleFactor;
    private float currentZoomLevel;
    private bool _isCircleMode;

    private object _updateCoroutine;

    private HashSet<string> allowList = new HashSet<string>
    {
        "QuestPoI(Clone)",
        "DeaddropPoI_Red(Clone)"
    };

    public void Initialize(Transform player, RectTransform mapImage, float worldScale, float zoom, bool trackProperties, bool trackContracts, bool trackVehicles, bool isCircle)
    {
        _playerTransform = player; 
        mapImageRT = mapImage;
        worldScaleFactor = worldScale;
        currentZoomLevel = zoom;
        _isCircleMode = isCircle;
        
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


    private System.Collections.IEnumerator UpdatePoIMarkersCoroutine()
    {
        var waitInterval = new WaitForSeconds(1.0f);

        while (true)
        {
            yield return waitInterval;

            if (MapApp.Instance == null || MapApp.Instance.PoIContainer == null || _playerTransform == null)
                continue;

            var allChildren = MapApp.Instance.PoIContainer.GetComponentsInChildren<RectTransform>(false);
            var currentChildren = new Dictionary<string, RectTransform>();
            
            // Only process static markers (skip moving markers like customers/dealers)
            foreach (var rt in allChildren)
            {
                if (rt.gameObject.activeSelf && allowList.Contains(rt.gameObject.name))
                {
                    // Use name + position as stable key (rounded to avoid floating point variations)
                    string key = $"{rt.gameObject.name}_{Mathf.RoundToInt(rt.anchoredPosition.x)}_{Mathf.RoundToInt(rt.anchoredPosition.y)}";
                    currentChildren[key] = rt;
                }
            }

            // Remove old markers that no longer exist
            var childrenToRemove = poiMarkers.Where(marker => !currentChildren.ContainsKey(marker.Key)).ToList();

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
                if (poiMarkers.ContainsKey(kvp.Key))
                    continue;
                
                var child = kvp.Value;
                
                // Create new marker
                var poiMarkerViewGO = new GameObject("PoIMarkerView_" + child.name);
                var rect = poiMarkerViewGO.AddComponent<RectTransform>();
                
                // All markers parented to mapImageRT (move with map)
                rect.SetParent(mapImageRT, false);
                
                var poiMarkerView = poiMarkerViewGO.AddComponent<PoIMarkerView>();
                poiMarkerView.Initialize(child, child.anchoredPosition, worldScaleFactor, currentZoomLevel, false, child.gameObject.name, mapImageRT, _isCircleMode);
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
        if (isTracking)
        {
            allowList.Add("OwnedVehiclePoI(Clone)");
        }
        else
        {
            allowList.Remove("OwnedVehiclePoI(Clone)");
        }
    } 
}
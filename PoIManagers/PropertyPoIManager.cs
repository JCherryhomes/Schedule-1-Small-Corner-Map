
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
    private Dictionary<string, PoIMarkerView> poiMarkers = []; // Use name+position as key for stability
    private Transform _playerTransform;
    private RectTransform mapImageRT;
    private float worldScaleFactor;
    private float currentZoomLevel;

    private object _updateCoroutine;

    private HashSet<string> allowList = new HashSet<string>
    {
        "PropertyPoI(Clone)",
        "OwnedVehiclePoI(Clone)",
        "QuestPoI(Clone)",
        "ContractPoI(Clone)",
        "DeaddropPoI_Red(Clone)"
    };

    public void Initialize(Transform player, RectTransform mapImage, float worldScale, float zoom)
    {
        _playerTransform = player; 
        mapImageRT = mapImage;
        worldScaleFactor = worldScale;
        currentZoomLevel = zoom;
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
                rect.SetParent(mapImageRT, false);
                
                var poiMarkerView = poiMarkerViewGO.AddComponent<PoIMarkerView>();
                poiMarkerView.Initialize(child, child.anchoredPosition, worldScaleFactor, currentZoomLevel, false);
                poiMarkers[kvp.Key] = poiMarkerView;
            }
        }
    }
}
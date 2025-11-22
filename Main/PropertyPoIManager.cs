using MelonLoader;
using Small_Corner_Map.Helpers;
using UnityEngine;


#if IL2CPP
using S1Property = Il2CppScheduleOne.Property;
#else
using S1Property = ScheduleOne.Property;
#endif

namespace Small_Corner_Map.Main;

public static class PropertyPoIManager
{
    private const string PropertyPoIKey = "PropertyPoI_Marker";
    private static Transform _iconContainer;
    
    public static void Initialize(MinimapContent minimapContent, GameObject cachedMapContent, MarkerRegistry markerRegistry)
    {
        CacheIconContainer(cachedMapContent);
        if (_iconContainer == null) {
            MelonLogger.Msg("PropertyPoIManager: IconContainer not found, cannot add property markers.");
            return;
        }
        var properties = S1Property.PropertyManager.Instance?.GetComponentsInChildren<S1Property.Property>();
        if (!(properties?.Length > 0)) return;
        var scale = minimapContent.CurrentMapScale;
        foreach (var property in properties)
        {
            if (!property.IsOwned) continue;
            var worldPos = property.gameObject.transform.position;
            AddPropertyMarker(markerRegistry, worldPos, scale, property.name);
        }
    }

    public static void DisableAllMarkers(MarkerRegistry markerRegistry)
    {
        var properties = S1Property.PropertyManager.Instance?.GetComponentsInChildren<S1Property.Property>();
        if (!(properties?.Length > 0)) return;
        foreach (var property in properties)
        {
            markerRegistry.RemoveMarker(PropertyPoIKey + "_" + property.name);
        }
    }
    
    public static void CacheIconContainerIfNeeded(GameObject cachedMapContent)
    {
        CacheIconContainer(cachedMapContent);
    }
    
    private static void CacheIconContainer(GameObject cachedMapContent)
    {
        if (_iconContainer != null) return;

        var propertyPoI = cachedMapContent.transform.Find("PropertyPoI(Clone)");
        if (propertyPoI == null) return;

        var iconContainer = propertyPoI.Find("IconContainer");
        if (iconContainer == null) return;

        _iconContainer = iconContainer;
    }

    private static void AddPropertyMarker(MarkerRegistry markerRegistry, Vector3 worldPos, float mapScale, string propertyName)
    {
        var markerData = new MarkerRegistry.MarkerData
        {
            Id = PropertyPoIKey + "_" + propertyName,
            WorldPos = worldPos,
            IconPrefab = _iconContainer.gameObject,
            Type = MarkerType.Property,
            DisplayName = propertyName,
            XOffset = -Constants.MarkerXOffset,
            ZOffset = -Constants.MarkerZOffset,
            IsTracked = true,
            IsVisibleOnMinimap = true,
            IsVisibleOnCompass = true
        };
        markerRegistry.AddOrUpdateMarker(markerData);
    }

    public static void RefreshAll(MinimapContent minimapContent, GameObject cachedMapContent, MarkerRegistry markerRegistry)
    {
        CacheIconContainer(cachedMapContent);
        if (_iconContainer == null)
        {
            MelonLogger.Warning("PropertyPoIManager: Cannot refresh markers, IconContainer is still null after caching attempt");
            return;
        }
        DisableAllMarkers(markerRegistry);
        Initialize(minimapContent, cachedMapContent, markerRegistry);
    }

    public static GameObject PropertyIconPrototype => _iconContainer != null ? _iconContainer.gameObject : null;
}
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
    private static Transform IconContainer = null;
    
    public static void Initialize(MinimapContent minimapContent, GameObject cachedMapContent)
    {
        CacheIconContainer(cachedMapContent);
        if (IconContainer == null) {
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
            // Pass through new scale-aware add
            AddPropertyMarker(minimapContent, worldPos, scale);
        }
    }

    public static void DisableAllMarkers()
    {
        MinimapPoIHelper.RemoveAllByKey(PropertyPoIKey);
    }
    
    public static void CacheIconContainerIfNeeded(GameObject cachedMapContent)
    {
        CacheIconContainer(cachedMapContent);
    }
    
    private static void CacheIconContainer(GameObject cachedMapContent)
    {
        if (IconContainer != null) return;
        
        var propertyPoI = cachedMapContent.transform.Find("PropertyPoI(Clone)");
        if (propertyPoI == null) return;
            
        var iconContainer = propertyPoI.Find("IconContainer");
        if (iconContainer == null) return;

        IconContainer = iconContainer;
    }

    private static void AddPropertyMarker(MinimapContent minimapContent, Vector3 worldPos, float mapScale)
    {
        MinimapPoIHelper.AddWhitePoIMarker(
            minimapContent,
            worldPos,
            IconContainer.gameObject,
            PropertyPoIKey);
    }

    public static void RefreshAll(MinimapContent minimapContent, GameObject cachedMapContent)
    {
        // Ensure IconContainer is cached before trying to refresh
        CacheIconContainer(cachedMapContent);
        
        if (IconContainer == null)
        {
            MelonLogger.Warning("PropertyPoIManager: Cannot refresh markers, IconContainer is still null after caching attempt");
            return;
        }
        
        DisableAllMarkers();
        Initialize(minimapContent, cachedMapContent);
    }
}
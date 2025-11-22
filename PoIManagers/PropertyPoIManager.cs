using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using UnityEngine;
#if IL2CPP
using S1Property = Il2CppScheduleOne.Property;
#else
using S1Property = ScheduleOne.Property;
#endif

namespace Small_Corner_Map.PoIManagers;

public class PropertyPoIManager : PoIManagerBase<S1Property.Property>, IPoIManager<S1Property.Property>
{
    public PropertyPoIManager(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry) : base(mapContent, preferences, registry)
    {
        MarkerKeyPrefix = "PropertyPoI_Marker";
    }
    
    public void Initialize()
    {
        AddAllMarkers();
    }
    
    public void CacheIconContainerIfNeeded()
    {
        CachePoIIcon(null);
    }

    public void RefreshAll()
    {
        if (IconPrefab == null)
        {
            MelonLogger.Warning("PropertyPoIManager: Cannot refresh markers, IconContainer is still null after caching attempt");
            return;
        }
        
        RemoveAllMarkers();
        Initialize();
    }

    public static GameObject PropertyIconPrototype => IconPrefab != null ? IconPrefab.gameObject : null;
    
    internal override void CachePoIIcon(S1Property.Property marker)
    {
        if (IconPrefab != null) return;

        if (marker != null)
        {
            IconPrefab = marker.PoI.IconContainer.gameObject;
            return;
        }
    }

    public void AddMarker(S1Property.Property property)
    {
        if (IconPrefab == null)
            CachePoIIcon(property);
        
        var scale = MapContent.CurrentMapScale;
        var worldPos = property.gameObject.transform.position;
        var markerData = GetMarkerData(property);
        Registry.AddOrUpdateMarker(markerData);
    }

    public void AddAllMarkers()
    {
        var properties = S1Property.PropertyManager.Instance?.GetComponentsInChildren<S1Property.Property>().ToList() ?? [];
        foreach (var property in properties)
        {
            if (!property.IsOwned) continue;
            AddMarker(property);
        }
    }

    public void RemoveMarker(S1Property.Property marker)
    {
        Registry.RemoveMarker(GetMarkerName(marker));
    }

    public void RemoveAllMarkers()
    {
        var properties = S1Property.PropertyManager.Instance?.GetComponentsInChildren<S1Property.Property>();
        if (!(properties?.Length > 0)) return;
        foreach (var property in properties)
        {
            Registry.RemoveMarker(GetMarkerName(property));
        }
    }

    internal override MarkerRegistry.MarkerData GetMarkerData(S1Property.Property property)
    {
        var scale = MapContent.CurrentMapScale;
        var worldPos = property.gameObject.transform.position;
        return new MarkerRegistry.MarkerData
        {
            Id = GetMarkerName(property),
            WorldPos = worldPos,
            IconPrefab = IconPrefab,
            Type = MarkerType.Property,
            DisplayName = property.name,
            XOffset = -Constants.MarkerXOffset,
            ZOffset = -Constants.MarkerZOffset,
            IsTracked = true,
            IsVisibleOnMinimap = true,
            IsVisibleOnCompass = true
        };
    }

    protected override string GetMarkerName(S1Property.Property property)
    {
        return MarkerKeyPrefix + "_" + property.name;
    }
}
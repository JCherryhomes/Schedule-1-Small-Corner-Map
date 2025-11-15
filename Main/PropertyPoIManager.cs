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
    public static void Initialize(MinimapContent minimapContent, Transform iconContainer)
    {
        var properties = S1Property.PropertyManager.Instance?.GetComponentsInChildren<S1Property.Property>();
        if (!(properties?.Length > 0)) return;
        
        foreach (var property in properties)
        {
            if (!property.IsOwned) continue;
            
            MinimapPoIHelper.AddWhitePoIMarker(
                minimapContent, property.gameObject.transform.position, iconContainer.gameObject);
        }
    }
}
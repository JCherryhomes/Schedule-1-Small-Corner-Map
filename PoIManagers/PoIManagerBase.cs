using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using UnityEngine;

namespace Small_Corner_Map.PoIManagers;

public abstract class PoIManagerBase<T>
{
    public static GameObject IconPrefab { get; set; }

    internal string MarkerKeyPrefix { get; set; }
    internal MinimapContent MapContent { get; }
    internal MapPreferences Preferences { get; }
    internal MarkerRegistry Registry { get; }
    internal GameObject MapContentObject => MapContent.MapContentObject;

    public PoIManagerBase(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry)
    {
        MapContent = mapContent;
        Preferences = preferences;
        Registry = registry;
    }
    
    protected virtual string GetMarkerName(T marker)
    {
        var guidProperty = marker.GetType().GetProperty("GUID");
        if (guidProperty == null)
        {
            MelonLogger.Msg("GetMarkerName: The type " + marker.GetType().Name + " does not have a GUID property.");
            throw new MissingMemberException("You may need to override GetMarkerName in the derived class.");
        }

        return MarkerKeyPrefix + "_" + guidProperty.GetValue(marker);
    }

    internal abstract MarkerRegistry.MarkerData GetMarkerData(T marker);

    internal abstract void CachePoIIcon(T marker);
}
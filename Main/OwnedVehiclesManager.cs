using MelonLoader;
using Small_Corner_Map.Helpers;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if IL2CPP
using S1Vehicles = Il2CppScheduleOne.Vehicles;
#else
using S1Vehicles = ScheduleOne.Vehicles;
#endif

namespace Small_Corner_Map.Main;

public class OwnedVehiclesManager
{
    private const string BaseKey = "OwnedVehicle_Marker";
    private const string ZeroGuid = "00000000-0000-0000-0000-000000000000";
    
    private MinimapContent MinimapContent { get; }
    private MapPreferences MapPreferences { get; }
    
    internal static RectTransform IconContainer { get; private set; }
    internal static GameObject VehicleIconPrototype => IconContainer != null ? IconContainer.gameObject : null;
    
    private readonly Dictionary<string, object> activeFades = new();
    private readonly Dictionary<int,string> vehicleInstanceToMarkerKey = new(); // instanceID -> current marker key
    private int tempCounter;
    
    public OwnedVehiclesManager(MinimapContent minimapContent, MapPreferences mapPreferences)
    {
        MinimapContent = minimapContent;
        MapPreferences = mapPreferences;
    }
    
    public void AddOwnedVehicleMarkers()
    {
        var ownedVehicles = S1Vehicles.VehicleManager.Instance.PlayerOwnedVehicles;
        if (ownedVehicles == null || ownedVehicles.Count == 0) return;
        foreach (var vehicle in ownedVehicles)
        {
            AddOrUpgradeVehicleMarker(vehicle);
        }
    }
    
    /// <summary>
    /// Adds a new vehicle marker or upgrades an existing temporary one to a real one.
    /// Temporary markers are used for vehicles with a zero GUID until they are assigned a real GUID.
    /// </summary>
    /// <param name="vehicle"></param>
    private void AddOrUpgradeVehicleMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (IconContainer == null)
            IconContainer = vehicle?.POI?.IconContainer;
        
        if (IconContainer == null)
        {
            MelonLogger.Warning("OwnedVehiclesManager: Cannot add vehicle marker, IconContainer is null!");
            return;
        }

        if (vehicle == null)
        {
            MelonLogger.Warning("OwnedVehiclesManager: Cannot add vehicle marker, vehicle is null!");
            return;
        }

        var instanceId = vehicle.gameObject.GetInstanceID();
        var realGuid = vehicle.GUID.ToString();
        var hasMapping = vehicleInstanceToMarkerKey.TryGetValue(instanceId, out var existingKey);
        // If we already have a marker and GUID is now real & key is temp, rename.
        if (hasMapping && existingKey != null && existingKey.Contains("_TEMP_"))
        {
            if (realGuid != ZeroGuid)
            {
                var newKey = BaseKey + "_" + realGuid;
                if (!MinimapPoIHelper.RenameMarker(existingKey, newKey)) return;
                vehicleInstanceToMarkerKey[instanceId] = newKey;
                return; // upgraded, no need to rebuild
            }
        }
        // If we already have a non-temp mapping, nothing to do unless we want to refresh position
        if (hasMapping && existingKey != null && !existingKey.Contains("_TEMP_"))
        {
            RefreshVehicleMarkerPosition(vehicle, existingKey);
            return;
        }
        // Need to create marker
        string key;
        if (realGuid == ZeroGuid)
        {
            key = BaseKey + "_TEMP_" + tempCounter++;
        }
        else
        {
            key = BaseKey + "_" + realGuid;
        }
        // Avoid duplicate creation if a marker with intended key already exists (rare race)
        if (MinimapPoIHelper.TryGetMarker(key) != null)
        {
            vehicleInstanceToMarkerKey[instanceId] = key;
            RefreshVehicleMarkerPosition(vehicle, key);
            return;
        }
        CreateVehicleMarker(vehicle, key);
        vehicleInstanceToMarkerKey[instanceId] = key;
    }
    
    private void CreateVehicleMarker(S1Vehicles.LandVehicle vehicle, string key)
    {
        var worldPos = vehicle.transform.position;
        var scale = Constants.DefaultMapScale * MapPreferences.MinimapScaleFactor;
        var mappedPos = new Vector2(worldPos.x * scale - Constants.MarkerXOffset, worldPos.z * scale);
        MinimapPoIHelper.RemoveAllByKey(key); // ensure clean
        MinimapPoIHelper.AddMarkerToMap(IconContainer.gameObject, MinimapContent.MapContentObject, key, mappedPos, worldPos);
    }
    
    private void RefreshVehicleMarkerPosition(S1Vehicles.LandVehicle vehicle, string key)
    {
        var worldPos = vehicle.transform.position;
        var scale = Constants.DefaultMapScale * MapPreferences.MinimapScaleFactor;
        var mappedPos = new Vector2(worldPos.x * scale - Constants.MarkerXOffset, worldPos.z * scale);
        MinimapPoIHelper.UpdateMarkerPosition(key, mappedPos);
    }
    
    private static IEnumerator FadeRoutine(GameObject marker, bool outFade, Action onComplete)
    {
        if (marker == null) { onComplete?.Invoke(); yield break; }
        var gfx = marker.GetComponentsInChildren<Graphic>(true);
        if (gfx.Length == 0) { onComplete?.Invoke(); yield break; }
        var start = gfx.Select(g => g.color.a).ToArray();
        var target = outFade ? 0f : 1f;
        var t = 0f;
        while (t < Constants.VehicleMarkerFadeDuration)
        {
            if (marker == null) { onComplete?.Invoke(); yield break; }
            t += Time.deltaTime;
            var lerpT = Mathf.Clamp01(t / Constants.VehicleMarkerFadeDuration);
            for (var i = 0; i < gfx.Length; i++)
            {
                var g = gfx[i]; if (g == null) continue;
                var c = g.color; c.a = Mathf.Lerp(start[i], target, lerpT); g.color = c;
            }
            yield return null;
        }
        if (outFade && marker != null)
            UnityEngine.Object.Destroy(marker);
        onComplete?.Invoke();
    }
    
    private void StartFade(string key, GameObject marker, bool outFade, Action after)
    {
        if (activeFades.TryGetValue(key, out var running) && running != null)
        {
            MelonCoroutines.Stop(running);
            activeFades.Remove(key);
        }
        var handle = MelonCoroutines.Start(FadeRoutine(marker, outFade, () => { activeFades.Remove(key); after?.Invoke(); }));
        activeFades[key] = handle;
    }
    
    public void HideVehicleMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (vehicle == null) return;
        var currentKey = GetCurrentKey(vehicle);
        if (currentKey == null) return;
        var marker = MinimapPoIHelper.TryGetMarker(currentKey);
        if (marker != null)
            StartFade(currentKey, marker, true, null);
    }
    
    public void ShowVehicleMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (vehicle == null) return;
        var instanceId = vehicle.gameObject.GetInstanceID();
        if (!vehicleInstanceToMarkerKey.TryGetValue(instanceId, out var key) || key == null)
        {
            AddOrUpgradeVehicleMarker(vehicle);
            key = vehicleInstanceToMarkerKey.GetValueOrDefault(instanceId);
            if (key == null) return;
        }
        var marker = MinimapPoIHelper.TryGetMarker(key);
        if (marker == null)
        {
            // Recreate if missing
            CreateVehicleMarker(vehicle, key);
            marker = MinimapPoIHelper.TryGetMarker(key);
            if (marker == null) return;
        }
        // Set alpha to 0 then fade in
        foreach (var g in marker.GetComponentsInChildren<Graphic>(true))
        {
            var c = g.color; c.a = 0f; g.color = c;
        }
        
        StartFade(key, marker, false, null);
    }
    
    private string GetCurrentKey(S1Vehicles.LandVehicle vehicle)
    {
        if (vehicle == null) return null;
        var instanceId = vehicle.gameObject.GetInstanceID();
        return vehicleInstanceToMarkerKey.GetValueOrDefault(instanceId);
    }
    
    public void RemoveOwnedVehicleMarkers()
    {
        foreach (var kv in vehicleInstanceToMarkerKey)
        {
            MinimapPoIHelper.RemoveAllByKey(kv.Value);
        }
        vehicleInstanceToMarkerKey.Clear();
    }
    
    public void UpgradeVehicleMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (vehicle == null) return;
        var instanceId = vehicle.gameObject.GetInstanceID();
        if (!vehicleInstanceToMarkerKey.TryGetValue(instanceId, out var oldKey)) return;
        if (oldKey == null || !oldKey.Contains("_TEMP_")) return; // already upgraded
        var realGuid = vehicle.GUID.ToString();
        if (realGuid == ZeroGuid) return; // still zero
        var newKey = BaseKey + "_" + realGuid;
        // Stop fade if active
        if (activeFades.TryGetValue(oldKey, out var fadeHandle) && fadeHandle != null)
        {
            MelonCoroutines.Stop(fadeHandle);
            activeFades.Remove(oldKey);
        }

        if (!MinimapPoIHelper.RenameMarker(oldKey, newKey)) return;
        vehicleInstanceToMarkerKey[instanceId] = newKey;
    }
}
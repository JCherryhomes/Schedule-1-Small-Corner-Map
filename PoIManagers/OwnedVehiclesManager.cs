using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

#if IL2CPP
using S1Vehicles = Il2CppScheduleOne.Vehicles;
#else
using S1Vehicles = ScheduleOne.Vehicles;
#endif

namespace Small_Corner_Map.PoIManagers;

public class OwnedVehiclesManager : PoIManagerBase<S1Vehicles.LandVehicle>, IPoIManager<S1Vehicles.LandVehicle>
{
    public OwnedVehiclesManager(MinimapContent minimapContent, MapPreferences mapPreferences, MarkerRegistry registry)
        : base(minimapContent, mapPreferences, registry)
    {
        MarkerKeyPrefix = "OwnedVehicle_Marker";
    }

    public void AddAllMarkers()
    {
        var ownedVehicles = S1Vehicles.VehicleManager.Instance.PlayerOwnedVehicles;
        if (ownedVehicles == null || ownedVehicles.Count == 0) return;
        foreach (var vehicle in ownedVehicles)
        {
            AddMarker(vehicle);
        }
    }

    public void AddMarker(S1Vehicles.LandVehicle vehicle)
    {
        AddOrUpgradeVehicleMarker(vehicle);
    }

    public void RemoveAllMarkers()
    {
        Registry.RemoveMarkersByKeyPrefix(MarkerKeyPrefix);
    }

    public void RemoveMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (vehicle == null) return;
        var key = GetMarkerName(vehicle);
        if (key == null) return;
        Registry.RemoveMarker(key);
    }

    internal override void CachePoIIcon(S1Vehicles.LandVehicle vehicle)
    {
        if (IconPrefab == null)
            IconPrefab = vehicle?.POI?.IconContainer.gameObject;
    }

    private void AddOrUpgradeVehicleMarker(S1Vehicles.LandVehicle vehicle)
    {
        if (IconPrefab == null)
            IconPrefab = vehicle?.POI?.IconContainer.gameObject;

        if (IconPrefab == null)
        {
            MelonLogger.Warning("OwnedVehiclesManager: Cannot add vehicle marker, IconContainer is null!");
            return;
        }

        if (vehicle == null)
        {
            MelonLogger.Warning("OwnedVehiclesManager: Cannot add vehicle marker, vehicle is null!");
            return;
        }
        
        Registry.AddOrUpdateMarker(GetMarkerData(vehicle));
    }

    protected override string GetMarkerName(S1Vehicles.LandVehicle vehicle)
    {
        return MarkerKeyPrefix + "_" + vehicle.GetInstanceID();
    }

    internal override MarkerRegistry.MarkerData GetMarkerData(S1Vehicles.LandVehicle vehicle)
    {
        var worldPos = vehicle.transform.position;
        return new MarkerRegistry.MarkerData
        {
            Id = GetMarkerName(vehicle),
            WorldPos = worldPos,
            IconPrefab = IconPrefab,
            Type = MarkerType.Vehicle,
            DisplayName = vehicle.name,
            XOffset = -Constants.MarkerXOffset,
            ZOffset = -Constants.MarkerZOffset,
            IsTracked = true,
            IsVisibleOnMinimap = true,
            IsVisibleOnCompass = true
        };
    }
}

using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

#if IL2CPP
using Il2CppScheduleOne.Economy;
#else
using ScheduleOne.Economy;
#endif

namespace Small_Corner_Map.PoIManagers;

public class SuppliersPoIManager : PoIManagerBase<SupplierLocation>, IPoIManager<SupplierLocation>
{
    public SuppliersPoIManager(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry) : base(mapContent, preferences, registry)
    {
        MarkerKeyPrefix = "SupplierPoI_Marker";
    }

    public void AddMarker(SupplierLocation supplierLocation)
    {
        if (IconPrefab == null)
            CachePoIIcon(supplierLocation);
        
        Registry.AddOrUpdateMarker(GetMarkerData(supplierLocation));
    }

    public void AddAllMarkers()
    {
        // Not implemented: Supplier meetings do not carry across saves
        // so we do not need to load them all at once.
    }

    public void RemoveMarker(SupplierLocation supplierLocation)
    {
        Registry.RemoveMarker(GetMarkerName(supplierLocation));
    }

    public void RemoveAllMarkers()
    {
        Registry.RemoveMarkersByKeyPrefix(MarkerKeyPrefix);
    }

    protected override string GetMarkerName(SupplierLocation supplierLocation)
    {
        return MarkerKeyPrefix + "_" + supplierLocation.GetInstanceID();
    }

    internal override MarkerRegistry.MarkerData GetMarkerData(SupplierLocation supplierLocation)
    {
        return new MarkerRegistry.MarkerData()
        {
            IconPrefab = IconPrefab,
            WorldPos = supplierLocation.transform.position,
            Id = GetMarkerName(supplierLocation),
            Type = MarkerType.Supplier,
            DisplayName = supplierLocation.LocationName,
            XOffset = -Constants.MarkerXOffset,
            ZOffset = -Constants.MarkerZOffset,
            IsTracked = true,
            IsVisibleOnMinimap = true,
            IsVisibleOnCompass = true
        };
    }

    internal override void CachePoIIcon(SupplierLocation supplierLocation)
    {
        if (supplierLocation == null || IconPrefab != null) return;
        IconPrefab = supplierLocation.PoI.IconContainer.gameObject;
    }
}
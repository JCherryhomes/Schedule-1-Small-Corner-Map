#if IL2CPP
using S1Quests = Il2CppScheduleOne.Quests;
#else
using S1Quests = ScheduleOne.Quests;
#endif
using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;
namespace Small_Corner_Map.Main;
/// <summary>
/// Coordinates all marker managers (contracts, properties) and handles tracking preferences.
/// </summary>
internal class MinimapMarkerCoordinator
{
    private readonly ContractMarkerManager contractMarkerManager;
    private readonly MapPreferences mapPreferences;
    private readonly MinimapContent minimapContent;
    private readonly MinimapSizeManager sizeManager;
    private GameObject cachedMapContent;
    public MinimapMarkerCoordinator(
        ContractMarkerManager contractManager,
        MapPreferences preferences,
        MinimapContent content,
        MinimapSizeManager sizeMgr)
    {
        contractMarkerManager = contractManager;
        mapPreferences = preferences;
        minimapContent = content;
        sizeManager = sizeMgr;
    }
    public void SetCachedMapContent(GameObject mapContent)
    {
        cachedMapContent = mapContent;
    }
    public void OnContractAccepted(S1Quests.Contract contract)
    {
        contractMarkerManager.AddContractPoIMarkerWorld(contract);
    }
    public void OnContractCompleted(S1Quests.Contract contract)
    {
        contractMarkerManager.RemoveContractPoIMarkers(contract);
    }
    public void OnContractTrackingChanged(bool previous, bool current)
    {
        if (contractMarkerManager == null) return;
        if (current)
        {
            contractMarkerManager.AddAllContractPoIMarkers();
        }
        else
        {
            contractMarkerManager.RemoveAllContractPoIMarkers();
        }
    }
    public void OnPropertyTrackingChanged(bool previous, bool current)
    {
        if (current)
        {
            if (cachedMapContent != null)
            {
                PropertyPoIManager.RefreshAll(minimapContent, cachedMapContent);
            }
        }
        else
        {
            PropertyPoIManager.DisableAllMarkers();
        }
    }
    public void OnSizeChanged()
    {
        minimapContent?.UpdateMapScale(sizeManager.CurrentWorldScale);
        MinimapPoIHelper.UpdateAllMarkerPositions(sizeManager.CurrentWorldScale);
        if (mapPreferences.TrackProperties.Value && cachedMapContent != null)
            PropertyPoIManager.RefreshAll(minimapContent, cachedMapContent);
    }
}
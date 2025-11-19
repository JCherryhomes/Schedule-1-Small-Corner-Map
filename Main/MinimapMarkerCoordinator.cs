#if IL2CPP
using S1Quests = Il2CppScheduleOne.Quests;
#else
using S1Quests = ScheduleOne.Quests;
#endif
using UnityEngine;
using Small_Corner_Map.Helpers;
namespace Small_Corner_Map.Main;
/// <summary>
/// Coordinates all marker managers (contracts, properties) and handles tracking preferences.
/// </summary>
internal class MinimapMarkerCoordinator
{
    private readonly QuestMarkerManager questMarkerManager;
    private readonly MapPreferences mapPreferences;
    private readonly MinimapContent minimapContent;
    private readonly MinimapSizeManager sizeManager;
    private GameObject cachedMapContent;
    
    public MinimapMarkerCoordinator(
        QuestMarkerManager questManager,
        MapPreferences preferences,
        MinimapContent content,
        MinimapSizeManager sizeMgr)
    {
        questMarkerManager = questManager;
        mapPreferences = preferences;
        minimapContent = content;
        sizeManager = sizeMgr;
    }
    
    public void SetCachedMapContent(GameObject mapContent)
    {
        cachedMapContent = mapContent;
    }
    
    public void OnQuestStarted(S1Quests.Quest quest)
    {
        questMarkerManager.AddQuestPoIMarkerWorld(quest);
    }
    
    public void OnQuestCompleted(S1Quests.Quest quest)
    {
        questMarkerManager.RemoveQuestPoIMarker(quest);
    }
    
    public void OnContractTrackingChanged(bool previous, bool current)
    {
        if (questMarkerManager == null) return;
        if (current)
        {
            questMarkerManager.AddAllContractPoIMarkers();
        }
        else
        {
            questMarkerManager.RemoveAllContractPoIMarkers();
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

    public void OnContractAccepted(S1Quests.Contract contract)
    {
        questMarkerManager.AddQuestPoIMarkerWorld(contract);
    }

    public void OnContractCompleted(S1Quests.Contract contract)
    {
        questMarkerManager.RemoveQuestPoIMarker(contract);
    }
}
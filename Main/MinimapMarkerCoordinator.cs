#if IL2CPP
using S1Quests = Il2CppScheduleOne.Quests;
#else
using S1Quests = ScheduleOne.Quests;
#endif
using UnityEngine;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.PoIManagers;

namespace Small_Corner_Map.Main;
/// <summary>
/// Coordinates all marker managers (contracts, properties) and handles tracking preferences.
/// </summary>
internal class MinimapMarkerCoordinator
{
    private readonly QuestMarkerManager questMarkerManager;
    private readonly MapPreferences mapPreferences;
    private readonly MinimapContent minimapContent;
    private readonly PropertyPoIManager propertyPoIManager;
    private GameObject cachedMapContent;
    private readonly MarkerRegistry markerRegistry;
    private CompassManager compassManager;
    
    public MinimapMarkerCoordinator(
        QuestMarkerManager questManager,
        MapPreferences preferences,
        MinimapContent content,
        MarkerRegistry registry,
        PropertyPoIManager propertyManager)
    {
        questMarkerManager = questManager;
        mapPreferences = preferences;
        minimapContent = content;
        markerRegistry = registry;
        propertyPoIManager = propertyManager;
    }
    
    public void SetCompassManager(CompassManager manager)
    {
        compassManager = manager;
    }
    
    public void SetCachedMapContent(GameObject mapContent)
    {
        cachedMapContent = mapContent;
    }
    
    public void OnQuestStarted(S1Quests.Quest quest)
    {
        questMarkerManager.AddMarker(quest);
    }
    
    public void OnQuestCompleted(S1Quests.Quest quest)
    {
        questMarkerManager.RemoveMarker(quest);
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
                propertyPoIManager.AddAllMarkers();
            }
        }
        else
        {
            propertyPoIManager.RemoveAllMarkers();
        }
    }
    

    public void OnContractAccepted(S1Quests.Contract contract)
    {
        questMarkerManager.AddMarker(contract);
    }

    public void OnContractCompleted(S1Quests.Contract contract)
    {
        questMarkerManager.RemoveMarker(contract);
    }
}
using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
using Small_Corner_Map.PoIManagers.QuestMarkerStrategies;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers;

public class QuestMarkerManager
{
    private bool isInitialized;
    private readonly MinimapContent minimapContent;
    private readonly MapPreferences mapPreferences;
    private readonly MarkerRegistry markerRegistry;
    
    private const string ContractKey = "Contract";
    private const string DeadDropKey = "DeadDrop";
    private const string RegularKey = "Regular";
    
    public bool IsInitialized => isInitialized;

    public QuestMarkerManager(MinimapContent minimapContent, MapPreferences preferences, MarkerRegistry registry)
    {
        this.minimapContent = minimapContent;
        this.mapPreferences = preferences;
        this.markerRegistry = registry;
    }
    
    public void Initialize()
    {
        MelonLogger.Msg("Waiting for QuestManager instance...");
        if (!QuestManager.InstanceExists) return;
        if (isInitialized) return;
        
        MelonLogger.Msg("QuestManager Instance available...");

        var quests = QuestManager.Instance.QuestContainer.GetComponentsInChildren<Quest>();

        // Make sure quests are completely loaded
        if (quests.First().GUID.ToString() == "00000000-0000-0000-0000-000000000000") return;
        
        AddAllContractPoIMarkers();
        AddAllDeadDropPoIMarkers();
        AddAllQuestPoIMarkers();
        isInitialized = true;
    }

    internal void AddMarker(Quest quest)
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, GetStrategyKey(quest), markerRegistry);
        MelonLogger.Msg("Using strategy: " + GetStrategyKey(quest));
        strategy.AddMarker(quest);
    }
    
    internal void AddAllContractPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, ContractKey, markerRegistry);
        strategy.AddAllMarkers();
    }
    
    internal void AddAllDeadDropPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, DeadDropKey, markerRegistry);
        strategy.AddAllMarkers();
    }
    
    internal void AddAllQuestPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, RegularKey, markerRegistry);
        strategy.AddAllMarkers();
    }

    internal void RemoveMarker(Quest quest)
    {
        var strategyKey = GetStrategyKey(quest);
        MelonLogger.Msg("Removing quest poi Marker: " + quest.name);
        MelonLogger.Msg("Using strategy: " + strategyKey);
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, GetStrategyKey(quest), markerRegistry);
        strategy.RemoveMarker(quest);
    }

    internal void RemoveAllContractPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, ContractKey, markerRegistry);
        strategy.RemoveAllMarkers();
    }

    internal void RemoveAllDeadDropPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, DeadDropKey, markerRegistry);
        strategy.RemoveAllMarkers();
    }

    internal void RemoveAllQuestPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, RegularKey, markerRegistry);
        strategy.RemoveAllMarkers();
    }

    private string GetStrategyKey(Quest quest)
    {
        return quest is Contract || quest.name.StartsWith(Constants.ContractQuestName) ? 
            "Contract" : quest.name == Constants.DeadDropQuestName ? 
                "DeadDrop" : "Regular";
    }
}
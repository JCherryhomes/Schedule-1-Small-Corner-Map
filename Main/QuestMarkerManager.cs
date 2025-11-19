using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main.QuestMarkerStrategies;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main;

public class QuestMarkerManager
{
    private bool isInitialized;
    private readonly MinimapContent minimapContent;
    private readonly MapPreferences mapPreferences;
    
    private const string ContractKey = "Contract";
    private const string DeadDropKey = "DeadDrop";
    private const string RegularKey = "Regular";
    
    public bool IsInitialized => isInitialized;

    public QuestMarkerManager(MinimapContent minimapContent, MapPreferences preferences)
    {
        this.minimapContent = minimapContent;
        this.mapPreferences = preferences;
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

    internal void AddQuestPoIMarkerWorld(Quest quest)
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, GetStrategyKey(quest));
        MelonLogger.Msg("Adding quest poi Marker: " + quest.name);
        MelonLogger.Msg("Using strategy: " + GetStrategyKey(quest));
        MelonLogger.Msg("Quest Is Contract: " + (quest is Contract));
        strategy.AddMarker(quest);
    }
    
    internal void AddAllContractPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, ContractKey);
        strategy.AddAllMarkers();
    }
    
    internal void AddAllDeadDropPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, DeadDropKey);
        strategy.AddAllMarkers();
    }
    
    internal void AddAllQuestPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, RegularKey);
        strategy.AddAllMarkers();
    }

    internal void RemoveQuestPoIMarker(Quest quest)
    {
        var strategyKey = GetStrategyKey(quest);
        MelonLogger.Msg("Removing quest poi Marker: " + quest.name);
        MelonLogger.Msg("Using strategy: " + strategyKey);
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, GetStrategyKey(quest));
        strategy.RemoveMarker(quest);
    }

    internal void RemoveAllContractPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, ContractKey);
        strategy.RemoveAllMarkers();
    }

    internal void RemoveAllDeadDropPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, DeadDropKey);
        strategy.RemoveAllMarkers();
    }

    internal void RemoveAllQuestPoIMarkers()
    {
        var strategy = QuestMarkerStrategyResolver.GetStrategy(minimapContent, mapPreferences, RegularKey);
        strategy.RemoveAllMarkers();
    }

    private string GetStrategyKey(Quest quest)
    {
        return quest is Contract ? 
            "Contract" : quest.name == QuestMarkerStrategyBase.DeadDropQuestName ? 
                "DeadDrop" : "Regular";
    }
}
using Small_Corner_Map.Helpers;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main.QuestMarkerStrategies;

public abstract class QuestMarkerStrategyBase
{
    internal static readonly string DeadDropQuestName = "Collect Dead Drop";
    internal static readonly string ContractQuestName = "Deal for";
    internal string MarkerKeyPrefix { get; set; }
    internal GameObject IconPrefab { get; set; }
    internal MinimapContent MinimapContent { get; }
    internal MapPreferences Preferences { get; }
    internal GameObject MapContentObject => MinimapContent.MapContentObject;

    public QuestMarkerStrategyBase(MinimapContent minimapContent, MapPreferences preferences)
    {
        this.MinimapContent = minimapContent;
        this.Preferences = preferences;
    }
    
    protected string GetMarkerName(Quest quest)
    {
        return MarkerKeyPrefix + "_" + quest.GUID;
    }

    internal abstract void CachePoIIcon(Quest quest);
}
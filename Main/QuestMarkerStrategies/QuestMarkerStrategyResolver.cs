#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main.QuestMarkerStrategies;

namespace Small_Corner_Map.Main.QuestMarkerStrategies
{
    internal static class QuestMarkerStrategyResolver
    {
        public static IQuestMarkerStrategy GetStrategy(MinimapContent minimapContent, MapPreferences preferences, string key, MarkerRegistry registry)
        {
            if (key == "Contract")
                return new ContractMarkerStrategy(minimapContent, preferences, registry);
            if (key == QuestMarkerStrategyBase.DeadDropQuestName)
                return new DeadDropQuestMarkerStrategy(minimapContent, preferences, registry);
            return new RegularQuestMarkerStrategy(minimapContent, preferences, registry);
        }
    }
}

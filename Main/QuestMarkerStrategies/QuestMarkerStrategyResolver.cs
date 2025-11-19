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
        public static IQuestMarkerStrategy GetStrategy(MinimapContent minimapContent, MapPreferences preferences, string key)
        {
            if (key == "Contract")
                return new ContractMarkerStrategy(minimapContent, preferences);
            if (key == QuestMarkerStrategyBase.DeadDropQuestName)
                return new DeadDropQuestMarkerStrategy(minimapContent, preferences);
            return new RegularQuestMarkerStrategy(minimapContent, preferences);
        }
    }
}


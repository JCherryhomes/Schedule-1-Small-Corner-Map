using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers.QuestMarkerStrategies
{
    internal static class QuestMarkerStrategyResolver
    {
        public static IPoIManager<Quest> GetStrategy(MinimapContent minimapContent, MapPreferences preferences, string key, MarkerRegistry registry)
        {
            if (key == "Contract")
                return new ContractMarkerStrategy(minimapContent, preferences, registry);
            if (key == Constants.DeadDropQuestName)
                return new DeadDropQuestMarkerStrategy(minimapContent, preferences, registry);
            return new RegularQuestMarkerStrategy(minimapContent, preferences, registry);
        }
    }
}

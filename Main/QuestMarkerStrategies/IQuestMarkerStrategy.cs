#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main.QuestMarkerStrategies;

internal interface IQuestMarkerStrategy
{
    void AddMarker(Quest quest);
    void AddAllMarkers();
    void RemoveMarker(Quest quest);
    void RemoveAllMarkers();
}

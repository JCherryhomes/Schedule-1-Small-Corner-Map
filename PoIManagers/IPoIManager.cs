#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers;

internal interface IPoIManager<T>
{
    void AddMarker(T marker);
    void AddAllMarkers();
    void RemoveMarker(T marker);
    void RemoveAllMarkers();
}

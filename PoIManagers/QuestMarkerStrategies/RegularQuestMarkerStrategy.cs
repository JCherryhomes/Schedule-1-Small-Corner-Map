using Il2CppScheduleOne.Quests;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;
#if IL2CPP

#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers.QuestMarkerStrategies
{
    internal class RegularQuestMarkerStrategy : PoIManagerBase<Quest>, IPoIManager<Quest>
    {
        public RegularQuestMarkerStrategy(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry) : base(mapContent, preferences, registry)
        {
            MarkerKeyPrefix = "Regular_Quest";
        }

        public void AddMarker(Quest quest)
        {
            if (!quest.IsTracked || quest.State != EQuestState.Active) return;
            
            Registry.AddOrUpdateMarker(GetMarkerData(quest));
        }
        public void AddAllMarkers()
        {
            var quests = QuestManager.Instance.QuestContainer.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name == Constants.DeadDropQuestName || quest is DeaddropQuest) continue;
                AddMarker(quest);
            }
        }
        public void RemoveMarker(Quest quest)
        {
            Registry.RemoveMarker(GetMarkerName(quest));
        }
        public void RemoveAllMarkers()
        {
            var quests = QuestManager.Instance.QuestContainer.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name == Constants.DeadDropQuestName || quest is DeaddropQuest) continue;
                RemoveMarker(quest);
            }
        }

        internal override MarkerRegistry.MarkerData GetMarkerData(Quest quest)
        {
            var activeEntry = quest.GetFirstActiveEntry();
            var worldPos = activeEntry?.PoILocation?.position ?? 
               activeEntry?.transform.position ?? 
               quest.PoIPrefab.transform.position;
            return new MarkerRegistry.MarkerData
            {
                Id = GetMarkerName(quest),
                WorldPos = worldPos,
                IconPrefab = quest.IconPrefab?.gameObject,
                Type = MarkerType.RegularQuest,
                DisplayName = quest.name,
                XOffset = -Constants.MarkerXOffset,
                ZOffset = -Constants.MarkerZOffset,
                IsTracked = quest.IsTracked,
                IsVisibleOnMinimap = true,
                IsVisibleOnCompass = true
            };
        }

        internal override void CachePoIIcon(Quest quest)
        {
            if (quest == null || quest.IconPrefab == null) return;
            IconPrefab = quest.IconPrefab.gameObject;
        }
    }
}

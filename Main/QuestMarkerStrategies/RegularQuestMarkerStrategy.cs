using Small_Corner_Map.Helpers;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main.QuestMarkerStrategies
{
    internal class RegularQuestMarkerStrategy : QuestMarkerStrategyBase, IQuestMarkerStrategy
    {
        private readonly MarkerRegistry markerRegistry;
        public RegularQuestMarkerStrategy(MinimapContent minimapContent, MapPreferences preferences, MarkerRegistry registry) : base(minimapContent, preferences)
        {
            MarkerKeyPrefix = "Regular_Quest";
            markerRegistry = registry;
        }

        public void AddMarker(Quest quest)
        {
            if (!quest.IsTracked || quest.State != EQuestState.Active) return;
            
            var activeEntry = quest.GetFirstActiveEntry();
            var worldPos = activeEntry?.PoILocation?.position ??
                           activeEntry?.transform.position ?? quest.PoIPrefab.transform.position;
            var markerData = new MarkerRegistry.MarkerData
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
            markerRegistry.AddOrUpdateMarker(markerData);
        }
        public void AddAllMarkers()
        {
            var quests = QuestManager.Instance.QuestContainer.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name == DeadDropQuestName || quest is DeaddropQuest) continue;
                AddMarker(quest);
            }
        }
        public void RemoveMarker(Quest quest)
        {
            markerRegistry.RemoveMarker(GetMarkerName(quest));
        }
        public void RemoveAllMarkers()
        {
            var quests = QuestManager.Instance.QuestContainer.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name == DeadDropQuestName || quest is DeaddropQuest) continue;
                RemoveMarker(quest);
            }
        }

        internal override void CachePoIIcon(Quest quest)
        {
            if (quest == null || quest.IconPrefab == null) return;
            IconPrefab = quest.IconPrefab.gameObject;
        }
    }
}

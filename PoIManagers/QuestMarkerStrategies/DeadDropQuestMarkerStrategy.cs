using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers.QuestMarkerStrategies
{
    internal class DeadDropQuestMarkerStrategy : PoIManagerBase<Quest>, IPoIManager<Quest>
    {
        public DeadDropQuestMarkerStrategy(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry) : base(mapContent, preferences, registry)
        {
            MarkerKeyPrefix = "DeadDrop_Marker";
        }
        
        public void AddMarker(Quest quest)
        {
            if (quest == null) return;
            if (!quest.IsTracked || quest.State != EQuestState.Active) return;
            
            Registry.AddOrUpdateMarker(GetMarkerData(quest));
        }

        public void AddAllMarkers()
        {
            var quests = QuestManager.Instance.DeaddropCollectionPrefab.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name != Constants.DeadDropQuestName) continue;
                AddMarker(quest);
            }
        }

        public void RemoveMarker(Quest quest)
        {
            Registry.RemoveMarker(GetMarkerName(quest));
        }

        public void RemoveAllMarkers()
        {
            var quests = QuestManager.Instance.DeaddropCollectionPrefab.GetComponentsInChildren<Quest>();
            foreach (var quest in quests)
            {
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name != Constants.DeadDropQuestName) continue;
                RemoveMarker(quest);
            }
        }

        internal override MarkerRegistry.MarkerData GetMarkerData(Quest quest)
        {
            var activeEntry = quest.GetFirstActiveEntry();
            var worldPos = activeEntry?.PoILocation?.position ??
                           activeEntry?.transform.position ?? quest.PoIPrefab.transform.position;
            return new MarkerRegistry.MarkerData
            {
                Id = GetMarkerName(quest),
                WorldPos = worldPos,
                IconPrefab = quest.IconPrefab?.gameObject,
                Type = MarkerType.DeadDrop,
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
            if (quest == null) return;
            var entry = quest.GetFirstActiveEntry();
            IconPrefab = entry.PoI.IconContainer.gameObject;
        }
    }
}

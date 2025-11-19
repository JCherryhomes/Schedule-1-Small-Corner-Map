
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
        public RegularQuestMarkerStrategy(MinimapContent minimapContent, MapPreferences preferences) : base(minimapContent, preferences)
        {
        }

        public void AddMarker(Quest quest)
        {
            var activeEntry = quest.GetFirstActiveEntry();

            // Use current dynamic scale factor from preferences
            var currentScale = Constants.DefaultMapScale * Preferences.MinimapScaleFactor;
            var worldPos = activeEntry?.PoILocation?.position ??
                           activeEntry?.transform.position ?? quest.PoIPrefab.transform.position;
            var xPosition = worldPos.x * currentScale;
            var zPosition = worldPos.z * currentScale;
            var mappedPos = new Vector2(xPosition, zPosition);

            if (MapContentObject == null) return;
            if (IconPrefab == null)
            {
                CachePoIIcon(quest);
            }

            MinimapPoIHelper.AddMarkerToMap(
                IconPrefab,
                MapContentObject,
                MarkerKeyPrefix + "_" + quest.GUID,
                mappedPos,
                worldPos,
                -Constants.MarkerXOffset);
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
            MinimapPoIHelper.RemoveMarker(GetMarkerName(quest));
        }
        
        public void RemoveAllMarkers()
        {
            MinimapPoIHelper.RemoveAllByKey(MarkerKeyPrefix);
        }

        private new void CachePoIIcon(Quest quest)
        {
            if (quest == null || quest.IconPrefab == null) return;
            IconPrefab = quest.IconPrefab.gameObject;
        }
    }
}


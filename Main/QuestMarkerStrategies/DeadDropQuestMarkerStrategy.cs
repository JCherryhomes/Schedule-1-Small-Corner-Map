
using MelonLoader;
using Small_Corner_Map.Helpers;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main.QuestMarkerStrategies
{
    internal class DeadDropQuestMarkerStrategy : QuestMarkerStrategyBase, IQuestMarkerStrategy
    {
        public DeadDropQuestMarkerStrategy(MinimapContent minimapContent, MapPreferences preferences) : base(minimapContent, preferences)
        {
            MarkerKeyPrefix = "DeadDrop_Marker";
        }
        
        public void AddMarker(Quest quest)
        {
            if (quest == null || MapContentObject == null)
                return;

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

            MelonLogger.Msg("Marker Key: " + (MarkerKeyPrefix + "_" + quest.GUID + " for quest " + quest.name));
            MinimapPoIHelper.AddMarkerToMap(
                IconPrefab,
                MapContentObject,
                MarkerKeyPrefix + "_" + quest.GUID,
                mappedPos,
                worldPos,
                -Constants.MarkerXOffset,
                0f);
        }

        public void AddAllMarkers()
        {
            var quests = QuestManager.Instance.DeaddropCollectionPrefab.GetComponentsInChildren<Quest>();
            var deaddrops = QuestManager.Instance.DeaddropCollectionPrefab.GetComponentsInChildren<DeaddropQuest>();
            
            MelonLogger.Msg("Found " + quests.Length + " quests in DeadDropCollection.");
            MelonLogger.Msg("Found " + deaddrops.Length + " deaddrop quests in DeadDropCollection.");

            foreach (var quest in quests)
            {
                MelonLogger.Msg("Adding " + quest.name);
                if (quest is Contract || !quest.IsTracked || quest.State != EQuestState.Active) continue;
                if (quest.name != DeadDropQuestName) continue;
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
            if (quest == null) return;
            var entry = quest.GetFirstActiveEntry();
            IconPrefab = entry.PoI.IconContainer.gameObject;
        }
    }
}


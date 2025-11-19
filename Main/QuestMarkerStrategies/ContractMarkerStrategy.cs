
using Small_Corner_Map.Helpers;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.Main.QuestMarkerStrategies
{
    internal class ContractMarkerStrategy : QuestMarkerStrategyBase, IQuestMarkerStrategy
    {
        public ContractMarkerStrategy(MinimapContent minimapContent, MapPreferences preferences) : base(minimapContent, preferences)
        {
            MarkerKeyPrefix = "Contract_Marker";
        }

        public void AddMarker(Quest quest)
        {
            if (quest == null || quest is not Contract contract || MapContentObject == null)
                return;

            // Use current dynamic scale factor from preferences
            var currentScale = Constants.DefaultMapScale * Preferences.MinimapScaleFactor;
            
            var worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var xPosition = worldPos.x * currentScale;
            var zPosition = worldPos.z * currentScale;
            var mappedPos = new Vector2(xPosition, zPosition);

            if (MapContentObject == null) return;
            if (IconPrefab == null)
            {
                CachePoIIcon(contract);
            }

            MinimapPoIHelper.AddMarkerToMap(
                IconPrefab,
                MapContentObject,
                MarkerKeyPrefix + "_" + contract.GUID,
                mappedPos,
                worldPos,
                -Constants.MarkerXOffset);
        }

        public void AddAllMarkers()
        {
            var contractContainer = QuestManager.Instance.ContractContainer;
            var contracts = contractContainer.GetComponentsInChildren<Contract>();
            foreach (var contract in contracts)
            {
                AddMarker(contract);
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

        private new void CachePoIIcon(Quest contract)
        {
            if (contract == null || contract.IconPrefab == null) return;
            this.IconPrefab = contract.IconPrefab.gameObject;
        }
    }
}


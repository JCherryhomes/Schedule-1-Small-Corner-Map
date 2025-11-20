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
        private readonly MarkerRegistry markerRegistry;
        public ContractMarkerStrategy(MinimapContent minimapContent, MapPreferences preferences, MarkerRegistry registry) : base(minimapContent, preferences)
        {
            MarkerKeyPrefix = "Contract_Marker";
            markerRegistry = registry;
        }

        public void AddMarker(Quest quest)
        {
            if (quest == null || quest is not Contract contract) return;
            if (!quest.IsTracked || quest.State != EQuestState.Active) return;
            var worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var markerData = new MarkerRegistry.MarkerData
            {
                Id = GetMarkerName(contract),
                WorldPos = worldPos,
                IconPrefab = contract.IconPrefab?.gameObject,
                Type = MarkerType.Contract,
                DisplayName = contract.name,
                XOffset = -Constants.MarkerXOffset,
                ZOffset = 0f,
                IsTracked = contract.IsTracked,
                IsVisibleOnMinimap = true,
                IsVisibleOnCompass = true
            };
            markerRegistry.AddOrUpdateMarker(markerData);
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
            markerRegistry.RemoveMarker(GetMarkerName(quest));
        }

        public void RemoveAllMarkers()
        {
            var contractContainer = QuestManager.Instance.ContractContainer;
            var contracts = contractContainer.GetComponentsInChildren<Contract>();
            foreach (var contract in contracts)
            {
                RemoveMarker(contract);
            }
        }

        internal override void CachePoIIcon(Quest contract)
        {
            if (contract == null || contract.IconPrefab == null) return;
            this.IconPrefab = contract.IconPrefab.gameObject;
        }
    }
}

using Small_Corner_Map.Helpers;
using UnityEngine;
using MelonLoader;


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
            var contract = quest as Contract;
            if (contract is null)
            {
                MelonLogger.Msg("Could not cast quest object to Contract");
                return;
            }

            MelonLogger.Msg("[ContractMarkerStrategy] Attempting to add marker for contract: " + (contract?.name ?? "null"));
            MelonLogger.Msg("[ContractMarkerStrategy] Contract IsTracked: " + (contract != null ? contract.IsTracked.ToString() : "N/A"));
            MelonLogger.Msg("[ContractMarkerStrategy] Contract Location: " + contract?.DeliveryLocation?.CustomerStandPoint?.position ?? "N/A");
            if (contract == null || !contract.name.StartsWith(QuestMarkerStrategyBase.ContractQuestName)) return;
            if (!contract.IsTracked || contract.State != EQuestState.Active) return;
            var worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            MelonLogger.Msg($"[ContractMarkerStrategy] Adding marker for contract {contract.name} at world position {worldPos}");
            // log marker generated id/name
            MelonLogger.Msg($"[ContractMarkerStrategy] Generated marker ID: {GetMarkerName(contract)}");
            var markerData = new MarkerRegistry.MarkerData
            {
                Id = GetMarkerName(contract),
                WorldPos = worldPos,
                IconPrefab = contract.IconPrefab?.gameObject,
                Type = MarkerType.Contract,
                DisplayName = contract.name,
                XOffset = -Constants.MarkerXOffset,
                ZOffset = -Constants.MarkerZOffset,
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

        public void RemoveMarker(Quest quest)
        {
            markerRegistry.RemoveMarker(GetMarkerName(quest));
        }
    }
}

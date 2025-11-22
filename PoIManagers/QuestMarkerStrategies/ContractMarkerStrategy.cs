using MelonLoader;
using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

#if IL2CPP
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Quests;
#endif

namespace Small_Corner_Map.PoIManagers.QuestMarkerStrategies
{
    internal class ContractMarkerStrategy : PoIManagerBase<Quest>, IPoIManager<Quest>
    {
        public ContractMarkerStrategy(MinimapContent mapContent, MapPreferences preferences, MarkerRegistry registry) : base(mapContent, preferences, registry)
        {
            MarkerKeyPrefix = "Contract_Marker";
        }

        public void AddMarker(Quest quest)
        {
            var contract = quest as Contract;
            if (contract is null)
            {
                MelonLogger.Msg("Could not cast quest object to Contract");
                return;
            }
            
            if (contract == null || !contract.name.StartsWith(Constants.ContractQuestName)) return;
            if (!contract.IsTracked || contract.State != EQuestState.Active) return;
            if (contract.DeliveryLocation?.CustomerStandPoint == null) return;
            
            var markerData = GetMarkerData(contract);
            Registry.AddOrUpdateMarker(markerData);
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

        internal override MarkerRegistry.MarkerData GetMarkerData(Quest contract)
        {
            var worldPos = ((Contract)contract).DeliveryLocation.CustomerStandPoint.position;
            return new MarkerRegistry.MarkerData
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
        }

        internal override void CachePoIIcon(Quest contract)
        {
            if (contract == null || contract.IconPrefab == null) return;
            IconPrefab = contract.IconPrefab.gameObject;
        }

        public void RemoveMarker(Quest quest)
        {
            Registry.RemoveMarker(GetMarkerName(quest));
        }
    }
}

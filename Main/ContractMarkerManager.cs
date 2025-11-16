#if IL2CPP
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Quests;
#else
using ScheduleOne.Economy;
using ScheduleOne.Quests;
#endif

using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.Main
{
    public class ContractMarkerManager
    {
        private GameObject contractPoIIconPrefab;
        private readonly float markerXAdjustment;
        private readonly GameObject mapContentObject;
        private readonly MapPreferences mapPreferences;

        private const string ContractPoIMarkerKey = "ContractPoI_Marker";

        public ContractMarkerManager(MinimapContent minimapContent, float markerXAdjustment, MapPreferences preferences)
        {
            this.mapContentObject = minimapContent.MapContentObject;
            this.markerXAdjustment = markerXAdjustment;
            this.mapPreferences = preferences;
        }

        internal void AddContractPoIMarkerWorld(Contract contract)
        {
            if (contract == null || mapContentObject == null)
                return;

            // Use current dynamic scale factor from preferences
            var currentScale = Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor;
            
            var worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var xPosition = worldPos.x * currentScale;
            var zPosition = worldPos.z * currentScale;
            var mappedPos = new Vector2(xPosition, zPosition);

            if (mapContentObject == null) return;
            if (contractPoIIconPrefab == null)
            {
                CacheContractPoIIcon(contract);
            }

            MinimapPoIHelper.AddMarkersToMap(
                contractPoIIconPrefab,
                mapContentObject,
                ContractPoIMarkerKey + "_" + contract.GUID,
                mappedPos,
                worldPos,
                -markerXAdjustment,
                0f);
        }

        internal void RemoveContractPoIMarkers(Contract contract)
        {
            var name = ContractPoIMarkerKey + "_" + contract.GUID;
            MinimapPoIHelper.RemoveMarker(name);
        }

        internal void RemoveAllContractPoIMarkers()
        {
            MinimapPoIHelper.RemoveAllByKey(ContractPoIMarkerKey);
        }
        
        internal void AddAllContractPoIMarkers()
        {
            var contractContainer = QuestManager.Instance.ContractContainer;
            MelonLogger.Msg("Adding ContractPoIMarkers");
            MelonLogger.Msg("Contract Container Child Count: " + contractContainer.childCount);
            for (var i = 0; i < contractContainer.childCount; i++)
            {
                var contractTransform = contractContainer.GetChild(i);
                var contract = contractTransform.GetComponent<Contract>();
                if (contract != null && contract.State == EQuestState.Active && contract.IsTracked)
                {
                    AddContractPoIMarkerWorld(contract);
                }
            }
        }

        private void CacheContractPoIIcon(Contract contract)
        {
            contractPoIIconPrefab = contract.IconPrefab.gameObject;
        }
    }
}

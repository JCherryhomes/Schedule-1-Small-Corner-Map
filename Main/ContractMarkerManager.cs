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
        private float mapScale;
        private readonly float markerXAdjustment;
        private GameObject mapContentObject;
        private MinimapContent mapContent;
        private readonly MapPreferences mapPreferences;

        private const string ContractPoIMarkerKey = "ContractPoI_Marker";

        public ContractMarkerManager(MinimapContent minimapContent, float mapScale, float markerXAdjustment, float markerZAdjustment, MapPreferences preferences)
        {
            this.mapContent = minimapContent;
            this.mapContentObject = minimapContent.MapContentObject;
            this.mapScale = mapScale;
            this.markerXAdjustment = markerXAdjustment;
            this.mapPreferences = preferences;
        }

        public void SetMapContentObject(GameObject mapContent)
        {
            mapContentObject = mapContent;
        }
        
        public void UpdateMapScale(float newMapScale)
        {
            mapScale = newMapScale;
        }

        internal void AddContractPoIMarkerWorld(Contract contract)
        {
            if (contract == null || mapContentObject == null)
                return;

            // Use current dynamic scale factor from preferences
            var currentScale = Constants.DefaultMapScale * mapPreferences.MinimapScaleFactor;
            
            Vector3 worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var xPosition = worldPos.x * currentScale;
            var zPosition = worldPos.z * currentScale;
            Vector2 mappedPos = new Vector2(xPosition, zPosition);

            if (mapContentObject != null)
            {
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

        private void CacheContractPoIIcon(Contract contract)
        {
            contractPoIIconPrefab = contract.IconPrefab.gameObject;
        }
    }
}

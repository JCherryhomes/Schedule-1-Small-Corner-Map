using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Quests;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Small_Corner_Map
{
    public class ContractMarkerManager
    {
        private readonly List<GameObject> contractPoIMarkers = new();
        private GameObject contractPoIIconPrefab;
        private readonly float mapScale;
        private readonly float markerXAdjustment;
        private GameObject mapContentObject;
        private MinimapContent mapContent;

        public List<GameObject> contractMarkers { get { return contractPoIMarkers; } }

        public ContractMarkerManager(MinimapContent minimapContent, float mapScale, float markerXAdjustment, float markerZAdjustment)
        {
            this.mapContent = minimapContent;
            this.mapContentObject = minimapContent.MapContentObject;
            this.mapScale = mapScale;
            this.markerXAdjustment = markerXAdjustment;
        }

        public void SetMapContentObject(GameObject mapContent)
        {
            mapContentObject = mapContent;
        }

        internal void AddContractPoIMarkerWorld(Contract contract)
        {
            if (contract == null || mapContentObject == null)
                return;

            Vector3 worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var xPosition = worldPos.x * mapScale;
            var zPosition = worldPos.z * mapScale;
            Vector2 mappedPos = new Vector2(xPosition, zPosition);
            mappedPos.x -= markerXAdjustment;

            if (mapContentObject != null)
            {
                if (contractPoIIconPrefab == null)
                {
                    CacheContractPoIIcon(contract);
                }

                GameObject markerObject = UnityEngine.Object.Instantiate(contractPoIIconPrefab);
                markerObject.transform.SetParent(mapContentObject.transform, false);
                markerObject.name = $"ContractPoI_Marker_{contract.GUID}";
                RectTransform markerRect = markerObject.GetComponent<RectTransform>();

                if (markerRect != null)
                {
                    markerRect.sizeDelta = new Vector2(15f, 15f);
                    markerRect.anchoredPosition = mappedPos;
                    markerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                    contractPoIMarkers.Add(markerObject);
                }
            }
        }

        internal void RemoveContractPoIMarkers(Contract contract)
        {
            var name = $"ContractPoI_Marker_{contract.GUID}";
            var marker = contractPoIMarkers.FirstOrDefault(m => m.name == name);
            if (marker != null)
            {
                UnityEngine.Object.Destroy(marker);
                contractPoIMarkers.Remove(marker);
            }
        }

        internal void RemoveAllContractPoIMarkers()
        {
            foreach (GameObject marker in contractPoIMarkers)
            {
                UnityEngine.Object.Destroy(marker);
            }
            contractPoIMarkers.Clear();
        }

        private void CacheContractPoIIcon(Contract contract)
        {
            contractPoIIconPrefab = contract.IconPrefab.gameObject;
        }

        public void loadInitialMarkers(QuestManager questManager)
        {
            var contractContainer = questManager.ContractContainer;
            var contractCount = contractContainer?.childCount ?? 0;

            MelonLogger.Msg($"Found {contractCount} contracts in the container.");

            if (contractCount > 0)
            {
                for (int i = 0; i < contractCount; i++)
                {
                    var child = contractContainer.GetChild(i).GetComponent<Contract>();

                    if (child.State == EQuestState.Active && child.IsTracked)
                    {
                        AddContractPoIMarkerWorld(child);
                    }
                }
            }
        }
    }
}

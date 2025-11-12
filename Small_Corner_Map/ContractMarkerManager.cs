using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.Quests;
using Il2CppSystem.Xml.Schema;
using UnityEngine.UI;

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

        public void CacheContractPoIIcon(Contract contract)
        {
            contractPoIIconPrefab = contract.IconPrefab.gameObject;
        }

        public void AddContractPoIMarkerWorld(Contract contract)
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
                markerObject.name = "ContractPoI_Marker";
                RectTransform markerRect = markerObject.GetComponent<RectTransform>();

                if (markerRect != null)
                {
                    markerRect.sizeDelta = new Vector2(15f, 15f);
                    markerRect.anchoredPosition = mappedPos;
                    markerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                    contractPoIMarkers.Add(markerObject);
                }

                MelonLogger.Msg("Green contract marker added at mapped position: " + markerRect.anchoredPosition);
            }
        }

        public void RemoveAllContractPoIMarkers()
        {
            foreach (GameObject marker in contractPoIMarkers)
            {
                UnityEngine.Object.Destroy(marker);
            }
            contractPoIMarkers.Clear();
        }

        public void UpdateContractMarkers(List<Contract> activeCPs)
        {
            MelonLogger.Msg($"[ContractMarkerManager] Updating contract markers... (total count: {activeCPs.Count}");
            // Add new markers
            foreach (var contract in activeCPs)
            {
                var exists = contractPoIMarkers.Any(m => m.name == contract.name);

                if (!exists)
                {
                    AddContractPoIMarkerWorld(contract);
                }
            }

            // Remove inactive markers
            foreach (var contract in contractPoIMarkers)
            {
                var stillActive = activeCPs.Any(c => c.name == contract.name);
                if (!stillActive)
                {
                    UnityEngine.Object.Destroy(contract);
                    contractPoIMarkers.Remove(contract);
                }
            }

            if (activeCPs.Count == 0)
            {
                RemoveAllContractPoIMarkers();
            }
        }
    }
}

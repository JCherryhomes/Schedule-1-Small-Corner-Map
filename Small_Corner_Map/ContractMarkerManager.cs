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
            Transform contractPoI = contract.PoIPrefab.transform;
            var temp = contractPoI.Find("IconContainer");
            MelonLogger.Msg("MinimapUI: Found IconContainer: " + temp?.name ?? "Not Found");
            if (contractPoI == null)
            {
                MelonLogger.Warning("MinimapUI: Could not find ContractPoI transform.");
                return;
            }

            contractPoIIconPrefab = contractPoI.gameObject;
            MelonLogger.Msg("MinimapUI: Cached contract marker with icon.");
        }

        public void AddContractPoIMarkerWorld(Contract contract)
        {
            if (contract == null || mapContentObject == null)
                return;

            Vector3 worldPos = contract.DeliveryLocation.CustomerStandPoint.position;
            var xPosition = worldPos.x * mapScale - 0.5f;
            var zPosition = worldPos.z * mapScale - 0.5f;
            Vector2 mappedPos = new Vector2(xPosition, zPosition);
            mappedPos.x -= markerXAdjustment;

            //if (contractPoIIconPrefab == null)
            //{
            //    CacheContractPoIIcon(contract);
            //    MelonLogger.Msg("ContractMarkerManager: Prefab value: " + contractPoIIconPrefab != null);
            //    if (contractPoIIconPrefab == null)
            //        return;
            //}

            if (mapContentObject != null)
            {
                mapContent.AddRedStaticMarker(worldPos);
                //MelonLogger.Msg("ContractMarkerManager: Adding ContractPoI marker at world position: " + worldPos);
                //// Create a backup marker if prefab is not available
                //GameObject marker = new GameObject("ContractPoIMarker_Backup");
                //var rectTransform = marker.AddComponent<RectTransform>();
                //var image = marker.AddComponent<UnityEngine.UI.Image>();
                //image.color = Color.green;
                //marker.transform.SetParent(mapContentObject.transform, false);
                //marker.transform.SetAsLastSibling();
                //rectTransform.anchoredPosition = mappedPos;
                //rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //rectTransform.sizeDelta = new Vector2(20f, 20f);
                //contractPoIMarkers.Add(marker);

                GameObject markerObject = new GameObject("ContractPoIMarker_Backup");
                markerObject.transform.SetParent(mapContentObject.transform, false);
                RectTransform markerRect = markerObject.AddComponent<RectTransform>();
                markerRect.sizeDelta = new Vector2(5f, 5f);
                float mappedX = worldPos.x * mapScale - 12f;
                float mappedZ = worldPos.z * mapScale + 3f;
                markerRect.anchoredPosition = new Vector2(mappedX, mappedZ);
                Image markerImage = markerObject.AddComponent<Image>();
                markerImage.color = Color.green;
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
            float threshold = 0.1f;

            // Add new markers
            foreach (var cp in activeCPs)
            {
                Vector3 wp = cp.DeliveryLocation.CustomerStandPoint.position;
                Vector2 desiredPos = new Vector2(wp.x * mapScale, wp.z * mapScale);
                desiredPos.x -= markerXAdjustment;

                bool markerFound = false;

                for (int i = 0; i < contractPoIMarkers.Count; i++)
                {
                    GameObject marker = contractPoIMarkers[i];
                    if (marker != null)
                    {
                        RectTransform rt = marker.GetComponent<RectTransform>();
                        if (rt != null && Vector2.Distance(rt.anchoredPosition, desiredPos) < threshold)
                        {
                            markerFound = true;
                            break;
                        }
                    }
                }

                if (!markerFound)
                {
                    AddContractPoIMarkerWorld(cp);
                }
            }

            // Remove markers that no longer exist
            for (int i = contractPoIMarkers.Count - 1; i >= 0; i--)
            {
                GameObject marker = contractPoIMarkers[i];

                if (marker == null)
                {
                    contractPoIMarkers.RemoveAt(i);
                    continue;
                }

                RectTransform rt = marker.GetComponent<RectTransform>();
                bool stillExists = false;

                if (rt != null)
                {
                    Vector2 markerPos = rt.anchoredPosition;

                    foreach (var cp in activeCPs)
                    {
                        var position = cp.DeliveryLocation.CustomerStandPoint.position;
                        Vector2 desiredPos = new Vector2(position.x * mapScale, position.z * mapScale);
                        desiredPos.x -= markerXAdjustment;

                        if (Vector2.Distance(markerPos, desiredPos) < threshold)
                        {
                            stillExists = true;
                            break;
                        }
                    }
                }

                if (!stillExists)
                {
                    UnityEngine.Object.Destroy(marker);
                    contractPoIMarkers.RemoveAt(i);
                }
            }
        }
    }
}
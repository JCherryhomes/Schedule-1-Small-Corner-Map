using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;

namespace Small_Corner_Map
{
    public class ContractMarkerManager
    {
        private readonly List<GameObject> contractPoIMarkers = new();
        private GameObject contractPoIIconPrefab;
        private readonly float mapScale;
        private readonly float markerXAdjustment;
        private GameObject mapContentObject;

        public ContractMarkerManager(GameObject mapContentObject, float mapScale, float markerXAdjustment)
        {
            this.mapContentObject = mapContentObject;
            this.mapScale = mapScale;
            this.markerXAdjustment = markerXAdjustment;
        }

        public void SetMapContentObject(GameObject mapContent)
        {
            mapContentObject = mapContent;
        }

        public void CacheContractPoIIcon()
        {
            string path = "GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content/ContractPoI(Clone)/IconContainer";
            GameObject iconContainer = GameObject.Find(path);

            if (iconContainer != null)
            {
                contractPoIIconPrefab = iconContainer;
            }
            else
            {
                MelonLogger.Warning("ContractMarkerManager: Could not find ContractPoI IconContainer at path: " + path);
            }
        }

        public void AddContractPoIMarkerWorld(Transform cpTransform)
        {
            if (cpTransform == null || mapContentObject == null)
                return;

            Vector3 worldPos = cpTransform.position;
            Vector2 mappedPos = new Vector2(worldPos.x * mapScale, worldPos.z * mapScale);
            mappedPos.x -= markerXAdjustment;

            if (contractPoIIconPrefab == null)
            {
                CacheContractPoIIcon();
                if (contractPoIIconPrefab == null)
                    return;
            }

            GameObject marker = UnityEngine.Object.Instantiate(contractPoIIconPrefab);
            marker.name = "ContractPoIMarker_" + cpTransform.GetInstanceID();
            marker.transform.SetParent(mapContentObject.transform, false);
            contractPoIMarkers.Add(marker);

            RectTransform markerRect = marker.GetComponent<RectTransform>();
            if (markerRect != null)
            {
                markerRect.anchoredPosition = mappedPos;
            }

            MelonLogger.Msg("Added Contract PoI Marker for: " + cpTransform.name + " at " + mappedPos);
        }

        public void RemoveAllContractPoIMarkers()
        {
            foreach (GameObject marker in contractPoIMarkers)
            {
                UnityEngine.Object.Destroy(marker);
            }
            contractPoIMarkers.Clear();
        }

        public void UpdateContractMarkers(List<Transform> activeCPs)
        {
            float threshold = 0.1f;

            // Add new markers
            foreach (Transform cp in activeCPs)
            {
                Vector3 wp = cp.position;
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

                    foreach (Transform cp in activeCPs)
                    {
                        Vector2 desiredPos = new Vector2(cp.position.x * mapScale, cp.position.z * mapScale);
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
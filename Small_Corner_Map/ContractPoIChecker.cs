using System.Collections;
using UnityEngine;
using Small_Corner_Map.Helpers;
using MelonLoader;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.Economy;
using System.ComponentModel;

namespace Small_Corner_Map
{
    internal class ContractPoIChecker : IEnumerator<object>, IDisposable
    {
        private ContractMarkerManager contractManager;
        private int _state;
        private object _current;

        public ContractPoIChecker(int state, ContractMarkerManager contractManager)
        {
            _state = state;
            this.contractManager = contractManager;
        }

        object IEnumerator<object>.Current => _current;
        object IEnumerator.Current => _current;

        public void Dispose()
        {
            // Clean up references
            _state = -2;
            _current = null;
        }

        public bool MoveNext()
        {
            MelonLogger.Msg("[ContractPoIChecker] MoveNext called. State: " + _state);
            switch (_state)
            {
                default:
                    return false;
                case 0:
                    _state = -1;
                    break;
                case 1:
                    _state = -1;
                    var contractContainer = QuestManager.Instance?.ContractContainer;
                    var contractCount = contractContainer?.childCount ?? 0;
                    List<Contract> activeCPs = new List<Contract>();

                    if (contractCount > 0)
                    {
                        for (int i = 0; i < contractCount; i++)
                        {
                            var child = contractContainer.GetChild(i).GetComponent<Contract>();

                            if (child.State == EQuestState.Active && child.IsTracked)
                            {
                                activeCPs.Add(child);
                            }
                        }

                        if (activeCPs.Count > 0)
                        {
                            MelonLogger.Msg($"[ContractPoIChecker] Found {activeCPs.Count} ContractPoI objects.");
                        }

                        float threshold = 0.1f;

                        try
                        {
                            // Add new markers
                            foreach (var cp in activeCPs)
                            {
                                Vector3 wp = cp.DeliveryLocation.CustomerStandPoint.position;
                                Vector2 desiredPos = new Vector2(wp.x * Constants.DefaultMapScale, wp.z * Constants.DefaultMapScale);
                                desiredPos.x -= 5f;

                                bool markerFound = false;
                                for (int i = 0; i < contractManager.contractMarkers.Count; i++)
                                {
                                    GameObject marker = contractManager.contractMarkers[i];
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
                                    contractManager.AddContractPoIMarkerWorld(cp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error($"[ContractPoIChecker] Exception while adding markers: {ex}");
                        }
                        finally
                        {
                            Dispose();
                        }

                        // Remove markers that no longer exist
                        for (int i = contractManager.contractMarkers.Count - 1; i >= 0; i--)
                        {
                            GameObject marker = contractManager.contractMarkers[i];
                            if (marker == null)
                            {
                                contractManager.contractMarkers.RemoveAt(i);
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
                                    Vector2 desiredPos = new Vector2(position.x * Constants.DefaultMapScale, position.z * Constants.DefaultMapScale);
                                    desiredPos.x -= 5f;
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
                                contractManager.contractMarkers.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        contractManager.RemoveAllContractPoIMarkers();
                    }
                    break;
            }

            _current = new WaitForSeconds(20f);
            _state = 1;
            return true;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}

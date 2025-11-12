using System.Collections;
using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.Quests;

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

                        try
                        {
                            contractManager.UpdateContractMarkers(new List<Contract>(activeCPs));
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error($"[ContractPoIChecker] Exception while adding markers: {ex}");
                        }
                        finally
                        {
                            Dispose();
                        }
                    }
                    else
                    {
                        contractManager.RemoveAllContractPoIMarkers();
                    }
                    break;
            }

            _current = new WaitForSeconds(5f);
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

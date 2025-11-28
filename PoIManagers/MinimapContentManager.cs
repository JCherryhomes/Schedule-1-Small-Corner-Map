using MelonLoader;
using System.Collections;
using UnityEngine;

using Small_Corner_Map.Helpers;

namespace Small_Corner_Map.PoIManagers
{
    [RegisterTypeInIl2Cpp]
    public class MinimapContentManager : MonoBehaviour
    {
        private RectTransform _mapContent;
        private Transform _playerTransform;
        private MinimapCoordinateSystem _coordinateSystem;

        public IEnumerator Initialize(RectTransform mapContent, Transform playerTransform)
        {
            if (mapContent == null || playerTransform == null)
            {
                yield return new WaitForSeconds(1.0f);
            }
            
            _mapContent = mapContent;
            _playerTransform = playerTransform;
            _coordinateSystem = new MinimapCoordinateSystem();
        }
        
        void Start()
        {
        }

        void Update()
        {
            if (_mapContent == null || _playerTransform == null)
            {
                return;
            }

            var playerPosition = _playerTransform.position;
            var newPosition = _coordinateSystem.GetMapContentPosition(playerPosition);
            _mapContent.anchoredPosition = newPosition;
        }
    }
}

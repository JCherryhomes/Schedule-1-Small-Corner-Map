using Small_Corner_Map.Helpers;
using UnityEngine;

namespace Small_Corner_Map.PoIManagers
{
    public class MinimapContentManager : MonoBehaviour
    {
        private RectTransform _mapContent;
        private Transform _playerTransform;
        private MinimapCoordinateSystem _coordinateSystem;

        public void Initialize(RectTransform mapContent, Transform playerTransform)
        {
            _mapContent = mapContent;
            _playerTransform = playerTransform;
            _coordinateSystem = new MinimapCoordinateSystem();
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

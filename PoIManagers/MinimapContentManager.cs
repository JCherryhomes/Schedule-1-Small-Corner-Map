using MelonLoader;
using System.Collections;
using UnityEngine;

using Small_Corner_Map.Helpers;
using Small_Corner_Map.Main;

namespace Small_Corner_Map.PoIManagers
{
    [RegisterTypeInIl2Cpp]
    public class MinimapContentManager : MonoBehaviour
    {
        private RectTransform _mapContent;
        private Transform _playerTransform;
        private MinimapCoordinateSystem _coordinateSystem;
        private PlayerMarkerManager _playerMarkerManager; // New field

        public IEnumerator Initialize(RectTransform mapContent, Transform playerTransform, MinimapCoordinateSystem coordinateSystem, PlayerMarkerManager playerMarkerManager) // Updated signature
        {
            if (mapContent == null || playerTransform == null || coordinateSystem == null || playerMarkerManager == null)
            {
                yield return new WaitForSeconds(1.0f);
            }
            
            _mapContent = mapContent;
            _playerTransform = playerTransform;
            _coordinateSystem = coordinateSystem;
            _playerMarkerManager = playerMarkerManager; // Store reference
        }
        
        void Start()
        {
        }

        void Update()
        {
            if (_mapContent == null || _playerTransform == null || _playerMarkerManager == null)
            {
                return;
            }

            var playerPosition = _playerTransform.position;
            var newPosition = _coordinateSystem.GetMapContentPosition(playerPosition);
            _mapContent.anchoredPosition = newPosition;
            
            // Rotate the map content inverse to the player's Y-rotation
            // This makes the map appear to rotate beneath the player, keeping north up relative to the player's view
            _mapContent.rotation = Quaternion.Euler(0, 0, -_playerTransform.eulerAngles.y);
            
            _playerMarkerManager.UpdateDirectionIndicator(_playerTransform); // Call UpdateDirectionIndicator
        }
    }
}

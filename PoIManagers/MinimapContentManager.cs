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
            _mapContent = mapContent;
            _playerTransform = playerTransform;
            _coordinateSystem = coordinateSystem;
            _playerMarkerManager = playerMarkerManager; // Store reference
            yield break; // Ensure it's still an IEnumerator
        }
        
        void Start()
        {
        }

        void Update()
        {
            if (_mapContent == null || _playerTransform == null || _playerMarkerManager == null)
            {
                // Add logging here to see if Update is called when these are null
                MelonLogger.Msg($"MinimapContentManager Update: Skipped due to nulls. _mapContent: {_mapContent}, _playerTransform: {_playerTransform}, _playerMarkerManager: {_playerMarkerManager}");
                return;
            }

            var playerPosition = _playerTransform.position;
            var newPosition = _coordinateSystem.GetMapContentPosition(playerPosition);

            _mapContent.anchoredPosition = newPosition;
            _playerMarkerManager.UpdateDirectionIndicator(_playerTransform); // Call UpdateDirectionIndicator
        }
    }
}

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
                // Add logging here to see if Update is called when these are null
                MelonLogger.Msg($"MinimapContentManager Update: Skipped due to nulls. _mapContent: {_mapContent}, _playerTransform: {_playerTransform}, _playerMarkerManager: {_playerMarkerManager}");
                return;
            }

            var playerPosition = _playerTransform.position;
            var newPosition = _coordinateSystem.GetMapContentPosition(playerPosition);

            // Log current values before applying
            MelonLogger.Msg($"MinimapContentManager Update: PlayerPos: {playerPosition}, Calculated NewPos: {newPosition}");
            MelonLogger.Msg($"MinimapContentManager Update: Applying NewPos to _mapContent.anchoredPosition (Old: {_mapContent.anchoredPosition})");

            _mapContent.anchoredPosition = newPosition;
            
            // Log after applying
            MelonLogger.Msg($"MinimapContentManager Update: _mapContent.anchoredPosition after update: {_mapContent.anchoredPosition}");
            
            _playerMarkerManager.UpdateDirectionIndicator(_playerTransform); // Call UpdateDirectionIndicator
        }
    }
}

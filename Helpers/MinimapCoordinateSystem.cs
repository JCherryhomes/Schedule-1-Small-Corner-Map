using UnityEngine;

namespace Small_Corner_Map.Helpers
{
    /// <summary>
    /// Centralizes all coordinate transformations for the minimap system.
    /// 
    /// COORDINATE SPACES:
    /// - World Space: Game world 3D coordinates (x, y, z) where the player moves
    /// - Map Space: 2D coordinates (x, z) scaled to match the map image dimensions
    /// - UI Space: Unity UI RectTransform anchored positions for positioning elements in the minimap
    /// 
    /// ARCHITECTURE:
    /// The minimap works by moving the MapContentObject (which contains the map image) in the opposite
    /// direction of player movement, creating the illusion that the player marker stays centered while
    /// the map moves beneath it.
    /// </summary>
    public static class MinimapCoordinateSystem
    {
        /// <summary>
        /// The effective world-to-UI scale, incorporating the base world scale factor and current zoom level.
        /// </summary>
        public static float WorldToUIScale(float worldScaleFactor, float currentZoomLevel) => worldScaleFactor * currentZoomLevel;
        
        /// <summary>
        /// Converts a world position to map space coordinates.
        /// Map space is the 2D coordinate system used for positioning markers on the map image.
        /// </summary>
        /// <param name="worldPos">World space position (3D)</param>
        /// <param name="worldScaleFactor">The base world-to-UI scale factor.</param>
        /// <param name="currentZoomLevel">Current zoom level applied to the minimap.</param>
        /// <returns>2D map coordinates (x, z)</returns>
        public static Vector2 WorldToMapSpace(Vector3 worldPos, float worldScaleFactor, float currentZoomLevel)
        {
            var scale = WorldToUIScale(worldScaleFactor, currentZoomLevel);
            return new Vector2(worldPos.x * scale, worldPos.z * scale);
        }

        /// <summary>
        /// Calculates the UI position for the MapContentObject to keep the player centered.
        /// 
        /// EXPLANATION:
        /// - We invert the coordinates (negative) because we're moving the MAP, not the player marker
        /// - The player marker stays at a fixed position in the minimap UI
        /// - By moving the map in the opposite direction, we create the centered effect
        /// - We add a visual centering offset to account for UI element positioning
        /// </summary>
        /// <param name="playerWorldPos">Player's world position</param>
        /// <param name="worldScaleFactor">The base world-to-UI scale factor.</param>
        /// <param name="currentZoomLevel">Current zoom level applied to the minimap.</param>
        /// <param name="minimapPlayerCenterXOffset">X offset for player centering.</param>
        /// <param name="minimapPlayerCenterYOffset">Y offset for player centering.</param>
        /// <returns>UI space position to apply to MapContentObject.anchoredPosition</returns>
        public static Vector2 GetMapContentPosition(Vector3 playerWorldPos, float worldScaleFactor, float currentZoomLevel, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
        {
            var scale = WorldToUIScale(worldScaleFactor, currentZoomLevel);
            
            // Invert coordinates to move map opposite of player
            // Apply configurable centering offsets
            var mappedX = -playerWorldPos.x * scale + minimapPlayerCenterXOffset;
            var mappedZ = -playerWorldPos.z * scale + minimapPlayerCenterYOffset;
            
            return new Vector2(mappedX, mappedZ);
        }

        /// <summary>
        /// Converts a world position to a position relative to the MapContentObject.
        /// Used for positioning POI markers on the map.
        /// </summary>
        /// <param name="poiWorldPos">POI's world space position</param>
        /// <param name="playerWorldPos">Player's world space position</param>
        /// <param name="worldScaleFactor">The base world-to-UI scale factor.</param>
        /// <param name="currentZoomLevel">Current zoom level applied to the minimap.</param>
        /// <returns>Position to use for marker's anchoredPosition relative to MapContent</returns>
        public static Vector2 WorldToMarkerPosition(Vector3 poiWorldPos, Vector3 playerWorldPos, float worldScaleFactor, float currentZoomLevel)
        {
            var scale = WorldToUIScale(worldScaleFactor, currentZoomLevel);
            
            // Calculate the POI's position relative to the player, then scale
            // The minimapPlayerX/YOffsets are handled by the parent mapImageRT's position
            var relativeX = (poiWorldPos.x - playerWorldPos.x) * scale;
            var relativeZ = (poiWorldPos.z - playerWorldPos.z) * scale;
            
            return new Vector2(relativeX, relativeZ);
        }

        /// <summary>
        /// Gets the offset applied to the MapImageObject relative to MapContentObject.
        /// This is a static offset that accounts for how the image is positioned within its container.
        /// </summary>
        public static Vector2 GetMapImageOffset()
        {
            return new Vector2(Constants.MinimapImageOffsetX, Constants.MinimapImageOffsetY);
        }

        /// <summary>
        /// Validates that the coordinate system is producing reasonable values.
        /// Useful for debugging coordinate transformation issues.
        /// </summary>
        /// <param name="worldPos">A known world position to test</param>
        /// <param name="worldScaleFactor">The base world-to-UI scale factor.</param>
        /// <param name="currentZoomLevel">Current zoom level applied to the minimap.</param>
        /// <param name="minimapPlayerCenterXOffset">X offset for player centering.</param>
        /// <param name="minimapPlayerCenterYOffset">Y offset for player centering.</param>
        /// <returns>Debug string with transformation details</returns>
        public static string GetDebugInfo(Vector3 worldPos, float worldScaleFactor, float currentZoomLevel, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
        {
            var mapSpace = WorldToMapSpace(worldPos, worldScaleFactor, currentZoomLevel);
            var contentPos = GetMapContentPosition(worldPos, worldScaleFactor, currentZoomLevel, minimapPlayerCenterXOffset, minimapPlayerCenterYOffset);
            // Note: WorldToMarkerPosition needs playerWorldPos for accurate debug info
            // For this debug info, we'll assume playerWorldPos is the same as worldPos for simplicity
            var markerPos = WorldToMarkerPosition(worldPos, worldPos, worldScaleFactor, currentZoomLevel);
            
            return $"World: {worldPos}\n" +
                   $"Map Space: {mapSpace}\n" +
                   $"Content Pos: {contentPos}\n" +
                   $"Marker Pos (relative to self as player): {markerPos}\n" +
                   $"Scale: {WorldToUIScale(worldScaleFactor, currentZoomLevel)}x";
        }
        
        /// <summary>
        /// Clamps a marker position to stay within the circular minimap bounds.
        /// </summary>
        /// <param name="markerPosition">The marker's current anchored position</param>
        /// <param name="minimapRadius">Radius of the circular minimap</param>
        /// <returns>Clamped position that stays within the circle</returns>
        public static Vector2 ClampToCircle(Vector2 markerPosition, float minimapRadius)
        {
            var distance = markerPosition.magnitude;
            if (distance > minimapRadius)
            {
                return markerPosition.normalized * minimapRadius;
            }
            return markerPosition;
        }
        
        /// <summary>
        /// Clamps a marker position to stay within the square minimap bounds.
        /// </summary>
        /// <param name="markerPosition">The marker's current anchored position</param>
        /// <param name="minimapHalfSize">Half the size of the square minimap (radius equivalent)</param>
        /// <returns>Clamped position that stays within the square</returns>
        public static Vector2 ClampToSquare(Vector2 markerPosition, float minimapHalfSize)
        {
            var clampedX = Mathf.Clamp(markerPosition.x, -minimapHalfSize, minimapHalfSize);
            var clampedY = Mathf.Clamp(markerPosition.y, -minimapHalfSize, minimapHalfSize);
            return new Vector2(clampedX, clampedY);
        }
        
        /// <summary>
        /// Checks if a marker is outside the minimap bounds.
        /// </summary>
        /// <param name="markerPosition">The marker's current anchored position</param>
        /// <param name="minimapRadius">Radius of the minimap (or half-size for square)</param>
        /// <param name="isCircle">Whether the minimap is circular (true) or square (false)</param>
        /// <returns>True if the marker is outside bounds</returns>
        public static bool IsOutsideBounds(Vector2 markerPosition, float minimapRadius, bool isCircle)
        {
            if (isCircle)
            {
                return markerPosition.magnitude > minimapRadius;
            }
            else
            {
                return Mathf.Abs(markerPosition.x) > minimapRadius || Mathf.Abs(markerPosition.y) > minimapRadius;
            }
        }
    }
}

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
    public class MinimapCoordinateSystem
    {
        /// <summary>
        /// The base world-to-UI scale factor.
        /// </summary>
        private readonly float _worldScaleFactor;
        
        /// <summary>
        /// Current zoom level applied to the minimap.
        /// </summary>
        private float _currentZoomLevel = 1.0f;

        public MinimapCoordinateSystem()
        {
            _worldScaleFactor = Constants.DefaultMapScale * Constants.InitialMapImageScale;
        }

        /// <summary>
        /// Updates the current zoom level of the minimap.
        /// </summary>
        /// <param name="zoom">The new zoom level (e.g., 1.0 for normal, 0.5 for zoomed out).</param>
        public void SetCurrentZoomLevel(float zoom)
        {
            _currentZoomLevel = zoom;
        }

        /// <summary>
        /// The effective world-to-UI scale, incorporating the base world scale factor and current zoom level.
        /// </summary>
        public float WorldToUIScale => _worldScaleFactor * _currentZoomLevel;
        
        /// <summary>
        /// Current UI size multiplier.
        /// </summary>
        public float SizeMultiplier => _currentZoomLevel;
        
        /// <summary>
        /// Updates the size multiplier. Call this when the minimap size changes.
        /// </summary>
        public void SetSizeMultiplier(float multiplier)
        {
            // Note: In this revised system, SizeMultiplier and CurrentZoomLevel are effectively the same.
            // Keeping both for now for backward compatibility if needed, but consider unifying.
            _currentZoomLevel = multiplier;
        }

        /// <summary>
        /// Converts a world position to map space coordinates.
        /// Map space is the 2D coordinate system used for positioning markers on the map image.
        /// </summary>
        /// <param name="worldPos">World space position (3D)</param>
        /// <returns>2D map coordinates (x, z)</returns>
        public Vector2 WorldToMapSpace(Vector3 worldPos)
        {
            // Map space uses the CurrentMapScale which matches how the map image is sized
            var scale = WorldToUIScale;
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
        /// <returns>UI space position to apply to MapContentObject.anchoredPosition</returns>
        public Vector2 GetMapContentPosition(Vector3 playerWorldPos)
        {
            var scale = WorldToUIScale;
            
            // Invert coordinates to move map opposite of player
            var mappedX = -playerWorldPos.x * scale;
            var mappedZ = -playerWorldPos.z * scale;
            
            return new Vector2(mappedX, mappedZ);
        }

        /// <summary>
        /// Converts a world position to a position relative to the MapContentObject.
        /// Used for positioning POI markers on the map.
        /// </summary>
        /// <param name="worldPos">World space position</param>
        /// <returns>Position to use for marker's anchoredPosition relative to MapContent</returns>
        public Vector2 WorldToMarkerPosition(Vector3 worldPos)
        {
            // Markers are children of MapContentObject, so we just need map-space coordinates
            // The MapContentObject's position already handles the player centering
            var scale = WorldToUIScale;
            return new Vector2(worldPos.x * scale, worldPos.z * scale);
        }

        /// <summary>
        /// Gets the offset applied to the MapImageObject relative to MapContentObject.
        /// This is a static offset that accounts for how the image is positioned within its container.
        /// </summary>
        public Vector2 GetMapImageOffset()
        {
            return new Vector2(Constants.MinimapImageOffsetX, Constants.MinimapImageOffsetY);
        }

        /// <summary>
        /// Validates that the coordinate system is producing reasonable values.
        /// Useful for debugging coordinate transformation issues.
        /// </summary>
        /// <param name="worldPos">A known world position to test</param>
        /// <returns>Debug string with transformation details</returns>
        public string GetDebugInfo(Vector3 worldPos)
        {
            var mapSpace = WorldToMapSpace(worldPos);
            var contentPos = GetMapContentPosition(worldPos);
            var markerPos = WorldToMarkerPosition(worldPos);
            
            return $"World: {worldPos}\n" +
                   $"Map Space: {mapSpace}\n" +
                   $"Content Pos: {contentPos}\n" +
                   $"Marker Pos: {markerPos}\n" +
                   $"Scale: {WorldToUIScale}x";
        }
    }
}

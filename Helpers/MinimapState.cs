using UnityEngine;

namespace Small_Corner_Map.Helpers
{
}

namespace Small_Corner_Map.Helpers
{
}

namespace Small_Corner_Map.Helpers
{
    /// <summary>
    /// Shared state for minimap configuration that markers can use for clamping.
    /// </summary>
    public static class MinimapState
    {
        /// <summary>
        /// Whether the minimap is currently in circle mode (true) or square mode (false).
        /// </summary>
        public static bool IsCircleMode { get; set; } = true;
        
        /// <summary>
        /// Current minimap radius (for circle) or half-size (for square).
        /// This is the effective visible area size accounting for scale factor.
        /// </summary>
        public static float MinimapRadius { get; set; } = Constants.BaseMinimapSize / 2f;
        
        /// <summary>
        /// Current scale factor of the minimap (1.0 = base size, 1.5 = increased size).
        /// </summary>
        public static float ScaleFactor { get; set; } = 1.0f;
        
        /// <summary>
        /// Updates the minimap state with new configuration.
        /// </summary>
        public static void UpdateState(bool isCircle, float scaleFactor)
        {
            IsCircleMode = isCircle;
            ScaleFactor = scaleFactor;
            MinimapRadius = (Constants.BaseMinimapSize / 2f) * scaleFactor;
        }
    }
}

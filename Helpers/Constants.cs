namespace Small_Corner_Map.Helpers
{
    internal static class Constants
    {
        // Mod Info
        public const string ModVersion = "2.1.0";
        public const string ModName = "Small Corner Map";
        public const string ModAuthor = "winzaar";
        public const string GameName = "Schedule I";
        public const string GameDeveloper = "TVGS";
        
        // Map Scaling
        public const float DefaultMapScale = 1.2487098f;
        
        // Marker Offsets
        public const float ContractMarkerXOffset = 12f;  // X offset for contract markers
        public const float ContractMarkerZOffset = -3f;  // Z offset for contract markers
        public const float PropertyMarkerXOffset = -12f; // Horizontal calibration for property markers
        public const float PropertyMarkerZOffset = 0f;   // Vertical calibration for property markers
        
        // Minimap UI Sizing
        public const float BaseMinimapSize = 150f;       // Base size of the minimap mask/frame
        public const float BaseMapContentSize = 500f;    // Base size of the map content
        public const float MinimapCornerOffset = -20f;   // Distance from screen corner
        
        // Marker Sizing
        public const float ContractMarkerSize = 15f;     // Width/height of contract markers
        public const float ContractMarkerScale = 0.5f;   // Scale applied to contract markers
        public const float PropertyMarkerSize = 10f;     // Width/height of property markers
        public const float PropertyMarkerScale = 0.5f;   // Scale applied to property markers
        public const float RedMarkerSize = 5f;           // Width/height of red debug markers
        public const float PlayerMarkerSize = 10f;       // Width/height of player marker
        public const float DirectionIndicatorSize = 6f;  // Width/height of direction indicator
        public const float DirectionIndicatorDistance = 15f; // Distance from player marker center
        
        // Map Grid
        public const int DefaultGridSize = 20;           // Grid divisions for map background
        
        // Canvas Settings
        public const int CanvasSortOrder = 9999;         // Sort order for minimap canvas
        public const float CanvasReferenceWidth = 1920f; // Reference resolution width
        public const float CanvasReferenceHeight = 1080f; // Reference resolution height
        
        // Update & Animation
        public const float MapContentLerpSpeed = 10f;    // Smoothing speed for map panning
        public const float PlayerMarkerOffsetX = 11.2f;  // Player marker horizontal centering offset
        public const float PlayerMarkerOffsetZ = -2.7f;  // Player marker vertical centering offset
        
        // Time Display
        public const float TimeDisplayWidth = 100f;      // Width of time display container
        public const float TimeDisplayHeight = 50f;      // Height of time display container
        public const float TimeDisplayOffsetY = 20f;     // Vertical offset from minimap
        
        // Colors (RGBA)
        public const float TimeBackgroundR = 0.2f;       // Time display background red
        public const float TimeBackgroundG = 0.2f;       // Time display background green
        public const float TimeBackgroundB = 0.2f;       // Time display background blue
        public const float TimeBackgroundA = 0.5f;       // Time display background alpha
        public const float PlayerMarkerR = 0.2f;         // Player marker color red
        public const float PlayerMarkerG = 0.6f;         // Player marker color green
        public const float PlayerMarkerB = 1f;           // Player marker color blue
        public const float PlayerMarkerA = 1f;           // Player marker color alpha
        
        // Player Icon Replacement Scale
        public const float PlayerIconReplacementScale = 0.5f; // Scale applied when replacing player icon
        
        // Scene Integration
        public const float SceneIntegrationInitialDelay = 2f;   // Wait before searching for game objects
        public const float SceneIntegrationRetryDelay = 0.5f;   // Wait between search attempts
        public const int SceneIntegrationMaxAttempts = 30;      // Max attempts to find game objects
        
        // GameObject Paths
        public const string GameplayMenuPath = "GameplayMenu";
        public const string MapAppPath = "GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content";
    }
}

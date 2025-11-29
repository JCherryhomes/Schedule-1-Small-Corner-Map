namespace Small_Corner_Map.Helpers
{
    internal static class Constants
    {
        // Mod Info
        public const string ModVersion = "3.2.1";
        public const string ModName = "Small Corner Map";
        public const string ModAuthor = "winzaar";
        public const string GameName = "Schedule I";
        public const string GameDeveloper = "TVGS";
        
        // Map Scaling
        /// <summary>
        /// Base world-to-UI scale factor. This constant defines the fundamental ratio
        /// between world units and UI pixels. Empirically determined to match the game's map image.
        /// </summary>
        public const float DefaultMapScale = 1.2487098f;
        
        /// <summary>
        /// Initial scaling factor for the map image to reduce its size.
        /// </summary>
        public const float InitialMapImageScale = 0.125f;

        // Marker Offsets
        public const float MarkerXOffset = 14f;          // X offset for markers (inverted for properties: -12f)
        public const float MarkerZOffset = -3.5f;         // Z offset for markers

        // Minimap Image Offset
        /// <summary>
        /// X offset applied to the MapImageObject within its MapContentObject parent.
        /// This accounts for the hierarchical positioning of UI elements.
        /// </summary>
        public const float MinimapImageOffsetX = 4f;
        
        /// <summary>
        /// Y offset applied to the MapImageObject within its MapContentObject parent.
        /// This accounts for the hierarchical positioning of UI elements.
        /// </summary>
        public const float MinimapImageOffsetY = 1.5f;
        
        // Minimap UI Sizing
        public const float BaseMinimapSize = 150f;       // Base size of the minimap mask/frame
        public const float BaseMapContentSize = 500f;    // Base size of the map content
        public const float MinimapCornerOffset = -20f;   // Distance from screen corner
        public const float MinimapSizeMultiplier = 1.5f; // Multiplier when minimap is increased
        
        // Marker Sizing
        public const float ContractMarkerSize = 8f;     // Width/height of contract markers (reduced from 15f)
        public const float ContractMarkerScale = 0.4f;   // Scale applied to contract markers (reduced from 0.5f)
        public const float PropertyMarkerSize = 5f;      // Width/height of property markers (reduced from 10f)
        public const float PropertyMarkerScale = 0.4f;   // Scale applied to property markers (reduced from 0.5f)
        public const float RedMarkerSize = 5f;           // Width/height of red debug markers
        public const float PlayerMarkerSize = 10f;       // Width/height of player marker
        public const float DirectionIndicatorSize = 6f;  // Width/height of direction indicator
        public const float DirectionIndicatorDistance = 15f; // Distance from player marker center
        
        // Minimap Border
        public const float MinimapBorderThickness = 2f;  // Pixel thickness outward from mask circle (reduced for less extension)
        public const float MinimapBorderR = 0.13f;      // Dark grey border color (R)
        public const float MinimapBorderG = 0.13f;      // Dark grey border color (G)
        public const float MinimapBorderB = 0.13f;      // Dark grey border color (B)
        public const float MinimapBorderA = 1f;         // Border alpha
        public const int MinimapBorderFeather = 2;      // Feather width in pixels for anti-aliased edge (reduced to prevent clipping)
        public const int MinimapCircleResolutionMultiplier = 2; // Resolution multiplier for smoother circle
        public const int MinimapMaskFeather = 2;         // Feather width for minimap mask edge (softer clipping, reduced from 2)
        public const int MinimapMaskDiameterOffset = 2;  // Extra pixels added to mask diameter to compensate feather shrink
        
        // Map Grid
        public const int DefaultGridSize = 20;           // Grid divisions for map background
        
        // Canvas Settings
        public const int CanvasSortOrder = 9999;         // Sort order for minimap canvas
        public const float CanvasReferenceWidth = 1920f; // Reference resolution width
        public const float CanvasReferenceHeight = 1080f; // Reference resolution height
        
        // Update & Animation
        /// <summary>
        /// Lerp speed for smooth map panning. Higher values make the map follow the player more quickly.
        /// </summary>
        public const float MapContentLerpSpeed = 10f;
        
        /// <summary>
        /// Horizontal centering offset for the player marker in UI space.
        /// This empirically determined value accounts for the minimap's UI hierarchy and pivot points.
        /// Scaled by MinimapScaleFactor when the minimap size changes.
        /// </summary>
        public const float PlayerMarkerOffsetX = 11.2f;
        
        /// <summary>
        /// Vertical centering offset for the player marker in UI space.
        /// This empirically determined value accounts for the minimap's UI hierarchy and pivot points.
        /// Scaled by MinimapScaleFactor when the minimap size changes.
        /// </summary>
        public const float PlayerMarkerOffsetZ = -2.7f;
        
        // New configurable offsets for player marker centering
        /// <summary>
        /// X offset to apply to the map content position to correctly center the player marker in the minimap viewport.
        /// This is a configurable value to be adjusted based on the new UI rendering.
        /// </summary>
        public const float MinimapPlayerCenterXOffset = 0f;
        
        /// <summary>
        /// Y offset to apply to the map content position to correctly center the player marker in the minimap viewport.
        /// This is a configurable value to be adjusted based on the new UI rendering.
        /// </summary>
        public const float MinimapPlayerCenterYOffset = 0f;
        
        // Time Display
        public const float TimeDisplayWidth = 100f;      // Width of time display container
        public const float TimeDisplayHeight = 50f;      // Height of time display container
        public const float TimeDisplayOffsetY = -20f;    // Vertical offset from minimap bottom
        
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
        
        public static readonly string DeadDropQuestName = "Collect Dead Drop";
        public static readonly string ContractQuestName = "Deal for";
        
        // Vehicle Marker Settings
        public const float VehicleMarkerFadeDuration = 0.15f; // Seconds to fade vehicle markers in/out
        
        // Compass Settings
        public const int CompassTickCount = 16; // Total ticks (including cardinal positions)
        public const float CompassRingPadding = 6f; // Padding outside minimap mask to start compass ring
        public const int CompassLetterFontSize = 12; // Font size for compass letters
        public const float CompassLetterColorR = 1f;
        public const float CompassLetterColorG = 1f;
        public const float CompassLetterColorB = 1f;
        public const float CompassLetterColorA = 0.9f;
        public const float CompassTickColorR = 1f;
        public const float CompassTickColorG = 1f;
        public const float CompassTickColorB = 1f;
        public const float CompassTickColorA = 0.75f;
        public const float CompassTickMajorScale = 1f; // Scale multiplier for cardinal ticks
        public const float CompassTickMinorScale = 0.6f; // Scale multiplier for non-cardinal ticks
        public const float CompassTickWidth = 2f;
        public const float CompassTickHeight = 8f; // Reduced base height to avoid overlap
        public const float CompassRingExtraThickness = 2f; // Extra thickness beyond tallest element
        public const string CompassPreferenceKey = "ShowCompass"; // Preference key
        public const float CompassLetterRadialOffset = 4f; // Additional outward offset for letters beyond ticks
        public const float CompassTickInset = 2f; // Inset ticks slightly inward relative to their half height
        public const int CompassBorderThickness = 2; // Pixel thickness of compass inner/outer border lines
        public const float CompassBorderColorR = 0.13f; // Dark grey
        public const float CompassBorderColorG = 0.13f;
        public const float CompassBorderColorB = 0.13f;
        public const float CompassBorderColorA = 0.9f; // Less translucent for clarity
        
        // Compass Vehicle Marker Settings
        public const float CompassVisibilityBuffer = 4f; // Extra pixels beyond map radius before showing on compass
        
        // Compass Background Color
        public const float CompassBackgroundColorR = 0.8f; // Light grey
        public const float CompassBackgroundColorG = 0.8f;
        public const float CompassBackgroundColorB = 0.8f;
        public const float CompassBackgroundColorA = 0.3f; // Slight transparency

        // Compass Icon Sizes
        public const float CompassDefaultIconSize = 15f; // Default size for vehicle/property/other compass icons
        public const float CompassContractIconSize = 20f; // Contract marker size
    }
}
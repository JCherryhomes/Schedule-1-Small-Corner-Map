using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MelonLoader;
using Small_Corner_Map.Helpers;
using System.Reflection;
using Small_Corner_Map.PoIManagers;

#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Vehicles;
#else
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
#endif

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class MinimapView : MonoBehaviour
    {
        private GameObject canvasGo;
        private GameObject mapImageGo;
        private Image mapImage;
        private Image maskImage;
        private Mask mask;
        private Image borderImage;

        private PlayerMarkerView playerMarkerView;
        private MapMarkerManager mapMarkerManager;
        private TimeDisplayView timeDisplayView;
        
        private Transform playerTransform; // Added to store player transform for Update

        private Sprite circleSprite;
        private Sprite squareSprite;

        // Store coordinate system primitives for Update method
        private float worldScaleFactor;
        private float minimapPlayerCenterXOffset;
        private float minimapPlayerCenterYOffset;
        private float currentZoomLevel; // Will be updated by preferences
        private RectTransform mapImageRT;
        private RectTransform containerRT;


        private void Start()
        {
            mapImageGo = new GameObject("MapImage");
            mapImageRT = mapImageGo.AddComponent<RectTransform>();
        }

        public void Initialize(
            Player player, bool minimapEnabled, float minimapScaleFactor, bool showSquareMinimap, bool showGameTime, float scaleFactor, float mapZoomLevel, float playerCenterXOffset, float playerCenterYOffset, bool trackProperties, bool trackContracts, bool trackVehicles, float positionX, float positionY)
        {
            MelonLogger.Msg("MinimapView initializing.");
            
            playerTransform = player.transform; // Store player transform
            worldScaleFactor = scaleFactor; // This comes from Constants.BaseWorldToUIScaleFactor in MinimapManager
            minimapPlayerCenterXOffset = playerCenterXOffset;
            minimapPlayerCenterYOffset = playerCenterYOffset;
            currentZoomLevel = mapZoomLevel;
            
            // Initialize minimap state for marker clamping
            MinimapState.UpdateState(!showSquareMinimap, minimapScaleFactor);

            // Generate sprites for mask
            if (!circleSprite) 
                circleSprite = Utils.CreateCircleSprite(Constants.MinimapCircleDrawingResolution, Color.grey, 2, 2);
            
            if (!squareSprite)
                squareSprite = Utils.CreateRoundedSquareSprite(
                    (int)Constants.BaseMinimapSize, 3f, Color.grey, 2);

            // Create the UI
            if (minimapEnabled)
            {
                CreateMinimapUI(player, minimapScaleFactor, showGameTime, trackProperties, trackContracts, trackVehicles, !showSquareMinimap, positionX, positionY);

                // Load the map sprite
                LoadMapSprite();
                
                // Set initial style
                SetStyle(!showSquareMinimap); // Default to circle if not square
            }
            ToggleMinimapVisibility(minimapEnabled);
        }

        public void ToggleMinimapVisibility(bool isVisible)
        {
            if (canvasGo != null)
            {
                canvasGo.SetActive(isVisible);
            }
        }

        public void UpdateMinimapUISize(float newScaleFactor)
        {
            if (canvasGo != null)
            {
                var containerRT = canvasGo.transform.Find("MinimapContainer").GetComponent<RectTransform>();
                if (containerRT != null)
                {
                    containerRT.sizeDelta = new Vector2(Constants.BaseMinimapSize * newScaleFactor, Constants.BaseMinimapSize * newScaleFactor);
                    
                    // Update shared state
                    MinimapState.UpdateState(MinimapState.IsCircleMode, newScaleFactor);
                }
            }
        }

        public void UpdateMinimapShape(bool isSquare)
        {
            bool isCircle = !isSquare;
            SetStyle(isCircle);
            
            // Update shared state for radius calculation
            MinimapState.UpdateState(isCircle, MinimapState.ScaleFactor);
            
            // Propagate shape change to marker manager
            if (mapMarkerManager != null)
            {
                mapMarkerManager.UpdateMinimapShape(isCircle);
            }
        }

        public void UpdateMapMovementScale(float newZoomLevel)
        {
            currentZoomLevel = newZoomLevel;
            if (playerMarkerView != null)
            {
                playerMarkerView.UpdateZoomLevel(newZoomLevel);
            }
            if (mapMarkerManager != null)
            {
                mapMarkerManager.UpdateZoomLevel(newZoomLevel);
            }
        }
        
        public void UpdateMinimapPlayerCenterXOffset(float newOffsetX)
        {
            minimapPlayerCenterXOffset = newOffsetX;
        }

        public void UpdateMinimapPlayerCenterYOffset(float newOffsetY)
        {
            minimapPlayerCenterYOffset = newOffsetY;
        }

        public void UpdateTimeDisplayVisibility(bool isVisible)
        {
            if (timeDisplayView != null)
            {
                timeDisplayView.ToggleVisibility(isVisible);
            }
        }   
        
        public void UpdateVehicleTracking(bool isVisible)
        {
            // Vehicle tracking handled in MapMarkerManager
            if (mapMarkerManager != null)
            {
                mapMarkerManager.OnTrackVehiclesChanged(isVisible);
            }
        }
        
        public void UpdatePropertyTracking(bool isVisible)
        {
            if (mapMarkerManager != null)
            {
                mapMarkerManager.OnTrackPropertiesChanged(isVisible);
            }
        }

        public void UpdateContractTracking(bool isVisible)
        {
            if (mapMarkerManager != null)
            {
                mapMarkerManager.OnTrackContractsChanged(isVisible);
            }
        }
        
        public void HandlePlayerEnterVehicle(LandVehicle vehicle)
        {
            // Notify player marker view
            if (mapMarkerManager != null)
            {
                mapMarkerManager.OnPlayerEnterVehicle(vehicle);
            }
        }
        
        public void HandlePlayerExitVehicle(LandVehicle vehicle)
        {
            // Notify player marker view
            if (mapMarkerManager != null)
            {
                mapMarkerManager.OnPlayerExitVehicle(vehicle);
            }
        }

        private void CreateMinimapUI(Player player, float minimapScaleFactor, bool showGameTime, bool trackProperties, bool trackContracts, bool trackVehicles, bool isCircle, float positionX, float positionY)
        {
            // --- Canvas ---
            canvasGo = new GameObject("MinimapCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGo);

            // --- Minimap Container (positioned based on preferences) ---
            var containerGo = new GameObject("MinimapContainer");
            containerGo.transform.SetParent(canvasGo.transform, false);
            containerRT = containerGo.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(1, 1);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.pivot = new Vector2(1, 1);
            containerRT.anchoredPosition = new Vector2(positionX, positionY);
            containerRT.sizeDelta = new Vector2(Constants.BaseMinimapSize * minimapScaleFactor, Constants.BaseMinimapSize * minimapScaleFactor);

            // --- Minimap Border ---
            var borderGo = new GameObject("MinimapBorder");
            borderGo.transform.SetParent(containerRT, false);
            var borderRT = borderGo.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-2, -2);
            borderRT.offsetMax = new Vector2(2, 2);
            borderImage = borderGo.AddComponent<Image>();
            borderImage.color = Color.black;

            // --- Minimap Mask ---
            var maskGo = new GameObject("MinimapMask");
            maskGo.transform.SetParent(containerRT, false);
            var maskRT = maskGo.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;
            maskImage = maskGo.AddComponent<Image>();
            maskImage.color = Color.white;
            mask = maskGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // --- Map Image ---
            mapImageGo.transform.SetParent(maskRT, false);
            mapImageRT.anchorMin = new Vector2(0.5f, 0.5f);
            mapImageRT.anchorMax = new Vector2(0.5f, 0.5f);
            mapImageRT.pivot = new Vector2(0.5f, 0.5f);
            mapImageRT.sizeDelta = new Vector2(500, 500); // Initial size, will be adjusted by zoom
            mapImage = mapImageGo.AddComponent<Image>();
            mapImage.color = Color.white;

            // --- Player Marker ---
            playerMarkerView = new GameObject("PlayerMarkerView").AddComponent<PlayerMarkerView>();
            playerMarkerView.transform.SetParent(containerRT, false); // Parent to containerRT for fixed center position
            playerMarkerView.Initialize(
                containerRT.transform, worldScaleFactor, currentZoomLevel, minimapPlayerCenterXOffset, minimapPlayerCenterYOffset);

            mapMarkerManager = new GameObject("PropertyPoIManager").AddComponent<MapMarkerManager>();
            mapMarkerManager.transform.SetParent(containerRT, false);
            mapMarkerManager.Initialize(
                mapImageRT, worldScaleFactor, currentZoomLevel, trackProperties, trackContracts, trackVehicles, isCircle, minimapPlayerCenterXOffset, minimapPlayerCenterYOffset);
            
            // --- Time Display ---
            timeDisplayView = new GameObject("TimeDisplayView").AddComponent<TimeDisplayView>();
            timeDisplayView.transform.SetParent(containerRT, false);
            timeDisplayView.Initialize(containerRT, showGameTime);
        }

        private void LoadMapSprite()
        {
             try
             {
                 var assembly = Assembly.GetExecutingAssembly();
                 using var stream = assembly.GetManifestResourceStream(Constants.MinimapImagePath);
                 if (stream == null)
                 {
                     MelonLogger.Error($"MinimapView: Failed to get manifest resource stream for {Constants.MinimapImagePath}. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
                     return;
                 }

                 var fileData = new byte[stream.Length];
                 stream.Read(fileData, 0, (int)stream.Length);

                 var texture = new Texture2D(2, 2);
                 if (texture.LoadImage(fileData))
                 {
                     var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                     mapImage.sprite = sprite;
                 }
                 else
                 {
                     MelonLogger.Error($"MinimapView: Failed to load image data into texture from embedded resource {Constants.MinimapImagePath}");
                 }
             }
             catch (System.Exception ex)
             {
                 MelonLogger.Error($"MinimapView: Error loading map sprite: {ex.Message}");
             }
        }

        public void SetStyle(bool useCircle)
        {
            if (maskImage == null || borderImage == null) return;

            var sprite = useCircle ? circleSprite : squareSprite;
            maskImage.sprite = sprite;
            borderImage.sprite = sprite;
        }

        private void Update()
        {
            if (!playerTransform || !mapImageGo) return;
            
            if (Player.Local.IsInVehicle)
            {
                playerTransform = Player.Local.CurrentVehicle.transform;
            }
            else
            {
                playerTransform = Player.Local.transform;
            }

            // Pass primitives to the static MinimapCoordinateSystem methods
            var newMapPosition = MinimapCoordinateSystem.GetMapContentPosition(
                playerTransform.position, 
                worldScaleFactor, 
                currentZoomLevel, 
                minimapPlayerCenterXOffset, 
                minimapPlayerCenterYOffset
            );
            mapImageRT.anchoredPosition = newMapPosition;
        }
        
        public void UpdateMinimapPosition(float positionX, float positionY)
        {
            if (containerRT != null)
            {
                containerRT.anchoredPosition = new Vector2(positionX, positionY);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using Small_Corner_Map.Helpers;
using System.IO;
using System.Reflection;
#if IL2CPP
using Il2CppScheduleOne.PlayerScripts;
#else
using ScheduleOne.PlayerScripts;
#endif

namespace Small_Corner_Map.Main
{
    [RegisterTypeInIl2Cpp]
    public class MinimapView : MonoBehaviour
    {
        private GameObject _canvasGO;
        private GameObject _mapImageGO;
        private Image _mapImage;
        private Image _maskImage;
        private Mask _mask;

        private PlayerMarkerView _playerMarkerView;
        private TimeDisplayView _timeDisplayView;
        
        private Transform _playerTransform; // Added to store player transform for Update

        private Sprite _circleSprite;
        private Sprite _squareSprite;

        // Store coordinate system primitives for Update method
        private float _worldScaleFactor;
        private float _minimapPlayerCenterXOffset;
        private float _minimapPlayerCenterYOffset;
        private float _currentZoomLevel; // Will be updated by preferences


        public void Initialize(Player player, bool minimapEnabled, float minimapScaleFactor, bool showSquareMinimap, MelonPreferences_Entry<bool> showGameTimePreference, float worldScaleFactor, float mapZoomLevel, float minimapPlayerCenterXOffset, float minimapPlayerCenterYOffset)
        {
            MelonLogger.Msg("MinimapView initializing.");
            
            _playerTransform = player.transform; // Store player transform
            _worldScaleFactor = worldScaleFactor;
            _minimapPlayerCenterXOffset = minimapPlayerCenterXOffset;
            _minimapPlayerCenterYOffset = minimapPlayerCenterYOffset;
            _currentZoomLevel = mapZoomLevel; // Set current zoom level from preferences

            // Generate sprites for mask
            _circleSprite = Utils.CreateCircleSprite(128, Color.white);
            _squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);

            // Create the UI
            if (minimapEnabled)
            {
                CreateMinimapUI(player, minimapScaleFactor, showGameTimePreference);

                // Load the map sprite
                LoadMapSprite();
                
                // Set initial style
                SetStyle(!showSquareMinimap); // Default to circle if not square
            }
            ToggleMinimapVisibility(minimapEnabled);
        }

        public void ToggleMinimapVisibility(bool isVisible)
        {
            if (_canvasGO != null)
            {
                _canvasGO.SetActive(isVisible);
            }
        }

        public void UpdateMinimapUISize(float newScaleFactor)
        {
            if (_canvasGO != null)
            {
                RectTransform containerRT = _canvasGO.transform.Find("MinimapContainer").GetComponent<RectTransform>();
                if (containerRT != null)
                {
                    containerRT.sizeDelta = new Vector2(Constants.BaseMinimapSize * newScaleFactor, Constants.BaseMinimapSize * newScaleFactor);
                }
            }
        }

        public void UpdateMinimapShape(bool isSquare)
        {
            SetStyle(!isSquare);
        }

        public void UpdateMapMovementScale(float newZoomLevel)
        {
            _currentZoomLevel = newZoomLevel;
            if (_playerMarkerView != null)
            {
                _playerMarkerView.UpdateZoomLevel(newZoomLevel);
            }
        }
        
        public void UpdateMinimapPlayerCenterXOffset(float newOffsetX)
        {
            _minimapPlayerCenterXOffset = newOffsetX;
        }

        public void UpdateMinimapPlayerCenterYOffset(float newOffsetY)
        {
            _minimapPlayerCenterYOffset = newOffsetY;
        }

        public void UpdateTimeDisplayVisibility(bool isVisible)
        {
            if (_timeDisplayView != null)
            {
                _timeDisplayView.ToggleVisibility(isVisible);
            }
        }



        public void UpdateCompassVisibility(bool isVisible)
        {
            // Implementation for compass visibility will go here later
            MelonLogger.Msg($"MinimapView: Compass visibility set to: {isVisible}");
        }


        private void CreateMinimapUI(Player player, float minimapScaleFactor, MelonPreferences_Entry<bool> showGameTimePreference)
        {
            // --- Canvas ---
            _canvasGO = new GameObject("MinimapCanvas");
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var canvasScaler = _canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            _canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(_canvasGO);

            // --- Minimap Container (top-right corner) ---
            var containerGO = new GameObject("MinimapContainer");
            containerGO.transform.SetParent(_canvasGO.transform, false);
            var containerRT = containerGO.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(1, 1);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.pivot = new Vector2(1, 1);
            containerRT.anchoredPosition = new Vector2(-20, -20);
            containerRT.sizeDelta = new Vector2(Constants.BaseMinimapSize * minimapScaleFactor, Constants.BaseMinimapSize * minimapScaleFactor);

            // --- Minimap Mask ---
            var maskGO = new GameObject("MinimapMask");
            maskGO.transform.SetParent(containerRT, false);
            var maskRT = maskGO.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;
            _maskImage = maskGO.AddComponent<Image>();
            _maskImage.color = Color.white;
            _mask = maskGO.AddComponent<Mask>();
            _mask.showMaskGraphic = false;

            // --- Map Image ---
            _mapImageGO = new GameObject("MapImage");
            _mapImageGO.transform.SetParent(maskRT, false);
            var mapImageRT = _mapImageGO.AddComponent<RectTransform>();
            mapImageRT.anchorMin = new Vector2(0.5f, 0.5f);
            mapImageRT.anchorMax = new Vector2(0.5f, 0.5f);
            mapImageRT.pivot = new Vector2(0.5f, 0.5f);
            mapImageRT.sizeDelta = new Vector2(500, 500); // Initial size, will be adjusted by zoom
            _mapImage = _mapImageGO.AddComponent<Image>();
            _mapImage.color = Color.white; 
            MelonLogger.Msg($"MinimapView: MapImage RectTransform sizeDelta: {mapImageRT.sizeDelta}");

            // --- Player Marker ---
            _playerMarkerView = new GameObject("PlayerMarkerView").AddComponent<PlayerMarkerView>();
            _playerMarkerView.transform.SetParent(containerRT, false); // Parent to containerRT for fixed center position
            _playerMarkerView.Initialize(player.transform, containerRT.transform, _worldScaleFactor, _currentZoomLevel, _minimapPlayerCenterXOffset, _minimapPlayerCenterYOffset);
            
            // --- Time Display ---
            _timeDisplayView = new GameObject("TimeDisplayView").AddComponent<TimeDisplayView>();
            _timeDisplayView.transform.SetParent(containerRT, false);
            _timeDisplayView.Initialize(containerRT, showGameTimePreference);
        }

        private void LoadMapSprite()
        {
             try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(Constants.MinimapImagePath))
                {
                    if (stream == null)
                    {
                        MelonLogger.Error($"MinimapView: Failed to get manifest resource stream for {Constants.MinimapImagePath}. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
                        return;
                    }

                    byte[] fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, (int)stream.Length);

                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(fileData))
                    {
                        MelonLogger.Msg($"MinimapView: Loaded texture with size: {texture.width}x{texture.height}");
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        _mapImage.sprite = sprite;
                        MelonLogger.Msg("MinimapView: Successfully loaded and assigned map sprite.");
                    }
                    else
                    {
                        MelonLogger.Error($"MinimapView: Failed to load image data into texture from embedded resource {Constants.MinimapImagePath}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"MinimapView: Error loading map sprite: {ex.Message}");
            }
        }

        public void SetStyle(bool useCircle)
        {
            if (_maskImage == null) return;

            if (useCircle)
            {
                _maskImage.sprite = _circleSprite;
            }
            else
            {
                _maskImage.sprite = _squareSprite;
            }
        }

        void Update()
        {
            if (_playerTransform != null && _mapImageGO != null)
            {
                RectTransform mapImageRT = _mapImageGO.GetComponent<RectTransform>();
                // Pass primitives to the static MinimapCoordinateSystem methods
                Vector2 newMapPosition = MinimapCoordinateSystem.GetMapContentPosition(
                    _playerTransform.position, 
                    _worldScaleFactor, 
                    _currentZoomLevel, 
                    _minimapPlayerCenterXOffset, 
                    _minimapPlayerCenterYOffset
                );
                mapImageRT.anchoredPosition = newMapPosition;
            }
        }
    }
}
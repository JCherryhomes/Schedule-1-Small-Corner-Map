using System.Collections;
using MelonLoader;
using S1API.Entities;
using UnityEngine;
using UnityEngine.UI;
using Small_Corner_Map.Helpers;
namespace Small_Corner_Map.Main;
/// <summary>
/// Handles scene integration: finding game objects, setting up markers, and applying sprites.
/// </summary>
internal class MinimapSceneIntegration
{
    private readonly MinimapContent minimapContent;
    private readonly PlayerMarkerManager playerMarkerManager;
    private readonly MapPreferences mapPreferences;
    public Player PlayerObject { get; private set; }
    public GameObject CachedMapContent { get; private set; }
    public MinimapSceneIntegration(
        MinimapContent content,
        PlayerMarkerManager playerMarker,
        MapPreferences preferences)
    {
        minimapContent = content;
        playerMarkerManager = playerMarker;
        mapPreferences = preferences;
    }
    public IEnumerator IntegrateWithScene()
    {
        MelonLogger.Msg("MinimapUI: Looking for game objects...");
        yield return new WaitForSeconds(2f);
        GameObject mapAppObject = null;
        GameObject viewportObject = null;
        var attempts = 0;
        while ((mapAppObject == null || PlayerObject == null) && attempts < 30)
        {
            attempts++;
            PlayerObject ??= Player.Local;
            if (mapAppObject == null) mapAppObject = FindMapApp();
            if (mapAppObject != null && viewportObject == null) viewportObject = FindViewport(mapAppObject);
            if (mapAppObject == null || PlayerObject == null)
                yield return new WaitForSeconds(Constants.SceneIntegrationRetryDelay);
        }
        LogIntegrationResults(mapAppObject, viewportObject);
        if (viewportObject != null) ApplyMapSprite(viewportObject);
        CachedMapContent = GameObject.Find("GameplayMenu/Phone/phone/AppsCanvas/MapApp/Container/Scroll View/Viewport/Content");
        if (CachedMapContent != null)
        {
            ReplacePlayerMarkerIcon();
            SetupPropertyMarkers();
        }
    }
    private GameObject FindMapApp()
    {
        var gameplayMenu = GameObject.Find("GameplayMenu");
        var mapApp = gameplayMenu?.transform.Find("Phone")?.Find("phone")?.Find("AppsCanvas")?.Find("MapApp");
        if (mapApp != null) MelonLogger.Msg("MinimapUI: Found MapApp");
        return mapApp?.gameObject;
    }
    private GameObject FindViewport(GameObject mapAppObject)
    {
        var viewport = mapAppObject.transform.Find("Container")?.Find("Scroll View")?.Find("Viewport");
        if (viewport != null) MelonLogger.Msg("MinimapUI: Found Map Viewport");
        return viewport?.gameObject;
    }
    private void LogIntegrationResults(GameObject mapAppObject, GameObject viewportObject)
    {
        if (mapAppObject == null) MelonLogger.Warning("MinimapUI: Could not find Map App after multiple attempts");
        else if (viewportObject == null) MelonLogger.Warning("MinimapUI: Found MapApp but could not find Viewport");
        if (PlayerObject == null) MelonLogger.Warning("MinimapUI: Could not find Player after multiple attempts");
        MelonLogger.Msg("MinimapUI: Game object search completed");
    }
    private void ApplyMapSprite(GameObject viewportObject)
    {
        try
        {
            if (viewportObject.transform.childCount <= 0) return;
            var contentTransform = viewportObject.transform.GetChild(0);
            MelonLogger.Msg("MinimapUI: Found viewport content: " + contentTransform.name);
            var contentImage = contentTransform.GetComponent<Image>();
            if (contentImage != null && contentImage.sprite != null)
            {
                ApplySpriteToMinimap(contentImage);
                return;
            }
            MelonLogger.Msg("MinimapUI: Content doesn't have an Image component or sprite");
            TryApplySpriteFromChildren(contentTransform);
        }
        catch (Exception ex)
        {
            MelonLogger.Error("MinimapUI: Error accessing map content: " + ex.Message);
        }
    }
    private void ApplySpriteToMinimap(Image sourceImage)
    {
        MelonLogger.Msg("MinimapUI: Found content image with sprite: " + sourceImage.sprite.name);
        var minimapImage = minimapContent.MapContentObject.GetComponent<Image>();
        if (minimapImage == null) minimapImage = minimapContent.MapContentObject.AddComponent<Image>();
        minimapImage.sprite = sourceImage.sprite;
        minimapImage.type = Image.Type.Simple;
        minimapImage.preserveAspect = true;
        minimapImage.enabled = true;
        MelonLogger.Msg("MinimapUI: Successfully applied map sprite to minimap!");
        if (minimapContent.GridContainer != null) minimapContent.GridContainer.gameObject.SetActive(false);
    }
    private void TryApplySpriteFromChildren(Transform contentTransform)
    {
        for (var i = 0; i < contentTransform.childCount; i++)
        {
            var child = contentTransform.GetChild(i);
            var childImage = child.GetComponent<Image>();
            if (childImage != null && childImage.sprite != null)
            {
                MelonLogger.Msg("MinimapUI: Found image in content child: " + child.name + ", Sprite: " + childImage.sprite.name);
                ApplySpriteToMinimap(childImage);
                break;
            }
        }
    }
    private void ReplacePlayerMarkerIcon()
    {
        var playerPoI = CachedMapContent.transform.Find("PlayerPoI(Clone)");
        var realIcon = playerPoI?.Find("IconContainer");
        if (realIcon != null)
        {
            playerMarkerManager.ReplaceWithRealPlayerIcon(realIcon.gameObject);
            MelonLogger.Msg("MinimapUI: Replaced fallback player marker with real player icon.");
        }
    }
    private void SetupPropertyMarkers()
    {
        PropertyPoIManager.CacheIconContainerIfNeeded(CachedMapContent);
        if (mapPreferences.TrackProperties.Value)
            PropertyPoIManager.Initialize(minimapContent, CachedMapContent);
    }
}

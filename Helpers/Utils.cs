using UnityEngine;
using System.Collections.Generic; // Added for List
// using MelonLoader; // Removed MelonLoader as Utils should not directly depend on it

namespace Small_Corner_Map.Helpers
{
    internal class Utils
    {
        internal static void RecursiveFind(Transform current, string targetName, List<Transform> result)
        {
            if (current.name == targetName)
            {
                result.Add(current);
            }
            for (int i = 0; i < current.childCount; i++)
            {
                RecursiveFind(current.GetChild(i), targetName, result);
            }
        }

        /// <summary>
        /// Creates a dynamically generated square sprite of a specified size and color.
        /// </summary>
        /// <param name="size">The width and height in pixels (e.g., 512).</param>
        /// <param name="color">The color to fill the square with.</param>
        /// <returns>The newly created Sprite.</returns>
        public static Sprite CreateSquareSprite(int size, Color color)
        {
            // 1. Create a new Texture2D
            // Format RGBA32 allows transparency; false means no mipmaps needed for UI
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // 2. Fill the texture with the specified color
            // Create an array of colors equal to the total number of pixels (size * size)
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply(); // Apply the changes to the texture

            // 3. Create a Sprite from the Texture2D
            // Rect defines the area of the texture to use (the whole thing in this case)
            Rect rect = new Rect(0, 0, size, size);

            // Pivot point (Vector2(0.5f, 0.5f) is center)
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // Pixels Per Unit (standard for UI is 100, but can be adjusted)
            float pixelsPerUnit = 100.0f;

            Sprite newSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
            newSprite.name = "Minimap_SquareSprite";

            return newSprite;
        }

        public static Sprite CreateCircleSprite(int diameter, Color color, int resolutionMultiplier = 1, int featherWidth = 0, bool featherInside = true)
        {
            // MelonLogger.Msg("Utils: Creating circle sprite."); // Removed MelonLogger call
            var texSize = diameter * resolutionMultiplier;
            var texture = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            var clear = new Color(0f, 0f, 0f, 0f);
            for (var y = 0; y < texSize; y++)
                for (var x = 0; x < texSize; x++)
                    texture.SetPixel(x, y, clear);

            var radius = texSize / 2f;
            var center = new Vector2(radius, radius);
            var effectiveFeather = Mathf.Max(0, featherWidth * resolutionMultiplier);

            for (var y = 0; y < texSize; y++)
            {
                for (var x = 0; x < texSize; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = 0f;
                    if (featherInside)
                    {
                        if (!(dist <= radius)) continue;
                        alpha = color.a;
                        if (effectiveFeather > 0)
                        {
                            var edgeDist = radius - dist;
                            if (edgeDist <= effectiveFeather)
                            {
                                alpha *= Mathf.Clamp01(edgeDist / effectiveFeather);
                            }
                        }
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        if (dist <= radius)
                        {
                            alpha = color.a;
                            texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                        }
                        else if (dist > radius && dist <= radius + effectiveFeather)
                        {
                            var edgeDist = dist - radius;
                            alpha = color.a * Mathf.Clamp01(1f - (edgeDist / effectiveFeather));
                            texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                        }
                    }
                }
            }

            texture.Apply();
            // MelonLogger.Msg("Utils: Circle sprite created."); // Removed MelonLogger call
            return Sprite.Create(texture, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), resolutionMultiplier);
        }
    }
}

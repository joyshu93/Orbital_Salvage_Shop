using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurioClerk.Presentation
{
    internal static class VisualAssetLibrary
    {
        private const int IconSize = 48;
        private static readonly Dictionary<string, Sprite> ArtifactSprites =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static Sprite s_DeskBackground;
        private static Sprite s_RepairIcon;
        private static Sprite s_StorageIcon;
        private static Sprite s_VaultIcon;
        private static Sprite s_HoldIcon;

        internal static Sprite DeskBackground
        {
            get
            {
                if (s_DeskBackground == null)
                {
                    s_DeskBackground = Resources.Load<Sprite>("Art/Desk/occult-desk-background");
                }

                return s_DeskBackground;
            }
        }

        internal static Sprite Artifact(string artifactId)
        {
            if (string.IsNullOrEmpty(artifactId))
            {
                return null;
            }

            if (!ArtifactSprites.TryGetValue(artifactId, out var sprite))
            {
                sprite = Resources.Load<Sprite>("Art/Artifacts/" + artifactId);
                ArtifactSprites.Add(artifactId, sprite);
            }

            return sprite;
        }

        internal static Sprite RepairIcon => s_RepairIcon ?? (s_RepairIcon = CreateIcon("repair", DrawRepair));

        internal static Sprite StorageIcon => s_StorageIcon ?? (s_StorageIcon = CreateIcon("storage", DrawStorage));

        internal static Sprite VaultIcon => s_VaultIcon ?? (s_VaultIcon = CreateIcon("vault", DrawVault));

        internal static Sprite HoldIcon => s_HoldIcon ?? (s_HoldIcon = CreateIcon("hold", DrawHold));

        private static Sprite CreateIcon(string name, Action<Color32[]> draw)
        {
            var pixels = new Color32[IconSize * IconSize];
            draw(pixels);
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
            {
                name = name + "-icon-texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, IconSize, IconSize), new Vector2(0.5f, 0.5f), IconSize);
            sprite.name = name + "-icon";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static void DrawRepair(Color32[] pixels)
        {
            Line(pixels, 14, 10, 34, 30, 4);
            Line(pixels, 29, 34, 39, 24, 5);
            Line(pixels, 25, 38, 35, 28, 5);
        }

        private static void DrawStorage(Color32[] pixels)
        {
            Rect(pixels, 10, 15, 38, 36, 3);
            Line(pixels, 10, 22, 38, 22, 3);
            Line(pixels, 15, 11, 33, 11, 3);
            Line(pixels, 15, 11, 10, 15, 3);
            Line(pixels, 33, 11, 38, 15, 3);
        }

        private static void DrawVault(Color32[] pixels)
        {
            Rect(pixels, 11, 21, 37, 38, 3);
            Arc(pixels, 24, 21, 10, 0, 180, 3);
            Circle(pixels, 24, 29, 2);
            Line(pixels, 24, 31, 24, 35, 2);
        }

        private static void DrawHold(Color32[] pixels)
        {
            Line(pixels, 13, 9, 35, 9, 3);
            Line(pixels, 13, 39, 35, 39, 3);
            Line(pixels, 15, 11, 33, 37, 3);
            Line(pixels, 33, 11, 15, 37, 3);
            Line(pixels, 19, 31, 29, 31, 3);
        }

        private static void Rect(Color32[] pixels, int left, int bottom, int right, int top, int thickness)
        {
            Line(pixels, left, bottom, right, bottom, thickness);
            Line(pixels, right, bottom, right, top, thickness);
            Line(pixels, right, top, left, top, thickness);
            Line(pixels, left, top, left, bottom, thickness);
        }

        private static void Circle(Color32[] pixels, int centerX, int centerY, int radius)
        {
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        SetPixel(pixels, centerX + x, centerY + y);
                    }
                }
            }
        }

        private static void Arc(Color32[] pixels, int centerX, int centerY, int radius, int startDegrees, int endDegrees, int thickness)
        {
            for (var degrees = startDegrees; degrees <= endDegrees; degrees += 2)
            {
                var radians = degrees * Mathf.Deg2Rad;
                var x = centerX + Mathf.RoundToInt(Mathf.Cos(radians) * radius);
                var y = centerY + Mathf.RoundToInt(Mathf.Sin(radians) * radius);
                Stamp(pixels, x, y, thickness);
            }
        }

        private static void Line(Color32[] pixels, int x0, int y0, int x1, int y1, int thickness)
        {
            var dx = Mathf.Abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -Mathf.Abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var error = dx + dy;
            while (true)
            {
                Stamp(pixels, x0, y0, thickness);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                var twiceError = 2 * error;
                if (twiceError >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (twiceError <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private static void Stamp(Color32[] pixels, int centerX, int centerY, int thickness)
        {
            var radius = Mathf.Max(1, thickness / 2);
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    SetPixel(pixels, centerX + x, centerY + y);
                }
            }
        }

        private static void SetPixel(Color32[] pixels, int x, int y)
        {
            if (x >= 0 && x < IconSize && y >= 0 && y < IconSize)
            {
                pixels[y * IconSize + x] = new Color32(255, 255, 255, 255);
            }
        }
    }
}

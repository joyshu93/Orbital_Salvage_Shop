using UnityEditor;

namespace CurioClerk.Editor
{
    internal sealed class VisualAssetImporter : AssetPostprocessor
    {
        private const string VisualAssetRoot = "Assets/Resources/Art/";

        [InitializeOnLoadMethod]
        private static void EnsureVisualAssetsAreSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { VisualAssetRoot.TrimEnd('/') }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && Configure(importer, path))
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(VisualAssetRoot, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            Configure(importer, assetPath);
        }

        private static bool Configure(TextureImporter importer, string path)
        {
            var maxTextureSize = MaximumTextureSize(path);
            var changed = importer.textureType != TextureImporterType.Sprite ||
                          importer.spriteImportMode != SpriteImportMode.Single ||
                          !importer.alphaIsTransparency ||
                          importer.mipmapEnabled ||
                          importer.filterMode != UnityEngine.FilterMode.Bilinear ||
                          importer.wrapMode != UnityEngine.TextureWrapMode.Clamp ||
                          importer.textureCompression != TextureImporterCompression.Compressed ||
                          importer.maxTextureSize != maxTextureSize;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = maxTextureSize;
            return changed;
        }

        private static int MaximumTextureSize(string path)
        {
            if (path.Contains("/Desk/") || path.Contains("/Effects/"))
            {
                return 2048;
            }

            return path.Contains("/Characters/") ? 1024 : 512;
        }
    }
}

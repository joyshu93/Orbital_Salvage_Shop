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
            var maxTextureSize = path.Contains("/Desk/") ? 2048 : 512;
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
    }
}

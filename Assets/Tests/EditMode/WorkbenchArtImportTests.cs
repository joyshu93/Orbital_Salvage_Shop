using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CurioClerk.Tests.EditMode
{
    public sealed class WorkbenchArtImportTests
    {
        [TestCase("workbench-states", 1536, 1024, 2048)]
        [TestCase("watch-discovery-states", 2172, 724, 4096)]
        public void ConfigureWorkbenchArt_RestoresNativeResolutionFromInheritedLowPlatformLimit(
            string name, int width, int height, int maximumSize)
        {
            var atlasPath = "Assets/Resources/Art/Workbench/" + name + ".png";
            var importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            var originalDefault = importer.GetDefaultPlatformTextureSettings();
            var originalAndroid = importer.GetPlatformTextureSettings("Android");

            try
            {
                // Reproduce the imported atlas settings found in the first Android candidate.
                // Keep this independent of whether BuildAll has already repaired the asset.
                var lowDefault = importer.GetDefaultPlatformTextureSettings();
                lowDefault.maxTextureSize = 512;
                importer.SetPlatformTextureSettings(lowDefault);
                var inheritedAndroid = importer.GetPlatformTextureSettings("Android");
                inheritedAndroid.overridden = false;
                importer.SetPlatformTextureSettings(inheritedAndroid);
                importer.SaveAndReimport();

                var builder = Type.GetType("CurioClerk.Editor.ProjectBuilder, CurioClerk.Editor", true);
                var configure = builder.GetMethod("ConfigureWorkbenchArt", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(configure, Is.Not.Null);
                configure.Invoke(null, null);

                importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
                var defaults = importer.GetDefaultPlatformTextureSettings();
                var android = importer.GetPlatformTextureSettings("Android");
                var effectiveAndroidLimit = android.overridden ? android.maxTextureSize : defaults.maxTextureSize;
                var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                Assert.That(atlas, Is.Not.Null);
                Assert.That(new[] { atlas.width, atlas.height, defaults.maxTextureSize, effectiveAndroidLimit },
                    Is.EqualTo(new[] { width, height, maximumSize, maximumSize }),
                    "Each workbench atlas must retain its native pixels; Android must inherit the full-size import limit.");
            }
            finally
            {
                importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
                importer.SetPlatformTextureSettings(originalDefault);
                importer.SetPlatformTextureSettings(originalAndroid);
                importer.SaveAndReimport();
            }
        }
    }
}

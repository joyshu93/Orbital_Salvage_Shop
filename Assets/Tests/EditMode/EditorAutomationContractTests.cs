using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CurioClerk.Tests.EditMode
{
    public sealed class EditorAutomationContractTests
    {
        [Test]
        public void ProjectBuilder_ExposesBatchEntryPoints()
        {
            var type = FindType("CurioClerk.Editor.ProjectBuilder");

            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("BuildAll", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(type.GetMethod("BuildAndroid", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(type.GetMethod("BuildAndroidOfflineQa", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null);
            Assert.That(type.GetMethod("GetOfflineQaGraphicsApis", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null);
            Assert.That(type.GetMethod("ResolveAndroidToolchainRoots", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null);
            Assert.That(type.GetMethod("ValidateReleaseEnvironment", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null);
            Assert.That(type.GetMethod("ValidateServiceIds", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        [Test]
        public void ProjectBuilder_OfflineQaGraphicsApis_UseOpenGles3Only()
        {
            var type = FindType("CurioClerk.Editor.ProjectBuilder");
            Assert.That(type, Is.Not.Null);
            var method = type.GetMethod(
                "GetOfflineQaGraphicsApis",
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var graphicsApis = method.Invoke(null, null) as GraphicsDeviceType[];

            Assert.That(graphicsApis, Is.EqualTo(new[] { GraphicsDeviceType.OpenGLES3 }));
        }

        [Test]
        public void ProjectBuilder_ResolveAndroidToolchainRoots_FallsBackToApprovedExternalLayout()
        {
            var directory = Path.Combine(Path.GetTempPath(), "curio-android-roots-" + Guid.NewGuid().ToString("N"));
            var localApplicationData = Path.Combine(directory, "local");
            var userProfile = Path.Combine(directory, "user");
            var sdk = Path.Combine(localApplicationData, "Android", "Sdk");
            var ndk = Path.Combine(sdk, "ndk", "27.2.12479018");
            var jdk = Path.Combine(userProfile, "UnityPersonal", "OpenJDK17", "jdk-17.0.20.1+1");
            var environmentNames = new[]
            {
                "CURIO_ANDROID_SDK_ROOT",
                "CURIO_ANDROID_NDK_ROOT",
                "CURIO_ANDROID_JDK_ROOT",
                "LOCALAPPDATA",
                "USERPROFILE"
            };
            var originalValues = new string[environmentNames.Length];

            try
            {
                WriteAndroidToolchainFixture(sdk, ndk, jdk);
                for (var index = 0; index < environmentNames.Length; index++)
                {
                    originalValues[index] = Environment.GetEnvironmentVariable(environmentNames[index]);
                }

                Environment.SetEnvironmentVariable("CURIO_ANDROID_SDK_ROOT", null);
                Environment.SetEnvironmentVariable("CURIO_ANDROID_NDK_ROOT", null);
                Environment.SetEnvironmentVariable("CURIO_ANDROID_JDK_ROOT", null);
                Environment.SetEnvironmentVariable("LOCALAPPDATA", localApplicationData);
                Environment.SetEnvironmentVariable("USERPROFILE", userProfile);

                var type = FindType("CurioClerk.Editor.ProjectBuilder");
                Assert.That(type, Is.Not.Null);
                var method = type.GetMethod("ResolveAndroidToolchainRoots", BindingFlags.Public | BindingFlags.Static);
                Assert.That(method, Is.Not.Null);
                var roots = method.Invoke(null, new object[] { Path.Combine(directory, "missing-bundled") }) as string[];

                Assert.That(roots, Is.EqualTo(new[] { sdk, ndk, jdk }));
            }
            finally
            {
                for (var index = 0; index < environmentNames.Length; index++)
                {
                    Environment.SetEnvironmentVariable(environmentNames[index], originalValues[index]);
                }

                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        [Test]
        public void RuntimeAssembly_ReferencesGoogleMobileAdsCoreForAndroidPlayerCompilation()
        {
            var asmdefPath = Path.Combine(
                Application.dataPath,
                "Scripts",
                "Runtime",
                "CurioClerk.Runtime.asmdef");
            var asmdef = JsonUtility.FromJson<AssemblyDefinitionContract>(File.ReadAllText(asmdefPath));

            Assert.That(asmdef.overrideReferences, Is.True);
            Assert.That(
                asmdef.precompiledReferences,
                Does.Contain("GoogleMobileAds.Core.dll"),
                "Android Player compilation requires the assembly that defines Reward and AdRequest.");
        }

        [Test]
        public void ProjectBuilder_ConfiguresApprovedNarrativeArtForMobileSprites()
        {
            var type = FindType("CurioClerk.Editor.ProjectBuilder");
            Assert.That(type, Is.Not.Null);
            Assert.That(
                type.GetMethod("ConfigureNarrativeArtAssets", BindingFlags.NonPublic | BindingFlags.Static),
                Is.Not.Null,
                "BuildAll must own deterministic narrative-art import settings.");

            var builderSource = File.ReadAllText(
                Path.Combine(Application.dataPath, "Scripts", "Editor", "ProjectBuilder.cs"));
            Assert.That(builderSource, Does.Contain("ConfigureNarrativeArtAssets();"));

            AssertNarrativeSpriteImporter(
                "Assets/Resources/Art/Characters/senior-clerk-neutral.png",
                1024);
            AssertNarrativeSpriteImporter(
                "Assets/Resources/Art/Characters/senior-clerk-concerned.png",
                1024);
            AssertNarrativeSpriteImporter(
                "Assets/Resources/Art/Characters/senior-clerk-alert.png",
                1024);
            AssertNarrativeSpriteImporter(
                "Assets/Resources/Art/Characters/senior-clerk-relieved.png",
                1024);
            AssertNarrativeSpriteImporter(
                "Assets/Resources/Art/Effects/frost-overlay.png",
                2048);
        }

        [Test]
        public void ProjectBuilder_ValidateServiceIds_AcceptsExactLiveIdShapes()
        {
            Assert.That(() => ValidateServiceIds(
                "ca-app-pub-1234567890123456~1234567890",
                "ca-app-pub-1234567890123456/1234567890"), Throws.Nothing);
        }

        [TestCase(null, "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-1234567890123456~1234567890", " ")]
        [TestCase(" ca-app-pub-1234567890123456~1234567890", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-1234567890123456~1234567890", "ca-app-pub-1234567890123456/1234567890 ")]
        [TestCase("ca-app-pub-1234567890123456~", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-1234567890123456/1234567890", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-1234567890123456~1234567890", "ca-app-pub-1234567890123456~1234567890")]
        [TestCase("ca-app-pub-١٢٣٤٥٦٧٨٩٠١٢٣٤٥٦~1234567890", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-3940256099942544~3347511713", "ca-app-pub-1234567890123456/1234567890")]
        [TestCase("ca-app-pub-1234567890123456~1234567890", "ca-app-pub-3940256099942544/5224354917")]
        public void ProjectBuilder_ValidateServiceIds_RejectsUnsafeValuesWithoutDisclosingThem(
            string appId,
            string rewardedId)
        {
            var exception = Assert.Throws<TargetInvocationException>(() => ValidateServiceIds(appId, rewardedId));
            Assert.That(exception.InnerException, Is.Not.Null);
            if (!string.IsNullOrWhiteSpace(appId))
            {
                Assert.That(exception.InnerException.Message, Does.Not.Contain(appId));
            }

            if (!string.IsNullOrWhiteSpace(rewardedId))
            {
                Assert.That(exception.InnerException.Message, Does.Not.Contain(rewardedId));
            }
        }

        [Test]
        public void ReleaseBuildManifest_ExposesWriteEntryPoint()
        {
            var type = FindType("CurioClerk.Editor.ReleaseBuildManifest");

            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("Write", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        [Test]
        public void ReleaseBuildManifest_Write_RejectsMissingAndEmptyAabWithoutDisclosingPath()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.aab");
            AssertManifestWriteRejectedWithoutPath(missingPath);

            var emptyPath = Path.GetTempFileName();
            try
            {
                AssertManifestWriteRejectedWithoutPath(emptyPath);
            }
            finally
            {
                File.Delete(emptyPath);
            }
        }

        [Test]
        public void ReleaseBuildManifest_Write_EmitsOnlyPublicPinnedMetadataAndUppercaseAabHash()
        {
            var directory = Path.Combine(Path.GetTempPath(), "curio-manifest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var aabPath = Path.Combine(directory, "CurioClerk.aab");
                File.WriteAllBytes(aabPath, new byte[] { 1, 2, 3 });

                InvokeManifestWrite(aabPath);

                var manifestPath = Path.Combine(directory, "CurioClerk-build.json");
                Assert.That(File.Exists(manifestPath), Is.True);
                var manifestJson = File.ReadAllText(manifestPath);
                var manifest = JsonUtility.FromJson<BuildManifestContract>(manifestJson);
                Assert.That(manifest.product, Is.EqualTo("Curio Clerk: Night Shift"));
                Assert.That(manifest.packageId, Is.EqualTo("com.joyshu93.curioclerknightshift"));
                Assert.That(manifest.versionName, Is.EqualTo("1.0.0"));
                Assert.That(manifest.versionCode, Is.EqualTo(10000));
                Assert.That(manifest.unityVersion, Is.EqualTo("6000.3.21f1"));
                Assert.That(manifest.minimumApi, Is.EqualTo(29));
                Assert.That(manifest.targetApi, Is.EqualTo(36));
                Assert.That(manifest.architecture, Is.EqualTo("ARM64"));
                Assert.That(manifest.backend, Is.EqualTo("IL2CPP"));
                Assert.That(manifest.aabSha256,
                    Is.EqualTo("039058C6F2C0CB492C533B0A4D14EF77CC0F78ABCCCED5287D84A1A2011CFB81"));
                Assert.That(manifestJson, Does.Not.Contain(directory));
                Assert.That(manifestJson, Does.Not.Contain("ca-app-pub-"));
                Assert.That(manifestJson, Does.Not.Contain("keystore"));
                Assert.That(manifestJson, Does.Not.Contain("password"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ContentValidator_ExposesAThrowingBuildGate()
        {
            var type = FindType("CurioClerk.Editor.ContentValidator");

            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("ValidateOrThrow", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        [Test]
        public void ReleaseConfiguration_PinsGalaxyStoreVersionAndAndroidContract()
        {
            var type = FindType("CurioClerk.Editor.ReleaseConfiguration");
            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetField("UnityVersion").GetRawConstantValue(), Is.EqualTo("6000.3.21f1"));
            Assert.That(type.GetField("VersionName").GetRawConstantValue(), Is.EqualTo("1.0.0"));
            Assert.That(type.GetField("VersionCode").GetRawConstantValue(), Is.EqualTo(10000));
            Assert.That(type.GetField("PackageId").GetRawConstantValue(),
                Is.EqualTo("com.joyshu93.curioclerknightshift"));
            Assert.That(type.GetField("MinimumApi").GetRawConstantValue(), Is.EqualTo(29));
            Assert.That(type.GetField("TargetApi").GetRawConstantValue(), Is.EqualTo(36));
            Assert.That(type.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void ValidateServiceIds(string appId, string rewardedId)
        {
            var type = FindType("CurioClerk.Editor.ProjectBuilder");
            Assert.That(type, Is.Not.Null);
            var method = type.GetMethod("ValidateServiceIds", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { appId, rewardedId });
        }

        private static void AssertNarrativeSpriteImporter(string path, int expectedMaxSize)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.That(sprite, Is.Not.Null, path + " must import as a Sprite.");

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null, path + " must have a TextureImporter.");
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.maxTextureSize, Is.EqualTo(expectedMaxSize));
        }

        private static void WriteAndroidToolchainFixture(string sdk, string ndk, string jdk)
        {
            WriteFixtureFile(Path.Combine(sdk, "platforms", "android-36", "android.jar"));
            WriteFixtureFile(Path.Combine(sdk, "build-tools", "36.0.0", "aapt2.exe"));
            WriteFixtureFile(Path.Combine(sdk, "platform-tools", "adb.exe"));
            WriteFixtureFile(Path.Combine(sdk, "cmdline-tools", "16.0", "bin", "sdkmanager.bat"));
            WriteFixtureFile(Path.Combine(sdk, "cmake", "3.22.1", "bin", "cmake.exe"));
            WriteFixtureFile(Path.Combine(ndk, "ndk-build.cmd"));
            WriteFixtureFile(Path.Combine(jdk, "bin", "java.exe"));
        }

        private static void WriteFixtureFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                      throw new InvalidOperationException("Fixture path has no parent."));
            File.WriteAllText(path, string.Empty);
        }

        private static void AssertManifestWriteRejectedWithoutPath(string path)
        {
            var type = FindType("CurioClerk.Editor.ReleaseBuildManifest");
            Assert.That(type, Is.Not.Null);
            var method = type.GetMethod("Write", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { path }));
            Assert.That(exception.InnerException, Is.Not.Null);
            Assert.That(exception.InnerException.Message, Does.Not.Contain(path));
        }

        private static void InvokeManifestWrite(string path)
        {
            var type = FindType("CurioClerk.Editor.ReleaseBuildManifest");
            Assert.That(type, Is.Not.Null);
            var method = type.GetMethod("Write", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { path });
        }

        [Serializable]
        private sealed class BuildManifestContract
        {
            public string product = string.Empty;
            public string packageId = string.Empty;
            public string versionName = string.Empty;
            public int versionCode = default;
            public string unityVersion = string.Empty;
            public int minimumApi = default;
            public int targetApi = default;
            public string architecture = string.Empty;
            public string backend = string.Empty;
            public string aabSha256 = string.Empty;
        }

        [Serializable]
        private sealed class AssemblyDefinitionContract
        {
            public bool overrideReferences = default;
            public string[] precompiledReferences = Array.Empty<string>();
        }
    }
}

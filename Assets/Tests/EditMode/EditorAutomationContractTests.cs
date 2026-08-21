using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

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
            Assert.That(type.GetMethod("ValidateReleaseEnvironment", BindingFlags.Public | BindingFlags.Static),
                Is.Not.Null);
            Assert.That(type.GetMethod("ValidateServiceIds", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
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
            if (!string.IsNullOrEmpty(appId))
            {
                Assert.That(exception.InnerException.Message, Does.Not.Contain(appId));
            }

            if (!string.IsNullOrEmpty(rewardedId))
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
    }
}

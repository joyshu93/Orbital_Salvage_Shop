using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CurioClerk.Tests.EditMode
{
    public sealed class AndroidBuildStateContractTests
    {
        private const string AdsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        private const string ServicePath = "Assets/Resources/ServiceConfiguration.asset";
        private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();
        private bool _bundle;
        private bool _customSigning;
        private string _keystore, _keystorePass, _alias, _aliasPass;

        private static Type Builder => Type.GetType("CurioClerk.Editor.ProjectBuilder, CurioClerk.Editor", true);

        [SetUp]
        public void PreserveLocalSettings()
        {
            _bundle = EditorUserBuildSettings.buildAppBundle;
            _customSigning = PlayerSettings.Android.useCustomKeystore;
            _keystore = PlayerSettings.Android.keystoreName;
            _keystorePass = PlayerSettings.Android.keystorePass;
            _alias = PlayerSettings.Android.keyaliasName;
            _aliasPass = PlayerSettings.Android.keyaliasPass;
            foreach (var path in new[] { AdsPath, AdsPath + ".meta", ServicePath, ServicePath + ".meta" })
                _files[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;
            AssetDatabase.DeleteAsset(AdsPath);
            AssetDatabase.DeleteAsset(ServicePath);
        }

        [TearDown]
        public void RestoreLocalSettings()
        {
            EditorUserBuildSettings.buildAppBundle = _bundle;
            PlayerSettings.Android.useCustomKeystore = _customSigning;
            PlayerSettings.Android.keystoreName = _keystore;
            PlayerSettings.Android.keystorePass = _keystorePass;
            PlayerSettings.Android.keyaliasName = _alias;
            PlayerSettings.Android.keyaliasPass = _aliasPass;
            AssetDatabase.DeleteAsset(AdsPath);
            AssetDatabase.DeleteAsset(ServicePath);
            foreach (var pair in _files)
                if (pair.Value != null) File.WriteAllBytes(pair.Key, pair.Value);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            _files.Clear();
        }

        [Test]
        public void DevelopmentSnapshot_DoesNotCreateSettingsBeforeProtectedConfiguration()
        {
            var type = Builder.GetNestedType("AndroidDevelopmentStateScope", BindingFlags.NonPublic);
            using ((IDisposable)Activator.CreateInstance(type, true))
            {
                Assert.That(File.Exists(AdsPath), Is.False,
                    "Taking a snapshot must not create an asset before the build's try/finally owns it.");
                Assert.That(File.Exists(ServicePath), Is.False);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DevelopmentConfiguration_RestoresPreGenerationBundleAndSigning(bool failGeneration)
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = "synthetic-qa-password";
            var method = Builder.GetMethod("ConfigureAndroidDevelopmentState", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method.GetParameters().Length, Is.EqualTo(1),
                "The protected scope must own asset generation so it can restore settings changed by BuildAll.");
            Action generate = () =>
            {
                // The real setting mutation performed by BuildAll's ReleaseConfiguration.Apply.
                EditorUserBuildSettings.buildAppBundle = true;
                if (failGeneration) throw new InvalidOperationException("synthetic generation failure");
            };
            IDisposable scope = null;
            try
            {
                if (failGeneration)
                {
                    var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { generate }));
                    Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
                }
                else
                {
                    scope = (IDisposable)method.Invoke(null, new object[] { generate });
                    Assert.That(EditorUserBuildSettings.buildAppBundle, Is.False);
                    Assert.That(PlayerSettings.Android.useCustomKeystore, Is.False);
                    Assert.That(PlayerSettings.Android.keystorePass, Is.Empty);
                    Assert.That(ReadString(AdsPath, "adMobAndroidAppId"), Is.EqualTo("ca-app-pub-3940256099942544~3347511713"));
                    Assert.That(ReadString(ServicePath, "_androidRewardedAdUnitId"), Is.EqualTo("ca-app-pub-3940256099942544/5224354917"));
                }
            }
            finally { scope?.Dispose(); }
            Assert.That(EditorUserBuildSettings.buildAppBundle, Is.False);
            Assert.That(PlayerSettings.Android.useCustomKeystore, Is.True);
            Assert.That(PlayerSettings.Android.keystorePass, Is.EqualTo("synthetic-qa-password"));
            Assert.That(File.Exists(AdsPath), Is.False);
            Assert.That(File.Exists(ServicePath), Is.False);
        }

        [Test]
        public void DevelopmentConfiguration_RestoresExistingServiceIdsAcrossRepeatedAssetReloads()
        {
            Builder.GetMethod("ConfigureServiceAssets", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { "synthetic-original-app", "synthetic-original-reward" });
            var configure = Builder.GetMethod("ConfigureAndroidDevelopmentState", BindingFlags.Static | BindingFlags.NonPublic);
            for (var attempt = 0; attempt < 2; attempt++)
            {
                using ((IDisposable)configure.Invoke(null, new object[] { (Action)(() => { }) }))
                {
                    Assert.That(ReadString(AdsPath, "adMobAndroidAppId"), Is.EqualTo("ca-app-pub-3940256099942544~3347511713"));
                    AssetDatabase.ImportAsset(AdsPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.ImportAsset(ServicePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }
                Assert.That(ReadString(AdsPath, "adMobAndroidAppId"), Is.EqualTo("synthetic-original-app"));
                Assert.That(ReadString(ServicePath, "_androidRewardedAdUnitId"), Is.EqualTo("synthetic-original-reward"));
            }
        }

        [Test]
        public void ExternalToolsScope_RestoresEmbeddedFlagsAndRememberedMissingCustomPaths()
        {
            var settingsType = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions", true);
            var roots = new List<RootSnapshot>();
            var stop = settingsType.GetProperty("stopGradleDaemonsOnExit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var originalStop = stop.GetValue(null);
            try
            {
                foreach (var name in new[] { "AndroidSDKRoot", "AndroidNDKRoot", "AndroidJavaRoot" })
                {
                    var type = settingsType.Assembly.GetType("UnityEditor.Android." + name, true);
                    var root = type.GetMethod("GetInstance", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                    var snapshot = new RootSnapshot(root);
                    roots.Add(snapshot);
                    snapshot.Set("UseEmbedded", true);
                    snapshot.Set("CustomDirectory", Path.Combine(Path.GetTempPath(), "curio-remembered-missing-" + name));
                }
                var scopeType = Builder.GetNestedType("AndroidExternalToolsScope", BindingFlags.NonPublic);
                var scope = (IDisposable)Activator.CreateInstance(scopeType, new object[] { settingsType });
                try
                {
                    foreach (var root in roots)
                    {
                        root.Set("UseEmbedded", false);
                        root.Set("CustomDirectory", "temporary-build-value");
                    }
                    stop.SetValue(null, !(bool)originalStop);
                }
                finally { scope.Dispose(); }
                foreach (var root in roots)
                {
                    Assert.That(root.Get("UseEmbedded"), Is.True);
                    Assert.That((string)root.Get("CustomDirectory"), Does.Contain("curio-remembered-missing-"));
                }
                Assert.That(stop.GetValue(null), Is.EqualTo(originalStop));
            }
            finally
            {
                foreach (var root in roots) root.Restore();
                stop.SetValue(null, originalStop);
            }
        }

        private static string ReadString(string path, string field)
            => new SerializedObject(AssetDatabase.LoadMainAssetAtPath(path)).FindProperty(field).stringValue;

        private sealed class RootSnapshot
        {
            private readonly object _root;
            private readonly object _embedded, _directory;
            private readonly string _embeddedKey, _directoryKey;
            private readonly bool _hadEmbedded, _hadDirectory;
            public RootSnapshot(object root)
            {
                _root = root;
                _embedded = Get("UseEmbedded");
                _directory = Get("CustomDirectory");
                _embeddedKey = (string)Get("EmbeddedPreferenceKey");
                _directoryKey = (string)Get("DirectoryPreferenceKey");
                _hadEmbedded = EditorPrefs.HasKey(_embeddedKey);
                _hadDirectory = EditorPrefs.HasKey(_directoryKey);
            }
            public object Get(string name) => Property(name).GetValue(_root);
            public void Set(string name, object value) => Property(name).SetValue(_root, value);
            private PropertyInfo Property(string name) => _root.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            public void Restore()
            {
                Set("CustomDirectory", _directory);
                Set("UseEmbedded", _embedded);
                if (!_hadEmbedded) EditorPrefs.DeleteKey(_embeddedKey);
                if (!_hadDirectory) EditorPrefs.DeleteKey(_directoryKey);
            }
        }
    }
}

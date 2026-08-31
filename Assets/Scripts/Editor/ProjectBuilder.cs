using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CurioClerk.Content;
using CurioClerk.Infrastructure;
using CurioClerk.Localization;
using CurioClerk.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Platform.Android;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using TMPro;

namespace CurioClerk.Editor
{
    public static class ProjectBuilder
    {
        private const string ContentRoot = "Assets/Resources/Content";
        private const string LocalizationRoot = "Assets/Localization";
        private const string RenderingRoot = "Assets/Rendering";
        private const string SceneRoot = "Assets/Scenes";
        private const string NarrativeCharacterRoot = "Assets/Resources/Art/Characters";
        private const string NarrativeEffectRoot = "Assets/Resources/Art/Effects";
        private static readonly string[] NarrativePortraitPaths =
        {
            NarrativeCharacterRoot + "/senior-clerk-neutral.png",
            NarrativeCharacterRoot + "/senior-clerk-concerned.png",
            NarrativeCharacterRoot + "/senior-clerk-alert.png",
            NarrativeCharacterRoot + "/senior-clerk-relieved.png"
        };
        private const string FrostOverlayPath = NarrativeEffectRoot + "/frost-overlay.png";
        private const string ServiceConfigurationPath = "Assets/Resources/ServiceConfiguration.asset";
        private const string GoogleMobileAdsSettingsPath =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        private const string GoogleSampleAppId = "ca-app-pub-3940256099942544~3347511713";
        private const string GoogleSampleRewardedId = "ca-app-pub-3940256099942544/5224354917";
        private static readonly Regex AppIdPattern =
            new Regex(@"\Aca-app-pub-[0-9]+~[0-9]+\z", RegexOptions.CultureInvariant);
        private static readonly Regex RewardedIdPattern =
            new Regex(@"\Aca-app-pub-[0-9]+/[0-9]+\z", RegexOptions.CultureInvariant);
        private static readonly string[] ReleaseSecretEnvironmentNames =
        {
            "CURIO_ADMOB_APP_ID",
            "CURIO_ADMOB_REWARDED_ID",
            "CURIO_ANDROID_KEYSTORE_PATH",
            "CURIO_ANDROID_KEYSTORE_PASS",
            "CURIO_ANDROID_KEY_ALIAS",
            "CURIO_ANDROID_KEY_PASS"
        };

        [MenuItem("Tools/Curio Clerk/Generate Project Assets")]
        public static void BuildAll()
        {
            ConfigurePlayer();
            EnsureFolders();
            ConfigureBrandAssets();
            ConfigureNarrativeArtAssets();
            ConfigureFontAssets();
            CreateContentAssets();
            CreateLocalizationAssets();
            CreateRenderingAssets();
            CreateScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ContentValidator.ValidateOrThrow();
            Debug.Log("Curio Clerk project assets and settings generated successfully.");
        }

        [MenuItem("Tools/Curio Clerk/Build Android AAB")]
        public static void BuildAndroid()
        {
            ReleaseEnvironment environment = null;
            try
            {
                ValidateUnityVersion();
                RunReleaseNoRemoteTelemetryGate();
                environment = ReadAndValidateReleaseEnvironment();
                ConfigureServiceAssets(environment.AdMobAppId, environment.AdMobRewardedId);
                ConfigureAndroidExternalTools();
                BuildAll();
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                EditorUserBuildSettings.buildAppBundle = true;

                var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Android/CurioClerk.aab"));
                Directory.CreateDirectory(Path.GetDirectoryName(output) ??
                                          throw new InvalidOperationException("Invalid build output path."));

                ConfigureReleaseSigning(environment);
                BuildReport report;
                try
                {
                    report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                    {
                        scenes = new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" },
                        locationPathName = output,
                        target = BuildTarget.Android,
                        options = BuildOptions.None
                    });
                }
                finally
                {
                    ClearReleaseSecrets(environment);
                }

                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new BuildFailedException($"Android build failed with {report.summary.totalErrors} errors.");
                }

                ReleaseBuildManifest.Write(output);
                Debug.Log($"Android AAB and sanitized manifest built successfully ({report.summary.totalSize} bytes).");
            }
            finally
            {
                ClearReleaseSecrets(environment);
            }
        }

        public static void ValidateReleaseEnvironment()
        {
            ReleaseEnvironment environment = null;
            try
            {
                environment = ReadAndValidateReleaseEnvironment();
                ConfigureServiceAssets(environment.AdMobAppId, environment.AdMobRewardedId);
            }
            finally
            {
                ClearReleaseSecrets(environment);
            }
        }

        public static void ValidateServiceIds(string appId, string rewardedId)
        {
            if (string.IsNullOrEmpty(appId) ||
                !AppIdPattern.IsMatch(appId) ||
                string.Equals(appId, GoogleSampleAppId, StringComparison.Ordinal))
            {
                throw new BuildFailedException("The AdMob app ID is missing, malformed, or a sample value.");
            }

            if (string.IsNullOrEmpty(rewardedId) ||
                !RewardedIdPattern.IsMatch(rewardedId) ||
                string.Equals(rewardedId, GoogleSampleRewardedId, StringComparison.Ordinal))
            {
                throw new BuildFailedException("The AdMob rewarded unit ID is missing, malformed, or a sample value.");
            }
        }

        private static void ValidateUnityVersion()
        {
            if (!string.Equals(Application.unityVersion, ReleaseConfiguration.UnityVersion,
                    StringComparison.Ordinal))
            {
                throw new BuildFailedException("The release must run in the repository-pinned Unity version.");
            }
        }

        private static void RunReleaseNoRemoteTelemetryGate()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var gatePath = Path.Combine(projectRoot, "scripts", "check-no-remote-telemetry.ps1");
            var windowsPowerShell = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell",
                "v1.0",
                "powershell.exe");
            if (!File.Exists(windowsPowerShell) || !File.Exists(gatePath))
            {
                throw new BuildFailedException("The release privacy gate host or script is missing.");
            }

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = windowsPowerShell,
                Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File " +
                            QuoteProcessArgument(gatePath) + " -ProjectRoot " +
                            QuoteProcessArgument(projectRoot) + " -Mode Release",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var environmentName in ReleaseSecretEnvironmentNames)
            {
                startInfo.EnvironmentVariables.Remove(environmentName);
            }

            try
            {
                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new BuildFailedException("The release privacy gate could not start.");
                    }

                    var standardOutput = process.StandardOutput.ReadToEndAsync();
                    var standardError = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    System.Threading.Tasks.Task.WaitAll(standardOutput, standardError);
                    if (process.ExitCode != 0)
                    {
                        throw new BuildFailedException("The release no-remote-telemetry gate failed.");
                    }
                }
            }
            catch (BuildFailedException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new BuildFailedException("The release no-remote-telemetry gate could not execute.");
            }
        }

        private static string QuoteProcessArgument(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('"') >= 0)
            {
                throw new BuildFailedException("The release privacy gate path is invalid.");
            }

            return '"' + value + '"';
        }

        private static ReleaseEnvironment ReadAndValidateReleaseEnvironment()
        {
            var environment = new ReleaseEnvironment
            {
                AdMobAppId = Environment.GetEnvironmentVariable("CURIO_ADMOB_APP_ID"),
                AdMobRewardedId = Environment.GetEnvironmentVariable("CURIO_ADMOB_REWARDED_ID"),
                KeystorePath = Environment.GetEnvironmentVariable("CURIO_ANDROID_KEYSTORE_PATH"),
                KeystorePassword = Environment.GetEnvironmentVariable("CURIO_ANDROID_KEYSTORE_PASS"),
                KeyAlias = Environment.GetEnvironmentVariable("CURIO_ANDROID_KEY_ALIAS"),
                KeyPassword = Environment.GetEnvironmentVariable("CURIO_ANDROID_KEY_PASS")
            };

            try
            {
                if (string.IsNullOrWhiteSpace(environment.AdMobAppId) ||
                    string.IsNullOrWhiteSpace(environment.AdMobRewardedId) ||
                    string.IsNullOrWhiteSpace(environment.KeystorePath) ||
                    string.IsNullOrWhiteSpace(environment.KeystorePassword) ||
                    string.IsNullOrWhiteSpace(environment.KeyAlias) ||
                    string.IsNullOrWhiteSpace(environment.KeyPassword))
                {
                    throw new BuildFailedException("The six required release environment values are incomplete.");
                }

                ValidateServiceIds(environment.AdMobAppId, environment.AdMobRewardedId);
                environment.KeystorePath = Path.GetFullPath(environment.KeystorePath);

                if (!File.Exists(environment.KeystorePath))
                {
                    throw new BuildFailedException("The release keystore is missing or inaccessible.");
                }

                return environment;
            }
            catch (BuildFailedException)
            {
                environment.Clear();
                throw;
            }
            catch (Exception)
            {
                environment.Clear();
                throw new BuildFailedException("The release environment could not be validated.");
            }
        }

        private static void ConfigureServiceAssets(string appId, string rewardedId)
        {
            EnsureFolder("Assets/Resources");
            var serviceConfiguration = AssetDatabase.LoadAssetAtPath<ServiceConfiguration>(ServiceConfigurationPath);
            if (serviceConfiguration == null)
            {
                serviceConfiguration = ScriptableObject.CreateInstance<ServiceConfiguration>();
                AssetDatabase.CreateAsset(serviceConfiguration, ServiceConfigurationPath);
            }

            var serializedServiceConfiguration = new SerializedObject(serviceConfiguration);
            var rewardedIdProperty = serializedServiceConfiguration.FindProperty("_androidRewardedAdUnitId");
            if (rewardedIdProperty == null)
            {
                throw new BuildFailedException("The local service configuration schema is invalid.");
            }

            rewardedIdProperty.stringValue = rewardedId;
            serializedServiceConfiguration.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serviceConfiguration);

            var mobileAdsSettings = AssetDatabase.LoadMainAssetAtPath(GoogleMobileAdsSettingsPath);
            if (mobileAdsSettings == null)
            {
                throw new BuildFailedException("Google Mobile Ads settings are missing. Resolve the package and create its settings asset.");
            }

            var serializedMobileAdsSettings = new SerializedObject(mobileAdsSettings);
            var appIdProperty = serializedMobileAdsSettings.FindProperty("adMobAndroidAppId");
            if (appIdProperty == null || appIdProperty.propertyType != SerializedPropertyType.String)
            {
                throw new BuildFailedException("The Google Mobile Ads settings schema is unsupported.");
            }

            appIdProperty.stringValue = appId;
            serializedMobileAdsSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mobileAdsSettings);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureReleaseSigning(ReleaseEnvironment environment)
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = environment.KeystorePath;
            PlayerSettings.Android.keystorePass = environment.KeystorePassword;
            PlayerSettings.Android.keyaliasName = environment.KeyAlias;
            PlayerSettings.Android.keyaliasPass = environment.KeyPassword;
        }

        private static void ClearReleaseSigning()
        {
            PlayerSettings.Android.keystorePass = string.Empty;
            PlayerSettings.Android.keyaliasPass = string.Empty;
            PlayerSettings.Android.keystoreName = string.Empty;
            PlayerSettings.Android.keyaliasName = string.Empty;
            PlayerSettings.Android.useCustomKeystore = false;
        }

        private static void ClearReleaseSecrets(ReleaseEnvironment environment)
        {
            if (environment == null)
            {
                return;
            }

            try
            {
                ClearReleaseSigning();
            }
            finally
            {
                environment?.Clear();
            }
        }

        private static void ConfigureAndroidExternalTools()
        {
            var currentEditorAndroid = Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath) ?? string.Empty,
                "Data", "PlaybackEngines", "AndroidPlayer");
            if (!Directory.Exists(Path.Combine(currentEditorAndroid, "SDK")) ||
                !Directory.Exists(Path.Combine(currentEditorAndroid, "NDK")) ||
                !Directory.Exists(Path.Combine(currentEditorAndroid, "OpenJDK")))
            {
                throw new BuildFailedException("Unity-provided Android SDK, NDK and OpenJDK were not found. Add the three Android child modules for Unity 6000.3.21f1 in Unity Hub.");
            }

            var settingsType = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
            if (settingsType == null)
            {
                throw new BuildFailedException("AndroidExternalToolsSettings API was not loaded. Verify Android Build Support for Unity 6000.3.21f1.");
            }

            SetStaticProperty(settingsType, "sdkRootPath", Path.Combine(currentEditorAndroid, "SDK"));
            SetStaticProperty(settingsType, "ndkRootPath", Path.Combine(currentEditorAndroid, "NDK"));
            SetStaticProperty(settingsType, "jdkRootPath", Path.Combine(currentEditorAndroid, "OpenJDK"));
            SetStaticProperty(settingsType, "stopGradleDaemonsOnExit", true);
            Debug.Log($"Android external tools configured from Unity installation: {currentEditorAndroid}");
        }

        private static void SetStaticProperty(Type type, string name, object value)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property == null || !property.CanWrite)
            {
                throw new BuildFailedException($"Android tooling property '{name}' is unavailable.");
            }

            property.SetValue(null, value);
        }

        public static void LogAndroidToolingApi()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(item => item.GetName().Name.Contains("Android")))
            {
                foreach (var type in assembly.GetTypes().Where(item => item.FullName != null && item.FullName.Contains("ExternalTool")))
                {
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        .Select(property => $"{property.Name}:{property.PropertyType.FullName}");
                    Debug.Log($"ANDROID_TOOL_API {type.FullName} => {string.Join(",", properties)}");
                }
            }
        }

        private static void ConfigurePlayer()
        {
            ReleaseConfiguration.Apply();
            ConfigureInputSystemOnly();
        }

        private static void ConfigureInputSystemOnly()
        {
            var projectSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
            if (projectSettings == null)
            {
                throw new BuildFailedException("Player ProjectSettings asset could not be loaded.");
            }

            var serialized = new SerializedObject(projectSettings);
            var activeInputHandler = serialized.FindProperty("activeInputHandler");
            if (activeInputHandler == null)
            {
                throw new BuildFailedException("Active Input Handling setting could not be found.");
            }

            activeInputHandler.intValue = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ContentRoot + "/Artifacts");
            EnsureFolder(ContentRoot + "/Rules");
            EnsureFolder(ContentRoot + "/Difficulties");
            EnsureFolder(ContentRoot + "/Cosmetics");
            EnsureFolder("Assets/Resources/Fonts");
            EnsureFolder(LocalizationRoot);
            EnsureFolder(RenderingRoot);
            EnsureFolder(SceneRoot);
        }

        private static void CreateContentAssets()
        {
            foreach (var content in ContentCatalog.CreateArtifacts())
            {
                var asset = LoadOrCreate<ArtifactDefinition>($"{ContentRoot}/Artifacts/{content.Id}.asset");
                asset.Configure(content);
                EditorUtility.SetDirty(asset);
            }

            foreach (var rule in ContentCatalog.CreateRuleTemplates())
            {
                var asset = LoadOrCreate<RuleDefinition>($"{ContentRoot}/Rules/{rule.Id}.asset");
                asset.Configure(rule);
                EditorUtility.SetDirty(asset);
            }

            for (var band = 1; band <= 5; band++)
            {
                var asset = LoadOrCreate<DifficultyProfile>($"{ContentRoot}/Difficulties/band_{band}.asset");
                asset.Configure(band);
                EditorUtility.SetDirty(asset);
            }

            foreach (var content in ContentCatalog.CreateCosmetics())
            {
                var asset = LoadOrCreate<CosmeticDefinition>($"{ContentRoot}/Cosmetics/{content.Id}.asset");
                asset.Configure(content);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ConfigureBrandAssets()
        {
            const string iconPath = "Assets/Art/Brand/AppIcon.png";
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(iconPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon == null)
            {
                throw new InvalidOperationException("App icon could not be imported.");
            }

#pragma warning disable CS0618
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { icon });
#pragma warning restore CS0618
        }

        private static void ConfigureNarrativeArtAssets()
        {
            EnsureFolder(NarrativeCharacterRoot);
            EnsureFolder(NarrativeEffectRoot);
            foreach (var path in NarrativePortraitPaths)
            {
                ConfigureNarrativeArtAsset(path, 1024);
            }

            ConfigureNarrativeArtAsset(FrostOverlayPath, 2048);
        }

        private static void ConfigureNarrativeArtAsset(string path, int maxTextureSize)
        {
            if (!File.Exists(Path.GetFullPath(path)))
            {
                throw new InvalidOperationException("Required narrative art is missing: " + path);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                throw new InvalidOperationException("Narrative art could not be imported: " + path);
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = maxTextureSize;
            importer.SaveAndReimport();
        }

        private static void ConfigureFontAssets()
        {
            EnsureTextMeshProResources();
            ClearDefaultTmpSpriteAsset();
            EnsureDynamicFontAsset(
                "Assets/Fonts/NotoSansKR/NotoSansKR-Variable.ttf",
                "Assets/Resources/Fonts/NotoSansKR-Dynamic.asset",
                "NotoSansKR-Dynamic");
            EnsureDynamicFontAsset(
                "Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf",
                "Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset",
                "GowunBatang-Bold-Dynamic");
        }

        private static void ClearDefaultTmpSpriteAsset()
        {
            var tmpSettings = Resources.Load<TMP_Settings>("TMP Settings");
            if (tmpSettings == null)
            {
                throw new InvalidOperationException("TMP Settings could not be loaded after ensuring TextMesh Pro resources.");
            }

            var serializedSettings = new SerializedObject(tmpSettings);
            var defaultSpriteAsset = serializedSettings.FindProperty("m_defaultSpriteAsset");
            if (defaultSpriteAsset == null)
            {
                throw new InvalidOperationException("TMP default sprite setting could not be inspected.");
            }

            defaultSpriteAsset.objectReferenceValue = null;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tmpSettings);
        }

        private static void EnsureDynamicFontAsset(string sourcePath, string assetPath, string assetName)
        {
            AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceSynchronousImport);
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException($"Source font could not be imported: {sourcePath}");
            }

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                fontAsset.name = assetName;
                AssetDatabase.CreateAsset(fontAsset, assetPath);

                foreach (var texture in fontAsset.atlasTextures)
                {
                    if (texture != null && !AssetDatabase.Contains(texture))
                    {
                        texture.name = fontAsset.name + " Atlas";
                        AssetDatabase.AddObjectToAsset(texture, fontAsset);
                    }
                }

                if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
                {
                    fontAsset.material.name = fontAsset.name + " Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }
            }

            fontAsset.name = assetName;
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fontAsset);
        }

        private static void EnsureTextMeshProResources()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null)
            {
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                throw new InvalidOperationException("Committed TMP Essential Resources are missing from Assets/TextMesh Pro.");
            }
        }

        private static void CreateLocalizationAssets()
        {
            var settingsPath = LocalizationRoot + "/LocalizationSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Curio Clerk Localization Settings";
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            var english = GetOrCreateLocale("en", "English");
            var korean = GetOrCreateLocale("ko", "Korean");
            var locales = new List<Locale> { english, korean };
            var collection = LocalizationEditorSettings.GetStringTableCollection("UI") ??
                             LocalizationEditorSettings.CreateStringTableCollection("UI", LocalizationRoot, locales);

            WriteTable(collection, "en", Localizer.Entries("en"));
            WriteTable(collection, "ko", Localizer.Entries("ko"));

            var appInfo = LocalizationSettings.Metadata.GetMetadata<AppInfo>();
            if (appInfo == null)
            {
                appInfo = new AppInfo();
                LocalizationSettings.Metadata.AddMetadata(appInfo);
            }

            appInfo.DisplayName = new LocalizedString("UI", "title");
            EditorUtility.SetDirty(settings);
        }

        private static Locale GetOrCreateLocale(string code, string label)
        {
            var path = $"{LocalizationRoot}/{code}.asset";
            var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null)
            {
                var identifier = new LocaleIdentifier(code);
                locale = Locale.CreateLocale(identifier);
                locale.name = label + " (" + code + ")";
                AssetDatabase.CreateAsset(locale, path);
            }

            if (!LocalizationEditorSettings.GetLocales().Contains(locale))
            {
                LocalizationEditorSettings.AddLocale(locale);
            }

            return locale;
        }

        private static void WriteTable(UnityEditor.Localization.StringTableCollection collection, string localeCode,
            IEnumerable<KeyValuePair<string, string>> entries)
        {
            var table = collection.GetTable(localeCode) as StringTable;
            if (table == null)
            {
                throw new InvalidOperationException($"Localization table '{localeCode}' was not created.");
            }

            foreach (var pair in entries)
            {
                table.AddEntry(pair.Key, pair.Value);
            }

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
        }

        private static void CreateRenderingAssets()
        {
            var rendererPath = RenderingRoot + "/CurioClerk-2D-Renderer.asset";
            var pipelinePath = RenderingRoot + "/CurioClerk-URP.asset";

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
        }

        private static void CreateScenes()
        {
            EnsureScene<BootstrapLoader>(SceneRoot + "/Bootstrap.unity", "Bootstrap");
            EnsureScene<GameApp>(SceneRoot + "/Main.unity", "CurioClerkApp");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneRoot + "/Bootstrap.unity", true),
                new EditorBuildSettingsScene(SceneRoot + "/Main.unity", true)
            };
        }

        private static void EnsureScene<T>(string path, string rootName) where T : Component
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject(rootName, typeof(T));
            EditorSceneManager.SaveScene(scene, path);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string fullPath)
        {
            var parts = fullPath.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private sealed class ReleaseEnvironment
        {
            public string AdMobAppId;
            public string AdMobRewardedId;
            public string KeystorePath;
            public string KeystorePassword;
            public string KeyAlias;
            public string KeyPassword;

            public void Clear()
            {
                AdMobAppId = null;
                AdMobRewardedId = null;
                KeystorePath = null;
                KeystorePassword = null;
                KeyAlias = null;
                KeyPassword = null;
            }
        }
    }
}

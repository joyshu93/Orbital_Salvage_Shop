using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CurioClerk.Content;
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
using Unity.Android.Types;
using UnityEditor.Android;

namespace CurioClerk.Editor
{
    public static class ProjectBuilder
    {
        private const string ContentRoot = "Assets/Resources/Content";
        private const string LocalizationRoot = "Assets/Localization";
        private const string RenderingRoot = "Assets/Rendering";
        private const string SceneRoot = "Assets/Scenes";

        [MenuItem("Tools/Curio Clerk/Generate Project Assets")]
        public static void BuildAll()
        {
            ConfigurePlayer();
            EnsureFolders();
            ConfigureBrandAssets();
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
            ConfigureAndroidExternalTools();
            BuildAll();
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = true;

            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Android/CurioClerk.aab"));
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? throw new InvalidOperationException("Invalid build output path."));

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed with {report.summary.totalErrors} errors.");
            }

            Debug.Log($"Android AAB built: {output} ({report.summary.totalSize} bytes)");
        }

        private static void ConfigureAndroidExternalTools()
        {
            var currentEditorAndroid = Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath) ?? string.Empty,
                "Data", "PlaybackEngines", "AndroidPlayer");
            var sharedUnityAndroid = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Unity", "Hub", "Editor", "6000.2.7f2", "Editor", "Data", "PlaybackEngines", "AndroidPlayer");
            var root = new[] { currentEditorAndroid, sharedUnityAndroid }.FirstOrDefault(candidate =>
                Directory.Exists(Path.Combine(candidate, "SDK")) &&
                Directory.Exists(Path.Combine(candidate, "NDK")) &&
                Directory.Exists(Path.Combine(candidate, "OpenJDK")));
            if (root == null)
            {
                throw new BuildFailedException("Unity-provided Android SDK, NDK and OpenJDK were not found. Add the three Android child modules in Unity Hub.");
            }

            var settingsType = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
            if (settingsType == null)
            {
                throw new BuildFailedException("AndroidExternalToolsSettings API was not loaded. Verify Android Build Support for Unity 6000.3.21f1.");
            }

            SetStaticProperty(settingsType, "sdkRootPath", Path.Combine(root, "SDK"));
            SetStaticProperty(settingsType, "ndkRootPath", Path.Combine(root, "NDK"));
            SetStaticProperty(settingsType, "jdkRootPath", Path.Combine(root, "OpenJDK"));
            SetStaticProperty(settingsType, "stopGradleDaemonsOnExit", true);
            Debug.Log($"Android external tools configured from Unity installation: {root}");
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
            PlayerSettings.companyName = "joyshu93";
            PlayerSettings.productName = "Curio Clerk: Night Shift";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.joyshu93.curioclerknightshift");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = UnityEditor.AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            ConfigureInputSystemOnly();
            EditorUserBuildSettings.androidBuildSystem = UnityEditor.AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = true;
            UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.SymbolTable;
            UserBuildSettings.DebugSymbols.format = DebugSymbolFormat.Zip | DebugSymbolFormat.LegacyExtensions;
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

        private static void ConfigureFontAssets()
        {
            const string sourcePath = "Assets/Fonts/NotoSansKR/NotoSansKR-Variable.ttf";
            const string assetPath = "Assets/Resources/Fonts/NotoSansKR-Dynamic.asset";
            EnsureTextMeshProResources();
            AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceSynchronousImport);
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException("Noto Sans KR source font could not be imported.");
            }

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
                fontAsset.name = "NotoSansKR-Dynamic";
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
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
            var bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Bootstrap", typeof(BootstrapLoader));
            EditorSceneManager.SaveScene(bootstrap, SceneRoot + "/Bootstrap.unity");

            var main = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("CurioClerkApp", typeof(GameApp));
            EditorSceneManager.SaveScene(main, SceneRoot + "/Main.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneRoot + "/Bootstrap.unity", true),
                new EditorBuildSettingsScene(SceneRoot + "/Main.unity", true)
            };
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
    }
}

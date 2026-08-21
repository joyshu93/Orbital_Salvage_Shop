using Unity.Android.Types;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace CurioClerk.Editor
{
    public static class ReleaseConfiguration
    {
        public const string ProductName = "Curio Clerk: Night Shift";
        public const string PackageId = "com.joyshu93.curioclerknightshift";
        public const string VersionName = "1.0.0";
        public const int VersionCode = 10000;
        public const int MinimumApi = 29;
        public const int TargetApi = 36;

        public static void Apply()
        {
            PlayerSettings.companyName = "joyshu93";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinimumApi;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetApi;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = true;
            UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.SymbolTable;
            UserBuildSettings.DebugSymbols.format = DebugSymbolFormat.Zip | DebugSymbolFormat.LegacyExtensions;
        }
    }
}

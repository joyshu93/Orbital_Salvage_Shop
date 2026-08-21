using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor.Build;
using UnityEngine;

namespace CurioClerk.Editor
{
    public static class ReleaseBuildManifest
    {
        private const string ManifestFileName = "CurioClerk-build.json";

        public static void Write(string aabPath)
        {
            if (string.IsNullOrWhiteSpace(aabPath) || !File.Exists(aabPath))
            {
                throw new BuildFailedException("The release AAB is missing.");
            }

            var aab = new FileInfo(aabPath);
            if (aab.Length == 0)
            {
                throw new BuildFailedException("The release AAB is empty.");
            }

            var outputDirectory = Path.GetDirectoryName(aab.FullName);
            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new BuildFailedException("The release manifest output directory is invalid.");
            }

            var manifest = new BuildManifest
            {
                product = ReleaseConfiguration.ProductName,
                packageId = ReleaseConfiguration.PackageId,
                versionName = ReleaseConfiguration.VersionName,
                versionCode = ReleaseConfiguration.VersionCode,
                unityVersion = ReleaseConfiguration.UnityVersion,
                minimumApi = ReleaseConfiguration.MinimumApi,
                targetApi = ReleaseConfiguration.TargetApi,
                architecture = "ARM64",
                backend = "IL2CPP",
                aabSha256 = ComputeSha256(aab.FullName)
            };

            File.WriteAllText(Path.Combine(outputDirectory, ManifestFileName), JsonUtility.ToJson(manifest, true));
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        [Serializable]
        private sealed class BuildManifest
        {
            public string product;
            public string packageId;
            public string versionName;
            public int versionCode;
            public string unityVersion;
            public int minimumApi;
            public int targetApi;
            public string architecture;
            public string backend;
            public string aabSha256;
        }
    }
}

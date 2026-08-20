using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace CurioClerk.Editor
{
    public sealed class ContentValidator : IPreprocessBuildWithReport
    {
        private const string ContentRoot = "Assets/Resources/Content";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidateOrThrow();
        }

        [MenuItem("Tools/Curio Clerk/Validate Project")]
        public static void ValidateOrThrow()
        {
            var errors = new List<string>();
            ValidateCatalog(errors);
            ValidateAssets(errors);
            ValidateScenes(errors);

            if (errors.Count > 0)
            {
                throw new BuildFailedException("Curio Clerk validation failed:\n- " + string.Join("\n- ", errors));
            }

            UnityEngine.Debug.Log("Curio Clerk validation passed: 24 artifacts, 10 rules, 5 difficulties, 6 cosmetics, 2 scenes.");
        }

        private static void ValidateCatalog(ICollection<string> errors)
        {
            var artifacts = ContentCatalog.CreateArtifacts();
            if (artifacts.Count != 24)
            {
                errors.Add($"Expected 24 artifacts, found {artifacts.Count}.");
            }

            AddDuplicateErrors(artifacts.Select(item => item.Id), "artifact", errors);
            foreach (var item in artifacts)
            {
                var traitCount = CountBits((int)item.Traits);
                if (string.IsNullOrWhiteSpace(item.NameEnglish) || string.IsNullOrWhiteSpace(item.NameKorean) ||
                    string.IsNullOrWhiteSpace(item.DescriptionEnglish) || string.IsNullOrWhiteSpace(item.DescriptionKorean))
                {
                    errors.Add($"Artifact '{item.Id}' has missing bilingual text.");
                }

                if (traitCount < 1 || traitCount > 3)
                {
                    errors.Add($"Artifact '{item.Id}' must have one to three traits.");
                }
            }

            var ruleTemplates = ContentCatalog.CreateRuleTemplates();
            if (ruleTemplates.Count != 10)
            {
                errors.Add($"Expected 10 rule templates, found {ruleTemplates.Count}.");
            }

            AddDuplicateErrors(ruleTemplates.Select(rule => rule.Id), "rule", errors);

            for (var band = 1; band <= 5; band++)
            {
                var rules = ContentCatalog.CreateRulesForBand(band, 7300 + band);
                if (rules.Count < 3 || !rules[rules.Count - 1].IsFallback)
                {
                    errors.Add($"Difficulty band {band} does not end in a fallback rule.");
                }
            }

            var cosmetics = ContentCatalog.CreateCosmetics();
            if (cosmetics.Count != 6)
            {
                errors.Add($"Expected 6 cosmetics, found {cosmetics.Count}.");
            }

            AddDuplicateErrors(cosmetics.Select(item => item.Id), "cosmetic", errors);
        }

        private static void ValidateAssets(ICollection<string> errors)
        {
            ValidateAssetCount<ArtifactDefinition>(ContentRoot + "/Artifacts", 24, errors);
            ValidateAssetCount<RuleDefinition>(ContentRoot + "/Rules", 10, errors);
            ValidateAssetCount<DifficultyProfile>(ContentRoot + "/Difficulties", 5, errors);
            ValidateAssetCount<CosmeticDefinition>(ContentRoot + "/Cosmetics", 6, errors);
        }

        private static void ValidateScenes(ICollection<string> errors)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (!scenes.SequenceEqual(new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" }))
            {
                errors.Add("Enabled build scenes must be Bootstrap then Main.");
            }
        }

        private static void ValidateAssetCount<T>(string folder, int expected, ICollection<string> errors)
            where T : UnityEngine.Object
        {
            var count = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }).Length;
            if (count != expected)
            {
                errors.Add($"Expected {expected} {typeof(T).Name} assets in {folder}, found {count}.");
            }
        }

        private static void AddDuplicateErrors(IEnumerable<string> ids, string label, ICollection<string> errors)
        {
            foreach (var duplicate in ids.GroupBy(id => id, StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                errors.Add($"Duplicate {label} id '{duplicate.Key}'.");
            }
        }

        private static int CountBits(int value)
        {
            var count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }
    }
}

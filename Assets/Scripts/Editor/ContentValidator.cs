using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CurioClerk.Editor
{
    public sealed class ContentValidator : IPreprocessBuildWithReport
    {
        private const string ContentRoot = "Assets/Resources/Content";
        private static readonly string[] NarrativeArtPaths =
        {
            "Assets/Resources/Art/Characters/senior-clerk-neutral.png",
            "Assets/Resources/Art/Characters/senior-clerk-concerned.png",
            "Assets/Resources/Art/Characters/senior-clerk-alert.png",
            "Assets/Resources/Art/Characters/senior-clerk-relieved.png",
            "Assets/Resources/Art/Effects/frost-overlay.png"
        };

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
            ValidateReleaseAssets(errors);

            if (errors.Count > 0)
            {
                throw new BuildFailedException("Curio Clerk validation failed:\n- " + string.Join("\n- ", errors));
            }

            Debug.Log("Curio Clerk validation passed: 24 artifacts, 10 rules, 2 rule packs, " +
                      "3 docket templates, 1 incident, 5 incident stages, 5 difficulties, 6 cosmetics, 2 scenes.");
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
                    string.IsNullOrWhiteSpace(item.DescriptionEnglish) || string.IsNullOrWhiteSpace(item.DescriptionKorean) ||
                    string.IsNullOrWhiteSpace(item.ResolutionEnglish) || string.IsNullOrWhiteSpace(item.ResolutionKorean))
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

            var ruleEngine = new RuleEngine();
            var rulePacks = ContentCatalog.CreateRulePacks();
            if (rulePacks.Count != 2)
            {
                errors.Add($"Expected 2 rule packs, found {rulePacks.Count}.");
            }

            AddDuplicateErrors(rulePacks.Select(pack => pack.Id), "rule pack", errors);
            foreach (var pack in rulePacks)
            {
                var counts = new int[3];
                foreach (var artifact in artifacts)
                {
                    var destination = ruleEngine.Resolve(artifact.ToArtifact(), pack.Rules);
                    counts[(int)destination]++;
                }

                if (counts[(int)Destination.Repair] < 4 ||
                    counts[(int)Destination.Storage] < 4 ||
                    counts[(int)Destination.Vault] < 4)
                {
                    errors.Add($"Rule pack '{pack.Id}' requires at least four artifacts per destination.");
                }
            }

            var sequenceAnalyzer = new DocketSequenceAnalyzer();
            var shiftTemplates = ContentCatalog.CreateShiftTemplates();
            if (shiftTemplates.Count != 3)
            {
                errors.Add($"Expected 3 docket templates, found {shiftTemplates.Count}.");
            }

            AddDuplicateErrors(shiftTemplates.Select(template => template.Id), "docket template", errors);
            foreach (var template in shiftTemplates)
            {
                var counts = new int[3];
                foreach (var destination in template.Destinations)
                {
                    counts[(int)destination]++;
                }

                if (counts.Any(count => count != 4))
                {
                    errors.Add($"Docket template '{template.Id}' must contain four of every destination.");
                }

                var minimumHolds = sequenceAnalyzer.MinimumHolds(template.Destinations);
                if (minimumHolds < 0 || minimumHolds < template.MinimumRequiredHolds)
                {
                    errors.Add($"Docket template '{template.Id}' does not satisfy its Hold requirement.");
                }
            }

            ValidateIncidents(artifacts, ruleEngine, sequenceAnalyzer, errors);

            var cosmetics = ContentCatalog.CreateCosmetics();
            if (cosmetics.Count != 6)
            {
                errors.Add($"Expected 6 cosmetics, found {cosmetics.Count}.");
            }

            AddDuplicateErrors(cosmetics.Select(item => item.Id), "cosmetic", errors);
        }

        private static void ValidateIncidents(
            IReadOnlyList<ArtifactContent> artifacts,
            RuleEngine ruleEngine,
            DocketSequenceAnalyzer sequenceAnalyzer,
            ICollection<string> errors)
        {
            var incidents = ContentCatalog.CreateIncidents();
            if (incidents.Count != 1)
            {
                errors.Add($"Expected 1 incident, found {incidents.Count}.");
            }

            AddDuplicateErrors(incidents.Select(incident => incident.Id), "incident", errors);
            var artifactById = artifacts.ToDictionary(artifact => artifact.Id, StringComparer.Ordinal);
            var stageIds = new List<string>();
            foreach (var incident in incidents)
            {
                if (!HasBilingualCopy(incident.Title))
                {
                    errors.Add($"Incident '{incident.Id}' has missing bilingual title text.");
                }

                if (incident.Stages.Count != 5)
                {
                    errors.Add($"Incident '{incident.Id}' must contain five stages.");
                }

                foreach (var stage in incident.Stages)
                {
                    stageIds.Add(stage.Id);
                    ValidateNarrativeBeats(stage, stage.IntroBeats, "intro", errors);
                    ValidateNarrativeBeats(stage, stage.OutroBeats, "outro", errors);
                    ValidateReactions(stage, errors);

                    var queueIds = new HashSet<string>(
                        stage.Queue.Select(entry => entry.ArtifactId),
                        StringComparer.Ordinal);
                    if (!artifactById.ContainsKey(stage.LeadArtifactId) || !queueIds.Contains(stage.LeadArtifactId))
                    {
                        errors.Add($"Incident stage '{stage.Id}' has an invalid lead artifact ID.");
                    }

                    if (!string.IsNullOrWhiteSpace(stage.ResonanceHoldArtifactId) &&
                        (!artifactById.ContainsKey(stage.ResonanceHoldArtifactId) ||
                         !queueIds.Contains(stage.ResonanceHoldArtifactId)))
                    {
                        errors.Add($"Incident stage '{stage.Id}' has an invalid resonance Hold artifact ID.");
                    }

                    ShiftPlan plan;
                    try
                    {
                        plan = stage.CreateShiftPlan(artifactById);
                    }
                    catch (Exception exception)
                    {
                        errors.Add($"Incident stage '{stage.Id}' cannot create a shift plan: {exception.Message}");
                        continue;
                    }

                    Destination[] destinations;
                    try
                    {
                        destinations = plan.Queue
                            .Select(artifact => ruleEngine.Resolve(artifact, plan.Rules))
                            .ToArray();
                    }
                    catch (Exception exception)
                    {
                        errors.Add($"Incident stage '{stage.Id}' has invalid sorting rules: {exception.Message}");
                        continue;
                    }

                    var counts = new int[3];
                    foreach (var destination in destinations)
                    {
                        counts[(int)destination]++;
                    }

                    if (counts.Any(count => count != 4))
                    {
                        errors.Add($"Incident stage '{stage.Id}' must resolve to four of every destination.");
                    }

                    var minimumHolds = sequenceAnalyzer.MinimumHolds(destinations);
                    if (minimumHolds < 0)
                    {
                        errors.Add($"Incident stage '{stage.Id}' is not solvable with one Hold slot.");
                    }
                    else if (minimumHolds != stage.MinimumRequiredHolds)
                    {
                        errors.Add(
                            $"Incident stage '{stage.Id}' declares {stage.MinimumRequiredHolds} minimum Holds " +
                            $"but requires {minimumHolds}.");
                    }
                }
            }

            AddDuplicateErrors(stageIds, "incident stage", errors);
        }

        private static void ValidateNarrativeBeats(
            IncidentStageDefinition stage,
            IReadOnlyList<NarrativeBeat> beats,
            string label,
            ICollection<string> errors)
        {
            if (beats == null || beats.Count == 0)
            {
                errors.Add($"Incident stage '{stage.Id}' is missing {label} beats.");
                return;
            }

            for (var index = 0; index < beats.Count; index++)
            {
                if (beats[index] == null || !HasBilingualCopy(beats[index].Copy))
                {
                    errors.Add($"Incident stage '{stage.Id}' has missing bilingual {label} text.");
                }
            }
        }

        private static void ValidateReactions(IncidentStageDefinition stage, ICollection<string> errors)
        {
            if (stage.Reactions == null)
            {
                errors.Add($"Incident stage '{stage.Id}' is missing quality reactions.");
                return;
            }

            if (!HasBilingualCopy(stage.Reactions.Stable) ||
                !HasBilingualCopy(stage.Reactions.Precise) ||
                !HasBilingualCopy(stage.Reactions.Resonant))
            {
                errors.Add($"Incident stage '{stage.Id}' has missing bilingual quality reactions.");
            }
        }

        private static bool HasBilingualCopy(LocalizedCopy copy)
            => copy != null &&
               !string.IsNullOrWhiteSpace(copy.English) &&
               !string.IsNullOrWhiteSpace(copy.Korean);

        private static void ValidateAssets(ICollection<string> errors)
        {
            ValidateAssetCount<ArtifactDefinition>(ContentRoot + "/Artifacts", 24, errors);
            ValidateAssetCount<RuleDefinition>(ContentRoot + "/Rules", 10, errors);
            ValidateAssetCount<DifficultyProfile>(ContentRoot + "/Difficulties", 5, errors);
            ValidateAssetCount<CosmeticDefinition>(ContentRoot + "/Cosmetics", 6, errors);
            ValidateNarrativeArt(errors);
        }

        private static void ValidateNarrativeArt(ICollection<string> errors)
        {
            foreach (var path in NarrativeArtPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
                {
                    errors.Add("Required narrative art sprite is missing: " + path);
                }
            }
        }

        private static void ValidateScenes(ICollection<string> errors)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (!scenes.SequenceEqual(new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Main.unity" }))
            {
                errors.Add("Enabled build scenes must be Bootstrap then Main.");
            }
        }

        private static void ValidateReleaseAssets(ICollection<string> errors)
        {
            var emojiPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.IndexOf("EmojiOne", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            if (emojiPaths.Length > 0)
            {
                errors.Add("Release assets must not contain EmojiOne files: " + string.Join(", ", emojiPaths));
            }

            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null)
            {
                errors.Add("TMP Settings asset is missing.");
                return;
            }

            var serializedSettings = new SerializedObject(settings);
            var defaultSpriteAsset = serializedSettings.FindProperty("m_defaultSpriteAsset");
            if (defaultSpriteAsset == null)
            {
                errors.Add("TMP default sprite setting could not be inspected.");
            }
            else if (defaultSpriteAsset.objectReferenceValue != null)
            {
                errors.Add("TMP Settings must not reference a default sprite asset.");
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

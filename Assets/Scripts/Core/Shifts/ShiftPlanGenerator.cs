using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftPlanGenerator
    {
        private readonly RuleEngine _ruleEngine = new RuleEngine();
        private readonly DocketSequenceAnalyzer _sequenceAnalyzer = new DocketSequenceAnalyzer();

        public ShiftPlan Generate(
            int seed,
            int band,
            IReadOnlyList<Artifact> source,
            IReadOnlyList<ShiftRulePack> rulePacks,
            IReadOnlyList<ShiftSequenceTemplate> templates)
        {
            ValidateInputs(source, rulePacks, templates);
            var supportedPacks = SupportedPacks(rulePacks, band);
            var supportedTemplates = SupportedTemplates(templates, band);
            var selectedPack = supportedPacks[
                new Random(seed ^ 0x51F15EED).Next(supportedPacks.Count)];
            var selectedTemplate = supportedTemplates[
                new Random(seed ^ 0x2D0C7E7).Next(supportedTemplates.Count)];

            ValidateUniqueArtifactIds(source);
            var buckets = CreateBuckets(source, selectedPack.Rules);
            var selectedBuckets = new[]
            {
                SelectFour(buckets[(int)Destination.Repair], seed ^ 0x13579),
                SelectFour(buckets[(int)Destination.Storage], seed ^ 0x24680),
                SelectFour(buckets[(int)Destination.Vault], seed ^ 0x369CF)
            };

            ValidateTemplate(selectedTemplate);
            var queue = new Artifact[selectedTemplate.Destinations.Count];
            for (var index = 0; index < selectedTemplate.Destinations.Count; index++)
            {
                queue[index] = selectedBuckets[(int)selectedTemplate.Destinations[index]].Dequeue();
            }

            ValidateGeneratedQueue(queue);
            return new ShiftPlan(
                selectedPack.Id,
                selectedTemplate.Id,
                queue,
                selectedPack.Rules);
        }

        private List<Artifact>[] CreateBuckets(
            IReadOnlyList<Artifact> source,
            IReadOnlyList<SortingRule> rules)
        {
            var buckets = new[]
            {
                new List<Artifact>(),
                new List<Artifact>(),
                new List<Artifact>()
            };

            for (var index = 0; index < source.Count; index++)
            {
                var artifact = source[index];
                var destination = _ruleEngine.Resolve(artifact, rules);
                if (!IsValidDestination(destination))
                {
                    throw new InvalidOperationException("A rule resolved outside the supported destinations.");
                }

                buckets[(int)destination].Add(artifact);
            }

            for (var index = 0; index < buckets.Length; index++)
            {
                if (buckets[index].Count < 4)
                {
                    throw new InvalidOperationException(
                        "The selected rule pack requires at least four artifacts per destination.");
                }
            }

            return buckets;
        }

        private static Queue<Artifact> SelectFour(IReadOnlyList<Artifact> bucket, int seed)
        {
            var shuffled = new List<Artifact>(bucket.Count);
            for (var index = 0; index < bucket.Count; index++)
            {
                shuffled.Add(bucket[index]);
            }

            var random = new Random(seed);
            for (var index = shuffled.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                var value = shuffled[index];
                shuffled[index] = shuffled[swapIndex];
                shuffled[swapIndex] = value;
            }

            return new Queue<Artifact>(shuffled.GetRange(0, 4));
        }

        private void ValidateTemplate(ShiftSequenceTemplate template)
        {
            var counts = new int[3];
            for (var index = 0; index < template.Destinations.Count; index++)
            {
                var destination = template.Destinations[index];
                if (!IsValidDestination(destination))
                {
                    throw new InvalidOperationException("A template contains an unsupported destination.");
                }

                counts[(int)destination]++;
            }

            for (var index = 0; index < counts.Length; index++)
            {
                if (counts[index] != 4)
                {
                    throw new InvalidOperationException(
                        "A shift template requires exactly four of every destination.");
                }
            }

            var minimumHolds = _sequenceAnalyzer.MinimumHolds(template.Destinations);
            if (minimumHolds < 0 || minimumHolds < template.MinimumRequiredHolds)
            {
                throw new InvalidOperationException(
                    "A shift template is unsolvable or requires fewer Holds than declared.");
            }
        }

        private static List<ShiftRulePack> SupportedPacks(
            IReadOnlyList<ShiftRulePack> rulePacks,
            int band)
        {
            var supported = new List<ShiftRulePack>();
            for (var index = 0; index < rulePacks.Count; index++)
            {
                var pack = rulePacks[index] ??
                    throw new ArgumentException("Rule pack collections cannot contain null entries.", nameof(rulePacks));
                if (pack.SupportsBand(band))
                {
                    supported.Add(pack);
                }
            }

            if (supported.Count == 0)
            {
                throw new InvalidOperationException("No rule pack supports the requested band.");
            }

            return supported;
        }

        private static List<ShiftSequenceTemplate> SupportedTemplates(
            IReadOnlyList<ShiftSequenceTemplate> templates,
            int band)
        {
            var supported = new List<ShiftSequenceTemplate>();
            for (var index = 0; index < templates.Count; index++)
            {
                var template = templates[index] ??
                    throw new ArgumentException("Template collections cannot contain null entries.", nameof(templates));
                if (template.SupportsBand(band))
                {
                    supported.Add(template);
                }
            }

            if (supported.Count == 0)
            {
                throw new InvalidOperationException("No sequence template supports the requested band.");
            }

            return supported;
        }

        private static void ValidateInputs(
            IReadOnlyList<Artifact> source,
            IReadOnlyList<ShiftRulePack> rulePacks,
            IReadOnlyList<ShiftSequenceTemplate> templates)
        {
            if (source == null || source.Count == 0)
            {
                throw new ArgumentException("Artifact source is required.", nameof(source));
            }

            if (rulePacks == null || rulePacks.Count == 0)
            {
                throw new ArgumentException("At least one rule pack is required.", nameof(rulePacks));
            }

            if (templates == null || templates.Count == 0)
            {
                throw new ArgumentException("At least one sequence template is required.", nameof(templates));
            }

            for (var index = 0; index < source.Count; index++)
            {
                if (source[index] == null)
                {
                    throw new ArgumentException("Artifact sources cannot contain null entries.", nameof(source));
                }
            }
        }

        private static void ValidateUniqueArtifactIds(IReadOnlyList<Artifact> source)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < source.Count; index++)
            {
                if (!ids.Add(source[index].Id))
                {
                    throw new InvalidOperationException("Artifact source IDs must be unique.");
                }
            }
        }

        private static void ValidateGeneratedQueue(IReadOnlyList<Artifact> queue)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < queue.Count; index++)
            {
                if (!ids.Add(queue[index].Id))
                {
                    throw new InvalidOperationException("Generated shift queues must contain unique artifacts.");
                }
            }
        }

        private static bool IsValidDestination(Destination destination)
            => destination == Destination.Repair ||
               destination == Destination.Storage ||
               destination == Destination.Vault;
    }
}

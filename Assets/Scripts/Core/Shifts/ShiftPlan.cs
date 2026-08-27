using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftPlan
    {
        public ShiftPlan(
            string rulePackId,
            string sequenceTemplateId,
            IReadOnlyList<Artifact> queue,
            IReadOnlyList<SortingRule> rules)
        {
            if (string.IsNullOrWhiteSpace(rulePackId))
            {
                throw new ArgumentException("Rule pack id is required.", nameof(rulePackId));
            }

            if (string.IsNullOrWhiteSpace(sequenceTemplateId))
            {
                throw new ArgumentException("Sequence template id is required.", nameof(sequenceTemplateId));
            }

            Queue = CopyQueue(queue);
            Rules = CopyRules(rules);
            RulePackId = rulePackId;
            SequenceTemplateId = sequenceTemplateId;
        }

        public string RulePackId { get; }

        public string SequenceTemplateId { get; }

        public IReadOnlyList<Artifact> Queue { get; }

        public IReadOnlyList<SortingRule> Rules { get; }

        private static IReadOnlyList<Artifact> CopyQueue(IReadOnlyList<Artifact> queue)
        {
            if (queue == null || queue.Count == 0)
            {
                throw new ArgumentException("A shift plan requires a queue.", nameof(queue));
            }

            var copy = new Artifact[queue.Count];
            for (var index = 0; index < queue.Count; index++)
            {
                copy[index] = queue[index] ??
                    throw new ArgumentException("Plan queues cannot contain null artifacts.", nameof(queue));
            }

            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<SortingRule> CopyRules(IReadOnlyList<SortingRule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                throw new ArgumentException("A shift plan requires rules.", nameof(rules));
            }

            var copy = new SortingRule[rules.Count];
            for (var index = 0; index < rules.Count; index++)
            {
                copy[index] = rules[index] ??
                    throw new ArgumentException("Plan rules cannot contain null entries.", nameof(rules));
            }

            return Array.AsReadOnly(copy);
        }
    }
}

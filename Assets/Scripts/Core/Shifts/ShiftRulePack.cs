using System;
using System.Collections.Generic;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftRulePack
    {
        public ShiftRulePack(
            string id,
            int minimumBand,
            int maximumBand,
            IReadOnlyList<SortingRule> rules)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Rule pack id is required.", nameof(id));
            }

            if (minimumBand > maximumBand)
            {
                throw new ArgumentException("Minimum band cannot exceed maximum band.", nameof(minimumBand));
            }

            Rules = CopyRules(rules);
            Id = id;
            MinimumBand = minimumBand;
            MaximumBand = maximumBand;
        }

        public string Id { get; }

        public int MinimumBand { get; }

        public int MaximumBand { get; }

        public IReadOnlyList<SortingRule> Rules { get; }

        public bool SupportsBand(int band)
            => band >= MinimumBand && band <= MaximumBand;

        private static IReadOnlyList<SortingRule> CopyRules(IReadOnlyList<SortingRule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                throw new ArgumentException("A rule pack requires at least one rule.", nameof(rules));
            }

            var copy = new SortingRule[rules.Count];
            for (var index = 0; index < rules.Count; index++)
            {
                copy[index] = rules[index] ??
                    throw new ArgumentException("Rule packs cannot contain null rules.", nameof(rules));
            }

            return Array.AsReadOnly(copy);
        }
    }
}

using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;

namespace CurioClerk.Core.Rules
{
    public sealed class RuleEngine
    {
        public Destination Resolve(Artifact artifact, IReadOnlyList<SortingRule> rules)
            => ResolveDetailed(artifact, rules).Destination;

        public RuleResolution ResolveDetailed(Artifact artifact, IReadOnlyList<SortingRule> rules)
        {
            if (artifact == null)
            {
                throw new ArgumentNullException(nameof(artifact));
            }

            Validate(rules);

            for (var index = 0; index < rules.Count; index++)
            {
                if (rules[index].Matches(artifact))
                {
                    return new RuleResolution(rules[index].Id, rules[index].Destination);
                }
            }

            throw new InvalidOperationException("The validated fallback rule did not match.");
        }

        private static void Validate(IReadOnlyList<SortingRule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                throw new InvalidOperationException("At least one sorting rule is required.");
            }

            for (var index = 0; index < rules.Count - 1; index++)
            {
                if (rules[index] == null || rules[index].IsFallback)
                {
                    throw new InvalidOperationException("Only the final sorting rule may be the fallback.");
                }
            }

            if (rules[rules.Count - 1] == null || !rules[rules.Count - 1].IsFallback)
            {
                throw new InvalidOperationException("The final sorting rule must be a fallback.");
            }
        }
    }
}

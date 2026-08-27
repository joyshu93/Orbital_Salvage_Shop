using System;

namespace CurioClerk.Core.Rules
{
    public sealed class RuleResolution
    {
        public RuleResolution(string ruleId, Destination destination)
        {
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                throw new ArgumentException("Rule id is required.", nameof(ruleId));
            }

            RuleId = ruleId;
            Destination = destination;
        }

        public string RuleId { get; }

        public Destination Destination { get; }
    }
}

using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class SortOutcome
    {
        public SortOutcome(bool wasCorrect, Destination expectedDestination)
        {
            WasCorrect = wasCorrect;
            ExpectedDestination = expectedDestination;
        }

        public bool WasCorrect { get; }

        public Destination ExpectedDestination { get; }
    }
}


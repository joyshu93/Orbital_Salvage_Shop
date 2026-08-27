using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class SortOutcome
    {
        public SortOutcome(
            SortDisposition disposition,
            Destination selectedDestination,
            Destination expectedDestination,
            string matchedRuleId,
            bool didCompleteDocket,
            bool didCompleteShift,
            int scoreDelta,
            int coinDelta)
        {
            Disposition = disposition;
            SelectedDestination = selectedDestination;
            ExpectedDestination = expectedDestination;
            MatchedRuleId = matchedRuleId;
            DidCompleteDocket = didCompleteDocket;
            DidCompleteShift = didCompleteShift;
            ScoreDelta = scoreDelta;
            CoinDelta = coinDelta;
        }

        public SortDisposition Disposition { get; }

        public bool WasCorrect => Disposition == SortDisposition.Correct;

        public Destination SelectedDestination { get; }

        public Destination ExpectedDestination { get; }

        public string MatchedRuleId { get; }

        public bool DidCompleteDocket { get; }

        public bool DidCompleteShift { get; }

        public int ScoreDelta { get; }

        public int CoinDelta { get; }
    }
}

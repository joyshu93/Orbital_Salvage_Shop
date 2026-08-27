using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftSession
    {
        private readonly IReadOnlyList<Artifact> _queue;
        private readonly IReadOnlyList<SortingRule> _rules;
        private readonly RuleEngine _ruleEngine = new RuleEngine();
        private readonly List<bool> _completedDocketPristine = new List<bool>();
        private readonly bool _legacyMode;
        private int _nextIndex;
        private int _legacyCombo;
        private bool _canHold = true;

        public ShiftSession(IReadOnlyList<Artifact> queue, IReadOnlyList<SortingRule> rules)
            : this(queue, rules, false)
        {
        }

        private ShiftSession(
            IReadOnlyList<Artifact> queue,
            IReadOnlyList<SortingRule> rules,
            bool legacyMode)
        {
            if (queue == null || queue.Count == 0 || (!legacyMode && queue.Count % 3 != 0))
            {
                throw new ArgumentException(
                    "A docket shift requires a positive artifact count divisible by three.",
                    nameof(queue));
            }

            _queue = queue;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _legacyMode = legacyMode;
            RequiredDockets = legacyMode ? 0 : queue.Count / 3;
            CurrentDocket = new DocketState();
            CurrentArtifact = queue[0] ??
                throw new ArgumentException("Artifact queues cannot contain null entries.", nameof(queue));
            _nextIndex = 1;
            Hearts = 3;
            State = ShiftState.Active;
        }

        public static ShiftSession CreateLegacySession(
            IReadOnlyList<Artifact> queue,
            IReadOnlyList<SortingRule> rules)
            => new ShiftSession(queue, rules, true);

        public Artifact CurrentArtifact { get; private set; }

        public Artifact HeldArtifact { get; private set; }

        public int Hearts { get; private set; }

        public int Score { get; private set; }

        public int Coins { get; private set; }

        public int CorrectSorts { get; private set; }

        public int Mistakes { get; private set; }

        public ShiftState State { get; private set; }

        public bool RewardClaimed { get; private set; }

        public DocketState CurrentDocket { get; private set; }

        public int CompletedDockets { get; private set; }

        public int RequiredDockets { get; }

        public int PristineDocketStreak { get; private set; }

        public IReadOnlyList<bool> CompletedDocketPristine => _completedDocketPristine;

        public RuleResolution CurrentResolution => CurrentArtifact == null
            ? null
            : _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);

        public bool ShouldSuggestHold => CurrentResolution != null &&
                                         CurrentDocket.IsStamped(CurrentResolution.Destination);

        public bool CanSort(Destination destination)
            => State == ShiftState.Active && !CurrentDocket.IsStamped(destination);

        public Artifact PeekNextArtifact(int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            var index = _nextIndex + offset;
            if (index < _queue.Count)
            {
                return _queue[index];
            }

            return index == _queue.Count && HeldArtifact != null
                ? HeldArtifact
                : null;
        }

        public SortOutcome Sort(Destination destination)
            => _legacyMode ? SortLegacy(destination) : SortDocket(destination);

        public bool Hold()
        {
            if (State != ShiftState.Active || !_canHold || CurrentArtifact == null)
            {
                return false;
            }

            if (HeldArtifact == null)
            {
                var next = TakeNextQueuedArtifact();
                if (next == null)
                {
                    return false;
                }

                HeldArtifact = CurrentArtifact;
                CurrentArtifact = next;
            }
            else
            {
                var previousCurrent = CurrentArtifact;
                CurrentArtifact = HeldArtifact;
                HeldArtifact = previousCurrent;
            }

            _canHold = false;
            return true;
        }

        public bool TryRevive()
        {
            if (RewardClaimed || State != ShiftState.Failed)
            {
                return false;
            }

            RewardClaimed = true;
            Hearts = 1;
            State = CurrentArtifact == null ? ShiftState.Completed : ShiftState.Active;
            return true;
        }

        public bool TryDoubleCoins()
        {
            if (RewardClaimed || State != ShiftState.Completed)
            {
                return false;
            }

            RewardClaimed = true;
            Coins *= 2;
            return true;
        }

        public ShiftResult CreateResult()
            => new ShiftResult(State, Score, Coins, CorrectSorts, Mistakes);

        private SortOutcome SortDocket(Destination destination)
        {
            EnsureActive();
            var resolution = _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);
            if (CurrentDocket.IsStamped(destination))
            {
                return Outcome(SortDisposition.Blocked, destination, resolution, false, false, 0, 0);
            }

            if (destination != resolution.Destination)
            {
                Hearts--;
                Mistakes++;
                CurrentDocket.MarkMistake();
                PristineDocketStreak = 0;
                if (Hearts <= 0)
                {
                    State = ShiftState.Failed;
                }

                return Outcome(SortDisposition.Wrong, destination, resolution, false, false, 0, 0);
            }

            var scoreDelta = 100;
            var coinDelta = 5;
            CorrectSorts++;
            CurrentDocket.TryStamp(destination);
            var completedDocket = CurrentDocket.IsComplete;
            if (completedDocket)
            {
                scoreDelta += 300;
                coinDelta += 5;
                if (CurrentDocket.IsPristine)
                {
                    scoreDelta += 100;
                    PristineDocketStreak++;
                }
                else
                {
                    PristineDocketStreak = 0;
                }

                _completedDocketPristine.Add(CurrentDocket.IsPristine);
                CompletedDockets++;
            }

            Advance();
            _canHold = true;
            var completedShift = CompletedDockets == RequiredDockets;
            if (completedShift)
            {
                if (_completedDocketPristine.TrueForAll(value => value))
                {
                    coinDelta += 20;
                }

                State = ShiftState.Completed;
            }
            else if (completedDocket)
            {
                CurrentDocket = new DocketState();
            }

            Score += scoreDelta;
            Coins += coinDelta;
            return Outcome(
                SortDisposition.Correct,
                destination,
                resolution,
                completedDocket,
                completedShift,
                scoreDelta,
                coinDelta);
        }

        private SortOutcome SortLegacy(Destination destination)
        {
            EnsureActive();
            var resolution = _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);
            var wasCorrect = destination == resolution.Destination;
            var scoreDelta = 0;
            var coinDelta = 0;

            if (wasCorrect)
            {
                _legacyCombo++;
                CorrectSorts++;
                scoreDelta = 100 + (_legacyCombo - 1) * 20;
                coinDelta = 5 + Math.Min(_legacyCombo - 1, 5);
                Score += scoreDelta;
                Coins += coinDelta;
            }
            else
            {
                Hearts--;
                Mistakes++;
                _legacyCombo = 0;
            }

            Advance();
            _canHold = true;
            if (Hearts <= 0)
            {
                State = ShiftState.Failed;
            }
            else if (CurrentArtifact == null)
            {
                State = ShiftState.Completed;
            }

            return Outcome(
                wasCorrect ? SortDisposition.Correct : SortDisposition.Wrong,
                destination,
                resolution,
                false,
                State == ShiftState.Completed,
                scoreDelta,
                coinDelta);
        }

        private static SortOutcome Outcome(
            SortDisposition disposition,
            Destination selectedDestination,
            RuleResolution resolution,
            bool didCompleteDocket,
            bool didCompleteShift,
            int scoreDelta,
            int coinDelta)
        {
            return new SortOutcome(
                disposition,
                selectedDestination,
                resolution.Destination,
                resolution.RuleId,
                didCompleteDocket,
                didCompleteShift,
                scoreDelta,
                coinDelta);
        }

        private void EnsureActive()
        {
            if (State != ShiftState.Active || CurrentArtifact == null)
            {
                throw new InvalidOperationException("Only an active shift can sort an artifact.");
            }
        }

        private void Advance()
        {
            CurrentArtifact = TakeNextQueuedArtifact();
            if (CurrentArtifact == null && HeldArtifact != null)
            {
                CurrentArtifact = HeldArtifact;
                HeldArtifact = null;
            }
        }

        private Artifact TakeNextQueuedArtifact()
        {
            if (_nextIndex >= _queue.Count)
            {
                return null;
            }

            var artifact = _queue[_nextIndex++];
            if (artifact == null)
            {
                throw new InvalidOperationException("Artifact queues cannot contain null entries.");
            }

            return artifact;
        }
    }
}

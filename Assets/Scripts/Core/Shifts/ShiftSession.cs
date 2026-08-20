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
        private int _nextIndex;
        private bool _canHold = true;

        public ShiftSession(IReadOnlyList<Artifact> queue, IReadOnlyList<SortingRule> rules)
        {
            if (queue == null || queue.Count == 0)
            {
                throw new ArgumentException("A shift requires at least one artifact.", nameof(queue));
            }

            _queue = queue;
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            CurrentArtifact = queue[0] ?? throw new ArgumentException("Artifact queues cannot contain null entries.", nameof(queue));
            _nextIndex = 1;
            Hearts = 3;
            State = ShiftState.Active;
        }

        public Artifact CurrentArtifact { get; private set; }

        public Artifact HeldArtifact { get; private set; }

        public int Hearts { get; private set; }

        public int Combo { get; private set; }

        public int Score { get; private set; }

        public int Coins { get; private set; }

        public int CorrectSorts { get; private set; }

        public int Mistakes { get; private set; }

        public ShiftState State { get; private set; }

        public bool RewardClaimed { get; private set; }

        public SortOutcome Sort(Destination destination)
        {
            EnsureActive();
            var expected = _ruleEngine.Resolve(CurrentArtifact, _rules);
            var wasCorrect = destination == expected;

            if (wasCorrect)
            {
                Combo++;
                CorrectSorts++;
                Score += 100 + (Combo - 1) * 20;
                Coins += 5 + Math.Min(Combo - 1, 5);
            }
            else
            {
                Hearts--;
                Mistakes++;
                Combo = 0;
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

            return new SortOutcome(wasCorrect, expected);
        }

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
        {
            return new ShiftResult(State, Score, Coins, CorrectSorts, Mistakes);
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

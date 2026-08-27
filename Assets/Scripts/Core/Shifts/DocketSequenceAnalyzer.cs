using System;
using System.Collections.Generic;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class DocketSequenceAnalyzer
    {
        private const int NoDestination = -1;
        private const int CompleteStampMask = 0b111;

        public int MinimumHolds(IReadOnlyList<Destination> destinations)
        {
            Validate(destinations);

            var initial = new SearchState(
                1,
                (int)destinations[0],
                NoDestination,
                0,
                0,
                true);
            var queue = new Queue<SearchNode>();
            var visited = new HashSet<SearchState>();
            queue.Enqueue(new SearchNode(initial, 0));
            visited.Add(initial);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var state = node.State;
                if (state.Current == NoDestination)
                {
                    if (state.CompletedDockets == destinations.Count / 3)
                    {
                        return node.HoldCount;
                    }

                    continue;
                }

                EnqueueSuccessfulSort(destinations, node, queue, visited);
                EnqueueHold(destinations, node, queue, visited);
            }

            return -1;
        }

        private static void EnqueueSuccessfulSort(
            IReadOnlyList<Destination> destinations,
            SearchNode node,
            Queue<SearchNode> queue,
            ISet<SearchState> visited)
        {
            var state = node.State;
            var destinationBit = 1 << state.Current;
            if ((state.StampMask & destinationBit) != 0)
            {
                return;
            }

            var stampMask = state.StampMask | destinationBit;
            var completedDockets = state.CompletedDockets;
            if (stampMask == CompleteStampMask)
            {
                stampMask = 0;
                completedDockets++;
            }

            var nextIndex = state.NextIndex;
            var held = state.Held;
            int current;
            if (nextIndex < destinations.Count)
            {
                current = (int)destinations[nextIndex];
                nextIndex++;
            }
            else if (held != NoDestination)
            {
                current = held;
                held = NoDestination;
            }
            else
            {
                current = NoDestination;
            }

            Enqueue(
                new SearchState(
                    nextIndex,
                    current,
                    held,
                    stampMask,
                    completedDockets,
                    true),
                node.HoldCount,
                queue,
                visited);
        }

        private static void EnqueueHold(
            IReadOnlyList<Destination> destinations,
            SearchNode node,
            Queue<SearchNode> queue,
            ISet<SearchState> visited)
        {
            var state = node.State;
            if (!state.CanHold)
            {
                return;
            }

            var nextIndex = state.NextIndex;
            var current = state.Current;
            var held = state.Held;
            if (held == NoDestination)
            {
                if (nextIndex >= destinations.Count)
                {
                    return;
                }

                held = current;
                current = (int)destinations[nextIndex];
                nextIndex++;
            }
            else
            {
                var previousCurrent = current;
                current = held;
                held = previousCurrent;
            }

            Enqueue(
                new SearchState(
                    nextIndex,
                    current,
                    held,
                    state.StampMask,
                    state.CompletedDockets,
                    false),
                node.HoldCount + 1,
                queue,
                visited);
        }

        private static void Enqueue(
            SearchState state,
            int holdCount,
            Queue<SearchNode> queue,
            ISet<SearchState> visited)
        {
            if (!visited.Add(state))
            {
                return;
            }

            queue.Enqueue(new SearchNode(state, holdCount));
        }

        private static void Validate(IReadOnlyList<Destination> destinations)
        {
            if (destinations == null || destinations.Count == 0 || destinations.Count % 3 != 0)
            {
                throw new ArgumentException(
                    "A sequence requires a positive destination count divisible by three.",
                    nameof(destinations));
            }

            for (var index = 0; index < destinations.Count; index++)
            {
                var destination = destinations[index];
                if (destination != Destination.Repair &&
                    destination != Destination.Storage &&
                    destination != Destination.Vault)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinations));
                }
            }
        }

        private readonly struct SearchNode
        {
            public SearchNode(SearchState state, int holdCount)
            {
                State = state;
                HoldCount = holdCount;
            }

            public SearchState State { get; }

            public int HoldCount { get; }
        }

        private readonly struct SearchState : IEquatable<SearchState>
        {
            public SearchState(
                int nextIndex,
                int current,
                int held,
                int stampMask,
                int completedDockets,
                bool canHold)
            {
                NextIndex = nextIndex;
                Current = current;
                Held = held;
                StampMask = stampMask;
                CompletedDockets = completedDockets;
                CanHold = canHold;
            }

            public int NextIndex { get; }

            public int Current { get; }

            public int Held { get; }

            public int StampMask { get; }

            public int CompletedDockets { get; }

            public bool CanHold { get; }

            public bool Equals(SearchState other)
            {
                return NextIndex == other.NextIndex &&
                       Current == other.Current &&
                       Held == other.Held &&
                       StampMask == other.StampMask &&
                       CompletedDockets == other.CompletedDockets &&
                       CanHold == other.CanHold;
            }

            public override bool Equals(object value)
                => value is SearchState other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = NextIndex;
                    hash = (hash * 397) ^ Current;
                    hash = (hash * 397) ^ Held;
                    hash = (hash * 397) ^ StampMask;
                    hash = (hash * 397) ^ CompletedDockets;
                    hash = (hash * 397) ^ (CanHold ? 1 : 0);
                    return hash;
                }
            }
        }
    }
}

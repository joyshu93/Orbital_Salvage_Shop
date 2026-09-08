using System;
using System.Collections.Generic;
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftSequenceTemplate
    {
        public ShiftSequenceTemplate(
            string id,
            int minimumBand,
            int maximumBand,
            int minimumRequiredHolds,
            IReadOnlyList<Destination> destinations)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Sequence template id is required.", nameof(id));
            }

            if (minimumBand > maximumBand)
            {
                throw new ArgumentException("Minimum band cannot exceed maximum band.", nameof(minimumBand));
            }

            if (minimumRequiredHolds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumRequiredHolds));
            }

            Destinations = CopyDestinations(destinations);
            Id = id;
            MinimumBand = minimumBand;
            MaximumBand = maximumBand;
            MinimumRequiredHolds = minimumRequiredHolds;
        }

        public string Id { get; }

        public int MinimumBand { get; }

        public int MaximumBand { get; }

        public int MinimumRequiredHolds { get; }

        public IReadOnlyList<Destination> Destinations { get; }

        public bool SupportsBand(int band)
            => band >= MinimumBand && band <= MaximumBand;

        private static IReadOnlyList<Destination> CopyDestinations(
            IReadOnlyList<Destination> destinations)
        {
            if (destinations == null || destinations.Count != 12)
            {
                throw new ArgumentException(
                    "A sequence template requires exactly twelve destinations.",
                    nameof(destinations));
            }

            var copy = new Destination[destinations.Count];
            for (var index = 0; index < destinations.Count; index++)
            {
                if (!IsValidDestination(destinations[index]))
                {
                    throw new ArgumentOutOfRangeException(nameof(destinations));
                }

                copy[index] = destinations[index];
            }

            return Array.AsReadOnly(copy);
        }

        private static bool IsValidDestination(Destination destination)
            => destination == Destination.Repair ||
               destination == Destination.Storage ||
               destination == Destination.Vault;
    }
}

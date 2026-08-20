using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;

namespace CurioClerk.Core.Shifts
{
    public sealed class ShiftGenerator
    {
        public IReadOnlyList<Artifact> GenerateArtifactQueue(
            int seed,
            IReadOnlyList<Artifact> source,
            int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (count <= 0 || count > source.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var shuffled = new List<Artifact>(source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                if (source[index] == null)
                {
                    throw new ArgumentException("Artifact sources cannot contain null entries.", nameof(source));
                }

                shuffled.Add(source[index]);
            }

            var random = new Random(seed);
            for (var index = shuffled.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                var value = shuffled[index];
                shuffled[index] = shuffled[swapIndex];
                shuffled[swapIndex] = value;
            }

            return shuffled.GetRange(0, count);
        }
    }
}


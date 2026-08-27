using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class DocketState
    {
        private int _stampMask;

        public bool IsPristine { get; private set; } = true;

        public int StampCount { get; private set; }

        public bool IsComplete => StampCount == 3;

        public bool IsStamped(Destination destination)
            => (_stampMask & (1 << (int)destination)) != 0;

        public bool TryStamp(Destination destination)
        {
            var bit = 1 << (int)destination;
            if ((_stampMask & bit) != 0)
            {
                return false;
            }

            _stampMask |= bit;
            StampCount++;
            return true;
        }

        public void MarkMistake() => IsPristine = false;
    }
}

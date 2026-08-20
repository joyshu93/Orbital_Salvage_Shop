using CurioClerk.Core.Shifts;

namespace CurioClerk.Infrastructure.Time
{
    public sealed class ShiftSeedProvider : IShiftSeedProvider
    {
        private readonly IClock _clock;

        public ShiftSeedProvider(IClock clock)
        {
            _clock = clock;
        }

        public int CreateStandardSeed(int completedShifts)
        {
            unchecked
            {
                return (_clock.LocalNow.Millisecond * 486187739) ^ (_clock.LocalNow.Second * 397) ^ completedShifts;
            }
        }

        public int CreateDailySeed(int contentVersion) => DailySeedProvider.ForDate(_clock.LocalNow, contentVersion);
    }
}


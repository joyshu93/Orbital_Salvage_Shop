namespace CurioClerk.Infrastructure.Time
{
    public interface IShiftSeedProvider
    {
        int CreateStandardSeed(int completedShifts);

        int CreateDailySeed(int contentVersion);
    }
}


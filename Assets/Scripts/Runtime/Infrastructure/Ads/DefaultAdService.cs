using System;

namespace CurioClerk.Infrastructure.Ads
{
    public sealed class DefaultAdService : IAdService
    {
#if UNITY_EDITOR
        public bool IsRewardedReady => true;

        public void SetRequestPermission(bool allowed)
        {
        }

        public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
        {
            completed?.Invoke(IsAllowedPlacement(placement)
                ? RewardedAdResult.Earned
                : RewardedAdResult.Unavailable);
        }
#else
        public bool IsRewardedReady => false;

        public void SetRequestPermission(bool allowed)
        {
        }

        public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
        {
            completed?.Invoke(RewardedAdResult.Unavailable);
        }
#endif

        private static bool IsAllowedPlacement(string placement)
        {
            return string.Equals(placement, "shift_failed_revive", StringComparison.Ordinal) ||
                   string.Equals(placement, "shift_complete_double", StringComparison.Ordinal);
        }
    }
}

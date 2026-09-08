using System;

namespace CurioClerk.Infrastructure.Ads
{
    public interface IAdService
    {
        bool IsRewardedReady { get; }

        void SetRequestPermission(bool allowed);

        void ShowRewarded(string placement, Action<RewardedAdResult> completed);
    }
}

using System;

namespace CurioClerk.Infrastructure.Ads
{
    public interface IAdService
    {
        bool IsRewardedReady { get; }

        void ShowRewarded(string placement, Action<bool> completed);
    }
}


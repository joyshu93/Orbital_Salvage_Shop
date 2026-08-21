using System;

namespace CurioClerk.Infrastructure.Ads
{
    public interface IRewardedAdClient
    {
        bool IsReady { get; }

        void SetRequestPermission(bool allowed);

        void Show(Action<RewardedAdResult> completed);
    }
}

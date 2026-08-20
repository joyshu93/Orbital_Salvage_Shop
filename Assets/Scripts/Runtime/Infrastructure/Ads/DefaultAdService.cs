using System;

namespace CurioClerk.Infrastructure.Ads
{
    public sealed class DefaultAdService : IAdService
    {
#if UNITY_EDITOR
        public bool IsRewardedReady => true;

        public void ShowRewarded(string placement, Action<bool> completed)
        {
            completed?.Invoke(true);
        }
#else
        public bool IsRewardedReady => false;

        public void ShowRewarded(string placement, Action<bool> completed)
        {
            completed?.Invoke(false);
        }
#endif
    }
}


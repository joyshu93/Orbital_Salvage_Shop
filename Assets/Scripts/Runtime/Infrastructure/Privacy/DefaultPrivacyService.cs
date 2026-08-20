using System;

namespace CurioClerk.Infrastructure.Privacy
{
    public sealed class DefaultPrivacyService : IPrivacyService
    {
#if UNITY_EDITOR
        public bool CanRequestAds => true;
#else
        public bool CanRequestAds => false;
#endif

        public bool PrivacyOptionsRequired => false;

        public void RequestConsent(Action<bool> completed)
        {
            completed?.Invoke(CanRequestAds);
        }

        public void ShowPrivacyOptions(Action<bool> completed)
        {
            completed?.Invoke(false);
        }
    }
}

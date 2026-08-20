using System;

namespace CurioClerk.Infrastructure.Privacy
{
    public interface IPrivacyService
    {
        bool CanRequestAds { get; }

        bool PrivacyOptionsRequired { get; }

        void RequestConsent(Action<bool> completed);

        void ShowPrivacyOptions(Action<bool> completed);
    }
}

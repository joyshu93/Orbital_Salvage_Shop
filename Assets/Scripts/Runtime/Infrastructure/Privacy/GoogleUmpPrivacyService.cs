#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

namespace CurioClerk.Infrastructure.Privacy
{
    public sealed class GoogleUmpPrivacyService : IPrivacyService
    {
        public GoogleUmpPrivacyService()
        {
            // UMP 11.3.0 also completes through MobileAds.RaiseAction.
#pragma warning disable 0618
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore 0618
        }

        public bool CanRequestAds => ConsentInformation.CanRequestAds();

        public bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public void RequestConsent(Action<bool> completed)
        {
            var updateHandled = false;
            var formHandled = false;
            ConsentInformation.Update(new ConsentRequestParameters(), _ =>
            {
                if (updateHandled)
                {
                    return;
                }

                updateHandled = true;
                ConsentForm.LoadAndShowConsentFormIfRequired(_ =>
                {
                    if (formHandled)
                    {
                        return;
                    }

                    formHandled = true;
                    completed?.Invoke(ConsentInformation.CanRequestAds());
                });
            });
        }

        public void ShowPrivacyOptions(Action<bool> completed)
        {
            var handled = false;
            ConsentForm.ShowPrivacyOptionsForm(_ =>
            {
                if (handled)
                {
                    return;
                }

                handled = true;
                completed?.Invoke(ConsentInformation.CanRequestAds());
            });
        }
    }
}
#endif

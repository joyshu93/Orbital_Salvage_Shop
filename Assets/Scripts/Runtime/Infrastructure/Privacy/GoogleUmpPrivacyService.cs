#if UNITY_ANDROID && !UNITY_EDITOR && !CURIO_OFFLINE_QA
using System;
using GoogleMobileAds.Ump.Api;
using CurioClerk.Infrastructure.Ads;

namespace CurioClerk.Infrastructure.Privacy
{
    public sealed class GoogleUmpPrivacyService : IPrivacyService
    {
#if DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA
        public bool QaForceEea { get; set; }
        public string QaStatus => $"status={ConsentInformation.ConsentStatus} options={ConsentInformation.PrivacyOptionsRequirementStatus} canRequest={CanRequestAds}";
#endif
        public GoogleUmpPrivacyService()
        {
            GoogleAdsCallbackDispatcher.Initialize();
        }

        public bool CanRequestAds => ConsentInformation.CanRequestAds();

        public bool PrivacyOptionsRequired =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public void RequestConsent(Action<bool> completed)
        {
            var updateHandled = false;
            var formHandled = false;
            var request = new ConsentRequestParameters();
#if DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA
            if (QaForceEea)
                request.ConsentDebugSettings = new ConsentDebugSettings { DebugGeography = DebugGeography.EEA };
            NativeAdsQaTrace.Record($"UMP update begin eea={QaForceEea}");
#endif
            ConsentInformation.Update(request, updateError =>
            {
                if (updateHandled)
                {
                    return;
                }

                updateHandled = true;
                NativeAdsQaTrace.Record($"UMP update error={updateError?.ErrorCode} message={updateError?.Message} canRequest={CanRequestAds}");
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formHandled)
                    {
                        return;
                    }

                    formHandled = true;
                    NativeAdsQaTrace.Record($"UMP required form completed error={formError?.ErrorCode} message={formError?.Message} canRequest={CanRequestAds} options={PrivacyOptionsRequired}");
                    completed?.Invoke(ConsentInformation.CanRequestAds());
                });
            });
        }

        public void ShowPrivacyOptions(Action<bool> completed)
        {
            var handled = false;
            NativeAdsQaTrace.Record("UMP privacy options begin");
            ConsentForm.ShowPrivacyOptionsForm(formError =>
            {
                if (handled)
                {
                    return;
                }

                handled = true;
                NativeAdsQaTrace.Record($"UMP privacy options completed error={formError?.ErrorCode} message={formError?.Message} canRequest={CanRequestAds} options={PrivacyOptionsRequired}");
                completed?.Invoke(ConsentInformation.CanRequestAds());
            });
        }
    }
}
#endif

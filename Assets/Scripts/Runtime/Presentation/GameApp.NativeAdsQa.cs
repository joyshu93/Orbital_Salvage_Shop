#if UNITY_EDITOR || (UNITY_ANDROID && DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA && !CURIO_OFFLINE_QA)
using System.Collections;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Qa;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed partial class GameApp
    {
        private NativeAdsQaSession _nativeQa;
        private bool _qaConsentPending;
        private string _qaLast = "none";
        private Coroutine _qaRefresh;
        private string QaText(string en, string ko) => _localizer.Locale == "ko" ? ko : en;

        public void ShowNativeAdsQa()
        {
            if (_nativeQa == null)
            {
                _nativeQa = new NativeAdsQaSession(_adService);
                _nativeQa.Recorded += message =>
                {
                    _qaLast = message;
                    NativeAdsQaTrace.Record(message);
                };
                _nativeQa.Begin(false);
            }
            _nativeQa.SetPermission(_canRequestAds && _privacy.CanRequestAds);
            ActiveScreen = AppScreen.Menu;
            var page = CreatePage("NativeAdsQaScreen");
            CreateText(page, "QaScopeNote", QaText("QA · Native SDK / memory-only rewards\nNo game save changes", "QA · 실제 SDK / 메모리 시험 보상\n게임 저장에는 반영되지 않습니다"), 27, Amber, TextAlignmentOptions.Center, new Vector2(.06f,.88f), new Vector2(.94f,.97f), true);
            var state = CreateText(page, "QaState", "", 24, Paper, TextAlignmentOptions.TopLeft, new Vector2(.07f,.67f), new Vector2(.93f,.87f));
            CreateButton(page, "QaCompletedSessionButton", QaText("New completed shift", "완료 교대 새로 준비"), new Vector2(.07f,.59f), new Vector2(.49f,.655f), Wine, Paper, () => _nativeQa.Begin(false), 22);
            CreateButton(page, "QaReviveSessionButton", QaText("New failed shift", "실패 교대 새로 준비"), new Vector2(.51f,.59f), new Vector2(.93f,.655f), Wine, Paper, () => _nativeQa.Begin(true), 22);
            var update = CreateButton(page, "QaConsentButton", QaText("UMP update / form", "UMP 갱신 / 폼"), new Vector2(.07f,.50f), new Vector2(.49f,.575f), Wine, Paper, () => QaConsent(false, false), 22);
            var eea = CreateButton(page, "QaEeaConsentButton", QaText("UMP test EEA form", "UMP 시험 EEA 폼"), new Vector2(.51f,.50f), new Vector2(.93f,.575f), Wine, Paper, () => QaConsent(true, false), 22);
            var privacy = CreateButton(page, "QaPrivacyButton", QaText("Privacy options / change or withdraw", "개인정보 폼 재표시 / 변경·철회"), new Vector2(.07f,.41f), new Vector2(.93f,.485f), Wine, Paper, () => QaConsent(false, true), 23);
            var reload = CreateButton(page, "QaReloadButton", QaText("Discard ad / load", "광고 해제 / 다시 로드"), new Vector2(.07f,.32f), new Vector2(.49f,.395f), Wine, Paper, QaReloadAd, 22);
            var show = CreateButton(page, "QaShowAdButton", QaText("Show sample / result", "샘플 표시 / 결과 확인"), new Vector2(.51f,.32f), new Vector2(.93f,.395f), Amber, Ink, () => _nativeQa.Show(), 22);
            CreateButton(page, "QaBlockButton", QaText("Block requests (QA only)", "요청 차단 (QA 전용)"), new Vector2(.07f,.235f), new Vector2(.93f,.305f), Wine, Paper, () => _nativeQa.SetPermission(false), 22);
            CreateText(page, "QaGateNote", QaText("UMP CanRequestAds controls loading.\nQA block is not UMP consent withdrawal.", "로드는 UMP CanRequestAds를 따릅니다.\nQA 요청 차단은 UMP 동의 철회가 아닙니다."), 21, Paper, TextAlignmentOptions.Center, new Vector2(.07f,.14f), new Vector2(.93f,.225f));
            var back = CreateButton(page, "QaBackButton", _localizer.Get("back"), new Vector2(.25f,.045f), new Vector2(.75f,.115f), Paper, Ink, () =>
            {
                if (_nativeQa.Pending || _qaConsentPending) return;
                if (_qaRefresh != null) StopCoroutine(_qaRefresh);
                _nativeQa.SetPermission(false);
                _adService.SetRequestPermission(_canRequestAds && _privacy.CanRequestAds);
                ShowMenu();
            });
            if (_qaRefresh != null) StopCoroutine(_qaRefresh);
            _qaRefresh = StartCoroutine(QaRefresh(state, update, eea, privacy, reload, show, back));
        }

        private IEnumerator QaRefresh(TMP_Text state, params Button[] actions)
        {
            while (state != null)
            {
                var consent = $"canRequest={_privacy.CanRequestAds} options={_privacy.PrivacyOptionsRequired}";
#if UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA
                if (_privacy is Infrastructure.Privacy.GoogleUmpPrivacyService ump) consent = ump.QaStatus;
#endif
                state.text = $"UMP {consent}\n" +
                    QaText($"Ready={_adService.IsRewardedReady} pending={_nativeQa.Pending}\nCoins={_nativeQa.Coins} hearts={_nativeQa.Hearts} awards={_nativeQa.Awards} completions={_nativeQa.Completions}\n", $"준비={_adService.IsRewardedReady} 대기={_nativeQa.Pending}\n코인={_nativeQa.Coins} 하트={_nativeQa.Hearts} 보상={_nativeQa.Awards} 완료={_nativeQa.Completions}\n") +
                    $"{_qaLast}\nSDK: {NativeAdsQaTrace.LastEvent}";
                foreach (var action in actions) if (action != null) action.interactable = _adConsentResolved && !_qaConsentPending && !_nativeQa.Pending;
                // Menu navigation remains available while startup consent is unresolved.
                actions[actions.Length - 1].interactable = !_qaConsentPending && !_nativeQa.Pending;
                yield return new WaitForSecondsRealtime(.25f);
            }
        }

        private void QaConsent(bool eea, bool privacyOptions)
        {
            if (!_adConsentResolved || _qaConsentPending || _nativeQa.Pending) return;
            _qaConsentPending = true;
            _nativeQa.SetPermission(false);
#if UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA
            if (_privacy is Infrastructure.Privacy.GoogleUmpPrivacyService ump) ump.QaForceEea = eea;
#endif
            void Done(bool allowed)
            {
                _qaConsentPending = false;
                _adConsentResolved = true;
                _canRequestAds = allowed && _privacy.CanRequestAds;
                _nativeQa.SetPermission(_canRequestAds);
            }
            if (privacyOptions) _privacy.ShowPrivacyOptions(Done);
            else _privacy.RequestConsent(Done);
        }

        private void QaReloadAd()
        {
            if (!_adConsentResolved || _nativeQa.Pending || _qaConsentPending) return;
            _nativeQa.SetPermission(false);
            _nativeQa.SetPermission(_canRequestAds && _privacy.CanRequestAds);
        }
    }
}
#endif

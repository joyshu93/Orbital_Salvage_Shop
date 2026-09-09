using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CurioClerk.Content;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Progression;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Infrastructure;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Feedback;
using CurioClerk.Infrastructure.Privacy;
using CurioClerk.Infrastructure.Save;
using CurioClerk.Infrastructure.Time;
using CurioClerk.Localization;
using CurioClerk.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class GameAppPlayModeTests
    {
        [UnityTest]
        public IEnumerator NativeAdsQa_UsesIsolatedCoinsAndReturnsToUnchangedSave()
        {
            var ad = new DeferredAdService();
            var app = CreateApp(ad, new ControllablePrivacyService());
            yield return null;
            var before = JsonUtility.ToJson(app.SaveData);
            var entry = typeof(GameApp).GetMethod("ShowNativeAdsQa");
            Assert.That(entry, Is.Not.Null, "A QA-only SDK validation screen is required.");
            entry.Invoke(app, null);
            yield return null;
            Assert.That(GameObject.Find("NativeAdsQaScreen"), Is.Not.Null);
            Assert.That(GameObject.Find("QaShowAdButton"), Is.Not.Null);
            Assert.That(GameObject.Find("QaEeaConsentButton"), Is.Not.Null);
            Assert.That(GameObject.Find("QaPrivacyButton"), Is.Not.Null);
            Assert.That(ObjectText("QaScopeNote"), Does.Contain("QA"));
            var session = (CurioClerk.Qa.NativeAdsQaSession)typeof(GameApp)
                .GetField("_nativeQa", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);
            GameObject.Find("QaShowAdButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            ad.Emit(RewardedAdResult.Earned);
            ad.Emit(RewardedAdResult.Earned);
            Assert.That(session.Coins, Is.EqualTo(80));
            Assert.That(session.Awards, Is.EqualTo(1));
            Assert.That(JsonUtility.ToJson(app.SaveData), Is.EqualTo(before));
            GameObject.Find("QaReviveSessionButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            GameObject.Find("QaShowAdButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            ad.Emit(RewardedAdResult.Earned);
            ad.Emit(RewardedAdResult.Earned);
            Assert.That(session.Hearts, Is.EqualTo(1));
            Assert.That(session.Awards, Is.EqualTo(1));
            GameObject.Find("QaCompletedSessionButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.That(JsonUtility.ToJson(app.SaveData), Is.EqualTo(before));
            GameObject.Find("QaBackButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(JsonUtility.ToJson(app.SaveData), Is.EqualTo(before));
        }

        [UnityTest]
        public IEnumerator NativeAdsQa_StartupConsentCannotBeOverlappedByQaConsent()
        {
            var privacy = new DeferredConsentPrivacyService();
            var app = CreateApp(new DeferredAdService(), privacy);
            yield return null;
            app.ShowNativeAdsQa();
            GameObject.Find("QaEeaConsentButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.That(privacy.RequestCalls, Is.EqualTo(1), "QA must not issue another UMP update during startup.");
            Assert.That(GameObject.Find("QaConsentButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator NativeAdsQa_StartupCompletionEnablesTheAlreadyOpenQaSession()
        {
            var privacy = new DeferredConsentPrivacyService();
            var ad = new DeferredAdService();
            var app = CreateApp(ad, privacy);
            yield return null;
            app.ShowNativeAdsQa();
            privacy.CanRequestAds = true;
            privacy.CompleteConsent();
            yield return new WaitForSecondsRealtime(.3f);
            GameObject.Find("QaShowAdButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.That(ad.LastPlacement, Is.EqualTo("shift_complete_double"));
            ad.Emit(RewardedAdResult.Earned);
            var session = (CurioClerk.Qa.NativeAdsQaSession)typeof(GameApp)
                .GetField("_nativeQa", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);
            Assert.That(session.Coins, Is.EqualTo(80));
        }

        [SetUp]
        public void SetUp()
        {
            ServiceFactory.ResetTestServices();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceFactory.ResetTestServices();
            foreach (var app in UnityEngine.Object.FindObjectsByType<GameApp>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(app.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator App_StartsAtIncidentLedMenuAndBuildsAPlayableFreeShiftLayout()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 0, false);
            SetEnglishLocale(app);
            app.ShowMenu();

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(GameObject.Find("CurioClerkCanvas"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentButton"), Is.Not.Null);
            Assert.That(GameObject.Find("FreeShiftButton"), Is.Not.Null);
            Assert.That(GameObject.Find("SettingsButton"), Is.Not.Null);
            Assert.That(GameObject.Find("StartShiftButton"), Is.Null);
            Assert.That(GameObject.Find("DailyShiftButton"), Is.Null);
            Assert.That(GameObject.Find("CollectionButton"), Is.Not.Null);
            Assert.That(GameObject.Find("Progress"), Is.Null);
            Assert.That(GameObject.Find("RewardedAdButton"), Is.Null);
            Assert.That(Session(app), Is.Null, "No shift may start before the player chooses an entry point.");
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            var fontProperty = textType.GetProperty("font");
            var titleText = GameObject.Find("Title").GetComponent(textType);
            var startButtonText = GameObject.Find("IncidentButton").GetComponentInChildren(textType);
            var titleFont = fontProperty.GetValue(titleText) as UnityEngine.Object;
            var startButtonFont = fontProperty.GetValue(startButtonText) as UnityEngine.Object;
            Assert.That(titleFont, Is.Not.Null);
            Assert.That(startButtonFont, Is.Not.Null);
            Assert.That(titleFont.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(startButtonFont.name, Does.StartWith("NotoSansKR"));
            Assert.That(titleFont, Is.Not.SameAs(startButtonFont));

            app.StartNewShift(4242);
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(GameObject.Find("CurrentArtifactCard"), Is.Not.Null);
            Assert.That(GameObject.Find("NextPreview0"), Is.Not.Null);
            Assert.That(GameObject.Find("NextPreview1"), Is.Not.Null);
            Assert.That(GameObject.Find("HoldButton"), Is.Not.Null);
            Assert.That(GameObject.Find("RepairButton"), Is.Not.Null);
            Assert.That(GameObject.Find("StorageButton"), Is.Not.Null);
            Assert.That(GameObject.Find("VaultButton"), Is.Not.Null);
            Assert.That(GameObject.Find("DocketProgress"), Is.Not.Null);
            Assert.That(GameObject.Find("DocketCounter"), Is.Not.Null);
            Assert.That(GameObject.Find("DocketStampRepair"), Is.Not.Null);
            Assert.That(GameObject.Find("DocketStampStorage"), Is.Not.Null);
            Assert.That(GameObject.Find("DocketStampVault"), Is.Not.Null);
            var dragType = Type.GetType("CurioClerk.Presentation.ArtifactDragHandler, CurioClerk.Runtime");
            Assert.That(dragType, Is.Not.Null, "Missing card drag interaction type.");
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent(dragType), Is.Not.Null,
                "The current artifact card must support drag-to-sort.");

        }

        [UnityTest]
        public IEnumerator Menu_LabelsNewContinuedAndCompletedIncidentInBothLanguages()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;

            SetIncidentProgress(app, 0, false);
            SetLocale(app, "en");
            app.ShowMenu();
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("The Unmelting Ice"));
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("Begin First Investigation"));

            SetIncidentProgress(app, 2, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("녹지 않는 얼음"));
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("조사 계속 · 3/5"));
            Assert.That(ObjectText("IncidentState"), Is.EqualTo("조사 진행 중"));

            SetIncidentProgress(app, 5, true);
            app.ShowMenu();
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("첫 조사 시작"));
            Assert.That(ObjectText("ReplayIncident_unmelting-ice"), Is.EqualTo("사건 다시보기"));
            Assert.That(GameObject.Find("ReplayIncident_unmelting-ice").GetComponent<UnityEngine.UI.Button>().interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator CompletedIncident_ReplayStartsAtFirstStageWithoutChangingSavedProgress()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 5, true);
            app.SaveData.incidentStageRecords.Add(new IncidentStageRecord
            {
                stageId = "ice-01-crack",
                bestQuality = (int)IncidentQuality.Resonant
            });
            SetLocale(app, "ko");
            app.ShowMenu();
            var saveStore = new RecordingSaveStore();
            typeof(GameApp)
                .GetField("_saveStore", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, saveStore);

            ClickButton("ReplayIncident_unmelting-ice");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("첫날이죠? 이것만 기억하세요. 이곳에 남겨진 물건은 결코 침묵하지 않습니다."));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(5));
            Assert.That(app.SaveData.completedIncidentIds, Does.Contain("unmelting-ice"));
            Assert.That(app.SaveData.incidentStageRecords[0].bestQuality,
                Is.EqualTo((int)IncidentQuality.Resonant));

            yield return AdvanceNarrativeToShift(app);
            yield return CompleteActiveShift(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(saveStore.SaveCalls, Is.Zero,
                "Replaying a resolved incident must not cross the persistence boundary.");
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(5));
            Assert.That(app.SaveData.completedIncidentIds, Does.Contain("unmelting-ice"));
            Assert.That(app.SaveData.incidentStageRecords, Has.Count.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords[0].bestQuality,
                Is.EqualTo((int)IncidentQuality.Resonant));
        }

        [UnityTest]
        public IEnumerator Menu_FirstResolved_ShowsRememberingRainAsCurrentAndIceAsReplayRecord()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 5, true);
            SetLocale(app, "ko");

            app.ShowMenu();

            Assert.That(GameObject.Find("CurrentIncidentCard"), Is.Not.Null);
            Assert.That(ObjectText("IncidentState"), Is.EqualTo("첫 조사 시작"));
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("기억하는 비"));
            Assert.That(GameObject.Find("IncidentArtwork"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentButton"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentWaitingState"), Is.Null);
            Assert.That(GameObject.Find("ResolvedIncidentCard_unmelting-ice"), Is.Not.Null);
            Assert.That(GameObject.Find("ReplayIncident_unmelting-ice"), Is.Not.Null);
            Assert.That(GameObject.Find("CollectionButton"), Is.Not.Null);
            Assert.That(GameObject.Find("FreeShiftButton"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator Menu_RememberingRainSecondShift_ShowsContinuableFiveStageProgress()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetSaveString(app, "activeIncidentId", "remembering-rain");
            SetSaveInt(app, "activeIncidentStage", 1);
            var completedIds = SaveStringList(app, "completedIncidentIds");
            completedIds.Clear();
            completedIds.Add("unmelting-ice");
            app.SaveData.incidentStageRecords.Clear();
            app.SaveData.incidentStageRecords.Add(new IncidentStageRecord
            {
                stageId = "rain-01-voices",
                bestQuality = (int)IncidentQuality.Precise
            });
            SetLocale(app, "ko");

            app.ShowMenu();

            Assert.That(GameObject.Find("CurrentIncidentCard"), Is.Not.Null);
            Assert.That(ObjectText("IncidentState"), Is.EqualTo("조사 진행 중"));
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("기억하는 비"));
            Assert.That(GameObject.Find("IncidentArtwork"), Is.Not.Null);
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("조사 계속 · 2/5"));
            Assert.That(GameObject.Find("IncidentWaitingState"), Is.Null);
            Assert.That(GameObject.Find("ResolvedIncidentCard_unmelting-ice"), Is.Not.Null);
            Assert.That(GameObject.Find("ReplayIncident_unmelting-ice"), Is.Not.Null);
            Assert.That(GameObject.Find("CollectionButton"), Is.Not.Null);
            Assert.That(GameObject.Find("FreeShiftButton"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator Menu_UnlockedZeroStageIncidentShowsStableReadOnlyPreviewWithoutRunner()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            var existing = ContentCatalog.CreateIncidents();
            var resolvedSecond = new IncidentDefinition(
                existing[1].Id,
                existing[1].Title,
                existing[1].LeadArtifactId,
                existing[1].BoardVisualCue,
                completesWhenAllStagesCompleted: true,
                awaitingContentClue: null,
                stages: existing[1].Stages);
            var preview = new IncidentDefinition(
                "preview-incident",
                new LocalizedCopy("Preview Incident", "예고 사건"),
                "backward-candle",
                IncidentVisualCue.AmberWarmth,
                completesWhenAllStagesCompleted: false,
                awaitingContentClue: new LocalizedCopy("The clock is waiting.", "시계가 기다리고 있다."),
                stages: Array.Empty<IncidentStageDefinition>());
            typeof(GameApp).GetField("_incidents", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, new[] { existing[0], resolvedSecond, preview });
            app.SaveData.completedIncidentIds.Clear();
            app.SaveData.completedIncidentIds.Add(existing[0].Id);
            app.SaveData.completedIncidentIds.Add(existing[1].Id);
            SetLocale(app, "ko");

            app.ShowMenu();

            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("예고 사건"));
            Assert.That(ObjectText("IncidentClue"), Is.EqualTo("시계가 기다리고 있다."));
            Assert.That(GameObject.Find("IncidentButton"), Is.Null);
            Assert.That(GameObject.Find("IncidentWaitingState"), Is.Not.Null);
            Assert.That(typeof(GameApp).GetField("_incidentRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.Null);

            Assert.DoesNotThrow(app.StartIncident);
            app.ShowMenu();
            yield return null;

            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("예고 사건"));
            Assert.That(GameObject.Find("IncidentButton"), Is.Null);
            Assert.That(typeof(GameApp).GetField("_incidentRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.Null);
        }

        [UnityTest]
        public IEnumerator RememberingRain_FirstShiftUsesApprovedKoreanOpeningAndQueue()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 5, true);
            SetLocale(app, "ko");
            app.ShowMenu();

            ClickButton("IncidentButton");
            yield return null;

            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("자정부터 봉인된 우산 안에서 비가 내리고 있어요. 절대 열지 마세요."));
            yield return AdvanceNarrativeToShift(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
        }

        [UnityTest]
        public IEnumerator RememberingRain_FirstShiftCompletionPersistsAndStartsSecondShift()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 5, true);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return AdvanceNarrativeToShift(app);
            yield return CompleteActiveShift(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(app.SaveData.activeIncidentId, Is.EqualTo("remembering-rain"));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords.Select(record => record.stageId), Does.Contain("rain-01-voices"));
            Assert.That(app.SaveData.completedIncidentIds, Does.Not.Contain("remembering-rain"));
            Assert.That(ObjectText("IncidentOutroBody"),
                Is.EqualTo("비가 멎습니다. 봉인 안쪽에 빗방울 하나만 남았습니다."));

            ClickButton("IncidentOutroContinueButton");
            Assert.That(ObjectText("IncidentOutroBody"),
                Is.EqualTo("빗방울이 속삭입니다. “돌아오겠다고 약속했잖아.” 선임 관리인은 대답하지 않습니다."));
            ClickButton("IncidentOutroContinueButton");
            Assert.That(ObjectText("NextStageButton"), Is.EqualTo("다음 교대"));
            ClickButton("NextStageButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"), Is.EqualTo(
                "비가 장부의 지금 이름을 모두 씻어 냈어요. 그 아래에서 오래된 이름들이 떠오릅니다."));
        }

        [UnityTest]
        public IEnumerator RememberingRain_FourthShiftResultContinuesAtFifthShift()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return AdvanceNarrativeToShift(app);
            yield return CompleteActiveShift(app);

            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(4));
            Assert.That(app.SaveData.incidentStageRecords.Select(record => record.stageId),
                Does.Contain("rain-04-dry-order"));
            yield return AdvanceIncidentOutroToNextAction();
            Assert.That(ObjectText("NextStageButton"), Is.EqualTo("다음 교대"));

            ClickButton("NextStageButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"), Is.EqualTo(
                "편지 조각과 비가 마지막 밤 하나를 되풀이하고 있어요. 이번에는 순서대로 듣겠습니다."));
        }

        [UnityTest]
        public IEnumerator RememberingRain_FinalShiftResolvesOnceAndRevealsReadOnlySuccessor()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 4, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return AdvanceNarrativeToShift(app);
            var saveStore = new RecordingSaveStore();
            typeof(GameApp).GetField("_saveStore", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, saveStore);

            yield return CompleteActiveShift(app);

            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(5));
            Assert.That(app.SaveData.completedIncidentIds.Count(id => id == "remembering-rain"), Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords.Count(record => record.stageId == "rain-05-testimony"),
                Is.EqualTo(1));
            Assert.That((bool)typeof(GameApp)
                .GetField("_pendingIncidentBoardReveal", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.True);
            Assert.That(saveStore.SaveCalls, Is.EqualTo(1));

            InvokePrivate(app, "ShowIncidentResults");
            Assert.That(saveStore.SaveCalls, Is.EqualTo(1));
            Assert.That(app.SaveData.completedIncidentIds.Count(id => id == "remembering-rain"), Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords.Count(record => record.stageId == "rain-05-testimony"),
                Is.EqualTo(1));

            yield return AdvanceIncidentOutroToNextAction();
            ClickButton("NextStageButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("1분 앞선 저녁"));
            Assert.That(ObjectText("IncidentState"), Is.EqualTo("다음 교대 준비 중"));
            Assert.That(ObjectText("IncidentClue"), Is.EqualTo("이끼가 2시 17분을 향해 자라고 있다."));
            Assert.That(GameObject.Find("IncidentButton"), Is.Null);
            Assert.That(GameObject.Find("IncidentWaitingState"), Is.Not.Null);
            Assert.That(GameObject.Find("ReplayIncident_remembering-rain"), Is.Not.Null);
            Assert.That(GameObject.Find("ResolvedIncidentCard_remembering-rain"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator RememberingRain_EnglishEndingShowsTheCompletedCase()
        {
            yield return AssertRememberingRainEndingShowsCompletedCase("en", "CASE RESOLVED", "The Remembering Rain");
        }

        [UnityTest]
        public IEnumerator Menu_TwoCompletedCasesRemainAboveUtilityButtons()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 5, true);
            foreach (var locale in new[] { "en", "ko" })
            {
                SetLocale(app, locale);
                app.ShowMenu();
                yield return null;
                foreach (var id in new[] { "unmelting-ice", "remembering-rain" })
                {
                    var card = FindRect("ResolvedIncidentCard_" + id);
                    foreach (var buttonName in new[] { "CollectionButton", "FreeShiftButton", "SettingsButton" })
                    {
                        Assert.That(card.anchorMin.y, Is.GreaterThan(FindRect(buttonName).anchorMax.y),
                            $"{locale}: {id} and its replay action must not be covered by {buttonName}.");
                    }
                }
                Assert.That(FindRect("CurrentIncidentCard").anchorMin.y,
                    Is.GreaterThan(FindRect("ResolvedIncidentCard_unmelting-ice").anchorMax.y));
            }
        }

        [UnityTest]
        public IEnumerator RememberingRain_KoreanEndingShowsTheCompletedCase()
        {
            yield return AssertRememberingRainEndingShowsCompletedCase("ko", "사건 해결", "기억하는 비");
        }

        private static IEnumerator AssertRememberingRainEndingShowsCompletedCase(string locale, string resolved, string title)
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginRememberingRainShift(app, 4, locale);
            yield return CompleteActiveShift(app);

            Assert.That(ObjectText("IncidentEndingTitle"), Is.EqualTo(resolved));
            Assert.That(ObjectText("IncidentEndingHook"), Is.EqualTo(title));
            Assert.That(GameObject.Find("IncidentResultArtifact").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("paper-fish"));
            Assert.That(GameObject.Find("IncidentEndingIce"), Is.Null);
            Assert.That(GameObject.Find("IncidentEndingUmbrella"), Is.Null);
            yield return AdvanceIncidentOutroToNextAction();
            Assert.That(ObjectText("NextStageButton"),
                Is.EqualTo(locale == "ko" ? "사건 보드로 돌아가기" : "Return to Incident Board"));
        }

        [UnityTest]
        public IEnumerator RememberingRain_KoreanAcceptanceRouteConnectsAllFiveShifts()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 0, false);
            SetLocale(app, "ko");
            app.ShowMenu();

            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("기억하는 비"));
            ClickButton("IncidentButton");
            yield return null;

            var protectedArtifacts = new[]
            {
                "paper-fish",
                "rain-jar",
                "paper-fish",
                "moon-umbrella",
                "paper-fish"
            };
            var stageHooks = new[]
            {
                "오래전에 지운",
                "화요일",
                "야간 보관소",
                "발신",
                "나는 네 선임보다 먼저 일한 관리인이야"
            };
            var fullStory = new List<string>();

            for (var stageIndex = 0; stageIndex < protectedArtifacts.Length; stageIndex++)
            {
                var stageStory = new List<string>();
                for (var safety = 0; safety < 8 && app.ActiveScreen == AppScreen.Narrative; safety++)
                {
                    stageStory.Add(ObjectText("NarrativeBody"));
                    AssertTextIsReadable("NarrativeBody");
                    AssertButtonIsVisible("NarrativeContinueButton");
                    ClickButton("NarrativeContinueButton");
                    yield return null;
                }

                Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift),
                    $"Rain stage {stageIndex + 1} must begin after its authored Korean intro.");

                var heldProtectedArtifact = false;
                var interludeCount = 0;
                for (var safety = 0; safety < 80 && SessionState(app) == "Active"; safety++)
                {
                    if (!heldProtectedArtifact &&
                        CurrentArtifactId(app) == protectedArtifacts[stageIndex] &&
                        ((ShiftSession)Session(app)).CanHold)
                    {
                        app.HoldCurrent();
                        heldProtectedArtifact = true;
                    }
                    else
                    {
                        var expected = ExpectedDestination(app);
                        var destinationButton = GameObject.Find(DestinationButtonName(expected))
                            .GetComponent<UnityEngine.UI.Button>();
                        if (destinationButton.interactable)
                        {
                            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
                        }
                        else
                        {
                            Assert.That(((ShiftSession)Session(app)).CanHold, Is.True,
                                "A sealed destination must be resolved by swapping through Hold.");
                            app.HoldCurrent();
                        }
                    }

                    yield return WaitForAcceptanceTransition(
                        app,
                        stageIndex,
                        stageStory,
                        count => interludeCount += count);
                }

                Assert.That(heldProtectedArtifact, Is.True,
                    $"Rain stage {stageIndex + 1} must deliberately protect {protectedArtifacts[stageIndex]} in Hold.");
                Assert.That(interludeCount, Is.EqualTo(3),
                    $"Rain stage {stageIndex + 1} must present exactly three one-tap docket interludes.");
                Assert.That(SessionState(app), Is.EqualTo("Completed"));
                Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));

                while (GameObject.Find("NextStageButton") == null)
                {
                    stageStory.Add(ObjectText("IncidentOutroBody"));
                    AssertTextIsReadable("IncidentOutroBody");
                    AssertButtonIsVisible("IncidentOutroContinueButton");
                    ClickButton("IncidentOutroContinueButton");
                    yield return null;
                }

                Assert.That(string.Join("\n", stageStory), Does.Contain(stageHooks[stageIndex]),
                    $"Rain stage {stageIndex + 1} must communicate its fixed recall hook in Korean.");
                fullStory.AddRange(stageStory);
                AssertButtonIsVisible("NextStageButton");
                ClickButton("NextStageButton");
                yield return null;
            }

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("1분 앞선 저녁"));
            Assert.That(ObjectText("IncidentClue"), Is.EqualTo("이끼가 2시 17분을 향해 자라고 있다."));
            AssertTextIsReadable("IncidentTitle");
            AssertTextIsReadable("IncidentClue");
            Assert.That(GameObject.Find("IncidentButton"), Is.Null,
                "The successor teaser must remain read-only.");

            var completeStory = string.Join("\n", fullStory);
            Assert.That(completeStory, Does.Contain("나는 네 선임보다 먼저 일한 관리인이야"));
            Assert.That(completeStory, Does.Contain("다음에 올 사람을 위해 진실을 남겼어"));
            Assert.That(completeStory, Does.Contain("바로 이 작업대입니다"));
            yield return new WaitForSecondsRealtime(2f);
            yield return CaptureAcceptanceFrame("rain-06-successor-teaser.png");
        }

        [UnityTest]
        public IEnumerator RememberingRain_ReplayDoesNotDuplicateCompletionRewardsOrRecords()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 5, true);
            SetLocale(app, "en");
            app.ShowMenu();
            var recordsBefore = app.SaveData.incidentStageRecords.Count;
            var completedBefore = app.SaveData.completedIncidentIds.Count;
            var coinsBefore = app.SaveData.coins;
            var saveStore = new RecordingSaveStore();
            typeof(GameApp).GetField("_saveStore", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, saveStore);

            ClickButton("ReplayIncident_remembering-rain");
            yield return AdvanceNarrativeToShift(app);
            yield return CompleteActiveShift(app);

            Assert.That(saveStore.SaveCalls, Is.Zero);
            Assert.That(app.SaveData.incidentStageRecords.Count, Is.EqualTo(recordsBefore));
            Assert.That(app.SaveData.completedIncidentIds.Count, Is.EqualTo(completedBefore));
            Assert.That(app.SaveData.coins, Is.EqualTo(coinsBefore));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator RememberingRain_OldAndMidCaseSavesResumeAndLanguageSwitchPreservesState()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetRememberingRainProgress(app, 1, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("조사 계속 · 2/5"));
            ClickButton("IncidentButton");
            yield return null;
            Assert.That(ObjectText("NarrativeBody"), Is.EqualTo(
                "비가 장부의 지금 이름을 모두 씻어 냈어요. 그 아래에서 오래된 이름들이 떠오릅니다."));

            SetRememberingRainProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("조사 계속 · 4/5"));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(3));

            SetLocale(app, "en");
            app.ShowMenu();
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("The Remembering Rain"));
            Assert.That(ObjectText("IncidentButton"), Is.EqualTo("Continue Investigation · 4/5"));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(3));
            ClickButton("IncidentButton");
            yield return null;
            Assert.That(ObjectText("NarrativeBody"), Does.Contain("perfectly dry order"));
        }

        [UnityTest]
        public IEnumerator ResolvedIceReplay_DoesNotMoveRememberingRainProgress()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetSaveString(app, "activeIncidentId", "remembering-rain");
            SetSaveInt(app, "activeIncidentStage", 1);
            var completedIds = SaveStringList(app, "completedIncidentIds");
            completedIds.Clear();
            completedIds.Add("unmelting-ice");
            app.SaveData.incidentStageRecords.Clear();
            app.SaveData.incidentStageRecords.Add(new IncidentStageRecord
            {
                stageId = "rain-01-voices",
                bestQuality = (int)IncidentQuality.Precise
            });
            app.ShowMenu();

            ClickButton("ReplayIncident_unmelting-ice");
            yield return AdvanceNarrativeToShift(app);
            yield return CompleteActiveShift(app);

            Assert.That(app.SaveData.activeIncidentId, Is.EqualTo("remembering-rain"));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(1));
            Assert.That(app.SaveData.completedIncidentIds, Does.Not.Contain("remembering-rain"));
            Assert.That(app.SaveData.incidentStageRecords.Select(record => record.stageId),
                Is.EqualTo(new[] { "rain-01-voices" }));
        }

        [UnityTest]
        public IEnumerator IncidentBoardTransition_DisableAppliesStaticFinalState()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 4, "en");
            yield return CompleteActiveShift(app);
            ClickButton("IncidentOutroContinueButton");
            ClickButton("NextStageButton");
            yield return null;

            var currentCard = GameObject.Find("CurrentIncidentCard");
            var resolvedCard = GameObject.Find("ResolvedIncidentCard_unmelting-ice");
            Assert.That(currentCard, Is.Not.Null);
            Assert.That(resolvedCard, Is.Not.Null);
            Assert.That(currentCard.GetComponent<CanvasGroup>(), Is.Not.Null);
            Assert.That(resolvedCard.GetComponent<CanvasGroup>(), Is.Not.Null);

            app.gameObject.SetActive(false);
            yield return null;
            app.gameObject.SetActive(true);
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(currentCard.activeInHierarchy, Is.True);
            Assert.That(resolvedCard.activeInHierarchy, Is.True);
            Assert.That(currentCard.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(resolvedCard.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(currentCard.GetComponent<RectTransform>().localScale, Is.EqualTo(Vector3.one));
            Assert.That(resolvedCard.GetComponent<RectTransform>().localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator IncidentOpening_IsLargeReadableKoreanNarrativeThenStartsAuthoredShift()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 0, false);
            SetLocale(app, "ko");
            app.ShowMenu();

            ClickButton("IncidentButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeSpeaker"), Is.EqualTo("선임 관리인"));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("첫날이죠? 이것만 기억하세요. 이곳에 남겨진 물건은 결코 침묵하지 않습니다."));
            Assert.That(FindText("NarrativeBody").fontSize, Is.InRange(36f, 42f));
            var portrait = FindRect("SeniorClerkPortrait");
            Assert.That(portrait.anchorMax.y - portrait.anchorMin.y, Is.GreaterThanOrEqualTo(0.44f));
            var continueRect = FindRect("NarrativeContinueButton");
            Assert.That(continueRect.anchorMax.x - continueRect.anchorMin.x, Is.GreaterThanOrEqualTo(0.84f));
            Assert.That(continueRect.anchorMax.y - continueRect.anchorMin.y, Is.GreaterThanOrEqualTo(0.10f));
            Assert.That(GameObject.Find("CurrentArtifactCard"), Is.Null);
            Assert.That(GameObject.Find("RepairButton"), Is.Null);

            ClickButton("NarrativeContinueButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("이 얼음은 녹기를 거부합니다. 서리가 선반에 닿기 전에 오늘 밤 물건들을 분류하세요."));
            ClickButton("NarrativeContinueButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("장부마다 세 책상의 인장을 하나씩 채웁니다. 이미 찍힌 곳의 물건은 보류에서 지키세요."));
            ClickButton("NarrativeContinueButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("unmelting-ice"));
            Assert.That(GameObject.Find("TutorialCoachPanel"), Is.Null,
                "The incident opening must teach in context instead of routing through the old tutorial wall.");
            var stageRun = typeof(GameApp)
                .GetField("_incidentStageRun", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app) as IncidentStageRun;
            Assert.That(stageRun, Is.Not.Null);
            Assert.That(stageRun.StageId, Is.EqualTo("ice-01-crack"));
        }

        [UnityTest]
        public IEnumerator IncidentOpening_ContinuesFromRestoredStageInsteadOfRestartingTheCase()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 2, false);
            SetLocale(app, "en");
            app.ShowMenu();

            ClickButton("IncidentButton");
            yield return null;

            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("The missing leaf is inside this watch, dated tomorrow. It is both frosted and temporal—time outranks frost."));
            ClickButton("NarrativeContinueButton");
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            var stageRun = typeof(GameApp)
                .GetField("_incidentStageRun", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app) as IncidentStageRun;
            Assert.That(stageRun.StageId, Is.EqualTo("ice-03-tomorrow"));
        }

        [UnityTest]
        public IEnumerator IncidentShift_UsesAuthoredJudgmentLayoutAndLocalizedFrost()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            yield return AdvanceNarrativeToShift(app);
            Canvas.ForceUpdateCanvases();

            var queue = PlannedQueue(app);
            Assert.That(queue.Count, Is.EqualTo(12));
            var authoredStage = ContentCatalog.CreateIncidents()[0].Stages[3];
            for (var index = 0; index < queue.Count; index++)
            {
                Assert.That(ArtifactId(queue[index]), Is.EqualTo(authoredStage.Queue[index].ArtifactId),
                    $"Incident queue item {index + 1} must preserve the authored order.");
            }

            var activeRules = (IReadOnlyList<SortingRule>)typeof(GameApp)
                .GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            Assert.That(activeRules.Count, Is.EqualTo(authoredStage.Rules.Count));
            for (var index = 0; index < activeRules.Count; index++)
            {
                Assert.That(activeRules[index].Id, Is.EqualTo(authoredStage.Rules[index].Id),
                    $"Incident rule {index + 1} must preserve authored priority.");
            }

            var destinations = new HashSet<Destination>();
            var engine = new RuleEngine();
            foreach (Artifact artifact in queue)
            {
                destinations.Add(engine.Resolve(artifact, activeRules));
            }

            Assert.That(destinations, Is.EquivalentTo(new[]
            {
                Destination.Repair,
                Destination.Storage,
                Destination.Vault
            }));

            var current = (Artifact)Session(app).GetType()
                .GetProperty("CurrentArtifact")
                .GetValue(Session(app));
            Assert.That(current.Traits & ArtifactTraits.Frosted, Is.EqualTo(ArtifactTraits.Frosted));
            ArtifactContent baseIce = null;
            foreach (var artifact in ContentCatalog.CreateArtifacts())
            {
                if (artifact.Id == "unmelting-ice")
                {
                    baseIce = artifact;
                    break;
                }
            }

            Assert.That(baseIce, Is.Not.Null);
            Assert.That(baseIce.Traits & ArtifactTraits.Frosted, Is.EqualTo(ArtifactTraits.None),
                "Incident frost must not leak back into the base artifact catalog.");
            Assert.That(ObjectText("ArtifactTraits"), Does.Contain("서리 묻음"));
            var highlightedRules = ObjectText("RuleList");
            Assert.That(highlightedRules, Does.Contain("<color=#E0A24B><b>1."),
                "Temporal is the first matching rule and must be highlighted before input.");
            Assert.That(highlightedRules.Split(
                    new[] { "<color=#E0A24B><b>" },
                    StringSplitOptions.None).Length - 1,
                Is.EqualTo(1), "Only the deciding rule may be highlighted.");
            Assert.That(HasEnabledOutline("RepairButton"), Is.False);
            Assert.That(HasEnabledOutline("StorageButton"), Is.False);
            Assert.That(HasEnabledOutline("VaultButton"), Is.False,
                "An incident may emphasize the deciding rule, but must not reveal the destination.");
            Assert.That(ObjectText("HoldButton"), Is.EqualTo("보호 보류"));
            Assert.That(GameObject.Find("IncidentFrostOverlay"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentFrostOverlay").GetComponent<UnityEngine.UI.Image>().enabled,
                Is.True);

            var card = FindRect("CurrentArtifactCard");
            var artwork = FindRect("ArtifactIllustration");
            var rules = FindRect("RulesPanel");
            var destination = FindRect("RepairButton");
            var hold = FindRect("HoldButton");
            Assert.That(card.anchorMax.y - card.anchorMin.y, Is.GreaterThanOrEqualTo(0.38f));
            Assert.That(artwork.anchorMax.x - artwork.anchorMin.x, Is.GreaterThanOrEqualTo(0.45f));
            Assert.That(rules.rect.height, Is.LessThan(card.rect.height));
            Assert.That(destination.rect.height, Is.GreaterThanOrEqualTo(110f));
            Assert.That(hold.anchorMin.y, Is.GreaterThanOrEqualTo(destination.anchorMax.y),
                "Protective Hold must remain directly above the one-hand destination row.");
        }

        [UnityTest]
        public IEnumerator FrozenSeal_RequiresHoldingTheWatchAfterTheIceUsesVault()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            yield return AdvanceNarrativeToShift(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("unmelting-ice"));

            ChooseDestination(app, (int)Destination.Vault);
            yield return WaitForFilingTransition(app);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("mossy-watch"));
            Assert.That(ObjectText("SortFeedback"), Is.EqualTo(
                "봉인고 인장이 얼었습니다. 시계는 봉인고가 맞지만 방금 그 인장을 썼어요. 보류에서 지키고 수리실을 먼저 여세요."));
            Assert.That(GameObject.Find("VaultButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False);
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.True);
            Assert.That(ObjectText("ArtifactTraits"), Does.Contain("서리 묻음").And.Contain("시간성"));
            Assert.That(GameObject.Find("IncidentFrostOverlay").GetComponent<UnityEngine.UI.Image>().enabled,
                Is.True, "The protected watch must visibly combine the learned frost and temporal judgment.");

            app.HoldCurrent();
            var stageRun = (IncidentStageRun)typeof(GameApp)
                .GetField("_incidentStageRun", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            Assert.That(stageRun.ResonanceConditionMet, Is.True,
                "The successful Hold must record mossy-watch before the session advances.");
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            var nextRuleText = ObjectText("RuleList");
            Assert.That(nextRuleText, Does.Contain("<color=#E0A24B><b>3."));
            Assert.That(nextRuleText, Does.Not.Contain("<color=#E0A24B><b>1."),
                "The highlighted rule must follow the new artifact rather than remain stale.");
        }

        [UnityTest]
        public IEnumerator FrozenSeal_ReturningHeldWatchKeepsMovingAndRingsOnlySealedDesks()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 3, "ko");

            ChooseDestination(app, (int)Destination.Vault);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);

            var heldArtwork = FindRect("HeldPreviewArtwork");
            var heldPosition = heldArtwork.anchoredPosition;
            yield return new WaitForSecondsRealtime(0.17f);
            Assert.That(Vector2.Distance(heldArtwork.anchoredPosition, heldPosition), Is.GreaterThan(0.2f),
                "The protected watch must remain visibly alive after entering Hold.");

            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Storage);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("mossy-watch"));

            var repairStamp = FindRect("DocketStampRepair");
            var storageStamp = FindRect("DocketStampStorage");
            var vaultStamp = FindRect("DocketStampVault");
            var repairRestScale = repairStamp.localScale;
            var storageRestScale = storageStamp.localScale;
            var vaultRestScale = vaultStamp.localScale;

            ChooseDestination(app, (int)Destination.Vault);
            yield return new WaitForSecondsRealtime(0.10f);
            Assert.That(ObjectText("IncidentReactionText"),
                Is.EqualTo("시계가 보류에서 맑은 종소리와 함께 깨어납니다. 보호한 물건들이 모두 우산을 향합니다."));

            yield return new WaitForSecondsRealtime(1.29f);
            Assert.That(Vector3.Distance(repairStamp.localScale, repairRestScale), Is.GreaterThan(0.04f),
                "The sealed Repair desk must answer the returned watch.");
            Assert.That(Vector3.Distance(storageStamp.localScale, storageRestScale), Is.LessThan(0.01f),
                "The still-open Storage desk must remain quiet.");
            Assert.That(Vector3.Distance(vaultStamp.localScale, vaultRestScale), Is.GreaterThan(0.04f),
                "The newly sealed Vault desk must answer the returned watch.");

            yield return WaitForFilingTransition(app);
        }

        [UnityTest]
        public IEnumerator FirstIncidentHoldPrompt_TeachesProtectionAndAnOpenDeskInBothLanguages()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 0, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            yield return AdvanceNarrativeToShift(app);

            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            Assert.That(ObjectText("SortFeedback"),
                Does.StartWith("보호 보류").And.Contain("비어 있는 목적지"));

            SetLocale(app, "en");
            InvokePrivate(app, "RefreshDecisionMessage");
            Assert.That(ObjectText("SortFeedback"),
                Does.StartWith("PROTECT IN HOLD").And.Contain("missing desk"));
        }

        [UnityTest]
        public IEnumerator FailedSecondHold_DoesNotRecordTheNewCurrentArtifact()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            ClickButton("NarrativeContinueButton");
            yield return null;
            var stageRun = (IncidentStageRun)typeof(GameApp)
                .GetField("_incidentStageRun", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);

            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("mossy-watch"));
            Assert.That(stageRun.ResonanceConditionMet, Is.False,
                "Holding the non-resonant ice first must not satisfy the stage condition.");

            app.HoldCurrent();

            Assert.That(CurrentArtifactId(app), Is.EqualTo("mossy-watch"));
            Assert.That(stageRun.ResonanceConditionMet, Is.False,
                "A rejected second Hold must not record the current mossy-watch.");
        }

        [UnityTest]
        public IEnumerator IncidentShift_ShowsCalmFeedbackAfterThreeConsecutiveCorrectSorts()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            ClickButton("NarrativeContinueButton");
            yield return null;

            ChooseDestination(app, (int)Destination.Vault);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Storage);

            Assert.That(ObjectText("SortFeedback"), Does.Contain("손길이 안정되었습니다"));
            Assert.That(SessionScore(app), Is.EqualTo(700),
                "The calm streak is presentation feedback, not a score multiplier.");
        }

        [UnityTest]
        public IEnumerator IncidentWrongSort_ResetsThePresentationOnlyCalmCounter()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 3, false);
            SetLocale(app, "ko");
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            ClickButton("NarrativeContinueButton");
            yield return null;
            ChooseDestination(app, (int)Destination.Vault);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Storage);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);

            var counter = typeof(GameApp)
                .GetField("_incidentConsecutiveCorrect", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(counter, Is.Not.Null);
            Assert.That(counter.GetValue(app), Is.EqualTo(4));
            ChooseDestination(app, (int)Destination.Vault);

            Assert.That(counter.GetValue(app), Is.Zero,
                "A real wrong filing must reset the accumulated incident calm streak.");
            Assert.That(ObjectText("SortFeedback"), Does.Not.Contain("손길이 안정되었습니다"));
        }

        [UnityTest]
        public IEnumerator IncidentWrongSort_LeadsWithTheRuleAndNextCorrectClosesTheDocketCrack()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 3, "ko");
            var heartsBefore = SessionHearts(app);
            var feedbackPanel = FindRect("SortFeedbackPanel");
            var feedbackRestScale = feedbackPanel.localScale;
            feedback.Cues.Clear();

            ChooseDestination(app, (int)Destination.Repair);
            yield return new WaitForSecondsRealtime(0.08f);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("unmelting-ice"),
                "An incident mistake must keep the curio available for correction.");
            Assert.That(SessionHearts(app), Is.EqualTo(heartsBefore - 1));
            Assert.That(SessionInt(app, "Mistakes"), Is.EqualTo(1));
            Assert.That(ObjectText("SortFeedback"),
                Does.StartWith("시간성 우선 -> 봉인고").And.Contain("잘못 분류했습니다"));
            Assert.That(ObjectText("SortFeedback"), Does.Not.StartWith("오답"));
            Assert.That(GameObject.Find("SortFeedbackPanel").GetComponent<UnityEngine.UI.Image>().color,
                Is.EqualTo(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Image>().color),
                "Incident correction must use the calm wine surface instead of a dominant pink WRONG banner.");
            var reaction = FindText("IncidentReactionText");
            Assert.That(reaction.enabled, Is.True);
            Assert.That(reaction.text, Is.EqualTo("시간성 우선 -> 봉인고"));
            var crack = GameObject.Find("DocketSigilCrack").GetComponent<UnityEngine.UI.Image>();
            Assert.That(crack.enabled, Is.True);
            Assert.That(crack.rectTransform.localScale.x, Is.GreaterThan(0.9f));
            Assert.That(feedback.Cues, Does.Contain(PlayerFeedbackCue.Wrong));
            Assert.That(Vector3.Distance(feedbackPanel.localScale, feedbackRestScale), Is.GreaterThan(0.04f),
                "Incident mistakes must retain the strong filing impact beneath their authored reaction.");

            yield return new WaitForSecondsRealtime(0.54f);
            Assert.That(reaction.enabled, Is.False);
            feedback.Cues.Clear();
            ChooseDestination(app, (int)Destination.Vault);

            Assert.That(crack.enabled, Is.True);
            Assert.That(crack.rectTransform.localScale.x, Is.LessThan(0.05f),
                "The first correction after a mistake must visibly close the docket crack.");
            Assert.That(crack.color,
                Is.EqualTo(GameObject.Find("StorageButton").GetComponent<UnityEngine.UI.Image>().color));
            Assert.That(feedback.Cues, Does.Contain(PlayerFeedbackCue.Correct));
            Assert.That(feedback.Cues.Contains(PlayerFeedbackCue.KeyReaction), Is.False);
            yield return WaitForFilingTransition(app);
        }

        [UnityTest]
        public IEnumerator IncidentOrdinaryCorrect_UsesTheProceduralCueWithoutAKeyReaction()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 3, "en");
            feedback.Cues.Clear();

            ChooseDestination(app, (int)Destination.Vault);

            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.Correct }));
            Assert.That(FindText("IncidentReactionText").enabled, Is.False);
            yield return WaitForFilingTransition(app);
        }

        [UnityTest]
        public IEnumerator IncidentLeadIce_OwnsTheCardWithAuthoredReactionThenFilesWithoutAnotherTap()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 0, "ko");
            feedback.Cues.Clear();

            ChooseDestination(app, (int)Destination.Repair);

            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.KeyReaction }));
            yield return new WaitForSecondsRealtime(0.16f);
            var reactionText = FindText("IncidentReactionText");
            Assert.That(reactionText.enabled, Is.True);
            Assert.That(reactionText.fontSize, Is.GreaterThanOrEqualTo(40f),
                "A key incident line must read as the dominant visual beat on a portrait phone.");
            var reactionVeil = GameObject.Find("IncidentReactionVeil").GetComponent<UnityEngine.UI.Image>();
            Assert.That(reactionVeil.enabled, Is.True);
            Assert.That(reactionVeil.color.a, Is.GreaterThan(0.55f));
            Assert.That(ObjectText("IncidentReactionText"),
                Is.EqualTo("침착한 손길에 모든 인장이 정확히 찍힙니다. 낙엽이 당신의 이름을 아는 듯 얼음 벽에 닿습니다."));
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("unmelting-ice"));
            Assert.That(typeof(GameApp).GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.True);

            yield return new WaitForSecondsRealtime(0.90f);
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("unmelting-ice"),
                "The authored key reaction must still own the outgoing card near the one-second mark.");

            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("moon-umbrella"),
                "The existing filing transition must continue automatically after the key reaction.");
        }

        [UnityTest]
        public IEnumerator IncidentLeadUmbrella_UsesItsAuthoredStageReaction()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 4, "ko");

            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            feedback.Cues.Clear();
            ChooseDestination(app, (int)Destination.Repair);

            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.KeyReaction }));
            Assert.That(ObjectText("IncidentReactionText"),
                Is.EqualTo("침착한 손길 뒤 책상은 마른 채로 남습니다. 봉인된 우산 안에서 빗방울 하나가 울립니다."));
            yield return WaitForFilingTransition(app);
        }

        [UnityTest]
        public IEnumerator IncidentLeadReaction_DisableThenEnableContinuesFilingExactlyOnce()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 0, "en");
            feedback.Cues.Clear();

            ChooseDestination(app, (int)Destination.Repair);
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1));
            app.gameObject.SetActive(false);
            yield return null;
            Assert.That(PendingTransition(app), Is.Null,
                "Disabling the screen owner must flush the gameplay continuation without waiting for a visual.");
            app.gameObject.SetActive(true);
            yield return null;
            yield return WaitForFilingTransition(app);

            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("moon-umbrella"));
            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.KeyReaction }));

            app.gameObject.SetActive(false);
            app.gameObject.SetActive(true);
            yield return null;
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1),
                "Repeated enable cycles must not replay the pending filing callback.");
        }

        [UnityTest]
        public IEnumerator IncidentLeadReaction_RebuildingTheScreenFlushesTheFilingContinuationExactlyOnce()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 0, "en");

            ChooseDestination(app, (int)Destination.Repair);
            Assert.That(PendingTransition(app), Is.Not.Null);
            app.ShowMenu();
            yield return null;

            Assert.That(PendingTransition(app), Is.Null);
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));

            yield return new WaitForSecondsRealtime(1.4f);
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1),
                "A stale visual callback must not replay a flushed filing continuation.");
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
        }

        [UnityTest]
        public IEnumerator IncidentPendingTransition_PauseAndDestroyEachFlushExactlyOnce()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 0, "en");

            ChooseDestination(app, (int)Destination.Repair);
            typeof(GameApp)
                .GetMethod("OnApplicationPause", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { true });

            Assert.That(PendingTransition(app), Is.Null);
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(1));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("moon-umbrella"));

            var destroyCompletions = 0;
            typeof(GameApp)
                .GetMethod("OwnTransition", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { (Action)(() => destroyCompletions++) });
            UnityEngine.Object.DestroyImmediate(app.gameObject);

            Assert.That(destroyCompletions, Is.EqualTo(1),
                "Destroy must flush a pending continuation once, and OnDisable plus OnDestroy must not duplicate it.");
        }

        [UnityTest]
        public IEnumerator IncidentDocketComplete_RevealsConnectedSigilAndWarmsDeskBeforeAdvancing()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 3, "ko");

            ChooseDestination(app, (int)Destination.Vault);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Repair);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, (int)Destination.Storage);
            yield return new WaitForSecondsRealtime(0.78f);

            var sigil = GameObject.Find("DocketCompletionSigil").GetComponent<UnityEngine.UI.Image>();
            var warmth = GameObject.Find("IncidentWarmthOverlay").GetComponent<UnityEngine.UI.Image>();
            Assert.That(sigil.enabled, Is.True);
            Assert.That(sigil.color.a, Is.GreaterThan(0.2f));
            Assert.That(warmth.enabled, Is.True);
            Assert.That(warmth.color.a, Is.GreaterThan(0.02f));
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("clockwork-moth"),
                "The connected seal tier must be visible before the next curio replaces the completed docket.");
            Assert.That(typeof(GameApp).GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.True);
            Assert.That(feedback.Cues.Contains(PlayerFeedbackCue.IncidentComplete), Is.False,
                "A completed docket must not spend the full incident-complete feedback tier.");

            yield return WaitForFilingTransition(app);
            Assert.That(GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("sleeping-teacup"));
            Assert.That(GameObject.Find("DocketSigilCrack").GetComponent<UnityEngine.UI.Image>().enabled,
                Is.False, "Docket presentation damage must reset when the next docket opens.");
            Assert.That(GameObject.Find("IncidentWarmthOverlay").GetComponent<UnityEngine.UI.Image>().enabled,
                Is.False, "The weaker docket warmth must clear when the next docket opens.");
        }

        [UnityTest]
        public IEnumerator RememberingRain_DocketInterludesBlockInputAdvanceOnceAndSkipFinalDocket()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginRememberingRainShift(app, 0, "ko");

            yield return CompleteCurrentDocketUntilInterlude(app, 1);

            Assert.That(ObjectText("IncidentDocketInterludeSpeaker"), Is.EqualTo("빗속의 목소리"));
            Assert.That(ObjectText("IncidentDocketInterludeBody"), Is.EqualTo("그 이름 말고. 그 전의 이름."));
            var cue = GameObject.Find("IncidentDocketInterludeCueSurface").GetComponent<UnityEngine.UI.Image>();
            Assert.That(cue.enabled, Is.True);
            Assert.That(cue.color.b, Is.GreaterThan(cue.color.r));
            Assert.That(InputLocked(app), Is.True);
            Assert.That(GameObject.Find("RepairButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("StorageButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("VaultButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);

            ClickButton("IncidentDocketInterludeContinueButton");
            yield return null;

            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Null);
            Assert.That(InputLocked(app), Is.False);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("backward-candle"));
            Assert.That(ObjectText("NextPreview0"), Does.Contain("참을성 많은 나침반"));

            yield return CompleteCurrentDocketUntilInterlude(app, 2);
            Assert.That(ObjectText("IncidentDocketInterludeSpeaker"), Is.EqualTo("선임 관리인"));
            Assert.That(ObjectText("IncidentDocketInterludeBody"),
                Is.EqualTo("비가 오래전에 지운 접수표의 이름들을 읊고 있어요."));
            ClickButton("IncidentDocketInterludeContinueButton");
            yield return null;

            yield return CompleteCurrentDocketUntilInterlude(app, 3);
            Assert.That(ObjectText("IncidentDocketInterludeBody"),
                Is.EqualTo("열쇠는 간직했구나. 약속도 간직했니?"));
            ClickButton("IncidentDocketInterludeContinueButton");
            yield return null;

            yield return CompleteActiveShift(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Null,
                "The fourth and final docket must go directly to incident results.");
        }

        [UnityTest]
        public IEnumerator IncidentDocketInterlude_DisabledViewFlushesOwnedContinuationExactlyOnce()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginRememberingRainShift(app, 0, "en");
            yield return CompleteCurrentDocketUntilInterlude(app, 1);

            var view = GameObject.Find("IncidentDocketInterlude").GetComponent<NarrativeSequenceView>();
            view.enabled = false;
            typeof(GameApp)
                .GetMethod("OnApplicationPause", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { true });

            Assert.That(PendingTransition(app), Is.Null);
            Assert.That(InputLocked(app), Is.False);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("backward-candle"));

            typeof(GameApp)
                .GetMethod("OnApplicationPause", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { true });
            Assert.That(CurrentArtifactId(app), Is.EqualTo("backward-candle"),
                "A second flush must not advance the shift again.");
        }

        [UnityTest]
        public IEnumerator FreeAndTutorialShifts_DoNotCreateIncidentDocketInterlude()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;

            app.StartNewShift(4242);
            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Null);

            BeginTutorial(app);
            yield return null;
            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Null);
        }

        [UnityTest]
        public IEnumerator IncidentSuccess_PersistsQualityOncePlaysOutroAndStartsTheNextAuthoredStage()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 0, "ko");
            var saveStore = new RecordingSaveStore();
            typeof(GameApp)
                .GetField("_saveStore", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, saveStore);
            yield return CompleteActiveShift(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(saveStore.SaveCalls, Is.EqualTo(1));
            Assert.That(saveStore.PersistedIncidentStage, Is.EqualTo(1));
            Assert.That(saveStore.PersistedRecordCount, Is.EqualTo(1));
            Assert.That(saveStore.PersistedBestQuality, Is.EqualTo((int)IncidentQuality.Precise));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(1),
                "The next stage must be persisted before the result screen can be left.");
            Assert.That(app.SaveData.incidentStageRecords.Count, Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords[0].stageId, Is.EqualTo("ice-01-crack"));
            Assert.That(app.SaveData.incidentStageRecords[0].bestQuality,
                Is.EqualTo((int)IncidentQuality.Precise));
            Assert.That(ObjectText("IncidentQualityLabel"), Is.EqualTo("정교"));
            Assert.That(ObjectText("IncidentOutroBody"),
                Is.EqualTo("금은 봉합됐어요. 그런데 안쪽의 낙엽은 움직였습니다."));
            Assert.That(GameObject.Find("NextStageButton"), Is.Null,
                "The authored outro must be acknowledged before the next shift is offered.");

            InvokePrivate(app, "ShowIncidentResults");
            Assert.That(saveStore.SaveCalls, Is.EqualTo(1),
                "Rebuilding the result view must not repeat the persistence boundary.");
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords.Count, Is.EqualTo(1),
                "Rebuilding results must not evaluate or advance the incident twice.");

            ClickButton("IncidentOutroContinueButton");
            Assert.That(saveStore.SaveCalls, Is.EqualTo(1));
            Assert.That(ObjectText("IncidentOutroBody"),
                Is.EqualTo("분류만 한 게 아니에요. 얼음이 당신에게 답했습니다. 다음 밤엔 서리가 고른 것을 따라가세요."));
            Assert.That(GameObject.Find("NextStageButton"), Is.Null,
                "Every authored outro beat must be acknowledged before the next shift is offered.");
            ClickButton("IncidentOutroContinueButton");
            Assert.That(GameObject.Find("NextStageButton"), Is.Not.Null);
            ClickButton("NextStageButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("서리가 네 물건을 골랐어요. 흰 테가 생긴 것은 모두 보관실로 보내, 추위가 얼음으로 돌아가지 못하게 하세요."));
        }

        [UnityTest]
        public IEnumerator IncidentResults_AllQualitiesShowBilingualBodiesAndAllowTheNextShift()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 0, "en");
            yield return CompleteActiveShift(app);

            var cases = new[]
            {
                new IncidentResultCopyCase(
                    IncidentQuality.Stable,
                    "en",
                    "Stable",
                    "The shift recovered. The incident remains safely contained.",
                    "The corrected route stops the frost at the shelves. The crack steadies, but the leaf keeps turning."),
                new IncidentResultCopyCase(
                    IncidentQuality.Precise,
                    "en",
                    "Precise",
                    "Calm care kept every seal intact.",
                    "Every seal lands cleanly under your calm hands. The leaf presses against the ice as if it knows your name."),
                new IncidentResultCopyCase(
                    IncidentQuality.Resonant,
                    "en",
                    "Resonant",
                    "Your care made the curio answer.",
                    "The final seal rings. The leaf opens like an eye, and the whole office exhales warm air."),
                new IncidentResultCopyCase(
                    IncidentQuality.Stable,
                    "ko",
                    "안정",
                    "실수를 바로잡았습니다. 사건은 안전하게 진정되었습니다.",
                    "바로잡은 경로가 선반 앞에서 서리를 막습니다. 금은 잦아들지만 낙엽은 계속 돕니다."),
                new IncidentResultCopyCase(
                    IncidentQuality.Precise,
                    "ko",
                    "정교",
                    "침착한 손길로 모든 인장을 지켰습니다.",
                    "침착한 손길에 모든 인장이 정확히 찍힙니다. 낙엽이 당신의 이름을 아는 듯 얼음 벽에 닿습니다."),
                new IncidentResultCopyCase(
                    IncidentQuality.Resonant,
                    "ko",
                    "공명",
                    "당신의 손길에 물건이 답했습니다.",
                    "마지막 인장이 울립니다. 낙엽이 눈처럼 펼쳐지고, 보관소 전체가 따뜻한 숨을 내쉽니다.")
            };

            foreach (var resultCase in cases)
            {
                SetLocale(app, resultCase.Locale);
                typeof(GameApp)
                    .GetField("_incidentResultQuality", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(app, resultCase.Quality);
                InvokePrivate(app, "ShowIncidentResults");

                Assert.That(ObjectText("IncidentQualityLabel"), Is.EqualTo(resultCase.Label));
                Assert.That(ObjectText("IncidentQualityBody"), Is.EqualTo(resultCase.QualityBody));
                Assert.That(ObjectText("IncidentReactionBody"), Is.EqualTo(resultCase.ReactionBody));
                ClickButton("IncidentOutroContinueButton");
                Assert.That(GameObject.Find("NextStageButton"), Is.Null,
                    "The first incident outro still has one more authored beat.");
                ClickButton("IncidentOutroContinueButton");
                Assert.That(GameObject.Find("NextStageButton"), Is.Not.Null,
                    resultCase.Quality + " must never block story progression in " + resultCase.Locale + ".");
            }
        }

        [UnityTest]
        public IEnumerator IncidentFailure_DoesNotAdvanceOrOfferAnAdAndRetriesTheSameStageImmediately()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginIncidentShift(app, 2, "ko");
            var coinsBefore = app.SaveData.coins;
            app.SaveData.incidentStageRecords.Add(new IncidentStageRecord
            {
                stageId = "ice-01-crack",
                bestQuality = (int)IncidentQuality.Resonant
            });

            SortCurrentIncorrectly(app);
            SortCurrentIncorrectly(app);
            SortCurrentIncorrectly(app);
            yield return WaitForFilingTransition(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(2));
            Assert.That(app.SaveData.incidentStageRecords.Count, Is.EqualTo(1));
            Assert.That(app.SaveData.incidentStageRecords[0].stageId, Is.EqualTo("ice-01-crack"));
            Assert.That(app.SaveData.incidentStageRecords[0].bestQuality,
                Is.EqualTo((int)IncidentQuality.Resonant));
            Assert.That(app.SaveData.completedIncidentIds, Is.Empty);
            Assert.That(app.SaveData.coins, Is.EqualTo(coinsBefore));
            Assert.That(ObjectText("IncidentFailureBody"),
                Is.EqualTo("보관소는 그대로입니다. 준비되면 같은 교대를 다시 시작하세요."));
            Assert.That(GameObject.Find("RetryStageButton"), Is.Not.Null);
            Assert.That(GameObject.Find("NextStageButton"), Is.Null);
            Assert.That(GameObject.Find("RewardedAdButton"), Is.Null);

            ClickButton("RetryStageButton");
            yield return null;
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(ObjectText("NarrativeBody"),
                Is.EqualTo("사라진 낙엽이 이 시계 안에 있어요. 날짜는 내일입니다. 서리와 시간성이 겹치면 시간 규칙이 먼저예요."));
        }

        [UnityTest]
        public IEnumerator FinalIncidentStage_RecedesFrostWarmsTheOfficeAndLeavesTheUmbrellaHook()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            yield return BeginIncidentShift(app, 4, "ko");
            feedback.Cues.Clear();
            yield return CompleteActiveShift(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.IncidentResults));
            Assert.That(app.SaveData.activeIncidentStage, Is.EqualTo(5));
            Assert.That(app.SaveData.completedIncidentIds, Does.Contain("unmelting-ice"));
            Assert.That(app.SaveData.incidentStageRecords[0].bestQuality,
                Is.EqualTo((int)IncidentQuality.Resonant));
            Assert.That(feedback.Cues.FindAll(cue => cue == PlayerFeedbackCue.IncidentComplete).Count,
                Is.EqualTo(1));
            Assert.That(ObjectText("IncidentQualityLabel"), Is.EqualTo("공명"));
            Assert.That(ObjectText("IncidentEndingTitle"), Is.EqualTo("첫 사건 해결"));
            Assert.That(ObjectText("IncidentEndingHook"),
                Is.EqualTo("다음 사건 · 실내에서 비를 맞은 우산"));
            Assert.That(ObjectText("IncidentReactionBody"),
                Is.EqualTo("얼음이 따뜻한 빛으로 무너집니다. 우산 안에서 비가 북을 울리고 보관소 전체가 답합니다."));
            Assert.That(ObjectText("IncidentOutroBody"),
                Is.EqualTo("얼음이 물 한 방울 없이 따뜻한 빛으로 무너집니다. 봉인된 우산 안에서 비가 대답합니다."));
            Assert.That(GameObject.Find("IncidentEndingIce"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentEndingUmbrella"), Is.Not.Null);
            Assert.That(GameObject.Find("IncidentEndingUmbrellaSeal"), Is.Not.Null);
            var frost = GameObject.Find("IncidentEndingFrost").GetComponent<UnityEngine.UI.Image>();
            var warmth = GameObject.Find("IncidentEndingWarmth").GetComponent<UnityEngine.UI.Image>();
            var startingFrost = frost.color.a;
            var startingWarmth = warmth.color.a;

            yield return new WaitForSecondsRealtime(0.70f);
            Assert.That(frost.color.a, Is.LessThan(startingFrost));
            Assert.That(warmth.color.a, Is.GreaterThan(startingWarmth));

            ClickButton("IncidentOutroContinueButton");
            Assert.That(ObjectText("NextStageButton"), Is.EqualTo("사건 보드로 돌아가기"));
            ClickButton("NextStageButton");
            yield return null;
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(ObjectText("IncidentState"), Is.EqualTo("첫 조사 시작"));
            Assert.That(ObjectText("IncidentTitle"), Is.EqualTo("기억하는 비"));
        }

        [UnityTest]
        public IEnumerator DocketStampLabels_AreLocalizedInEnglishAndKorean()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;

            SetLocale(app, "en");
            app.StartNewShift(4242);
            yield return null;

            Assert.That(ObjectText("DocketStampRepairStatus"), Is.EqualTo("EMPTY"));
            Assert.That(ObjectText("DocketStampStorageStatus"), Is.EqualTo("EMPTY"));
            Assert.That(ObjectText("DocketStampVaultStatus"), Is.EqualTo("EMPTY"));

            SetLocale(app, "ko");
            app.StartNewShift(4242);
            yield return null;

            Assert.That(ObjectText("DocketStampRepairStatus"), Is.EqualTo("빈칸"));
            Assert.That(ObjectText("DocketStampStorageStatus"), Is.EqualTo("빈칸"));
            Assert.That(ObjectText("DocketStampVaultStatus"), Is.EqualTo("빈칸"));
        }

        [UnityTest]
        public IEnumerator ShiftLayout_UsesCurioFirstReadableBands()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);
            yield return null;

            Assert.That(FindRect("DocketProgress").anchorMin.y, Is.GreaterThanOrEqualTo(0.84f));
            Assert.That(FindRect("RulesPanel").anchorMin.y,
                Is.GreaterThan(FindRect("CurrentArtifactCard").anchorMax.y));
            Assert.That(FindText("RuleList").fontSize, Is.GreaterThanOrEqualTo(24f));
            Assert.That(FindText("ArtifactName").fontSize, Is.GreaterThanOrEqualTo(40f));
            Assert.That(FindText("ArtifactTraits").fontSize, Is.GreaterThanOrEqualTo(23f));
            Assert.That(FindText("RepairButton").fontSize, Is.GreaterThanOrEqualTo(28f));
            Assert.That(FindRect("CurrentArtifactCard").rect.height,
                Is.GreaterThan(FindRect("RulesPanel").rect.height));
            Assert.That(FindText("ArtifactName").font.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(FindText("RuleList").font.name, Does.StartWith("NotoSansKR"));
        }

        [UnityTest]
        public IEnumerator EnglishIncidentRules_FitPortraitPanels()
        {
            yield return AssertIncidentRulesFitPortraitPanels("en");
        }

        [UnityTest]
        public IEnumerator EnglishFallbackRule_ShowsAuthoredDestination()
        {
            yield return AssertFallbackRuleShowsAuthoredDestination("en", "Otherwise → VAULT", "Otherwise → REPAIR");
        }

        [UnityTest]
        public IEnumerator KoreanFallbackRule_ShowsAuthoredDestination()
        {
            yield return AssertFallbackRuleShowsAuthoredDestination("ko", "그 외 → 봉인고", "그 외 → 수리실");
        }

        private static IEnumerator AssertFallbackRuleShowsAuthoredDestination(string locale, string vaultRule, string repairRule)
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            yield return BeginRememberingRainShift(app, 2, locale);
            Assert.That(ObjectText("RuleList"), Does.Contain(vaultRule),
                "Stage 3 fallback artifacts are accepted by the vault, so the visible instruction must name it.");
            yield return BeginRememberingRainShift(app, 3, locale);
            Assert.That(ObjectText("RuleList"), Does.Contain(repairRule),
                "Stage 4 fallback artifacts are accepted by repair, so the visible instruction must name it.");
        }

        [UnityTest]
        public IEnumerator KoreanIncidentRules_FitPortraitPanels()
        {
            yield return AssertIncidentRulesFitPortraitPanels("ko");
        }

        private static IEnumerator AssertIncidentRulesFitPortraitPanels(string locale)
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            foreach (var stageIndex in new[] { 1, 4 })
            {
                yield return BeginRememberingRainShift(app, stageIndex, locale);
                var root = FindRect("ScreenRoot");
                root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
                foreach (var height in new[] { 1920f, 2400f })
                {
                    root.sizeDelta = new Vector2(1080f, height);
                    Canvas.ForceUpdateCanvases();
                    var rules = FindText("RuleList");
                    rules.ForceMeshUpdate();
                    Assert.That(rules.preferredHeight, Is.LessThanOrEqualTo(rules.rectTransform.rect.height),
                        $"{locale} stage {stageIndex + 1}, 1080x{height}: every rule must fit its panel.");
                    Assert.That(rules.isTextOverflowing, Is.False);
                    Assert.That(rules.fontSize, Is.GreaterThanOrEqualTo(24f));
                    Assert.That(FindRect("RulesPanel").anchorMin.y,
                        Is.GreaterThan(FindRect("NextPreviewCard0").anchorMax.y));
                    Assert.That(FindRect("NextPreviewCard0").anchorMin.y,
                        Is.GreaterThan(FindRect("CurrentArtifactCard").anchorMax.y));
                    Assert.That(FindRect("CurrentArtifactCard").rect.height,
                        Is.GreaterThan(FindRect("RulesPanel").rect.height));
                }
            }
        }

        [UnityTest]
        public IEnumerator KoreanStoryAndArtifactCopy_UseReadableBodyTypography()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetIncidentProgress(app, 0, false);
            SetLocale(app, "ko");
            app.ShowMenu();

            ClickButton("IncidentButton");
            yield return null;

            var narrativeBody = FindText("NarrativeBody");
            Assert.That(narrativeBody.fontSize, Is.GreaterThanOrEqualTo(40f));
            Assert.That(narrativeBody.fontStyle & FontStyles.Bold, Is.EqualTo(FontStyles.Bold),
                "Korean story copy must remain legible over a handheld portrait presentation.");

            yield return AdvanceNarrativeToShift(app);
            var artifactDescription = FindText("ArtifactDescription");
            Assert.That(artifactDescription.fontSize, Is.GreaterThanOrEqualTo(27f));
            Assert.That(artifactDescription.fontStyle & FontStyles.Bold, Is.EqualTo(FontStyles.Bold),
                "The curio's story clue must read as primary decision information, not fine print.");
        }

        [UnityTest]
        public IEnumerator HoldTransition_ShowsLocalizedInputLockAndThenReflectsActualHoldAvailability()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetLocale(app, "ko");
            app.StartNewShift(4242);

            app.HoldCurrent();

            Assert.That(GameObject.Find("ShiftInputLockPanel"), Is.Not.Null,
                "A visible veil must explain why the one-hand controls temporarily ignore input.");
            Assert.That(ObjectText("ShiftInputLockText"), Is.EqualTo("처리 중…"));
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False);
            Assert.That(GameObject.Find("RepairButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False);

            yield return WaitForFilingTransition(app);

            Assert.That(GameObject.Find("ShiftInputLockPanel"), Is.Null);
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False,
                "Hold must look unavailable when the session will reject a second Hold.");
        }

        [UnityTest]
        public IEnumerator DuplicateDesk_DisablesThatDeskAndSuggestsHold()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);

            var first = ExpectedDestination(app);
            ChooseDestination(app, Convert.ToInt32(first));
            yield return WaitForFilingTransition(app);

            Assert.That(first.ToString(), Is.EqualTo("Repair"));
            Assert.That(GameObject.Find("RepairButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False);
            Assert.That(HasEnabledOutline("HoldButton"), Is.True);
            Assert.That(ObjectText("DocketCounter"), Does.Contain("1 / 4"));
            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("REPAIR").And.Contain("already full"));
            Assert.That(ObjectText("HoldButton"), Does.Contain("NEXT DOCKET"));

            var heartsBeforeBlockedSort = SessionHearts(app);
            var holdMessageBeforeBlockedSort = ObjectText("SortFeedback");
            feedback.Cues.Clear();
            ChooseDestination(app, Convert.ToInt32(first));

            Assert.That(SessionHearts(app), Is.EqualTo(heartsBeforeBlockedSort));
            Assert.That(ObjectText("SortFeedback"), Is.EqualTo(holdMessageBeforeBlockedSort));
            Assert.That(feedback.Cues.Contains(PlayerFeedbackCue.Wrong), Is.False,
                "A stamped desk is a Hold prompt, not a sorting mistake.");

            SetLocale(app, "ko");
            app.StartNewShift(4242);
            first = ExpectedDestination(app);
            ChooseDestination(app, Convert.ToInt32(first));
            yield return WaitForFilingTransition(app);

            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("수리실").And.Contain("이미 찼어요"));
            Assert.That(ObjectText("HoldButton"), Does.Contain("다음 장부"));
        }

        [UnityTest]
        public IEnumerator DailyFile_RemainsAvailableThroughItsPublicEntryPoint()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetClock(app, new DateTime(2026, 8, 26, 21, 30, 0));
            SetSaveString(app, "lastDailyCompletedDate", string.Empty);
            SetSaveInt(app, "dailyBestScore", 0);

            app.StartDailyShift();
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(ObjectText("DailyChallengeBadge"), Is.EqualTo("DAILY FILE · 2026-08-26"));
        }

        [UnityTest]
        public IEnumerator DailyCompletion_PersistsBestAndRefreshesMenuStatus()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetClock(app, new DateTime(2026, 8, 26, 21, 30, 0));
            SetSaveString(app, "lastDailyCompletedDate", string.Empty);
            SetSaveInt(app, "dailyBestScore", 0);

            StartDailyShift(app);
            yield return CompleteActiveShift(app);

            var completedScore = SessionScore(app);
            Assert.That(SaveString(app, "lastDailyCompletedDate"), Is.EqualTo("2026-08-26"));
            Assert.That(SaveInt(app, "dailyBestScore"), Is.EqualTo(completedScore));
            Assert.That(ObjectText("DailyResultStatus"), Is.EqualTo($"Today's best: {completedScore}"));

            app.ShowMenu();
            Assert.That(GameObject.Find("DailyShiftButton"), Is.Null,
                "Daily File stays implemented but no longer competes with the authored incident on the main menu.");
        }

        [UnityTest]
        public IEnumerator FailedDailyChallenge_DoesNotMarkTheDateComplete()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetClock(app, new DateTime(2026, 8, 26, 21, 30, 0));
            SetSaveString(app, "lastDailyCompletedDate", string.Empty);
            SetSaveInt(app, "dailyBestScore", 0);

            StartDailyShift(app);
            for (var index = 0; index < 3; index++)
            {
                SortCurrentIncorrectly(app);
            }

            yield return WaitForFilingTransition(app);

            Assert.That(SessionState(app), Is.EqualTo("Failed"));
            Assert.That(SaveString(app, "lastDailyCompletedDate"), Is.Empty);
            Assert.That(SaveInt(app, "dailyBestScore"), Is.Zero);
        }

        [UnityTest]
        public IEnumerator App_CreatesAnActiveCameraWhenSceneHasNone()
        {
            foreach (var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(camera.gameObject);
            }

            Assert.That(UnityEngine.Object.FindFirstObjectByType<Camera>(), Is.Null,
                "The test requires a scene with no camera, matching the generated Main scene.");

            var host = new GameObject("GameAppCameraTestHost");
            host.AddComponent<GameApp>();
            yield return null;

            var renderCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            Assert.That(renderCamera, Is.Not.Null,
                "GameApp must supply a camera so the Game view does not show 'No cameras rendering'.");
            Assert.That(renderCamera.isActiveAndEnabled, Is.True);
            Assert.That(UnityEngine.Object.FindFirstObjectByType<AudioListener>(), Is.Not.Null,
                "Runtime-generated feedback audio requires an active listener on the display camera.");

            UnityEngine.Object.Destroy(host);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShiftPreviews_IncludeLocalizedNamesForNextAndHeldArtifacts()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;

            foreach (var locale in new[] { "ko", "en" })
            {
                SetLocale(app, locale);
                app.StartNewShift(4242);
                yield return null;

                var queue = (IList)typeof(GameApp)
                    .GetField("_plannedQueue", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(app);
                var contentById = (IDictionary)typeof(GameApp)
                    .GetField("_artifactById", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(app);
                var nextArtifact = queue[1];
                var nextId = (string)nextArtifact.GetType().GetProperty("Id").GetValue(nextArtifact);
                var nextContent = contentById[nextId];
                var nextNameProperty = locale == "ko" ? "NameKorean" : "NameEnglish";
                var expectedNextName = (string)nextContent.GetType().GetProperty(nextNameProperty).GetValue(nextContent);

                Assert.That(ObjectText("NextPreview0"), Does.Contain(expectedNextName),
                    locale + " next preview must identify the upcoming artifact by name.");

                app.HoldCurrent();
                yield return WaitForFilingTransition(app);

                var heldArtifact = Session(app).GetType().GetProperty("HeldArtifact").GetValue(Session(app));
                var heldId = (string)heldArtifact.GetType().GetProperty("Id").GetValue(heldArtifact);
                var heldContent = contentById[heldId];
                var expectedHeldName = (string)heldContent.GetType().GetProperty(nextNameProperty).GetValue(heldContent);
                Assert.That(ObjectText("HeldArtifactText"), Does.Contain(expectedHeldName),
                    locale + " hold preview must identify the held artifact by name.");
            }
        }

        [UnityTest]
        public IEnumerator Tutorial_BeginsSixArtifactTwoDocketLessonWithoutCompletingSave()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            var completedBefore = CompletedShifts(app);
            var coinsBefore = Coins(app);

            app.ShowTutorial();
            Assert.That(ObjectText("TutorialBody"),
                Is.EqualTo("Clock in, then learn the three desks by sorting six curios."));
            Assert.That(GameObject.Find("TutorialIcons"), Is.Null,
                "The clock-in screen must not front-load a second block of control instructions.");
            ClickButton("BeginTutorialShiftButton");
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(TutorialCompleted(app), Is.False,
                "Opening the guided shift must not mark the tutorial complete.");
            var expectedIds = new[]
            {
                "whispering-key",
                "borrowed-shadow",
                "sleeping-teacup",
                "clockwork-moth",
                "rain-jar",
                "moon-umbrella"
            };
            var queue = PlannedQueue(app);
            Assert.That(queue.Count, Is.EqualTo(expectedIds.Length));
            for (var index = 0; index < expectedIds.Length; index++)
            {
                Assert.That(ArtifactId(queue[index]), Is.EqualTo(expectedIds[index]),
                    $"Tutorial queue item {index + 1} must teach the approved two-docket sequence.");
            }

            Assert.That(CurrentArtifactId(app), Is.EqualTo("whispering-key"));
            Assert.That(SessionInt(app, "RequiredDockets"), Is.EqualTo(2));
            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("one REPAIR, one STORAGE, and one VAULT"));
            Assert.That(ObjectText("RuleList"),
                Does.Contain("<color=#E0A24B><b>1.").And.Contain("CURSED"));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>CURSED</b></color>"));
            Assert.That(CompletedShifts(app), Is.EqualTo(completedBefore));
            Assert.That(Coins(app), Is.EqualTo(coinsBefore));

            SetLocale(app, "ko");
            app.ShowTutorial();
            Assert.That(ObjectText("TutorialBody"),
                Is.EqualTo("출근해서 여섯 물건을 분류하며 세 장소를 배워보세요."));
            Assert.That(GameObject.Find("TutorialIcons"), Is.Null);
            ClickButton("BeginTutorialShiftButton");
            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("수리실·보관실·봉인고 도장이 하나씩"));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>저주받음</b></color>"));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator Tutorial_WrongSortPreservesArtifactAndHearts()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            BeginTutorial(app);

            ChooseDestination(app, 0);
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("whispering-key"));
            Assert.That(SessionHearts(app), Is.EqualTo(3));
            Assert.That(ObjectText("SortFeedback"), Does.Contain("VAULT"));
            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("one REPAIR, one STORAGE, and one VAULT"));
            Assert.That(ObjectText("RuleList"), Does.Contain("<color=#E0A24B><b>1."));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>CURSED</b></color>"));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator Tutorial_DuplicateVaultRequiresHoldAndClosesFirstDocket()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            BeginTutorial(app);
            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("borrowed-shadow"));
            Assert.That(GameObject.Find("VaultButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.False, "A stamped destination must be unavailable for the next matching artifact.");
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable,
                Is.True);
            Assert.That(HasEnabledOutline("HoldButton"), Is.True,
                "Hold must be the only highlighted route past the duplicate Vault.");
            Assert.That(HasEnabledOutline("RepairButton"), Is.False);
            Assert.That(HasEnabledOutline("StorageButton"), Is.False);
            Assert.That(HasEnabledOutline("VaultButton"), Is.False);
            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("correct desk").And.Contain("already full"));
            Assert.That(ObjectText("RuleList"), Does.Contain("<color=#E0A24B><b>1."));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>CURSED</b></color>"));

            app.HoldCurrent();
            yield return WaitForFilingTransition(app);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("sleeping-teacup"));
            Assert.That(HeldArtifactId(app), Is.EqualTo("borrowed-shadow"));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>FRAGILE</b></color>"));

            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return new WaitForSecondsRealtime(0.65f);

            Assert.That(ObjectText("TutorialDocketCompleteCard"),
                Does.Contain("used each desk once"));
            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("DOCKET COMPLETE").And.Contain("PRISTINE STREAK 1"));
            yield return WaitForFilingTransition(app);

            Assert.That(SessionInt(app, "CompletedDockets"), Is.EqualTo(1));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("rain-jar"));
            Assert.That(HeldArtifactId(app), Is.EqualTo("borrowed-shadow"));

            SetLocale(app, "ko");
            BeginTutorial(app);
            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);
            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("정답은 봉인고").And.Contain("이미 찼어요"));
            Assert.That(ObjectText("RuleList"), Does.Contain("<color=#E0A24B><b>1."));
            Assert.That(ObjectText("ArtifactTraits"),
                Does.Contain("<color=#E0A24B><b>저주받음</b></color>"));
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return new WaitForSecondsRealtime(0.65f);
            Assert.That(ObjectText("TutorialDocketCompleteCard"),
                Does.Contain("세 장소를 한 번씩"));
            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("장부 완성").And.Contain("완벽 장부 연속 1"));
            yield return WaitForFilingTransition(app);
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator Tutorial_CompletionPersistsWithoutAwardingShiftProgression()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            var completedBefore = CompletedShifts(app);
            var coinsBefore = Coins(app);
            var discoveredBefore = DiscoveredCount(app);
            BeginTutorial(app);

            Assert.That(ObjectText("TutorialCoach"),
                Does.Contain("one REPAIR, one STORAGE, and one VAULT"));
            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("borrowed-shadow"),
                "The held shadow must return automatically after the six queued items.");
            Assert.That(HeldArtifactId(app), Is.Null);
            ChooseDestination(app, 2);

            yield return new WaitForSecondsRealtime(0.65f);
            Assert.That(TutorialCompleted(app), Is.False,
                "The final tutorial docket must pulse before the completion screen replaces it.");
            Assert.That(GameObject.Find("ShiftScreen"), Is.Not.Null);
            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("DOCKET COMPLETE").And.Contain("PRISTINE STREAK 2"));

            yield return WaitForFilingTransition(app);

            Assert.That(TutorialCompleted(app), Is.True);
            Assert.That(GameObject.Find("TutorialCompleteScreen"), Is.Not.Null);
            Assert.That(ObjectText("TutorialCompleteBody"),
                Does.Contain("Fill all three stamps").And.Contain("Hold"));
            Assert.That(CompletedShifts(app), Is.EqualTo(completedBefore));
            Assert.That(Coins(app), Is.EqualTo(coinsBefore));
            Assert.That(DiscoveredCount(app), Is.EqualTo(discoveredBefore));

            SetLocale(app, "ko");
            BeginTutorial(app);
            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);
            Assert.That(ObjectText("TutorialCompleteBody"),
                Does.Contain("세 도장을").And.Contain("보류"));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator VisualSlice_ShowsDeskArtifactAndDestinationArtworkAndRefreshesCurrentArtifact()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);

            BeginTutorial(app);
            yield return null;

            var desk = GameObject.Find("OccultDeskBackground").GetComponent<UnityEngine.UI.Image>();
            var artifact = GameObject.Find("ArtifactIllustration").GetComponent<UnityEngine.UI.Image>();
            Assert.That(desk.sprite, Is.Not.Null, "The vertical slice must use the illustrated desk backdrop.");
            Assert.That(artifact.sprite, Is.Not.Null, "The current artifact card must use release-facing artwork.");
            Assert.That(artifact.sprite.name, Is.EqualTo("whispering-key"));
            Assert.That(artifact.preserveAspect, Is.True);

            foreach (var iconName in new[]
                     {
                         "RepairButtonIcon",
                         "StorageButtonIcon",
                         "VaultButtonIcon",
                         "HoldButtonIcon"
                     })
            {
                var icon = GameObject.Find(iconName)?.GetComponent<UnityEngine.UI.Image>();
                Assert.That(icon, Is.Not.Null, iconName + " must exist in the active shift view.");
                Assert.That(icon.sprite, Is.Not.Null, iconName + " must use a readable sprite.");
            }

            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);

            Assert.That(artifact.sprite.name, Is.EqualTo("borrowed-shadow"),
                "The artifact illustration must follow the authoritative current artifact.");
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator ShiftPreviews_ShowArtworkForNextAndHeldArtifacts()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);

            BeginTutorial(app);
            yield return null;

            var nextOne = GameObject.Find("NextPreviewArtwork0")?.GetComponent<UnityEngine.UI.Image>();
            var nextTwo = GameObject.Find("NextPreviewArtwork1")?.GetComponent<UnityEngine.UI.Image>();
            Assert.That(nextOne, Is.Not.Null, "The first next preview must provide an artwork surface.");
            Assert.That(nextTwo, Is.Not.Null, "The second next preview must provide an artwork surface.");
            Assert.That(nextOne.sprite?.name, Is.EqualTo("borrowed-shadow"));
            Assert.That(nextTwo.sprite?.name, Is.EqualTo("sleeping-teacup"));

            ChooseDestination(app, 2);
            yield return WaitForFilingTransition(app);
            app.HoldCurrent();
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 0);
            yield return WaitForFilingTransition(app);
            ChooseDestination(app, 1);
            yield return WaitForFilingTransition(app);

            Assert.That(nextOne.sprite?.name, Is.EqualTo("moon-umbrella"));
            Assert.That(nextTwo.sprite?.name, Is.EqualTo("borrowed-shadow"),
                "The held artifact must preview its queue-end return.");

            var held = GameObject.Find("HeldPreviewArtwork")?.GetComponent<UnityEngine.UI.Image>();
            Assert.That(held, Is.Not.Null, "The hold preview must provide an artwork surface.");
            Assert.That(held.sprite?.name, Is.EqualTo("borrowed-shadow"));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator ShiftArtwork_LoadsBespokeIllustrationForEveryCatalogArtifact()
        {
            yield return null;

            foreach (var artifact in ContentCatalog.CreateArtifacts())
            {
                var sprite = VisualAssetLibrary.Artifact(artifact.Id);
                Assert.That(sprite, Is.Not.Null,
                    $"Catalog artifact '{artifact.Id}' must have a Resources artwork sprite.");
                Assert.That(sprite.name, Is.EqualTo(artifact.Id));
            }
        }

        [UnityTest]
        public IEnumerator NarrativeArtwork_LoadsEverySeniorClerkMoodAndFrostOverlay()
        {
            yield return null;

            foreach (SeniorClerkMood mood in Enum.GetValues(typeof(SeniorClerkMood)))
            {
                var portrait = VisualAssetLibrary.SeniorClerk(mood);
                Assert.That(portrait, Is.Not.Null, mood + " must have a Resources portrait sprite.");
                Assert.That(portrait.rect.width, Is.GreaterThanOrEqualTo(768));
                Assert.That(portrait.rect.height, Is.GreaterThanOrEqualTo(1024));
            }

            var frost = VisualAssetLibrary.FrostOverlay;
            Assert.That(frost, Is.Not.Null, "The first incident must have a frost overlay sprite.");
            Assert.That(frost.rect.width, Is.GreaterThanOrEqualTo(1024));
            Assert.That(frost.rect.height, Is.GreaterThanOrEqualTo(1536));
        }

        [UnityTest]
        public IEnumerator Casebook_ShowsIllustratedCardsAndKeepsUnknownDetailsLocked()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var discovered = SaveStringList(app, "discoveredArtifactIds");
            discovered.Clear();
            discovered.Add("sleeping-teacup");

            app.ShowCollection();
            yield return null;

            Assert.That(ObjectText("CollectionProgress"), Is.EqualTo("1 / 24 DISCOVERED"));
            Assert.That(ObjectText("CasebookName_sleeping-teacup"), Is.EqualTo("Sleeping Teacup"));
            Assert.That(ObjectText("CasebookDescription_sleeping-teacup"), Does.Contain("kettle sings"));
            Assert.That(ObjectText("CasebookResolution_sleeping-teacup"),
                Does.Contain("dreams of warm tea"));
            Assert.That(ObjectText("CasebookTraits_sleeping-teacup"), Does.Contain("ALIVE").And.Contain("FRAGILE"));
            Assert.That(ObjectText("CasebookName_borrowed-shadow"), Is.EqualTo("?????"));
            Assert.That(ObjectText("CasebookDescription_borrowed-shadow"), Is.EqualTo("LOCKED CASE FILE"));
            Assert.That(GameObject.Find("CasebookResolution_borrowed-shadow"), Is.Null,
                "A locked case file must not reveal its post-filing resolution.");

            var knownArt = GameObject.Find("CasebookArtwork_sleeping-teacup")?.GetComponent<UnityEngine.UI.Image>();
            var unknownArt = GameObject.Find("CasebookArtwork_borrowed-shadow")?.GetComponent<UnityEngine.UI.Image>();
            Assert.That(knownArt?.sprite?.name, Is.EqualTo("sleeping-teacup"));
            Assert.That(unknownArt?.sprite?.name, Is.EqualTo("borrowed-shadow"));
            Assert.That(unknownArt.color.r, Is.LessThan(knownArt.color.r),
                "Undiscovered artwork must remain a dark silhouette rather than reveal the finished illustration.");
        }

        [UnityTest]
        public IEnumerator CosmeticsTab_ShowsSixIllustratedUnlockCards()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetSaveInt(app, "coins", 0);
            SaveStringList(app, "unlockedCosmeticIds").Clear();
            app.ShowCollection();
            ClickButton("CosmeticsTabButton");
            yield return null;

            Assert.That(ObjectText("CollectionCoins"), Is.EqualTo("COINS 0"));
            foreach (var cosmetic in ContentCatalog.CreateCosmetics())
            {
                var artwork = GameObject.Find("CosmeticArtwork_" + cosmetic.Id)?.GetComponent<UnityEngine.UI.Image>();
                Assert.That(artwork, Is.Not.Null, cosmetic.Id + " must expose a cosmetic preview image.");
                Assert.That(artwork.sprite?.name, Is.EqualTo(cosmetic.Id));
                Assert.That(ObjectText("CosmeticStatus_" + cosmetic.Id),
                    Is.EqualTo("UNLOCK · " + cosmetic.Cost + " COINS"));
            }
        }

        [UnityTest]
        public IEnumerator CosmeticUnlock_InsufficientCoinsShowsLocalizedFeedbackAndPreservesOwnership()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetLocale(app, "ko");
            SetSaveInt(app, "coins", 0);
            SaveStringList(app, "unlockedCosmeticIds").Clear();
            app.ShowCollection();
            ClickButton("CosmeticsTabButton");
            ClickButton("Cosmetic_brass-lamp");
            yield return null;

            Assert.That(ObjectText("CosmeticFeedback"), Is.EqualTo("코인이 부족합니다"));
            Assert.That(SaveStringList(app, "unlockedCosmeticIds"), Does.Not.Contain("brass-lamp"));
            Assert.That(Coins(app), Is.Zero);
        }

        [UnityTest]
        public IEnumerator EquippedCosmetic_AppearsAsArtworkOnMenuAndShiftDesk()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetSaveInt(app, "coins", 1000);
            SaveStringList(app, "unlockedCosmeticIds").Clear();
            app.ShowCollection();
            ClickButton("CosmeticsTabButton");
            ClickButton("Cosmetic_brass-lamp");
            yield return null;

            Assert.That(SaveString(app, "equippedCosmeticId"), Is.EqualTo("brass-lamp"));
            Assert.That(ObjectText("CosmeticFeedback"), Is.EqualTo("Brass Lamp equipped"));
            app.ShowMenu();
            yield return null;
            Assert.That(GameObject.Find("EquippedDeskCharmArtwork")?.GetComponent<UnityEngine.UI.Image>().sprite?.name,
                Is.EqualTo("brass-lamp"));

            app.StartNewShift(4242);
            yield return null;
            Assert.That(GameObject.Find("EquippedDeskCharmArtwork")?.GetComponent<UnityEngine.UI.Image>().sprite?.name,
                Is.EqualTo("brass-lamp"));
        }

        [UnityTest]
        public IEnumerator CorrectSort_KeepsOutgoingCurioUntilTransitionCompletes()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");

            Assert.That(ObjectText("SortFeedback"),
                Is.EqualTo("Compare its traits with tonight's rules."));

            var precedingResolution = CatalogResolution("mirror-seed", "en");
            app.ChooseDestination(Destination.Vault);

            Assert.That(GameObject.Find("ArtifactIllustration")
                    .GetComponent<UnityEngine.UI.Image>().sprite?.name,
                Is.EqualTo("mirror-seed"));
            Assert.That(ObjectText("SortFeedback"), Does.Contain(precedingResolution));

            yield return new WaitForSecondsRealtime(0.9f);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("clockwork-moth"));
            Assert.That(GameObject.Find("ArtifactIllustration")
                    .GetComponent<UnityEngine.UI.Image>().sprite?.name,
                Is.EqualTo("clockwork-moth"));
            Assert.That(ObjectText("SortFeedback"),
                Is.EqualTo("Compare its traits with tonight's rules."));
            Assert.That(ObjectText("SortFeedback"), Does.Not.Contain(precedingResolution));
        }

        [UnityTest]
        public IEnumerator FilingTransition_PreventsCardDragUntilTheNextCurioAppears()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");
            yield return new WaitForSecondsRealtime(0.25f);

            var card = GameObject.Find("CurrentArtifactCard").GetComponent<RectTransform>();
            var drag = card.GetComponent<ArtifactDragHandler>();
            var restPosition = card.anchoredPosition;
            app.ChooseDestination(Destination.Vault);

            var pointer = new PointerEventData(EventSystem.current)
            {
                delta = new Vector2(42f, 26f),
                position = Vector2.zero
            };
            drag.OnBeginDrag(pointer);
            drag.OnDrag(pointer);

            Assert.That(Vector2.Distance(card.anchoredPosition, restPosition), Is.LessThan(0.01f),
                "Locked filing transitions must ignore drag visuals as well as drop callbacks.");

            yield return WaitForFilingTransition(app);
            drag.OnBeginDrag(pointer);
            drag.OnDrag(pointer);
            Assert.That(Vector2.Distance(card.anchoredPosition, restPosition), Is.GreaterThan(1f),
                "Card dragging must be restored for the next decision.");
            drag.OnEndDrag(pointer);
        }

        [UnityTest]
        public IEnumerator FilingTransition_CancelsDragThatEndsAfterTheNextCurioAppears()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");
            yield return new WaitForSecondsRealtime(0.25f);

            var card = GameObject.Find("CurrentArtifactCard").GetComponent<RectTransform>();
            var drag = card.GetComponent<ArtifactDragHandler>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                pointerId = 7,
                delta = new Vector2(24f, 18f),
                position = RectTransformUtility.WorldToScreenPoint(null, card.position)
            };
            drag.OnBeginDrag(pointer);
            drag.OnDrag(pointer);

            app.ChooseDestination(Destination.Vault);
            yield return WaitForFilingTransition(app);
            Assert.That(CurrentArtifactId(app), Is.EqualTo("clockwork-moth"));
            var correctBeforeStaleEnd = SessionCorrectSorts(app);
            var storage = GameObject.Find("StorageButton").GetComponent<RectTransform>();
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, storage.position);

            drag.OnEndDrag(pointer);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("clockwork-moth"),
                "A drag canceled by the filing lock must not sort the next curio when its old pointer ends.");
            Assert.That(SessionCorrectSorts(app), Is.EqualTo(correctBeforeStaleEnd));
        }

        [UnityTest]
        public IEnumerator PriorityRuleFeedback_ExplainsCursedVaultAndResolutionInBothLanguages()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");

            ShowSortFeedbackForCurrent(app, Destination.Vault);

            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("CURSED took priority -> VAULT")
                    .And.Contain("Its reflection curls safely around the seed."));

            SetLocale(app, "ko");
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");
            ShowSortFeedbackForCurrent(app, Destination.Vault);

            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("저주받음 우선 -> 봉인고")
                    .And.Contain("반사된 빛이 씨앗 둘레를 안전하게 감싼다."));
        }

        [UnityTest]
        public IEnumerator FallbackRuleFeedback_ExplainsStorageAndResolution()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "clockwork-moth", "sleeping-teacup", "whispering-key");

            ShowSortFeedbackForCurrent(app, Destination.Storage);

            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("No special rule -> STORAGE")
                    .And.Contain("Its wings settle into the rhythm of the desk lamp."));
        }

        [UnityTest]
        public IEnumerator CorrectSort_ShowsAuthoredCurioFarewellBeforeNextDecision()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");
            var resolution = CatalogResolution("mirror-seed", "en");
            var cardRestColor = GameObject.Find("CurrentArtifactCard").GetComponent<UnityEngine.UI.Image>().color;

            app.ChooseDestination(Destination.Vault);

            Assert.That(ObjectText("CurioResolution"), Is.EqualTo(resolution));
            Assert.That(GameObject.Find("CurioFarewellSeal"), Is.Not.Null);
            Assert.That(GameObject.Find("CurioFarewellSeal").GetComponent<UnityEngine.UI.Image>().sprite,
                Is.Not.Null);
            Assert.That(GameObject.Find("CurioFarewellSeal").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("vault-icon"));
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent<UnityEngine.UI.Image>().color,
                Is.Not.EqualTo(cardRestColor),
                "A correct filing must tint the curio card with its destination color.");

            yield return new WaitForSecondsRealtime(0.26f);
            var responseVeil = GameObject.Find("CurioResponseVeil");
            Assert.That(responseVeil.GetComponent<CanvasGroup>().alpha,
                Is.GreaterThan(0.75f),
                "The authored curio response must visibly replace the ordinary card for a clear story beat.");
            Assert.That(responseVeil.GetComponent<UnityEngine.UI.Image>().color.a,
                Is.GreaterThan(0.9f));
            Assert.That(GameObject.Find("CurioResolution").GetComponent<TMP_Text>().fontSize,
                Is.GreaterThanOrEqualTo(46f));
            Assert.That(GameObject.Find("CurioResponseSeal").GetComponent<UnityEngine.UI.Image>().sprite.name,
                Is.EqualTo("vault-icon"));

            yield return WaitForFilingTransition(app);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("clockwork-moth"));
            Assert.That(GameObject.Find("CurioFarewellSeal"), Is.Null,
                "The filing seal must clear before the next decision.");
            Assert.That(GameObject.Find("CurioResolution"), Is.Null,
                "The prior curio's resolution must not leak into the next card.");
            Assert.That(GameObject.Find("ArtifactDescription"), Is.Not.Null);
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent<UnityEngine.UI.Image>().color,
                Is.EqualTo(cardRestColor));
        }

        [UnityTest]
        public IEnumerator CorrectFiling_DisableThenEnableRestoresTheNextDecision()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            StartAuthoredShift(app, "mirror-seed", "clockwork-moth", "sleeping-teacup");
            var cardRestColor = GameObject.Find("CurrentArtifactCard").GetComponent<UnityEngine.UI.Image>().color;

            app.ChooseDestination(Destination.Vault);
            yield return null;
            app.gameObject.SetActive(false);
            yield return null;
            app.gameObject.SetActive(true);
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("clockwork-moth"));
            Assert.That(GameObject.Find("ArtifactDescription"), Is.Not.Null);
            Assert.That(GameObject.Find("CurioResolution"), Is.Null);
            Assert.That(GameObject.Find("CurioFarewellSeal"), Is.Null);
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent<UnityEngine.UI.Image>().color,
                Is.EqualTo(cardRestColor));
            Assert.That(typeof(GameApp).GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app), Is.False);
        }

        [UnityTest]
        public IEnumerator WrongSort_ShowsCorrectionBannerAndHighlightsExpectedDestination()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);

            var artifactIdBefore = CurrentArtifactId(app);
            var expected = ExpectedDestination(app);
            var incorrect = Enum.ToObject(expected.GetType(), (Convert.ToInt32(expected) + 1) % 3);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { incorrect });
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo(artifactIdBefore),
                "A wrong sort must keep the current artifact available for correction.");
            Assert.That(GameObject.Find("SortFeedbackPanel"), Is.Not.Null);
            Assert.That(ObjectText("SortFeedback"), Does.StartWith("WRONG · "));
            Assert.That(ObjectText("SortFeedback"), Does.Contain(EnglishDestinationName(expected)));
            Assert.That(HasEnabledOutline(DestinationButtonName(expected)), Is.True,
                "An incorrect sort must highlight the correct destination.");
            Assert.That(HasEnabledOutline(DestinationButtonName(incorrect)), Is.False,
                "The incorrect selection must not remain highlighted as the answer.");
            Assert.That(ObjectText("RuleList"), Does.Contain("<color=#E0A24B><b>"),
                "A wrong filing must highlight the rule that determined the correct desk.");

            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
            yield return WaitForFilingTransition(app);
            Assert.That(ObjectText("RuleList"), Does.Not.Contain("<color=#E0A24B><b>"),
                "The rule highlight must clear for the next curio.");
        }

        [UnityTest]
        public IEnumerator CompletedShift_IsSavedBeforeLeavingResults_AndOnlyCorrectSortsAreDiscovered()
        {
            var appType = Type.GetType("CurioClerk.Presentation.GameApp, CurioClerk.Runtime");
            var ruleEngineType = Type.GetType("CurioClerk.Core.Rules.RuleEngine, CurioClerk.Core");
            var destinationType = Type.GetType("CurioClerk.Core.Rules.Destination, CurioClerk.Core");
            Assert.That(appType, Is.Not.Null);
            Assert.That(ruleEngineType, Is.Not.Null);
            Assert.That(destinationType, Is.Not.Null);

            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);

            var save = appType.GetProperty("SaveData").GetValue(app);
            var saveType = save.GetType();
            var completedField = saveType.GetField("completedShifts");
            var discovered = (IList)saveType.GetField("discoveredArtifactIds").GetValue(save);
            discovered.Clear();
            var completedBefore = (int)completedField.GetValue(save);

            appType.GetMethod("StartNewShift").Invoke(app, new object[] { 4242 });
            var sessionField = appType.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);
            SortCurrentIncorrectly(app);
            yield return CompleteUntilCorrectSorts(app, 11);
            var finalArtifactId = CurrentArtifactId(app);
            var finalResolution = CatalogResolution(finalArtifactId, "en");
            var finalDestination = ExpectedDestination(app);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { finalDestination });

            yield return new WaitForSecondsRealtime(0.65f);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift),
                "The final docket must pulse before the results screen replaces the desk.");
            Assert.That(ObjectText("SortFeedback"),
                Does.Contain("DOCKET COMPLETE").And.Contain("+500").And.Contain("PRISTINE STREAK"),
                "Completing a docket must make its score and clean streak emotionally legible.");

            yield return WaitForFilingTransition(app);
            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Results"));
            Assert.That(FindText("ResultTitle").font.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(FindText("ResultResolution").font.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(ObjectText("ResultScore"), Does.Contain("CORRECT"));
            Assert.That(ObjectText("ResultScore"), Does.Contain("MISTAKES"));
            Assert.That(ObjectText("ResultScore"), Does.Not.Contain("✓").And.Not.Contain("✕"),
                "The result summary must avoid symbols that are missing from the release font atlas.");
            Assert.That((int)completedField.GetValue(save), Is.EqualTo(completedBefore + 1),
                "A completed result must be durable before the player leaves the results screen.");
            Assert.That(discovered.Count, Is.EqualTo(12),
                "The corrected artifact and every other item must be discovered.");
            Assert.That(ObjectText("ResultDocket0"), Does.Contain("INKED"));
            for (var docket = 1; docket < 4; docket++)
            {
                Assert.That(ObjectText("ResultDocket" + docket), Does.Contain("PRISTINE"));
            }
            for (var docket = 0; docket < 4; docket++)
            {
                Assert.That(GameObject.Find("ResultDocket" + docket).GetComponent<CanvasGroup>(),
                    Is.Not.Null, "Each completed docket row must participate in the ledger reveal.");
            }

            Assert.That(ObjectText("ResultResolution"), Does.Contain(finalResolution),
                "The completed ledger must close on the final curio's authored resolution.");
            Assert.That(GameObject.Find("RewardedAdButton"), Is.Null,
                "Core-fun validation must not display monetization offers.");

            var coinsField = saveType.GetField("coins");
            var coinsBeforeReward = (int)coinsField.GetValue(save);
            var completedSession = sessionField.GetValue(app);
            var baseShiftCoins = (int)completedSession.GetType().GetProperty("Coins").GetValue(completedSession);
            appType.GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, new object[] { true });
            adService.Emit(RewardedAdResult.Earned);
            Assert.That((int)coinsField.GetValue(save), Is.EqualTo(coinsBeforeReward + baseShiftCoins),
                "A successful double-coins reward must persist only the bonus delta immediately.");

            UnityEngine.Object.Destroy(app.gameObject);
            yield return null;
        }

        [Test]
        public void TestServices_ResetRestoresProductionFactoryBehavior()
        {
            var adService = new DeferredAdService();
            var privacyService = new ControllablePrivacyService();
            ServiceFactory.SetTestServices(adService, privacyService);

            Assert.That(ServiceFactory.CreateAdService(), Is.SameAs(adService));
            Assert.That(ServiceFactory.CreatePrivacyService(), Is.SameAs(privacyService));

            ServiceFactory.ResetTestServices();

            Assert.That(ServiceFactory.CreateAdService(), Is.Not.SameAs(adService));
            Assert.That(ServiceFactory.CreatePrivacyService(), Is.Not.SameAs(privacyService));
        }

        [UnityTest]
        public IEnumerator Reward_EarnedCallbackGrantsOnlyOnce()
        {
            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            yield return CompleteShift(app);
            var coinsBeforeReward = Coins(app);
            var baseCoins = SessionCoins(app);

            RequestCompletedShiftReward(app);
            adService.Emit(RewardedAdResult.Earned);
            var coinsAfterFirstCallback = Coins(app);
            adService.Emit(RewardedAdResult.Earned);

            Assert.That(coinsAfterFirstCallback, Is.EqualTo(coinsBeforeReward + baseCoins));
            Assert.That(Coins(app), Is.EqualTo(coinsAfterFirstCallback),
                "A duplicate terminal callback must not grant a second bonus.");
        }

        [UnityTest]
        public IEnumerator Reward_NonEarnedTerminalResultsPreserveCoinsAndRejectLaterCallbacks()
        {
            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            yield return CompleteShift(app);
            var coinsBeforeReward = Coins(app);
            var cases = new[]
            {
                new RewardFeedbackCase(RewardedAdResult.Dismissed, "Ad dismissed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Failed, "Ad failed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Unavailable, "Rewarded ad unavailable")
            };

            foreach (var current in cases)
            {
                RequestCompletedShiftReward(app);
                adService.Emit(current.Result);
                yield return null;

                Assert.That(Coins(app), Is.EqualTo(coinsBeforeReward),
                    current.Result + " must not add or remove coins.");
                Assert.That(FeedbackText(), Is.EqualTo(current.ExpectedFeedback));

                adService.Emit(RewardedAdResult.Earned);
                Assert.That(Coins(app), Is.EqualTo(coinsBeforeReward),
                    "A later callback after " + current.Result + " must be ignored.");
            }
        }

        [UnityTest]
        public IEnumerator Settings_ContainsOnlyUmpPrivacyControlAndForwardsCurrentPermission()
        {
            var adService = new DeferredAdService();
            var privacyService = new ControllablePrivacyService
            {
                CanRequestAds = true,
                PrivacyOptionsRequired = true
            };
            var app = CreateApp(adService, privacyService);
            yield return null;

            Assert.That(adService.PermissionHistory, Is.EqualTo(new[] { true }),
                "The initial UMP result must configure ad request permission.");

            app.ShowSettings();
            Assert.That(GameObject.Find("AnalyticsConsentButton"), Is.Null);
            Assert.That(GameObject.Find("CrashConsentButton"), Is.Null);
            Assert.That(GameObject.Find("AdPrivacyOptionsButton"), Is.Not.Null);

            privacyService.CanRequestAds = false;
            InvokePrivate(app, "ShowAdPrivacyOptions");

            Assert.That(adService.PermissionHistory, Is.EqualTo(new[] { true, false }),
                "The privacy-options result must forward the current UMP permission.");
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Settings));

            app.StartNewShift(4242);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift),
                "Declining ad permission must not block gameplay.");
        }

        [UnityTest]
        public IEnumerator Settings_DeferredInitialConsentRefreshesPrivacyOptionsAndForwardsPermission()
        {
            var adService = new DeferredAdService();
            var privacyService = new DeferredConsentPrivacyService();
            var app = CreateApp(adService, privacyService);
            yield return null;

            app.ShowSettings();
            Assert.That(GameObject.Find("AdPrivacyOptionsButton"), Is.Null);
            Assert.That(adService.PermissionHistory, Is.Empty);

            privacyService.CanRequestAds = true;
            privacyService.PrivacyOptionsRequired = true;
            privacyService.CompleteConsent();
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Settings));
            Assert.That(GameObject.Find("AdPrivacyOptionsButton"), Is.Not.Null,
                "Settings must refresh when the deferred initial UMP request completes.");
            Assert.That(adService.PermissionHistory, Is.EqualTo(new[] { true }));
        }

        [UnityTest]
        public IEnumerator Settings_TogglesPersistedFeedbackPreferencesAndReconfiguresService()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            SetEnglishLocale(app);
            SetSaveBool(app, "soundEnabled", true);
            SetSaveBool(app, "hapticsEnabled", true);

            app.ShowSettings();
            Assert.That(ObjectText("SoundToggleButton"), Is.EqualTo("Sound: On"));
            Assert.That(ObjectText("HapticsToggleButton"), Is.EqualTo("Haptics: On"));

            ClickButton("SoundToggleButton");
            Assert.That(SaveBool(app, "soundEnabled"), Is.False);
            Assert.That(ObjectText("SoundToggleButton"), Is.EqualTo("Sound: Off"));
            Assert.That(feedback.SoundEnabled, Is.False);
            Assert.That(feedback.HapticsEnabled, Is.True);

            ClickButton("HapticsToggleButton");
            Assert.That(SaveBool(app, "hapticsEnabled"), Is.False);
            Assert.That(ObjectText("HapticsToggleButton"), Is.EqualTo("Haptics: Off"));
            Assert.That(feedback.SoundEnabled, Is.False);
            Assert.That(feedback.HapticsEnabled, Is.False);
        }

        [UnityTest]
        public IEnumerator ShiftFeedback_PlaysInteractionCuesAndAnimatesArtifactEntrance()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            SetSaveBool(app, "soundEnabled", true);
            SetSaveBool(app, "hapticsEnabled", true);
            app.StartNewShift(4242);

            var card = GameObject.Find("CurrentArtifactCard").GetComponent<RectTransform>();
            Assert.That(card.localScale.x, Is.LessThan(0.99f),
                "A newly displayed artifact must begin with a subtle entrance transition.");
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(card.localScale.x, Is.EqualTo(1f).Within(0.01f));

            feedback.Cues.Clear();
            app.HoldCurrent();
            Assert.That(feedback.Cues, Does.Contain(PlayerFeedbackCue.Hold));
            yield return WaitForFilingTransition(app);

            var expected = ExpectedDestination(app);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
            Assert.That(feedback.Cues, Does.Contain(PlayerFeedbackCue.Correct));
            yield return WaitForFilingTransition(app);

            SortCurrentIncorrectly(app);
            Assert.That(feedback.Cues, Does.Contain(PlayerFeedbackCue.Wrong));
        }

        [UnityTest]
        public IEnumerator TerminalCorrectSort_UsesCompletionCueAndAnimatesResultInsteadOfOverlappingCues()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            app.StartNewShift(4242);

            yield return CompleteUntilCorrectSorts(app, 11);

            feedback.Cues.Clear();
            var finalDestination = ExpectedDestination(app);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { finalDestination });

            yield return WaitForFilingTransition(app);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.ShiftComplete }),
                "The terminal correct tone must not overlap the distinct completion cue.");
            var resultTitle = GameObject.Find("ResultTitle").GetComponent<RectTransform>();
            Assert.That(resultTitle.localScale.x, Is.LessThan(0.99f),
                "An immediate result transition must provide its own visible entrance feedback.");
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(resultTitle.localScale.x, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TerminalCorrectSort_DisableFlushesToAReadableInactiveResultScreen()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            app.StartNewShift(4242);
            yield return CompleteUntilCorrectSorts(app, 11);

            var finalDestination = ExpectedDestination(app);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { finalDestination });
            app.gameObject.SetActive(false);
            yield return null;

            Assert.That(PendingTransition(app), Is.Null);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
            app.gameObject.SetActive(true);
            yield return null;

            Assert.That(GameObject.Find("ResultDocket0").GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));
            Assert.That(GameObject.Find("ResultTitle").transform.localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator TerminalWrongSort_PrioritizesWrongCueAndHapticOverCompletionTone()
        {
            var feedback = new RecordingPlayerFeedbackService();
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService(), feedback);
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);

            SortCurrentIncorrectly(app);
            SortCurrentIncorrectly(app);
            Assert.That(SessionState(app), Is.EqualTo("Active"));
            Assert.That(SessionHearts(app), Is.EqualTo(1));

            feedback.Cues.Clear();
            SortCurrentIncorrectly(app);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift),
                "The final wrong response must remain visible before results replace the desk.");
            Assert.That(ObjectText("ShiftHud"), Does.Contain("HEARTS 0"),
                "The visible desk must show the lost final heart during the wrong-response animation.");
            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.Wrong }),
                "A final wrong sort must retain its corrective tone and haptic without overlapping the completion tone.");
            yield return new WaitForSecondsRealtime(0.35f);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
            var failedLedgerRows = new CanvasGroup[4];
            for (var docket = 0; docket < 4; docket++)
            {
                var row = GameObject.Find("ResultDocket" + docket);
                Assert.That(ObjectText("ResultDocket" + docket), Does.Contain("INKED"),
                    "Unfinished docket rows must visibly remain unresolved.");
                var group = row.GetComponent<CanvasGroup>();
                Assert.That(group, Is.Not.Null,
                    "Failed shifts must use the same ledger reveal as completed shifts.");
                failedLedgerRows[docket] = group;
            }
            Assert.That(failedLedgerRows[0].alpha,
                Is.GreaterThan(failedLedgerRows[3].alpha + 0.1f),
                "Failed ledger rows must participate in the same ordered reveal.");

            yield return new WaitForSecondsRealtime(0.8f);
            for (var docket = 0; docket < 4; docket++)
            {
                var row = GameObject.Find("ResultDocket" + docket);
                Assert.That(row.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.01f));
                Assert.That(Vector3.Distance(row.transform.localScale, Vector3.one), Is.LessThan(0.01f));
            }

            Assert.That(GameObject.Find("RewardedAdButton"), Is.Null,
                "A failed core-fun test run must not surface a rewarded offer.");
        }

        [UnityTest]
        public IEnumerator TerminalWrongSort_DisableFlushesItsOwnedResultContinuationOnce()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            app.StartNewShift(4242);
            SortCurrentIncorrectly(app);
            SortCurrentIncorrectly(app);
            SortCurrentIncorrectly(app);

            Assert.That(PendingTransition(app), Is.Not.Null);
            app.gameObject.SetActive(false);
            yield return null;

            Assert.That(PendingTransition(app), Is.Null);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
            app.gameObject.SetActive(true);
            yield return null;
            Assert.That(GameObject.Find("ResultDocket0").GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));

            app.gameObject.SetActive(false);
            app.gameObject.SetActive(true);
            yield return null;
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
        }

        [UnityTest]
        public IEnumerator FailedShift_EarnedRewardRevivesOnceAndDuplicateCallbacksDoNothing()
        {
            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            var saveCoinsBefore = Coins(app);
            var completedShiftsBefore = CompletedShifts(app);
            var discoveredBefore = DiscoveredCount(app);
            yield return FailShift(app);

            Assert.That(SessionState(app), Is.EqualTo("Failed"));
            Assert.That(SessionHearts(app), Is.Zero);
            Assert.That(SessionCoins(app), Is.Zero);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));

            RequestFailedShiftReward(app);
            Assert.That(adService.LastPlacement, Is.EqualTo("shift_failed_revive"));
            adService.Emit(RewardedAdResult.Earned);

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(SessionState(app), Is.EqualTo("Active"));
            Assert.That(SessionHearts(app), Is.EqualTo(1));
            Assert.That(SessionRewardClaimed(app), Is.True);
            Assert.That(Coins(app), Is.EqualTo(saveCoinsBefore));
            Assert.That(CompletedShifts(app), Is.EqualTo(completedShiftsBefore));
            Assert.That(DiscoveredCount(app), Is.EqualTo(discoveredBefore));

            adService.Emit(RewardedAdResult.Earned);
            Assert.That(SessionState(app), Is.EqualTo("Active"));
            Assert.That(SessionHearts(app), Is.EqualTo(1));

            SortCurrentIncorrectly(app);
            yield return WaitForFilingTransition(app);
            Assert.That(SessionState(app), Is.EqualTo("Failed"));
            Assert.That(SessionHearts(app), Is.Zero);
            RequestFailedShiftReward(app);
            adService.Emit(RewardedAdResult.Earned);

            Assert.That(SessionState(app), Is.EqualTo("Failed"),
                "A claimed revive must not be granted again after the revived shift fails.");
            Assert.That(SessionHearts(app), Is.Zero);
            Assert.That(Coins(app), Is.EqualTo(saveCoinsBefore));
            Assert.That(CompletedShifts(app), Is.EqualTo(completedShiftsBefore));
            Assert.That(DiscoveredCount(app), Is.EqualTo(discoveredBefore));
        }

        [UnityTest]
        public IEnumerator FailedShift_NonEarnedResultsPreserveStateProgressionAndRejectLaterEarned()
        {
            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var saveCoinsBefore = Coins(app);
            var completedShiftsBefore = CompletedShifts(app);
            var discoveredBefore = DiscoveredCount(app);
            yield return FailShift(app);
            var cases = new[]
            {
                new RewardFeedbackCase(RewardedAdResult.Dismissed, "Ad dismissed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Failed, "Ad failed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Unavailable, "Rewarded ad unavailable")
            };

            foreach (var current in cases)
            {
                RequestFailedShiftReward(app);
                adService.Emit(current.Result);
                yield return null;

                Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));
                Assert.That(SessionState(app), Is.EqualTo("Failed"));
                Assert.That(SessionHearts(app), Is.Zero);
                Assert.That(SessionCoins(app), Is.Zero);
                Assert.That(SessionRewardClaimed(app), Is.False);
                Assert.That(Coins(app), Is.EqualTo(saveCoinsBefore));
                Assert.That(CompletedShifts(app), Is.EqualTo(completedShiftsBefore));
                Assert.That(DiscoveredCount(app), Is.EqualTo(discoveredBefore));
                Assert.That(FeedbackText(), Is.EqualTo(current.ExpectedFeedback));

                adService.Emit(RewardedAdResult.Earned);
                Assert.That(SessionState(app), Is.EqualTo("Failed"));
                Assert.That(SessionHearts(app), Is.Zero);
                Assert.That(SessionRewardClaimed(app), Is.False);
            }
        }

        private static GameApp CreateApp(
            IAdService adService,
            IPrivacyService privacyService,
            IPlayerFeedbackService feedbackService = null)
        {
            ServiceFactory.SetTestServices(adService, privacyService, feedbackService);
            return new GameObject("GameAppRewardTestHost").AddComponent<GameApp>();
        }

        private static void SetEnglishLocale(GameApp app)
        {
            SetLocale(app, "en");
        }

        private static void SetLocale(GameApp app, string locale)
        {
            var localizer = (Localizer)typeof(GameApp)
                .GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            localizer.SetLocale(locale);
        }

        private static void SetIncidentProgress(GameApp app, int stageIndex, bool completed)
        {
            SetSaveString(app, "activeIncidentId", "unmelting-ice");
            SetSaveInt(app, "activeIncidentStage", stageIndex);
            app.SaveData.incidentStageRecords.Clear();
            var completedIds = SaveStringList(app, "completedIncidentIds");
            completedIds.Clear();
            if (completed)
            {
                completedIds.Add("unmelting-ice");
            }
        }

        private static void SetRememberingRainProgress(GameApp app, int stageIndex, bool completed)
        {
            SetSaveString(app, "activeIncidentId", "remembering-rain");
            SetSaveInt(app, "activeIncidentStage", stageIndex);
            app.SaveData.incidentStageRecords.Clear();
            var stageIds = new[]
            {
                "rain-01-voices",
                "rain-02-names-under-water",
                "rain-03-unsent-letter",
                "rain-04-dry-order",
                "rain-05-testimony"
            };
            for (var index = 0; index < Math.Min(stageIndex, stageIds.Length); index++)
            {
                app.SaveData.incidentStageRecords.Add(new IncidentStageRecord
                {
                    stageId = stageIds[index],
                    bestQuality = (int)IncidentQuality.Precise
                });
            }

            var completedIds = SaveStringList(app, "completedIncidentIds");
            completedIds.Clear();
            completedIds.Add("unmelting-ice");
            if (completed)
            {
                completedIds.Add("remembering-rain");
            }
        }

        private static IEnumerator BeginIncidentShift(GameApp app, int stageIndex, string locale)
        {
            SetIncidentProgress(app, stageIndex, false);
            SetLocale(app, locale);
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            yield return AdvanceNarrativeToShift(app);
        }

        private static IEnumerator BeginRememberingRainShift(GameApp app, int stageIndex, string locale)
        {
            SetSaveString(app, "activeIncidentId", "remembering-rain");
            SetSaveInt(app, "activeIncidentStage", stageIndex);
            app.SaveData.incidentStageRecords.Clear();
            var completedIds = SaveStringList(app, "completedIncidentIds");
            completedIds.Clear();
            completedIds.Add("unmelting-ice");
            SetLocale(app, locale);
            app.ShowMenu();
            ClickButton("IncidentButton");
            yield return null;
            yield return AdvanceNarrativeToShift(app);
        }

        private static IEnumerator AdvanceNarrativeToShift(GameApp app)
        {
            for (var safety = 0; safety < 8 && app.ActiveScreen == AppScreen.Narrative; safety++)
            {
                ClickButton("NarrativeContinueButton");
                yield return null;
            }

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift),
                "Incident narrative must reach the authored shift within the safety bound.");
        }

        private static IEnumerator AdvanceIncidentOutroToNextAction()
        {
            for (var safety = 0; safety < 6 && GameObject.Find("NextStageButton") == null; safety++)
            {
                ClickButton("IncidentOutroContinueButton");
                yield return null;
            }

            Assert.That(GameObject.Find("NextStageButton"), Is.Not.Null,
                "The incident outro must reveal its next action within the authored beat bound.");
        }

        private static IEnumerator WaitForAcceptanceTransition(
            GameApp app,
            int stageIndex,
            ICollection<string> stageStory,
            Action<int> recordInterlude)
        {
            var deadline = Time.realtimeSinceStartup + 6f;
            while (InputLocked(app) &&
                   GameObject.Find("IncidentDocketInterlude") == null &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            var interlude = GameObject.Find("IncidentDocketInterlude");
            if (interlude == null)
            {
                Assert.That(InputLocked(app), Is.False,
                    "A filing transition must release input when it does not open an interlude.");
                yield break;
            }

            recordInterlude(1);
            stageStory.Add(ObjectText("IncidentDocketInterludeBody"));
            Assert.That(ObjectText("IncidentDocketInterludeSpeaker"), Is.Not.Empty);
            AssertTextIsReadable("IncidentDocketInterludeSpeaker");
            AssertTextIsReadable("IncidentDocketInterludeBody");
            AssertButtonIsVisible("IncidentDocketInterludeContinueButton");
            Assert.That(InputLocked(app), Is.True);
            Assert.That(GameObject.Find("RepairButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("StorageButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("VaultButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Assert.That(GameObject.Find("HoldButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);

            var docketNumber = SessionInt(app, "CompletedDockets");
            if (docketNumber == 1)
            {
                yield return CaptureAcceptanceFrame($"rain-{stageIndex + 1:00}-interlude.png");
            }

            ClickButton("IncidentDocketInterludeContinueButton");
            yield return null;
            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Null);
            Assert.That(InputLocked(app), Is.False,
                "One Continue tap must close the interlude and return control to the desk.");
        }

        private static void AssertTextIsReadable(string objectName)
        {
            var text = GameObject.Find(objectName)?.GetComponent<TMP_Text>();
            Assert.That(text, Is.Not.Null, objectName + " must expose TMP text.");
            text.ForceMeshUpdate();
            Assert.That(text.isTextOverflowing, Is.False,
                objectName + " must not truncate or overflow its Korean copy.");
        }

        private static void AssertButtonIsVisible(string objectName)
        {
            var button = GameObject.Find(objectName)?.GetComponent<UnityEngine.UI.Button>();
            Assert.That(button, Is.Not.Null, objectName + " must remain visible and tappable.");
            Assert.That(button.gameObject.activeInHierarchy, Is.True);
            Assert.That(button.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(44f),
                objectName + " must preserve a practical touch target beneath the portrait layout.");
        }

        private static IEnumerator CaptureAcceptanceFrame(string fileName)
        {
            if (!string.Equals(
                    Environment.GetEnvironmentVariable("CURIO_CAPTURE_ACCEPTANCE"),
                    "1",
                    StringComparison.Ordinal))
            {
                yield break;
            }

            var directory = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(Application.dataPath, "..", "Logs", "Acceptance", "RememberingRain"));
            System.IO.Directory.CreateDirectory(directory);
            var path = System.IO.Path.Combine(directory, fileName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            var screenCaptureType = Type.GetType(
                "UnityEngine.ScreenCapture, UnityEngine.ScreenCaptureModule",
                throwOnError: true);
            screenCaptureType.GetMethod("CaptureScreenshot", new[] { typeof(string) })
                .Invoke(null, new object[] { path });
            var deadline = Time.realtimeSinceStartup + 3f;
            while (!System.IO.File.Exists(path) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(System.IO.File.Exists(path), Is.True, "Acceptance screenshot was not written: " + path);
        }

        private static void BeginTutorial(GameApp app)
        {
            app.ShowTutorial();
            ClickButton("BeginTutorialShiftButton");
        }

        private static void ClickButton(string objectName)
        {
            var button = GameObject.Find(objectName);
            Assert.That(button, Is.Not.Null, objectName + " must exist in the active view.");
            button.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        }

        private static void ChooseDestination(GameApp app, int destinationValue)
        {
            var destinationType = Type.GetType("CurioClerk.Core.Rules.Destination, CurioClerk.Core");
            var destination = Enum.ToObject(destinationType, destinationValue);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { destination });
        }

        private static IEnumerator CompleteShift(GameApp app)
        {
            app.StartNewShift(4242);
            yield return CompleteActiveShift(app);
        }

        private static void StartAuthoredShift(GameApp app, params string[] artifactIds)
        {
            var queue = new List<Artifact>();
            var catalog = ContentCatalog.CreateArtifacts();
            foreach (var artifactId in artifactIds)
            {
                ArtifactContent content = null;
                foreach (var candidate in catalog)
                {
                    if (candidate.Id == artifactId)
                    {
                        content = candidate;
                        break;
                    }
                }

                Assert.That(content, Is.Not.Null, "Missing authored test artifact: " + artifactId);
                queue.Add(content.ToArtifact());
            }

            var rules = ContentCatalog.CreateRulePacks()[0].Rules;
            typeof(GameApp).GetField("_plannedQueue", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, queue);
            typeof(GameApp).GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, rules);
            typeof(GameApp).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, new ShiftSession(queue, rules));
            InvokePrivate(app, "BuildShiftScreen");
        }

        private static void ShowSortFeedbackForCurrent(GameApp app, Destination destination)
        {
            var session = (ShiftSession)Session(app);
            var artifact = session.CurrentArtifact;
            ArtifactContent content = null;
            foreach (var candidate in ContentCatalog.CreateArtifacts())
            {
                if (candidate.Id == artifact.Id)
                {
                    content = candidate;
                    break;
                }
            }

            Assert.That(content, Is.Not.Null, "Missing authored test artifact: " + artifact.Id);
            var outcome = session.Sort(destination);
            typeof(GameApp)
                .GetMethod("ShowSortFeedback", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { artifact, content, outcome, false, false });
        }

        private static IEnumerator CompleteActiveShift(GameApp app)
        {
            for (var safety = 0; safety < 64 && SessionState(app) == "Active"; safety++)
            {
                var expected = ExpectedDestination(app);
                var button = GameObject.Find(DestinationButtonName(expected))
                    .GetComponent<UnityEngine.UI.Button>();
                if (button.interactable)
                {
                    typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
                }
                else
                {
                    app.HoldCurrent();
                }

                yield return WaitForFilingTransition(app);
            }

            Assert.That(SessionState(app), Is.EqualTo("Completed"),
                "The generated plan must complete within the safety bound using one Hold slot.");
        }

        private static IEnumerator CompleteUntilCorrectSorts(GameApp app, int targetCorrectSorts)
        {
            for (var safety = 0;
                 safety < 64 && SessionState(app) == "Active" &&
                 SessionCorrectSorts(app) < targetCorrectSorts;
                 safety++)
            {
                var expected = ExpectedDestination(app);
                var button = GameObject.Find(DestinationButtonName(expected))
                    .GetComponent<UnityEngine.UI.Button>();
                if (button.interactable)
                {
                    typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
                }
                else
                {
                    app.HoldCurrent();
                }

                yield return WaitForFilingTransition(app);
            }

            Assert.That(SessionCorrectSorts(app), Is.EqualTo(targetCorrectSorts));
        }

        private static IEnumerator CompleteCurrentDocketUntilInterlude(GameApp app, int expectedDocket)
        {
            for (var safety = 0; safety < 16 && SessionInt(app, "CompletedDockets") < expectedDocket; safety++)
            {
                var completedBefore = SessionInt(app, "CompletedDockets");
                var expected = ExpectedDestination(app);
                var button = GameObject.Find(DestinationButtonName(expected))
                    .GetComponent<UnityEngine.UI.Button>();
                if (button.interactable)
                {
                    typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
                }
                else
                {
                    app.HoldCurrent();
                }

                if (SessionInt(app, "CompletedDockets") == completedBefore)
                {
                    var transitionDeadline = Time.realtimeSinceStartup + 6f;
                    while (InputLocked(app) &&
                           GameObject.Find("IncidentDocketInterlude") == null &&
                           Time.realtimeSinceStartup < transitionDeadline)
                    {
                        yield return null;
                    }

                    Assert.That(
                        !InputLocked(app) || GameObject.Find("IncidentDocketInterlude") != null,
                        Is.True,
                        "A filing transition must either release input or hand ownership to the interlude.");
                }
            }

            var remainingDestination = ExpectedDestination(app);
            var remainingButton = GameObject.Find(DestinationButtonName(remainingDestination))
                .GetComponent<UnityEngine.UI.Button>();
            Assert.That(
                SessionInt(app, "CompletedDockets"),
                Is.EqualTo(expectedDocket),
                $"Current={CurrentArtifactId(app)}, expected={remainingDestination}, " +
                $"button={remainingButton.interactable}, canHold={((ShiftSession)Session(app)).CanHold}, " +
                $"inputLocked={InputLocked(app)}");
            var deadline = Time.realtimeSinceStartup + 2.5f;
            while (GameObject.Find("IncidentDocketInterlude") == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameObject.Find("IncidentDocketInterlude"), Is.Not.Null,
                $"Docket {expectedDocket} must show its authored incident interlude.");
        }

        private static IEnumerator WaitForFilingTransition(GameApp app)
        {
            var inputLocked = typeof(GameApp)
                .GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(inputLocked, Is.Not.Null);
            var reaction = GameObject.Find("IncidentReactionText")?.GetComponent<TMP_Text>();
            var timeoutSeconds = reaction != null && reaction.enabled ? 3f : 1.5f;
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while ((bool)inputLocked.GetValue(app) && Time.realtimeSinceStartup < deadline)
            {
                if (GameObject.Find("IncidentDocketInterlude") != null)
                {
                    ClickButton("IncidentDocketInterludeContinueButton");
                }

                yield return null;
            }

            Assert.That((bool)inputLocked.GetValue(app), Is.False,
                $"The filing transition must release input within {timeoutSeconds:0.0} seconds.");
        }

        private static object PendingTransition(GameApp app)
            => typeof(GameApp)
                .GetField("_pendingTransition", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);

        private static bool InputLocked(GameApp app)
            => (bool)typeof(GameApp)
                .GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);

        private static void StartDailyShift(GameApp app)
        {
            var method = typeof(GameApp).GetMethod("StartDailyShift");
            Assert.That(method, Is.Not.Null, "GameApp must expose the daily challenge entry point.");
            method.Invoke(app, null);
        }

        private static void SetClock(GameApp app, DateTime localNow)
        {
            var clock = new FixedClock(localNow);
            typeof(GameApp).GetField("_clock", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(app, clock);
            typeof(GameApp).GetField("_seedProvider", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, new ShiftSeedProvider(clock));
        }

        private static IEnumerator FailShift(GameApp app)
        {
            app.StartNewShift(4242);
            for (var index = 0; index < 3; index++)
            {
                SortCurrentIncorrectly(app);
            }

            yield return WaitForFilingTransition(app);
        }

        private static void SortCurrentIncorrectly(GameApp app)
        {
            var expected = ExpectedDestination(app);
            var session = Session(app);
            var canSort = session.GetType().GetMethod("CanSort");
            for (var offset = 1; offset < 3; offset++)
            {
                var incorrect = Enum.ToObject(
                    expected.GetType(),
                    (Convert.ToInt32(expected) + offset) % 3);
                if ((bool)canSort.Invoke(session, new[] { incorrect }))
                {
                    typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { incorrect });
                    return;
                }
            }

            Assert.Fail("The current docket has no available incorrect destination.");
        }

        private static object ExpectedDestination(GameApp app)
        {
            var appType = typeof(GameApp);
            var session = appType
                .GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            var rules = appType
                .GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            var artifact = session.GetType().GetProperty("CurrentArtifact").GetValue(session);
            var ruleEngineType = Type.GetType("CurioClerk.Core.Rules.RuleEngine, CurioClerk.Core");
            var ruleEngine = Activator.CreateInstance(ruleEngineType);
            return ruleEngineType.GetMethod("Resolve").Invoke(ruleEngine, new[] { artifact, rules });
        }

        private static string DestinationButtonName(object destination)
        {
            switch (Convert.ToInt32(destination))
            {
                case 0: return "RepairButton";
                case 2: return "VaultButton";
                default: return "StorageButton";
            }
        }

        private static string EnglishDestinationName(object destination)
        {
            switch (Convert.ToInt32(destination))
            {
                case 0: return "REPAIR";
                case 2: return "VAULT";
                default: return "STORAGE";
            }
        }

        private static bool HasEnabledOutline(string objectName)
        {
            var outlineType = Type.GetType("UnityEngine.UI.Outline, UnityEngine.UI");
            var target = GameObject.Find(objectName);
            Assert.That(target, Is.Not.Null, objectName + " must exist in the active shift view.");
            var outline = target.GetComponent(outlineType) as Behaviour;
            return outline != null && outline.enabled;
        }

        private static int Coins(GameApp app)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (int)save.GetType().GetField("coins").GetValue(save);
        }

        private static void SetSaveInt(GameApp app, string fieldName, int value)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            save.GetType().GetField(fieldName).SetValue(save, value);
        }

        private static int SaveInt(GameApp app, string fieldName)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (int)save.GetType().GetField(fieldName).GetValue(save);
        }

        private static void SetSaveString(GameApp app, string fieldName, string value)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            save.GetType().GetField(fieldName).SetValue(save, value);
        }

        private static IList SaveStringList(GameApp app, string fieldName)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (IList)save.GetType().GetField(fieldName).GetValue(save);
        }

        private static string SaveString(GameApp app, string fieldName)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (string)save.GetType().GetField(fieldName).GetValue(save);
        }

        private static int SessionCoins(GameApp app)
        {
            var session = typeof(GameApp)
                .GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            return (int)session.GetType().GetProperty("Coins").GetValue(session);
        }

        private static int SessionHearts(GameApp app)
        {
            return (int)Session(app).GetType().GetProperty("Hearts").GetValue(Session(app));
        }

        private static int SessionScore(GameApp app)
        {
            return (int)Session(app).GetType().GetProperty("Score").GetValue(Session(app));
        }

        private static int SessionCorrectSorts(GameApp app)
        {
            return (int)Session(app).GetType().GetProperty("CorrectSorts").GetValue(Session(app));
        }

        private static string SessionState(GameApp app)
        {
            return Session(app).GetType().GetProperty("State").GetValue(Session(app)).ToString();
        }

        private static bool SessionRewardClaimed(GameApp app)
        {
            return (bool)Session(app).GetType().GetProperty("RewardClaimed").GetValue(Session(app));
        }

        private static object Session(GameApp app)
        {
            return typeof(GameApp)
                .GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
        }

        private static IList PlannedQueue(GameApp app)
        {
            return (IList)typeof(GameApp)
                .GetField("_plannedQueue", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
        }

        private static string ArtifactId(object artifact)
        {
            return (string)artifact.GetType().GetProperty("Id").GetValue(artifact);
        }

        private static int SessionInt(GameApp app, string propertyName)
        {
            return (int)Session(app).GetType().GetProperty(propertyName).GetValue(Session(app));
        }

        private static string CurrentArtifactId(GameApp app)
        {
            var artifact = Session(app).GetType().GetProperty("CurrentArtifact").GetValue(Session(app));
            return (string)artifact.GetType().GetProperty("Id").GetValue(artifact);
        }

        private static string HeldArtifactId(GameApp app)
        {
            var artifact = Session(app).GetType().GetProperty("HeldArtifact").GetValue(Session(app));
            return artifact == null ? null : (string)artifact.GetType().GetProperty("Id").GetValue(artifact);
        }

        private static bool TutorialCompleted(GameApp app)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (bool)save.GetType().GetField("tutorialCompleted").GetValue(save);
        }

        private static void SetTutorialCompleted(GameApp app, bool completed)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            save.GetType().GetField("tutorialCompleted").SetValue(save, completed);
        }

        private static int CompletedShifts(GameApp app)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (int)save.GetType().GetField("completedShifts").GetValue(save);
        }

        private static int DiscoveredCount(GameApp app)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return ((IList)save.GetType().GetField("discoveredArtifactIds").GetValue(save)).Count;
        }

        private static bool SaveBool(GameApp app, string fieldName)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            return (bool)save.GetType().GetField(fieldName).GetValue(save);
        }

        private static void SetSaveBool(GameApp app, string fieldName, bool value)
        {
            var save = typeof(GameApp).GetProperty("SaveData").GetValue(app);
            save.GetType().GetField(fieldName).SetValue(save, value);
        }

        private static void RequestCompletedShiftReward(GameApp app)
        {
            typeof(GameApp)
                .GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { true });
        }

        private static void RequestFailedShiftReward(GameApp app)
        {
            typeof(GameApp)
                .GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { false });
        }

        private static string CatalogResolution(string artifactId, string locale)
        {
            foreach (var artifact in ContentCatalog.CreateArtifacts())
            {
                if (artifact.Id == artifactId)
                {
                    return locale == "ko" ? artifact.ResolutionKorean : artifact.ResolutionEnglish;
                }
            }

            Assert.Fail("Missing catalog resolution for " + artifactId);
            return string.Empty;
        }

        private static void InvokePrivate(GameApp app, string methodName)
        {
            typeof(GameApp)
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, null);
        }

        private static string FeedbackText()
        {
            return ObjectText("RewardedAdFeedback");
        }

        private static string ObjectText(string objectName)
        {
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            var textObject = GameObject.Find(objectName);
            Assert.That(textObject, Is.Not.Null, objectName + " must exist in the active view.");
            var textComponent = textObject.GetComponent(textType) ?? textObject.GetComponentInChildren(textType);
            Assert.That(textComponent, Is.Not.Null, objectName + " must expose visible TMP text.");
            return (string)textType.GetProperty("text").GetValue(textComponent);
        }

        private static RectTransform FindRect(string objectName)
        {
            var target = GameObject.Find(objectName);
            Assert.That(target, Is.Not.Null, objectName + " must exist in the active view.");
            return target.GetComponent<RectTransform>();
        }

        private static TMP_Text FindText(string objectName)
        {
            var target = GameObject.Find(objectName);
            Assert.That(target, Is.Not.Null, objectName + " must exist in the active view.");
            var text = target.GetComponent<TMP_Text>() ?? target.GetComponentInChildren<TMP_Text>();
            Assert.That(text, Is.Not.Null, objectName + " must expose visible TMP text.");
            return text;
        }

        private readonly struct IncidentResultCopyCase
        {
            public IncidentResultCopyCase(
                IncidentQuality quality,
                string locale,
                string label,
                string qualityBody,
                string reactionBody)
            {
                Quality = quality;
                Locale = locale;
                Label = label;
                QualityBody = qualityBody;
                ReactionBody = reactionBody;
            }

            public IncidentQuality Quality { get; }
            public string Locale { get; }
            public string Label { get; }
            public string QualityBody { get; }
            public string ReactionBody { get; }
        }

        private readonly struct RewardFeedbackCase
        {
            public RewardFeedbackCase(RewardedAdResult result, string expectedFeedback)
            {
                Result = result;
                ExpectedFeedback = expectedFeedback;
            }

            public RewardedAdResult Result { get; }

            public string ExpectedFeedback { get; }
        }

        private sealed class DeferredAdService : IAdService
        {
            private Action<RewardedAdResult> _completed;

            public bool IsRewardedReady => PermissionHistory.Count > 0 && PermissionHistory[PermissionHistory.Count - 1];

            public List<bool> PermissionHistory { get; } = new List<bool>();

            public string LastPlacement { get; private set; }

            public void SetRequestPermission(bool allowed)
            {
                PermissionHistory.Add(allowed);
            }

            public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
            {
                LastPlacement = placement;
                _completed = completed;
            }

            public void Emit(RewardedAdResult result)
            {
                Assert.That(_completed, Is.Not.Null, "No rewarded-ad request is pending.");
                _completed(result);
            }
        }

        private sealed class FixedClock : IClock
        {
            public FixedClock(DateTime localNow)
            {
                LocalNow = localNow;
            }

            public DateTime LocalNow { get; }
        }

        private sealed class RecordingPlayerFeedbackService : IPlayerFeedbackService
        {
            public List<PlayerFeedbackCue> Cues { get; } = new List<PlayerFeedbackCue>();

            public bool SoundEnabled { get; private set; }

            public bool HapticsEnabled { get; private set; }

            public void Configure(bool soundEnabled, bool hapticsEnabled)
            {
                SoundEnabled = soundEnabled;
                HapticsEnabled = hapticsEnabled;
            }

            public void Play(PlayerFeedbackCue cue)
            {
                Cues.Add(cue);
            }

            public void Dispose()
            {
            }
        }

        private sealed class RecordingSaveStore : ISaveStore
        {
            public int SaveCalls { get; private set; }
            public int PersistedIncidentStage { get; private set; }
            public int PersistedRecordCount { get; private set; }
            public int PersistedBestQuality { get; private set; } = -1;

            public PlayerSaveData LoadOrDefault() => new PlayerSaveData();

            public void Save(PlayerSaveData data)
            {
                SaveCalls++;
                PersistedIncidentStage = data.activeIncidentStage;
                PersistedRecordCount = data.incidentStageRecords.Count;
                PersistedBestQuality = data.incidentStageRecords.Count > 0
                    ? data.incidentStageRecords[0].bestQuality
                    : -1;
            }
        }

        private sealed class ControllablePrivacyService : IPrivacyService
        {
            public bool CanRequestAds { get; set; } = true;

            public bool PrivacyOptionsRequired { get; set; }

            public void RequestConsent(Action<bool> completed)
            {
                completed?.Invoke(CanRequestAds);
            }

            public void ShowPrivacyOptions(Action<bool> completed)
            {
                completed?.Invoke(CanRequestAds);
            }
        }

        private sealed class DeferredConsentPrivacyService : IPrivacyService
        {
            private Action<bool> _consentCompleted;
            public int RequestCalls { get; private set; }

            public bool CanRequestAds { get; set; }

            public bool PrivacyOptionsRequired { get; set; }

            public void RequestConsent(Action<bool> completed)
            {
                RequestCalls++;
                _consentCompleted = completed;
            }

            public void ShowPrivacyOptions(Action<bool> completed)
            {
                completed?.Invoke(CanRequestAds);
            }

            public void CompleteConsent()
            {
                Assert.That(_consentCompleted, Is.Not.Null, "No initial consent request is pending.");
                var completed = _consentCompleted;
                _consentCompleted = null;
                completed(CanRequestAds);
            }
        }
    }
}

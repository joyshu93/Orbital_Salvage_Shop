using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CurioClerk.Content;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Infrastructure;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Feedback;
using CurioClerk.Infrastructure.Privacy;
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
        public IEnumerator App_StartsAtMenuAndBuildsAPlayableShiftLayout()
        {
            var appType = Type.GetType("CurioClerk.Presentation.GameApp, CurioClerk.Runtime");
            Assert.That(appType, Is.Not.Null, "Missing production type: CurioClerk.Presentation.GameApp");
            var host = new GameObject("GameAppTestHost");
            var app = host.AddComponent(appType);
            yield return null;

            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Menu"));
            Assert.That(GameObject.Find("CurioClerkCanvas"), Is.Not.Null);
            Assert.That(GameObject.Find("StartShiftButton"), Is.Not.Null);
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            var fontProperty = textType.GetProperty("font");
            var titleText = GameObject.Find("Title").GetComponent(textType);
            var startButtonText = GameObject.Find("StartShiftButton").GetComponentInChildren(textType);
            var titleFont = fontProperty.GetValue(titleText) as UnityEngine.Object;
            var startButtonFont = fontProperty.GetValue(startButtonText) as UnityEngine.Object;
            Assert.That(titleFont, Is.Not.Null);
            Assert.That(startButtonFont, Is.Not.Null);
            Assert.That(titleFont.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(startButtonFont.name, Does.StartWith("NotoSansKR"));
            Assert.That(titleFont, Is.Not.SameAs(startButtonFont));

            appType.GetMethod("StartNewShift").Invoke(app, new object[] { 4242 });
            yield return null;

            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Shift"));
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

            UnityEngine.Object.Destroy(host);
            yield return null;
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
        public IEnumerator DailyFile_ShowsAvailabilityAndStartsDatedChallenge()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            SetClock(app, new DateTime(2026, 8, 26, 21, 30, 0));
            SetSaveString(app, "lastDailyCompletedDate", string.Empty);
            SetSaveInt(app, "dailyBestScore", 0);

            app.ShowMenu();

            Assert.That(ObjectText("DailyShiftButton"), Does.Contain("2026-08-26"));
            Assert.That(ObjectText("DailyShiftButton"), Does.Contain("Available"));

            ClickButton("DailyShiftButton");
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
            Assert.That(ObjectText("DailyShiftButton"), Does.Contain($"Completed · Best {completedScore}"));
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

        private static IEnumerator WaitForFilingTransition(GameApp app)
        {
            var inputLocked = typeof(GameApp)
                .GetField("_inputLocked", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(inputLocked, Is.Not.Null);
            var deadline = Time.realtimeSinceStartup + 1.5f;
            while ((bool)inputLocked.GetValue(app) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That((bool)inputLocked.GetValue(app), Is.False,
                "The filing transition must release input within 1.5 seconds.");
        }

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

            public bool CanRequestAds { get; set; }

            public bool PrivacyOptionsRequired { get; set; }

            public void RequestConsent(Action<bool> completed)
            {
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

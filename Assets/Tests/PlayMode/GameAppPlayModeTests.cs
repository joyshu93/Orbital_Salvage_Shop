using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CurioClerk.Infrastructure;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Privacy;
using CurioClerk.Localization;
using CurioClerk.Presentation;
using NUnit.Framework;
using UnityEngine;
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
            var titleText = GameObject.Find("Title").GetComponent(textType);
            var titleFont = textType.GetProperty("font").GetValue(titleText) as UnityEngine.Object;
            Assert.That(titleFont, Is.Not.Null);
            Assert.That(titleFont.name, Does.StartWith("NotoSansKR"));

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
            var dragType = Type.GetType("CurioClerk.Presentation.ArtifactDragHandler, CurioClerk.Runtime");
            Assert.That(dragType, Is.Not.Null, "Missing card drag interaction type.");
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent(dragType), Is.Not.Null,
                "The current artifact card must support drag-to-sort.");

            UnityEngine.Object.Destroy(host);
            yield return null;
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
                yield return null;

                var heldArtifact = Session(app).GetType().GetProperty("HeldArtifact").GetValue(Session(app));
                var heldId = (string)heldArtifact.GetType().GetProperty("Id").GetValue(heldArtifact);
                var heldContent = contentById[heldId];
                var expectedHeldName = (string)heldContent.GetType().GetProperty(nextNameProperty).GetValue(heldContent);
                Assert.That(ObjectText("HeldArtifactText"), Does.Contain(expectedHeldName),
                    locale + " hold preview must identify the held artifact by name.");
            }
        }

        [UnityTest]
        public IEnumerator Tutorial_BeginsFourArtifactGuidedShiftWithoutCompletingSave()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            var completedBefore = CompletedShifts(app);
            var coinsBefore = Coins(app);

            BeginTutorial(app);
            yield return null;

            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Shift));
            Assert.That(TutorialCompleted(app), Is.False,
                "Opening the guided shift must not mark the tutorial complete.");
            Assert.That(PlannedQueue(app).Count, Is.EqualTo(4));
            Assert.That(CurrentArtifactId(app), Is.EqualTo("sleeping-teacup"));
            Assert.That(ObjectText("TutorialCoach"), Does.StartWith("1 / 4"));
            Assert.That(CompletedShifts(app), Is.EqualTo(completedBefore));
            Assert.That(Coins(app), Is.EqualTo(coinsBefore));
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

            ChooseDestination(app, 1);
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("sleeping-teacup"));
            Assert.That(SessionHearts(app), Is.EqualTo(3));
            Assert.That(ObjectText("SortFeedback"), Does.Contain("REPAIR"));
            Assert.That(ObjectText("TutorialCoach"), Does.StartWith("1 / 4"));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator Tutorial_ThirdLessonRequiresHoldBeforeSorting()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            var tutorialBefore = TutorialCompleted(app);
            SetTutorialCompleted(app, false);
            BeginTutorial(app);
            ChooseDestination(app, 0);
            ChooseDestination(app, 0);

            Assert.That(CurrentArtifactId(app), Is.EqualTo("thimble-storm"));
            ChooseDestination(app, 1);
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("thimble-storm"));
            Assert.That(SessionHearts(app), Is.EqualTo(3));
            Assert.That(ObjectText("TutorialCoach"), Does.Contain("HOLD"));

            app.HoldCurrent();
            yield return null;

            Assert.That(CurrentArtifactId(app), Is.EqualTo("whispering-key"));
            Assert.That(HeldArtifactId(app), Is.EqualTo("thimble-storm"));
            Assert.That(ObjectText("TutorialCoach"), Does.StartWith("3 / 4"));
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

            ChooseDestination(app, 0);
            ChooseDestination(app, 0);
            app.HoldCurrent();
            ChooseDestination(app, 2);
            Assert.That(ObjectText("TutorialCoach"), Does.StartWith("4 / 4"));
            ChooseDestination(app, 1);
            yield return null;

            Assert.That(TutorialCompleted(app), Is.True);
            Assert.That(GameObject.Find("TutorialCompleteScreen"), Is.Not.Null);
            Assert.That(CompletedShifts(app), Is.EqualTo(completedBefore));
            Assert.That(Coins(app), Is.EqualTo(coinsBefore));
            Assert.That(DiscoveredCount(app), Is.EqualTo(discoveredBefore));
            SetTutorialCompleted(app, tutorialBefore);
        }

        [UnityTest]
        public IEnumerator CorrectSort_ShowsPositiveBannerAndHighlightsSelectedDestination()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);

            var expected = ExpectedDestination(app);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
            yield return null;

            Assert.That(GameObject.Find("SortFeedbackPanel"), Is.Not.Null);
            Assert.That(ObjectText("SortFeedback"), Does.StartWith("CORRECT · "));
            Assert.That(HasEnabledOutline(DestinationButtonName(expected)), Is.True,
                "A correct sort must highlight the selected destination.");
        }

        [UnityTest]
        public IEnumerator WrongSort_ShowsCorrectionBannerAndHighlightsExpectedDestination()
        {
            var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
            yield return null;
            SetEnglishLocale(app);
            app.StartNewShift(4242);

            var expected = ExpectedDestination(app);
            var incorrect = Enum.ToObject(expected.GetType(), (Convert.ToInt32(expected) + 1) % 3);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { incorrect });
            yield return null;

            Assert.That(GameObject.Find("SortFeedbackPanel"), Is.Not.Null);
            Assert.That(ObjectText("SortFeedback"), Does.StartWith("WRONG · "));
            Assert.That(ObjectText("SortFeedback"), Does.Contain(EnglishDestinationName(expected)));
            Assert.That(HasEnabledOutline(DestinationButtonName(expected)), Is.True,
                "An incorrect sort must highlight the correct destination.");
            Assert.That(HasEnabledOutline(DestinationButtonName(incorrect)), Is.False,
                "The incorrect selection must not remain highlighted as the answer.");
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
            var rulesField = appType.GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic);
            var choose = appType.GetMethod("ChooseDestination");
            var ruleEngine = Activator.CreateInstance(ruleEngineType);
            var resolve = ruleEngineType.GetMethod("Resolve");

            for (var index = 0; index < 12; index++)
            {
                var session = sessionField.GetValue(app);
                var artifact = session.GetType().GetProperty("CurrentArtifact").GetValue(session);
                var expected = resolve.Invoke(ruleEngine, new[] { artifact, rulesField.GetValue(app) });
                var selected = index == 0
                    ? Enum.ToObject(destinationType, ((int)expected + 1) % 3)
                    : expected;
                choose.Invoke(app, new[] { selected });
            }

            yield return null;
            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Results"));
            Assert.That(ObjectText("ResultScore"), Does.Contain("CORRECT"));
            Assert.That(ObjectText("ResultScore"), Does.Contain("MISTAKES"));
            Assert.That(ObjectText("ResultScore"), Does.Not.Contain("✓").And.Not.Contain("✕"),
                "The result summary must avoid symbols that are missing from the release font atlas.");
            Assert.That((int)completedField.GetValue(save), Is.EqualTo(completedBefore + 1),
                "A completed result must be durable before the player leaves the results screen.");
            Assert.That(discovered.Count, Is.EqualTo(11),
                "The incorrectly sorted artifact must remain undiscovered.");

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
            CompleteShift(app);
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
            CompleteShift(app);
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
        public IEnumerator FailedShift_EarnedRewardRevivesOnceAndDuplicateCallbacksDoNothing()
        {
            var adService = new DeferredAdService();
            var app = CreateApp(adService, new ControllablePrivacyService());
            yield return null;
            var saveCoinsBefore = Coins(app);
            var completedShiftsBefore = CompletedShifts(app);
            var discoveredBefore = DiscoveredCount(app);
            FailShift(app);

            Assert.That(SessionState(app), Is.EqualTo("Failed"));
            Assert.That(SessionHearts(app), Is.Zero);
            Assert.That(SessionCoins(app), Is.Zero);
            Assert.That(app.ActiveScreen, Is.EqualTo(AppScreen.Results));

            ClickRewardedAdButton();
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
            Assert.That(SessionState(app), Is.EqualTo("Failed"));
            Assert.That(SessionHearts(app), Is.Zero);
            ClickRewardedAdButton();
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
            FailShift(app);
            var cases = new[]
            {
                new RewardFeedbackCase(RewardedAdResult.Dismissed, "Ad dismissed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Failed, "Ad failed. No changes were made."),
                new RewardFeedbackCase(RewardedAdResult.Unavailable, "Rewarded ad unavailable")
            };

            foreach (var current in cases)
            {
                ClickRewardedAdButton();
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

        private static GameApp CreateApp(IAdService adService, IPrivacyService privacyService)
        {
            ServiceFactory.SetTestServices(adService, privacyService);
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

        private static void CompleteShift(GameApp app)
        {
            var appType = typeof(GameApp);
            var ruleEngineType = Type.GetType("CurioClerk.Core.Rules.RuleEngine, CurioClerk.Core");
            app.StartNewShift(4242);
            var sessionField = appType.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);
            var rulesField = appType.GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic);
            var choose = appType.GetMethod("ChooseDestination");
            var ruleEngine = Activator.CreateInstance(ruleEngineType);
            var resolve = ruleEngineType.GetMethod("Resolve");

            for (var index = 0; index < 12; index++)
            {
                var session = sessionField.GetValue(app);
                var artifact = session.GetType().GetProperty("CurrentArtifact").GetValue(session);
                var expected = resolve.Invoke(ruleEngine, new[] { artifact, rulesField.GetValue(app) });
                choose.Invoke(app, new[] { expected });
            }
        }

        private static void FailShift(GameApp app)
        {
            app.StartNewShift(4242);
            for (var index = 0; index < 3; index++)
            {
                SortCurrentIncorrectly(app);
            }
        }

        private static void SortCurrentIncorrectly(GameApp app)
        {
            var expected = ExpectedDestination(app);
            var incorrect = Enum.ToObject(expected.GetType(), (Convert.ToInt32(expected) + 1) % 3);
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { incorrect });
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

        private static void RequestCompletedShiftReward(GameApp app)
        {
            typeof(GameApp)
                .GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(app, new object[] { true });
        }

        private static void ClickRewardedAdButton()
        {
            var buttonType = Type.GetType("UnityEngine.UI.Button, UnityEngine.UI");
            var button = GameObject.Find("RewardedAdButton");
            Assert.That(button, Is.Not.Null, "The failed results screen must expose the rewarded-ad offer.");
            var onClick = buttonType.GetProperty("onClick").GetValue(button.GetComponent(buttonType));
            onClick.GetType().GetMethod("Invoke").Invoke(onClick, null);
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
            return (string)textType.GetProperty("text").GetValue(textObject.GetComponent(textType));
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

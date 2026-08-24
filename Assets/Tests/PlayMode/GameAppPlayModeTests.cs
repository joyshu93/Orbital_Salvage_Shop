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
            var localizer = (Localizer)typeof(GameApp)
                .GetField("_localizer", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(app);
            localizer.SetLocale("en");
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
            var expected = ruleEngineType.GetMethod("Resolve").Invoke(ruleEngine, new[] { artifact, rules });
            var incorrect = Enum.ToObject(expected.GetType(), (Convert.ToInt32(expected) + 1) % 3);
            appType.GetMethod("ChooseDestination").Invoke(app, new[] { incorrect });
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
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            var feedback = GameObject.Find("RewardedAdFeedback");
            Assert.That(feedback, Is.Not.Null, "A terminal ad result must be visible to the player.");
            return (string)textType.GetProperty("text").GetValue(feedback.GetComponent(textType));
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

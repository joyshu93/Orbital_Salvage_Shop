using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Progression;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Privacy;
using CurioClerk.Infrastructure.Save;
using CurioClerk.Infrastructure.Time;
using CurioClerk.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class GameApp : MonoBehaviour
    {
        private static TMP_FontAsset s_InterfaceFont;
        private static readonly Color Plum = Hex("#351B2B");
        private static readonly Color Wine = Hex("#5B2944");
        private static readonly Color Paper = Hex("#F2E5C4");
        private static readonly Color Ink = Hex("#2B2025");
        private static readonly Color Amber = Hex("#E0A24B");
        private static readonly Color Sage = Hex("#6F8A6B");
        private static readonly Color DustyRose = Hex("#B56D78");

        private readonly ShiftGenerator _shiftGenerator = new ShiftGenerator();
        private readonly ProgressionService _progression = new ProgressionService();
        private readonly HashSet<string> _seenThisShift = new HashSet<string>(StringComparer.Ordinal);
        private IReadOnlyList<ArtifactContent> _artifactContent;
        private Dictionary<string, ArtifactContent> _artifactById;
        private IReadOnlyList<SortingRule> _activeRules;
        private IReadOnlyList<Artifact> _plannedQueue;
        private ShiftSession _session;
        private PlayerSaveData _save;
        private ISaveStore _saveStore;
        private IAdService _adService;
        private IAnalyticsService _analytics;
        private IPrivacyService _privacy;
        private ICrashReporter _crashReporter;
        private IShiftSeedProvider _seedProvider;
        private Localizer _localizer;
        private RectTransform _screenRoot;
        private TMP_Text _currentSymbol;
        private TMP_Text _currentName;
        private TMP_Text _currentDescription;
        private TMP_Text _currentTraits;
        private TMP_Text _heldText;
        private readonly TMP_Text[] _nextTexts = new TMP_Text[2];
        private TMP_Text _statusText;
        private TMP_Text _hudText;
        private int _sortedCount;
        private bool _resultApplied;
        private int _appliedResultCoins;
        private bool _adConsentResolved;
        private bool _canRequestAds;

        public AppScreen ActiveScreen { get; private set; }

        public PlayerSaveData SaveData => _save;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            _artifactContent = ContentCatalog.CreateArtifacts();
            _artifactById = _artifactContent.ToDictionary(item => item.Id, StringComparer.Ordinal);
            _saveStore = new JsonFileSaveStore(Path.Combine(Application.persistentDataPath, "curio-clerk-save.json"));
            _save = _saveStore.LoadOrDefault();
            _localizer = new Localizer(_save.locale);
            _adService = Infrastructure.ServiceFactory.CreateAdService();
            _analytics = Infrastructure.ServiceFactory.CreateAnalyticsService();
            _privacy = Infrastructure.ServiceFactory.CreatePrivacyService();
            _crashReporter = Infrastructure.ServiceFactory.CreateCrashReporter();
            _analytics.SetConsent(_save.analyticsConsent);
            _crashReporter.SetConsent(_save.crashReportingConsent);
            _seedProvider = new ShiftSeedProvider(new SystemClock());
            BuildShell();
            ShowMenu();
            RequestAdConsent();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Save();
            }
        }

        private void OnDestroy() => Save();

        public void ShowMenu()
        {
            ActiveScreen = AppScreen.Menu;
            var page = CreatePage("MainMenuScreen");
            CreateText(page, "Eyebrow", _localizer.Get("subtitle"), 34, Amber, TextAlignmentOptions.Center, new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.87f), true);
            CreateText(page, "Title", _localizer.Get("title"), 72, Paper, TextAlignmentOptions.Center, new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.80f), true);
            CreateText(page, "WelcomeNote", _localizer.Locale == "ko" ? "밤새 들어오는 기묘한 물건을 규칙대로 정리하세요." : "File strange arrivals by lamplight until morning.", 27, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.54f), new Vector2(0.85f, 0.64f));
            CreateButton(page, "StartShiftButton", _localizer.Get("start"), new Vector2(0.15f, 0.40f), new Vector2(0.85f, 0.49f), Amber, Ink, OnStartPressed);
            CreateButton(page, "DailyShiftButton", _localizer.Get("daily"), new Vector2(0.15f, 0.30f), new Vector2(0.85f, 0.38f), Paper, Ink, () => StartNewShift(_seedProvider.CreateDailySeed(ContentCatalog.ContentVersion)));
            CreateButton(page, "CollectionButton", _localizer.Get("collection"), new Vector2(0.15f, 0.20f), new Vector2(0.49f, 0.28f), Wine, Paper, ShowCollection);
            CreateButton(page, "SettingsButton", _localizer.Get("settings"), new Vector2(0.51f, 0.20f), new Vector2(0.85f, 0.28f), Wine, Paper, ShowSettings);
            CreateText(page, "Progress", $"{_localizer.Get("coins")}: {_save.coins}   •   {_save.completedShifts}/∞", 23, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.10f), new Vector2(0.85f, 0.17f));

            var equipped = ContentCatalog.CreateCosmetics().FirstOrDefault(item => item.Id == _save.equippedCosmeticId);
            if (equipped != null)
            {
                var charm = CreatePanel(page, "EquippedDeskCharm", Hex(equipped.AccentHex), new Vector2(0.74f, 0.88f), new Vector2(0.94f, 0.96f));
                CreateText(charm, "EquippedDeskCharmLabel", "✦  " + CosmeticName(equipped), 18, Ink, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, true);
            }
        }

        public void ShowTutorial()
        {
            ActiveScreen = AppScreen.Tutorial;
            var page = CreatePage("TutorialScreen");
            CreateText(page, "TutorialTitle", _localizer.Get("tutorial_title"), 48, Amber, TextAlignmentOptions.Center, new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.84f), true);
            CreateText(page, "TutorialBody", _localizer.Get("tutorial_body"), 30, Paper, TextAlignmentOptions.Top, new Vector2(0.12f, 0.39f), new Vector2(0.88f, 0.69f));
            CreateText(page, "TutorialIcons", "[ 1 ]  REPAIR     [ 2 ]  STORAGE     [ 3 ]  VAULT\n\n                        [ HOLD ]", 25, Amber, TextAlignmentOptions.Center, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.39f), true);
            CreateButton(page, "BeginTutorialShiftButton", _localizer.Get("begin"), new Vector2(0.18f, 0.11f), new Vector2(0.82f, 0.20f), Amber, Ink, () =>
            {
                _save.tutorialCompleted = true;
                Save();
                StartNewShift(1107);
            });
        }

        public void StartNewShift(int seed)
        {
            var band = Mathf.Clamp(1 + _save.completedShifts / 5, 1, 5);
            var artifacts = _artifactContent.Select(item => item.ToArtifact()).ToArray();
            _plannedQueue = _shiftGenerator.GenerateArtifactQueue(seed, artifacts, 12);
            _activeRules = ContentCatalog.CreateRulesForBand(band, seed);
            _session = new ShiftSession(_plannedQueue, _activeRules);
            _seenThisShift.Clear();
            _sortedCount = 0;
            _resultApplied = false;
            _appliedResultCoins = 0;
            _analytics.Track("shift_started", new Dictionary<string, string> { ["band"] = band.ToString() });
            BuildShiftScreen();
        }

        public void ChooseDestination(Destination destination)
        {
            if (_session == null || _session.State != ShiftState.Active)
            {
                return;
            }

            var artifactId = _session.CurrentArtifact.Id;
            var outcome = _session.Sort(destination);
            _sortedCount++;
            if (outcome.WasCorrect)
            {
                _seenThisShift.Add(artifactId);
                _statusText.text = _localizer.Get("correct");
                _statusText.color = Sage;
            }
            else
            {
                _statusText.text = _localizer.Get("wrong", DestinationName(outcome.ExpectedDestination));
                _statusText.color = DustyRose;
            }

            if (_session.State == ShiftState.Active)
            {
                RefreshShiftView();
            }
            else
            {
                ShowResults();
            }
        }

        public void HoldCurrent()
        {
            if (_session != null && _session.Hold())
            {
                RefreshShiftView();
            }
        }

        public void ShowCollection()
        {
            ActiveScreen = AppScreen.Collection;
            var page = CreatePage("CollectionScreen");
            CreateText(page, "CollectionTitle", _localizer.Get("collection"), 50, Amber, TextAlignmentOptions.Center, new Vector2(0.08f, 0.89f), new Vector2(0.92f, 0.97f), true);
            var scrollContent = CreateScrollContent(page, "CasebookScroll", new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.87f));
            if (_save.discoveredArtifactIds.Count == 0)
            {
                CreateLayoutText(scrollContent, _localizer.Get("casebook_empty"), 27, Paper, 110);
            }

            foreach (var artifact in _artifactContent)
            {
                var known = _save.discoveredArtifactIds.Contains(artifact.Id);
                var name = known ? Name(artifact) : "?????";
                var description = known ? Description(artifact) : "···";
                CreateLayoutText(scrollContent, $"{artifact.Symbol}   {name}\n<size=21>{description}</size>", 28, known ? Paper : DustyRose, 112);
            }

            CreateText(page, "CosmeticHeader", _localizer.Get("cosmetics"), 30, Amber, TextAlignmentOptions.Center, new Vector2(0.08f, 0.23f), new Vector2(0.92f, 0.29f), true);
            var cosmetics = ContentCatalog.CreateCosmetics();
            for (var index = 0; index < cosmetics.Count; index++)
            {
                var item = cosmetics[index];
                var minX = 0.08f + (index % 3) * 0.29f;
                var maxX = minX + 0.27f;
                var minY = index < 3 ? 0.15f : 0.08f;
                var maxY = minY + 0.06f;
                var owned = _save.unlockedCosmeticIds.Contains(item.Id);
                var equipped = owned && _save.equippedCosmeticId == item.Id;
                var status = equipped ? _localizer.Get("equipped") : owned ? _localizer.Get("equip") : _localizer.Get("unlock", item.Cost);
                var label = CosmeticName(item) + "\n<size=18>" + status + "</size>";
                var color = equipped ? Amber : owned ? Sage : Wine;
                CreateButton(page, "Cosmetic_" + item.Id, label, new Vector2(minX, minY), new Vector2(maxX, maxY), color, equipped ? Ink : Paper, () => SelectCosmetic(item));
            }

            CreateButton(page, "CollectionBackButton", _localizer.Get("back"), new Vector2(0.34f, 0.015f), new Vector2(0.66f, 0.065f), Paper, Ink, ShowMenu);
        }

        public void ShowSettings()
        {
            ActiveScreen = AppScreen.Settings;
            var page = CreatePage("SettingsScreen");
            CreateText(page, "SettingsTitle", _localizer.Get("settings"), 54, Amber, TextAlignmentOptions.Center, new Vector2(0.10f, 0.84f), new Vector2(0.90f, 0.94f), true);
            CreateText(page, "LanguageHeader", _localizer.Get("language"), 28, Paper, TextAlignmentOptions.Left, new Vector2(0.14f, 0.72f), new Vector2(0.86f, 0.78f), true);
            CreateButton(page, "EnglishButton", "English", new Vector2(0.14f, 0.62f), new Vector2(0.48f, 0.70f), _localizer.Locale == "en" ? Amber : Wine, _localizer.Locale == "en" ? Ink : Paper, () => SetLocale("en"));
            CreateButton(page, "KoreanButton", "한국어", new Vector2(0.52f, 0.62f), new Vector2(0.86f, 0.70f), _localizer.Locale == "ko" ? Amber : Wine, _localizer.Locale == "ko" ? Ink : Paper, () => SetLocale("ko"));
            CreateText(page, "PrivacyHeader", _localizer.Get("privacy"), 28, Paper, TextAlignmentOptions.Left, new Vector2(0.14f, 0.49f), new Vector2(0.86f, 0.55f), true);
            CreateButton(page, "AnalyticsConsentButton", _localizer.Get(_save.analyticsConsent ? "analytics_on" : "analytics_off"), new Vector2(0.14f, 0.39f), new Vector2(0.86f, 0.47f), _save.analyticsConsent ? Sage : Wine, Paper, ToggleAnalytics);
            CreateButton(page, "CrashConsentButton", _localizer.Get(_save.crashReportingConsent ? "crash_on" : "crash_off"), new Vector2(0.14f, 0.29f), new Vector2(0.86f, 0.37f), _save.crashReportingConsent ? Sage : Wine, Paper, ToggleCrashReports);
            if (_privacy.PrivacyOptionsRequired)
            {
                CreateButton(page, "AdPrivacyOptionsButton", _localizer.Get("privacy_options"), new Vector2(0.14f, 0.19f), new Vector2(0.86f, 0.27f), Wine, Paper, ShowAdPrivacyOptions);
            }

            CreateText(page, "PrivacyNote", _localizer.Locale == "ko" ? "동의하지 않아도 모든 게임 기능을 이용할 수 있습니다." : "All gameplay remains available without consent.", 22, Paper, TextAlignmentOptions.Top, new Vector2(0.14f, 0.11f), new Vector2(0.86f, 0.18f));
            CreateButton(page, "SettingsBackButton", _localizer.Get("back"), new Vector2(0.30f, 0.03f), new Vector2(0.70f, 0.09f), Paper, Ink, ShowMenu);
        }

        private void OnStartPressed()
        {
            if (_save.tutorialCompleted)
            {
                StartNewShift(_seedProvider.CreateStandardSeed(_save.completedShifts));
            }
            else
            {
                ShowTutorial();
            }
        }

        private void BuildShiftScreen()
        {
            ActiveScreen = AppScreen.Shift;
            var page = CreatePage("ShiftScreen");
            _hudText = CreateText(page, "ShiftHud", string.Empty, 26, Paper, TextAlignmentOptions.Center, new Vector2(0.07f, 0.93f), new Vector2(0.93f, 0.98f), true);
            CreateText(page, "RulesHeader", _localizer.Get("rules"), 23, Amber, TextAlignmentOptions.Left, new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.90f), true);
            CreateText(page, "RuleList", RulesText(), 22, Paper, TextAlignmentOptions.TopLeft, new Vector2(0.07f, 0.68f), new Vector2(0.93f, 0.85f));

            _nextTexts[0] = CreateText(page, "NextPreview0", string.Empty, 21, Paper, TextAlignmentOptions.Center, new Vector2(0.08f, 0.59f), new Vector2(0.42f, 0.66f), true);
            _nextTexts[1] = CreateText(page, "NextPreview1", string.Empty, 21, Paper, TextAlignmentOptions.Center, new Vector2(0.44f, 0.59f), new Vector2(0.78f, 0.66f), true);
            _heldText = CreateText(page, "HeldArtifactText", string.Empty, 20, Amber, TextAlignmentOptions.Center, new Vector2(0.80f, 0.59f), new Vector2(0.94f, 0.66f), true);

            var card = CreatePanel(page, "CurrentArtifactCard", Paper, new Vector2(0.10f, 0.27f), new Vector2(0.90f, 0.58f));
            _currentSymbol = CreateText(card, "ArtifactSymbol", string.Empty, 84, Wine, TextAlignmentOptions.Center, new Vector2(0.05f, 0.62f), new Vector2(0.28f, 0.94f), true);
            _currentName = CreateText(card, "ArtifactName", string.Empty, 37, Ink, TextAlignmentOptions.Left, new Vector2(0.30f, 0.70f), new Vector2(0.94f, 0.93f), true);
            _currentDescription = CreateText(card, "ArtifactDescription", string.Empty, 24, Ink, TextAlignmentOptions.TopLeft, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.68f));
            _currentTraits = CreateText(card, "ArtifactTraits", string.Empty, 20, Wine, TextAlignmentOptions.Center, new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.24f), true);

            CreateButton(page, "HoldButton", _localizer.Get("hold"), new Vector2(0.36f, 0.20f), new Vector2(0.64f, 0.26f), Wine, Paper, HoldCurrent);
            var repair = CreateButton(page, "RepairButton", _localizer.Get("repair"), new Vector2(0.05f, 0.08f), new Vector2(0.32f, 0.18f), DustyRose, Paper, () => ChooseDestination(Destination.Repair));
            var storage = CreateButton(page, "StorageButton", _localizer.Get("storage"), new Vector2(0.365f, 0.08f), new Vector2(0.635f, 0.18f), Sage, Paper, () => ChooseDestination(Destination.Storage));
            var vault = CreateButton(page, "VaultButton", _localizer.Get("vault"), new Vector2(0.68f, 0.08f), new Vector2(0.95f, 0.18f), Amber, Ink, () => ChooseDestination(Destination.Vault));
            card.gameObject.AddComponent<ArtifactDragHandler>().Configure(
                new[]
                {
                    repair.GetComponent<RectTransform>(),
                    storage.GetComponent<RectTransform>(),
                    vault.GetComponent<RectTransform>()
                },
                index => ChooseDestination((Destination)index));
            _statusText = CreateText(page, "SortFeedback", string.Empty, 22, Paper, TextAlignmentOptions.Center, new Vector2(0.07f, 0.01f), new Vector2(0.93f, 0.07f), true);
            RefreshShiftView();
        }

        private void RefreshShiftView()
        {
            if (_session?.CurrentArtifact == null)
            {
                return;
            }

            var content = _artifactById[_session.CurrentArtifact.Id];
            _currentSymbol.text = content.Symbol;
            _currentName.text = Name(content);
            _currentDescription.text = Description(content);
            _currentTraits.text = TraitsText(content.Traits);
            _heldText.text = _session.HeldArtifact == null ? "—" : _artifactById[_session.HeldArtifact.Id].Symbol + "\n" + _localizer.Get("hold");
            for (var index = 0; index < _nextTexts.Length; index++)
            {
                var queueIndex = _sortedCount + index + 1;
                _nextTexts[index].text = queueIndex < _plannedQueue.Count ? "NEXT  " + _artifactById[_plannedQueue[queueIndex].Id].Symbol : "NEXT  —";
            }

            _hudText.text = $"♥ {_session.Hearts}     COMBO {_session.Combo}     {_localizer.Get("coins")} {_session.Coins}";
        }

        private void ShowResults()
        {
            ApplyResultOnce();
            ActiveScreen = AppScreen.Results;
            var page = CreatePage("ResultsScreen");
            var completed = _session.State == ShiftState.Completed;
            CreateText(page, "ResultTitle", _localizer.Get(completed ? "complete" : "failed"), 58, completed ? Amber : DustyRose, TextAlignmentOptions.Center, new Vector2(0.08f, 0.69f), new Vector2(0.92f, 0.82f), true);
            CreateText(page, "ResultScore", $"{_localizer.Get("score")}  {_session.Score}\n{_localizer.Get("coins")}  {_session.Coins}\n✓ {_session.CorrectSorts}   ✕ {_session.Mistakes}", 33, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.45f), new Vector2(0.85f, 0.66f), true);
            var rewardLabel = completed ? _localizer.Get("double") : _localizer.Get("revive");
            if (!CanShowRewarded || _session.RewardClaimed)
            {
                rewardLabel = _localizer.Get("ad_unavailable");
            }

            var reward = CreateButton(page, "RewardedAdButton", rewardLabel, new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.40f), Wine, Paper, () => RequestReward(completed));
            reward.interactable = CanShowRewarded && !_session.RewardClaimed;
            CreateButton(page, "ResultsContinueButton", _localizer.Get("continue"), new Vector2(0.20f, 0.15f), new Vector2(0.80f, 0.25f), Amber, Ink, ReturnFromResults);
        }

        private void RequestReward(bool completed)
        {
            if (!CanShowRewarded || _session == null || _session.RewardClaimed)
            {
                return;
            }

            var placement = completed ? "shift_complete_double" : "shift_failed_revive";
            _adService.ShowRewarded(placement, result =>
            {
                if (result != RewardedAdResult.Earned)
                {
                    return;
                }

                if (completed && _session.TryDoubleCoins())
                {
                    PersistAdditionalRewardCoins();
                    ShowResults();
                }
                else if (!completed && _session.TryRevive())
                {
                    BuildShiftScreen();
                }
            });
        }

        private void ReturnFromResults()
        {
            ApplyResultOnce();
            ShowMenu();
        }

        private void ApplyResultOnce()
        {
            if (_resultApplied || _session == null || _session.State != ShiftState.Completed)
            {
                return;
            }

            _progression.ApplyShift(_save, _session.CreateResult(), _seenThisShift);
            _resultApplied = true;
            _appliedResultCoins = _session.Coins;
            _analytics.Track("shift_completed", new Dictionary<string, string>
            {
                ["score"] = _session.Score.ToString(),
                ["mistakes"] = _session.Mistakes.ToString(),
                ["rewarded"] = _session.RewardClaimed.ToString()
            });
            Save();
        }

        private void SelectCosmetic(CosmeticContent cosmetic)
        {
            var changed = _save.unlockedCosmeticIds.Contains(cosmetic.Id)
                ? _progression.TryEquipCosmetic(_save, cosmetic.Id)
                : _progression.TryUnlockCosmetic(_save, cosmetic.Id, cosmetic.Cost);
            if (changed)
            {
                Save();
                ShowCollection();
            }
        }

        private void RequestAdConsent()
        {
            _adConsentResolved = false;
            _canRequestAds = false;
            _privacy.RequestConsent(canRequestAds =>
            {
                _adConsentResolved = true;
                _canRequestAds = canRequestAds && _privacy.CanRequestAds;
                _adService?.SetRequestPermission(_canRequestAds);
                if (_screenRoot != null && ActiveScreen == AppScreen.Results)
                {
                    ShowResults();
                }
            });
        }

        private void ShowAdPrivacyOptions()
        {
            _privacy.ShowPrivacyOptions(canRequestAds =>
            {
                _adConsentResolved = true;
                _canRequestAds = canRequestAds && _privacy.CanRequestAds;
                _adService?.SetRequestPermission(_canRequestAds);
                ShowSettings();
            });
        }

        private void PersistAdditionalRewardCoins()
        {
            if (!_resultApplied || _session.State != ShiftState.Completed)
            {
                return;
            }

            var additionalCoins = Math.Max(0, _session.Coins - _appliedResultCoins);
            if (additionalCoins == 0)
            {
                return;
            }

            _save.coins += additionalCoins;
            _appliedResultCoins = _session.Coins;
            Save();
        }

        private bool CanShowRewarded =>
            _adConsentResolved &&
            _canRequestAds &&
            _privacy != null &&
            _privacy.CanRequestAds &&
            _adService != null &&
            _adService.IsRewardedReady;

        private void ToggleAnalytics()
        {
            _save.analyticsConsent = !_save.analyticsConsent;
            _analytics.SetConsent(_save.analyticsConsent);
            Save();
            ShowSettings();
        }

        private void ToggleCrashReports()
        {
            _save.crashReportingConsent = !_save.crashReportingConsent;
            _crashReporter.SetConsent(_save.crashReportingConsent);
            Save();
            ShowSettings();
        }

        private void SetLocale(string locale)
        {
            _save.locale = locale == "ko" ? "ko" : "en";
            _localizer.SetLocale(_save.locale);
            Save();
            ShowSettings();
        }

        private void Save()
        {
            if (_saveStore != null && _save != null)
            {
                _saveStore.Save(_save);
            }
        }

        private void BuildShell()
        {
            var canvasObject = new GameObject("CurioClerkCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var background = CreatePanel(canvasObject.transform, "OccultDeskBackground", Plum, Vector2.zero, Vector2.one);
            var safeArea = CreatePanel(background, "SafeArea", Color.clear, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            _screenRoot = CreatePanel(safeArea, "ScreenRoot", Color.clear, Vector2.zero, Vector2.one);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystemObject.transform.SetParent(transform, false);
                eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private RectTransform CreatePage(string name)
        {
            ClearScreen();
            return CreatePanel(_screenRoot, name, Color.clear, Vector2.zero, Vector2.one);
        }

        private void ClearScreen()
        {
            for (var index = _screenRoot.childCount - 1; index >= 0; index--)
            {
                var child = _screenRoot.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private RectTransform CreateScrollContent(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var root = CreatePanel(parent, name, Wine, min, max);
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            var viewport = CreatePanel(root, "Viewport", Color.clear, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = CreatePanel(viewport, "Content", Color.clear, new Vector2(0, 1), new Vector2(1, 1));
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            return content;
        }

        private TMP_Text CreateLayoutText(Transform parent, string value, float size, Color color, float height)
        {
            var text = CreateText(parent, "Entry", value, size, color, TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one);
            var element = text.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            return text;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, min, max);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.01f;
            return rect;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, bool bold = false)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, min, max);
            var text = gameObject.GetComponent<TextMeshProUGUI>();
            if (s_InterfaceFont == null)
            {
                s_InterfaceFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Dynamic");
            }

            if (s_InterfaceFont != null)
            {
                text.font = s_InterfaceFont;
            }

            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color background, Color foreground, UnityEngine.Events.UnityAction action)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, min, max);
            var image = gameObject.GetComponent<Image>();
            image.color = background;
            var button = gameObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            CreateText(rect, "Label", label, 26, foreground, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, true);
            return button;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private string RulesText()
        {
            var lines = new List<string>(_activeRules.Count);
            for (var index = 0; index < _activeRules.Count; index++)
            {
                var rule = _activeRules[index];
                if (rule.IsFallback)
                {
                    lines.Add($"{index + 1}. {_localizer.Get("fallback")}");
                    continue;
                }

                var conditions = rule.RequiredAll != ArtifactTraits.None ? TraitsText(rule.RequiredAll) : TraitsText(rule.RequiredAny);
                var joiner = rule.RequiredAny != ArtifactTraits.None && rule.RequiredAll == ArtifactTraits.None ? " / " : " + ";
                if (joiner == " / ")
                {
                    conditions = conditions.Replace(" · ", joiner);
                }

                lines.Add($"{index + 1}. {conditions}  →  {DestinationName(rule.Destination)}");
            }

            return string.Join("\n", lines);
        }

        private string TraitsText(ArtifactTraits traits)
        {
            var labels = new List<string>(3);
            AddTrait(labels, traits, ArtifactTraits.Cursed, "trait_cursed");
            AddTrait(labels, traits, ArtifactTraits.Fragile, "trait_fragile");
            AddTrait(labels, traits, ArtifactTraits.Alive, "trait_alive");
            AddTrait(labels, traits, ArtifactTraits.Temporal, "trait_temporal");
            AddTrait(labels, traits, ArtifactTraits.Wet, "trait_wet");
            AddTrait(labels, traits, ArtifactTraits.Metallic, "trait_metallic");
            return string.Join(" · ", labels);
        }

        private void AddTrait(ICollection<string> labels, ArtifactTraits value, ArtifactTraits trait, string key)
        {
            if ((value & trait) != 0)
            {
                labels.Add(_localizer.Get(key));
            }
        }

        private string DestinationName(Destination destination)
        {
            switch (destination)
            {
                case Destination.Repair: return _localizer.Get("repair");
                case Destination.Vault: return _localizer.Get("vault");
                default: return _localizer.Get("storage");
            }
        }

        private string Name(ArtifactContent content) => _localizer.Locale == "ko" ? content.NameKorean : content.NameEnglish;

        private string Description(ArtifactContent content) => _localizer.Locale == "ko" ? content.DescriptionKorean : content.DescriptionEnglish;

        private string CosmeticName(CosmeticContent content) => _localizer.Locale == "ko" ? content.NameKorean : content.NameEnglish;

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }
    }
}

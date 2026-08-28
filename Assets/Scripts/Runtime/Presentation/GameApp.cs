using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Progression;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Feedback;
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
        private enum TutorialStage
        {
            None,
            FirstVault,
            HoldDuplicateVault,
            FirstRepair,
            FirstStorage,
            SecondStorage,
            SecondRepair,
            FinalHeldVault,
            Complete
        }

        private enum CollectionTab
        {
            Casebook,
            Cosmetics
        }

        private enum TextRole
        {
            Interface,
            Display
        }

        private static TMP_FontAsset s_InterfaceFont;
        private static TMP_FontAsset s_DisplayFont;
        private static readonly Color Plum = Hex("#351B2B");
        private static readonly Color Wine = Hex("#5B2944");
        private static readonly Color Paper = Hex("#F2E5C4");
        private static readonly Color Ink = Hex("#2B2025");
        private static readonly Color Amber = Hex("#E0A24B");
        private static readonly Color Sage = Hex("#6F8A6B");
        private static readonly Color DustyRose = Hex("#B56D78");

        private readonly ShiftPlanGenerator _shiftPlanGenerator = new ShiftPlanGenerator();
        private readonly RuleEngine _ruleEngine = new RuleEngine();
        private readonly ProgressionService _progression = new ProgressionService();
        private readonly HashSet<string> _seenThisShift = new HashSet<string>(StringComparer.Ordinal);
        private IReadOnlyList<ArtifactContent> _artifactContent;
        private Dictionary<string, ArtifactContent> _artifactById;
        private IReadOnlyList<SortingRule> _activeRules;
        private IReadOnlyList<Artifact> _plannedQueue;
        private ShiftPlan _activePlan;
        private ShiftSession _session;
        private PlayerSaveData _save;
        private ISaveStore _saveStore;
        private IAdService _adService;
        private IPrivacyService _privacy;
        private IPlayerFeedbackService _feedbackService;
        private IClock _clock;
        private IShiftSeedProvider _seedProvider;
        private Localizer _localizer;
        private RectTransform _screenRoot;
        private TMP_Text _currentSymbol;
        private TMP_Text _currentName;
        private TMP_Text _currentDescription;
        private TMP_Text _currentTraits;
        private Image _artifactIllustration;
        private TMP_Text _heldText;
        private readonly TMP_Text[] _nextTexts = new TMP_Text[2];
        private Image _heldIllustration;
        private readonly Image[] _nextIllustrations = new Image[2];
        private TMP_Text _ruleListText;
        private TMP_Text _tutorialCoach;
        private Button _holdButton;
        private TMP_Text _holdButtonLabel;
        private readonly Button[] _destinationButtons = new Button[3];
        private readonly Outline[] _destinationHighlights = new Outline[3];
        private Outline _holdHighlight;
        private Image _sortFeedbackPanel;
        private TMP_Text _statusText;
        private TMP_Text _hudText;
        private DocketProgressView _docketProgress;
        private ShiftFeedbackAnimator _feedbackAnimator;
        private ArtifactDragHandler _artifactDragHandler;
        private GameObject _tutorialDocketCompleteCard;
        private bool _resultApplied;
        private int _appliedResultCoins;
        private string _lastCorrectArtifactId;
        private bool _adConsentResolved;
        private bool _canRequestAds;
        private string _rewardFeedbackKey;
        private TutorialStage _tutorialStage;
        private CollectionTab _collectionTab;
        private string _cosmeticFeedback;
        private bool _isDailyShift;
        private string _dailyDateKey = string.Empty;
        private bool _inputLocked;

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
            _privacy = Infrastructure.ServiceFactory.CreatePrivacyService();
            _feedbackService = Infrastructure.ServiceFactory.CreatePlayerFeedbackService(gameObject);
            ConfigureFeedback();
            _clock = new SystemClock();
            _seedProvider = new ShiftSeedProvider(_clock);
            EnsureDisplayCamera();
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

        private void OnDestroy()
        {
            Save();
            _feedbackService?.Dispose();
        }

        public void ShowMenu()
        {
            ActiveScreen = AppScreen.Menu;
            var page = CreatePage("MainMenuScreen");
            CreateText(page, "Eyebrow", _localizer.Get("subtitle"), 34, Amber, TextAlignmentOptions.Center, new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.87f), true);
            CreateText(page, "Title", _localizer.Get("title"), 72, Paper, TextAlignmentOptions.Center, new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.80f), true, TextRole.Display);
            CreateText(page, "WelcomeNote", _localizer.Locale == "ko" ? "밤새 들어오는 기묘한 물건을 규칙대로 정리하세요." : "File strange arrivals by lamplight until morning.", 27, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.54f), new Vector2(0.85f, 0.64f));
            CreateButton(page, "StartShiftButton", _localizer.Get("start"), new Vector2(0.15f, 0.40f), new Vector2(0.85f, 0.49f), Amber, Ink, OnStartPressed);
            CreateButton(page, "DailyShiftButton", DailyButtonText(), new Vector2(0.15f, 0.30f), new Vector2(0.85f, 0.38f), Paper, Ink, StartDailyShift);
            CreateButton(page, "CollectionButton", _localizer.Get("collection"), new Vector2(0.15f, 0.20f), new Vector2(0.49f, 0.28f), Wine, Paper, ShowCollection);
            CreateButton(page, "SettingsButton", _localizer.Get("settings"), new Vector2(0.51f, 0.20f), new Vector2(0.85f, 0.28f), Wine, Paper, ShowSettings);
            CreateText(page, "Progress", $"{_localizer.Get("coins")}: {_save.coins}   •   {_save.completedShifts}/∞", 23, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.10f), new Vector2(0.85f, 0.17f));

            var equipped = ContentCatalog.CreateCosmetics().FirstOrDefault(item => item.Id == _save.equippedCosmeticId);
            if (equipped != null)
            {
                CreateEquippedCosmeticArtwork(page, equipped, new Vector2(0.72f, 0.86f), new Vector2(0.95f, 0.98f), true);
            }
        }

        public void ShowTutorial()
        {
            _tutorialStage = TutorialStage.None;
            ActiveScreen = AppScreen.Tutorial;
            var page = CreatePage("TutorialScreen");
            CreateText(page, "TutorialTitle", _localizer.Get("tutorial_title"), 48, Amber, TextAlignmentOptions.Center, new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.84f), true);
            CreateText(page, "TutorialBody", _localizer.Get("tutorial_body"), 27, Paper, TextAlignmentOptions.Center, new Vector2(0.07f, 0.35f), new Vector2(0.93f, 0.66f), true);
            CreateButton(page, "BeginTutorialShiftButton", _localizer.Get("begin"), new Vector2(0.18f, 0.11f), new Vector2(0.82f, 0.20f), Amber, Ink, StartTutorialShift);
        }

        public void StartNewShift(int seed)
        {
            _tutorialStage = TutorialStage.None;
            var band = Mathf.Clamp(1 + _save.completedShifts / 5, 1, 3);
            StartShift(seed, band, false, string.Empty);
        }

        public void StartDailyShift()
        {
            _tutorialStage = TutorialStage.None;
            var localNow = _clock.LocalNow;
            var dateKey = localNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var seed = DailySeedProvider.ForDate(localNow, ContentCatalog.ContentVersion);
            StartShift(seed, 3, true, dateKey);
        }

        private void StartShift(int seed, int band, bool isDailyShift, string dailyDateKey)
        {
            _isDailyShift = isDailyShift;
            _dailyDateKey = dailyDateKey ?? string.Empty;
            var supportedBand = Mathf.Clamp(band, 1, 3);
            var artifacts = _artifactContent.Select(item => item.ToArtifact()).ToArray();
            _activePlan = _shiftPlanGenerator.Generate(
                seed,
                supportedBand,
                artifacts,
                ContentCatalog.CreateRulePacks(),
                ContentCatalog.CreateShiftTemplates());
            _plannedQueue = _activePlan.Queue;
            _activeRules = _activePlan.Rules;
            _session = new ShiftSession(_plannedQueue, _activeRules);
            _seenThisShift.Clear();
            _resultApplied = false;
            _appliedResultCoins = 0;
            _lastCorrectArtifactId = null;
            _rewardFeedbackKey = null;
            BuildShiftScreen();
        }

        public void ChooseDestination(Destination destination)
        {
            if (_inputLocked || _session == null || _session.State != ShiftState.Active)
            {
                return;
            }

            if (IsTutorialActive)
            {
                ChooseTutorialDestination(destination);
                return;
            }

            var artifact = _session.CurrentArtifact;
            var artifactId = artifact.Id;
            var content = _artifactById[artifactId];
            var outcome = _session.Sort(destination);
            if (outcome.Disposition == SortDisposition.Correct)
            {
                _seenThisShift.Add(artifactId);
                _lastCorrectArtifactId = artifactId;
            }

            if (outcome.Disposition == SortDisposition.Blocked)
            {
                ShowBlockedFeedback();
                RefreshShiftView(false, false);
                return;
            }

            if (outcome.Disposition == SortDisposition.Wrong)
            {
                if (_session.State == ShiftState.Failed)
                {
                    ShowSortFeedback(artifact, content, outcome, true, false);
                    RefreshShiftView(false, false);
                    SetShiftInputLocked(true);
                    _feedbackAnimator?.SetIdleEnabled(false);
                    if (_feedbackAnimator == null)
                    {
                        CompleteTerminalWrongTransition();
                    }
                    else
                    {
                        _feedbackAnimator.PlayWrong(CompleteTerminalWrongTransition);
                    }

                    return;
                }

                ShowSortFeedback(artifact, content, outcome);
                RefreshShiftView(false, false);
                return;
            }

            var terminalCorrectSort = outcome.DidCompleteShift;
            ShowSortFeedback(artifact, content, outcome, false);
            if (!terminalCorrectSort)
            {
                _feedbackService.Play(PlayerFeedbackCue.Correct);
            }

            RefreshDocketDuringTransition(outcome);
            SetShiftInputLocked(true);
            _feedbackAnimator?.SetIdleEnabled(false);
            if (_feedbackAnimator == null)
            {
                CompleteCorrectTransition(outcome);
            }
            else
            {
                _feedbackAnimator.PlayCorrect(() => CompleteCorrectTransition(outcome));
            }
        }

        public void HoldCurrent()
        {
            if (_inputLocked)
            {
                return;
            }

            if (IsTutorialActive)
            {
                HoldTutorialArtifact();
                return;
            }

            if (_session != null && _session.Hold())
            {
                _feedbackService.Play(PlayerFeedbackCue.Hold);
                SetShiftInputLocked(true);
                _feedbackAnimator?.SetIdleEnabled(false);
                if (_feedbackAnimator == null)
                {
                    CompleteHoldTransition();
                }
                else
                {
                    _feedbackAnimator.PlayHold(CompleteHoldTransition);
                }
            }
        }

        public void ShowCollection()
        {
            _collectionTab = CollectionTab.Casebook;
            _cosmeticFeedback = null;
            BuildCollectionScreen();
        }

        private void BuildCollectionScreen()
        {
            ActiveScreen = AppScreen.Collection;
            var page = CreatePage("CollectionScreen");
            CreateText(page, "CollectionTitle", _localizer.Get("collection"), 50, Amber, TextAlignmentOptions.Center, new Vector2(0.08f, 0.89f), new Vector2(0.92f, 0.97f), true);
            CreateButton(page, "CasebookTabButton", _localizer.Get("casebook_tab"), new Vector2(0.08f, 0.79f), new Vector2(0.49f, 0.85f), _collectionTab == CollectionTab.Casebook ? Amber : Wine, _collectionTab == CollectionTab.Casebook ? Ink : Paper, ShowCasebookTab);
            CreateButton(page, "CosmeticsTabButton", _localizer.Get("cosmetics_tab"), new Vector2(0.51f, 0.79f), new Vector2(0.92f, 0.85f), _collectionTab == CollectionTab.Cosmetics ? Amber : Wine, _collectionTab == CollectionTab.Cosmetics ? Ink : Paper, ShowCosmeticsTab);

            if (_collectionTab == CollectionTab.Casebook)
            {
                BuildCasebook(page);
            }
            else
            {
                BuildCosmetics(page);
            }

            CreateButton(page, "CollectionBackButton", _localizer.Get("back"), new Vector2(0.34f, 0.015f), new Vector2(0.66f, 0.065f), Paper, Ink, ShowMenu);
        }

        private void BuildCasebook(Transform page)
        {
            CreateText(page, "CollectionProgress", _localizer.Get("casebook_discovered", _save.discoveredArtifactIds.Count, _artifactContent.Count), 24, Paper, TextAlignmentOptions.Center, new Vector2(0.08f, 0.735f), new Vector2(0.92f, 0.785f), true);
            var scrollContent = CreateScrollContent(page, "CasebookScroll", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.73f));
            if (_save.discoveredArtifactIds.Count == 0)
            {
                CreateLayoutText(scrollContent, _localizer.Get("casebook_empty"), 27, Paper, 110);
            }

            foreach (var artifact in _artifactContent)
            {
                var known = _save.discoveredArtifactIds.Contains(artifact.Id);
                var name = known ? Name(artifact) : "?????";
                var description = known ? Description(artifact) : _localizer.Get("casebook_locked");
                var card = CreatePanel(scrollContent, "CasebookCard_" + artifact.Id, known ? Paper : new Color(Wine.r, Wine.g, Wine.b, 0.94f), Vector2.zero, Vector2.one);
                var layout = card.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 220;
                AddSurfaceChrome(card, known ? Amber : DustyRose, 1.5f, 0.22f);
                var artwork = CreateArtworkImage(card, "CasebookArtwork_" + artifact.Id, new Vector2(0.025f, 0.08f), new Vector2(0.32f, 0.92f));
                artwork.sprite = VisualAssetLibrary.Artifact(artifact.Id);
                artwork.enabled = artwork.sprite != null;
                artwork.color = known ? Color.white : new Color(0.13f, 0.06f, 0.10f, 0.96f);
                CreateText(card, "CasebookName_" + artifact.Id, name, 28, known ? Ink : Paper, TextAlignmentOptions.Left, new Vector2(0.35f, 0.70f), new Vector2(0.96f, 0.91f), true, TextRole.Display);
                CreateText(card, "CasebookDescription_" + artifact.Id, description, 18, known ? Ink : DustyRose, TextAlignmentOptions.TopLeft, new Vector2(0.35f, 0.44f), new Vector2(0.96f, 0.70f));
                if (known)
                {
                    CreateText(
                        card,
                        "CasebookResolution_" + artifact.Id,
                        _localizer.Get("resolution_label") + " · " + Resolution(artifact),
                        17,
                        Wine,
                        TextAlignmentOptions.TopLeft,
                        new Vector2(0.35f, 0.18f),
                        new Vector2(0.96f, 0.44f));
                }

                CreateText(card, "CasebookTraits_" + artifact.Id, known ? TraitsText(artifact.Traits) : string.Empty, 16, known ? Wine : DustyRose, TextAlignmentOptions.BottomLeft, new Vector2(0.35f, 0.05f), new Vector2(0.96f, 0.18f), true);
            }
        }

        private void BuildCosmetics(Transform page)
        {
            CreateText(page, "CollectionCoins", _localizer.Get("collection_coins", _save.coins), 24, Paper, TextAlignmentOptions.Center, new Vector2(0.08f, 0.735f), new Vector2(0.92f, 0.785f), true);
            if (!string.IsNullOrEmpty(_cosmeticFeedback))
            {
                CreateText(page, "CosmeticFeedback", _cosmeticFeedback, 21, Amber, TextAlignmentOptions.Center, new Vector2(0.08f, 0.69f), new Vector2(0.92f, 0.735f), true);
            }

            var scrollContent = CreateScrollContent(page, "CosmeticsScroll", new Vector2(0.08f, 0.08f), new Vector2(0.92f, string.IsNullOrEmpty(_cosmeticFeedback) ? 0.73f : 0.685f));
            var cosmetics = ContentCatalog.CreateCosmetics();
            foreach (var item in cosmetics)
            {
                var owned = _save.unlockedCosmeticIds.Contains(item.Id);
                var equipped = owned && _save.equippedCosmeticId == item.Id;
                var status = equipped
                    ? _localizer.Get("cosmetic_equipped_status")
                    : owned
                        ? _localizer.Get("cosmetic_equip_status")
                        : _localizer.Get("cosmetic_unlock_status", item.Cost);
                var color = equipped ? Amber : owned ? Sage : Wine;
                var button = CreateButton(scrollContent, "Cosmetic_" + item.Id, CosmeticName(item), Vector2.zero, Vector2.one, color, equipped ? Ink : Paper, () => SelectCosmetic(item));
                var layout = button.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 230;
                var label = button.transform.Find("Label").GetComponent<TMP_Text>();
                label.name = "CosmeticName_" + item.Id;
                label.alignment = TextAlignmentOptions.TopLeft;
                label.fontSize = 28;
                SetAnchors(label.rectTransform, new Vector2(0.38f, 0.42f), new Vector2(0.95f, 0.86f));
                var artwork = CreateArtworkImage(button.transform, "CosmeticArtwork_" + item.Id, new Vector2(0.035f, 0.08f), new Vector2(0.34f, 0.92f));
                artwork.sprite = VisualAssetLibrary.Cosmetic(item.Id);
                artwork.enabled = artwork.sprite != null;
                CreateText(button.transform, "CosmeticStatus_" + item.Id, status, 20, equipped ? Ink : Paper, TextAlignmentOptions.BottomLeft, new Vector2(0.38f, 0.13f), new Vector2(0.95f, 0.44f), true);
            }
        }

        private void ShowCasebookTab()
        {
            _collectionTab = CollectionTab.Casebook;
            _cosmeticFeedback = null;
            BuildCollectionScreen();
        }

        private void ShowCosmeticsTab()
        {
            _collectionTab = CollectionTab.Cosmetics;
            BuildCollectionScreen();
        }

        public void ShowSettings()
        {
            ActiveScreen = AppScreen.Settings;
            var page = CreatePage("SettingsScreen");
            CreateText(page, "SettingsTitle", _localizer.Get("settings"), 54, Amber, TextAlignmentOptions.Center, new Vector2(0.10f, 0.84f), new Vector2(0.90f, 0.94f), true);
            CreateText(page, "LanguageHeader", _localizer.Get("language"), 28, Paper, TextAlignmentOptions.Left, new Vector2(0.14f, 0.72f), new Vector2(0.86f, 0.78f), true);
            CreateButton(page, "EnglishButton", "English", new Vector2(0.14f, 0.62f), new Vector2(0.48f, 0.70f), _localizer.Locale == "en" ? Amber : Wine, _localizer.Locale == "en" ? Ink : Paper, () => SetLocale("en"));
            CreateButton(page, "KoreanButton", "한국어", new Vector2(0.52f, 0.62f), new Vector2(0.86f, 0.70f), _localizer.Locale == "ko" ? Amber : Wine, _localizer.Locale == "ko" ? Ink : Paper, () => SetLocale("ko"));
            CreateText(page, "FeedbackHeader", _localizer.Get("feedback_settings"), 28, Paper, TextAlignmentOptions.Left, new Vector2(0.14f, 0.53f), new Vector2(0.86f, 0.59f), true);
            CreateButton(page, "SoundToggleButton", FeedbackToggleLabel("sound", _save.soundEnabled), new Vector2(0.14f, 0.43f), new Vector2(0.48f, 0.51f), _save.soundEnabled ? Sage : Wine, Paper, ToggleSound);
            CreateButton(page, "HapticsToggleButton", FeedbackToggleLabel("haptics", _save.hapticsEnabled), new Vector2(0.52f, 0.43f), new Vector2(0.86f, 0.51f), _save.hapticsEnabled ? Sage : Wine, Paper, ToggleHaptics);
            CreateText(page, "PrivacyHeader", _localizer.Get("privacy"), 28, Paper, TextAlignmentOptions.Left, new Vector2(0.14f, 0.33f), new Vector2(0.86f, 0.39f), true);
            if (_privacy.PrivacyOptionsRequired)
            {
                CreateButton(page, "AdPrivacyOptionsButton", _localizer.Get("privacy_options"), new Vector2(0.14f, 0.24f), new Vector2(0.86f, 0.31f), Wine, Paper, ShowAdPrivacyOptions);
            }

            CreateText(page, "PrivacyNote", _localizer.Locale == "ko" ? "광고 동의 없이도 모든 게임 기능을 이용할 수 있습니다." : "All gameplay remains available without ad consent.", 22, Paper, TextAlignmentOptions.Top, new Vector2(0.14f, 0.11f), new Vector2(0.86f, 0.22f));
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
            _inputLocked = false;
            ActiveScreen = AppScreen.Shift;
            var page = CreatePage("ShiftScreen");
            var equipped = ContentCatalog.CreateCosmetics().FirstOrDefault(item => item.Id == _save.equippedCosmeticId);
            var hudMinimum = _isDailyShift ? new Vector2(0.32f, 0.945f) : new Vector2(0.06f, 0.945f);
            _hudText = CreateText(page, "ShiftHud", string.Empty, 28, Paper, TextAlignmentOptions.Center, hudMinimum, new Vector2(0.86f, 0.985f), true);
            if (_isDailyShift)
            {
                CreateText(page, "DailyChallengeBadge", _localizer.Get("daily_badge", _dailyDateKey), 18, Amber, TextAlignmentOptions.Left, new Vector2(0.03f, 0.945f), new Vector2(0.31f, 0.985f), true);
            }
            var rulesPanel = CreatePanel(page, "RulesPanel", new Color(Wine.r, Wine.g, Wine.b, 0.82f), new Vector2(0.045f, 0.70f), new Vector2(0.955f, 0.84f));
            AddSurfaceChrome(rulesPanel, Amber, 2f, 0.28f);
            CreateText(rulesPanel, "RulesHeader", _localizer.Get("rules"), 26, Amber, TextAlignmentOptions.Left, new Vector2(0.035f, 0.68f), new Vector2(0.965f, 0.94f), true);
            _ruleListText = CreateText(rulesPanel, "RuleList", RulesText(), 24, Paper, TextAlignmentOptions.TopLeft, new Vector2(0.035f, 0.05f), new Vector2(0.965f, 0.70f));
            if (equipped != null)
            {
                CreateEquippedCosmeticArtwork(page, equipped, new Vector2(0.87f, 0.945f), new Vector2(0.98f, 0.99f), false);
            }

            _docketProgress = null;
            const float previewBottom = 0.625f;
            const float previewTop = 0.69f;
            BuildDocketProgress(page);

            _nextIllustrations[0] = CreateArtifactPreview(page, "NextPreviewCard0", "NextPreviewArtwork0", "NextPreview0", Paper, new Vector2(0.05f, previewBottom), new Vector2(0.34f, previewTop), out _nextTexts[0]);
            _nextIllustrations[1] = CreateArtifactPreview(page, "NextPreviewCard1", "NextPreviewArtwork1", "NextPreview1", Paper, new Vector2(0.355f, previewBottom), new Vector2(0.645f, previewTop), out _nextTexts[1]);
            _heldIllustration = CreateArtifactPreview(page, "HeldPreviewCard", "HeldPreviewArtwork", "HeldArtifactText", Amber, new Vector2(0.66f, previewBottom), new Vector2(0.95f, previewTop), out _heldText);
            _tutorialCoach = null;
            if (IsTutorialActive)
            {
                _nextTexts[0].gameObject.SetActive(false);
                _nextTexts[1].gameObject.SetActive(false);
                var coachPanel = CreatePanel(page, "TutorialCoachPanel", Wine, new Vector2(0.05f, previewBottom), new Vector2(0.65f, previewTop));
                _tutorialCoach = CreateText(coachPanel, "TutorialCoach", string.Empty, 20, Paper, TextAlignmentOptions.Center, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f), true);
            }

            var card = CreatePanel(page, "CurrentArtifactCard", Paper, new Vector2(0.08f, 0.305f), new Vector2(0.92f, 0.615f));
            AddSurfaceChrome(card, Amber, 3f, 0.34f);
            _artifactIllustration = CreateArtworkImage(card, "ArtifactIllustration", new Vector2(0.035f, 0.16f), new Vector2(0.46f, 0.94f));
            _currentSymbol = CreateText(card, "ArtifactSymbol", string.Empty, 92, Wine, TextAlignmentOptions.Center, new Vector2(0.05f, 0.30f), new Vector2(0.44f, 0.86f), true);
            _currentName = CreateText(card, "ArtifactName", string.Empty, 42, Ink, TextAlignmentOptions.Left, new Vector2(0.47f, 0.72f), new Vector2(0.95f, 0.94f), true, TextRole.Display);
            _currentDescription = CreateText(card, "ArtifactDescription", string.Empty, 25, Ink, TextAlignmentOptions.TopLeft, new Vector2(0.47f, 0.30f), new Vector2(0.94f, 0.71f));
            _currentTraits = CreateText(card, "ArtifactTraits", string.Empty, 24, Wine, TextAlignmentOptions.Center, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.20f), true);

            _holdButton = CreateButton(page, "HoldButton", _localizer.Get("hold"), new Vector2(0.34f, 0.175f), new Vector2(0.66f, 0.225f), Wine, Paper, HoldCurrent, 28);
            _holdButtonLabel = _holdButton.transform.Find("Label").GetComponent<TMP_Text>();
            AddButtonIcon(_holdButton, "HoldButtonIcon", VisualAssetLibrary.HoldIcon, Paper);
            _holdHighlight = CreateButtonHighlight(_holdButton);
            var feedbackPanel = CreatePanel(page, "SortFeedbackPanel", Color.clear, new Vector2(0.05f, 0.235f), new Vector2(0.95f, 0.295f));
            _sortFeedbackPanel = feedbackPanel.GetComponent<Image>();
            _statusText = CreateText(feedbackPanel, "SortFeedback", string.Empty, 24, Paper, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, true);
            _feedbackAnimator = page.gameObject.AddComponent<ShiftFeedbackAnimator>();
            _feedbackAnimator.Configure(
                card,
                _artifactIllustration.rectTransform,
                feedbackPanel,
                _heldIllustration.rectTransform);

            var repair = CreateButton(page, "RepairButton", _localizer.Get("repair"), new Vector2(0.05f, 0.035f), new Vector2(0.32f, 0.145f), DustyRose, Paper, () => ChooseDestination(Destination.Repair), 30);
            var storage = CreateButton(page, "StorageButton", _localizer.Get("storage"), new Vector2(0.365f, 0.035f), new Vector2(0.635f, 0.145f), Sage, Paper, () => ChooseDestination(Destination.Storage), 30);
            var vault = CreateButton(page, "VaultButton", _localizer.Get("vault"), new Vector2(0.68f, 0.035f), new Vector2(0.95f, 0.145f), Amber, Ink, () => ChooseDestination(Destination.Vault), 30);
            AddButtonIcon(repair, "RepairButtonIcon", VisualAssetLibrary.RepairIcon, Paper);
            AddButtonIcon(storage, "StorageButtonIcon", VisualAssetLibrary.StorageIcon, Paper);
            AddButtonIcon(vault, "VaultButtonIcon", VisualAssetLibrary.VaultIcon, Ink);
            _destinationButtons[(int)Destination.Repair] = repair;
            _destinationButtons[(int)Destination.Storage] = storage;
            _destinationButtons[(int)Destination.Vault] = vault;
            _destinationHighlights[(int)Destination.Repair] = CreateButtonHighlight(repair);
            _destinationHighlights[(int)Destination.Storage] = CreateButtonHighlight(storage);
            _destinationHighlights[(int)Destination.Vault] = CreateButtonHighlight(vault);
            _artifactDragHandler = card.gameObject.AddComponent<ArtifactDragHandler>();
            _artifactDragHandler.Configure(
                new[]
                {
                    repair.GetComponent<RectTransform>(),
                    storage.GetComponent<RectTransform>(),
                    vault.GetComponent<RectTransform>()
                },
                index => ChooseDestination((Destination)index));
            RefreshShiftView();
            RefreshTutorialGuidance();
        }

        private bool IsTutorialActive =>
            _tutorialStage >= TutorialStage.FirstVault &&
            _tutorialStage <= TutorialStage.FinalHeldVault;

        private void StartTutorialShift()
        {
            _isDailyShift = false;
            _dailyDateKey = string.Empty;
            var tutorialIds = new[]
            {
                "whispering-key",
                "borrowed-shadow",
                "sleeping-teacup",
                "clockwork-moth",
                "rain-jar",
                "moon-umbrella"
            };
            _plannedQueue = tutorialIds.Select(id => _artifactById[id].ToArtifact()).ToArray();
            _activeRules = ContentCatalog.CreateRulePacks()
                .Single(pack => pack.Id == "pack-cursed-fragile")
                .Rules;
            _session = new ShiftSession(_plannedQueue, _activeRules);
            _activePlan = null;
            _seenThisShift.Clear();
            _resultApplied = false;
            _appliedResultCoins = 0;
            _lastCorrectArtifactId = null;
            _rewardFeedbackKey = null;
            _tutorialStage = TutorialStage.FirstVault;
            BuildShiftScreen();
        }

        private void ChooseTutorialDestination(Destination destination)
        {
            if (_tutorialStage == TutorialStage.HoldDuplicateVault)
            {
                RefreshDecisionMessage();
                RefreshTutorialGuidance();
                return;
            }

            var artifact = _session.CurrentArtifact;
            var content = _artifactById[artifact.Id];
            var resolution = _ruleEngine.ResolveDetailed(artifact, _activeRules);
            if (destination != resolution.Destination)
            {
                var wrong = new SortOutcome(
                    SortDisposition.Wrong,
                    destination,
                    resolution.Destination,
                    resolution.RuleId,
                    false,
                    false,
                    0,
                    0);
                ShowSortFeedback(artifact, content, wrong);
                return;
            }

            var outcome = _session.Sort(destination);
            var completingTutorial = _tutorialStage == TutorialStage.FinalHeldVault;
            var nextStage = NextTutorialStage(_tutorialStage);
            ShowSortFeedback(artifact, content, outcome, false);
            if (!completingTutorial)
            {
                _feedbackService.Play(PlayerFeedbackCue.Correct);
            }

            RefreshDocketDuringTransition(outcome);
            SetShiftInputLocked(true);
            _feedbackAnimator?.SetIdleEnabled(false);
            Action completed = () => CompleteTutorialSortTransition(
                outcome,
                nextStage,
                completingTutorial);
            if (_feedbackAnimator == null)
            {
                completed();
            }
            else
            {
                _feedbackAnimator.PlayCorrect(completed);
            }
        }

        private void HoldTutorialArtifact()
        {
            if (_tutorialStage != TutorialStage.HoldDuplicateVault)
            {
                _sortFeedbackPanel.color = Wine;
                _statusText.text = _localizer.Get("tutorial_follow_step");
                _statusText.color = Paper;
                return;
            }

            if (_session.Hold())
            {
                _feedbackService.Play(PlayerFeedbackCue.Hold);
                SetShiftInputLocked(true);
                _feedbackAnimator?.SetIdleEnabled(false);
                if (_feedbackAnimator == null)
                {
                    CompleteTutorialHoldTransition();
                }
                else
                {
                    _feedbackAnimator.PlayHold(CompleteTutorialHoldTransition);
                }
            }
        }

        private static TutorialStage NextTutorialStage(TutorialStage stage)
        {
            switch (stage)
            {
                case TutorialStage.FirstVault: return TutorialStage.HoldDuplicateVault;
                case TutorialStage.FirstRepair: return TutorialStage.FirstStorage;
                case TutorialStage.FirstStorage: return TutorialStage.SecondStorage;
                case TutorialStage.SecondStorage: return TutorialStage.SecondRepair;
                case TutorialStage.SecondRepair: return TutorialStage.FinalHeldVault;
                case TutorialStage.FinalHeldVault: return TutorialStage.Complete;
                default: return stage;
            }
        }

        private void CompleteCorrectTransition(SortOutcome outcome)
        {
            if (outcome.DidCompleteDocket && _docketProgress != null)
            {
                _docketProgress.PlayComplete(() => FinishCorrectTransition(outcome));
                return;
            }

            FinishCorrectTransition(outcome);
        }

        private void FinishCorrectTransition(SortOutcome outcome)
        {
            if (outcome.DidCompleteShift)
            {
                _inputLocked = false;
                _feedbackService.Play(PlayerFeedbackCue.ShiftComplete);
                ShowResults();
                return;
            }

            RefreshShiftView();
            SetShiftInputLocked(false);
        }

        private void CompleteHoldTransition()
        {
            RefreshShiftView();
            SetShiftInputLocked(false);
        }

        private void CompleteTerminalWrongTransition()
        {
            _inputLocked = false;
            ShowResults();
        }

        private void CompleteTutorialSortTransition(
            SortOutcome outcome,
            TutorialStage nextStage,
            bool completingTutorial)
        {
            Action finish = () =>
            {
                HideTutorialDocketCompleteCard();
                if (completingTutorial)
                {
                    _inputLocked = false;
                    CompleteTutorial();
                    return;
                }

                _tutorialStage = nextStage;
                RefreshShiftView();
                SetShiftInputLocked(false);
            };

            if (outcome.DidCompleteDocket && _docketProgress != null)
            {
                if (!completingTutorial && _session.CompletedDockets == 1)
                {
                    ShowTutorialDocketCompleteCard();
                }

                _docketProgress.PlayComplete(finish);
            }
            else
            {
                finish();
            }
        }

        private void CompleteTutorialHoldTransition()
        {
            _tutorialStage = TutorialStage.FirstRepair;
            RefreshShiftView();
            SetShiftInputLocked(false);
        }

        private void RefreshDocketDuringTransition(SortOutcome outcome)
        {
            if (_docketProgress == null || _session.RequiredDockets <= 0)
            {
                return;
            }

            var docket = _session.CurrentDocket;
            var completedDockets = _session.CompletedDockets;
            if (outcome.DidCompleteDocket)
            {
                docket = new DocketState();
                docket.TryStamp(Destination.Repair);
                docket.TryStamp(Destination.Storage);
                docket.TryStamp(Destination.Vault);
                completedDockets = Math.Max(0, completedDockets - 1);
            }

            _docketProgress.Refresh(
                docket,
                completedDockets,
                _session.RequiredDockets,
                _localizer.Get("docket_empty"),
                _localizer.Get("docket_complete"));
        }

        private void SetShiftInputLocked(bool locked)
        {
            _inputLocked = locked;
            _artifactDragHandler?.SetInputEnabled(!locked);
            if (_holdButton == null)
            {
                return;
            }

            if (locked)
            {
                _holdButton.interactable = false;
                for (var index = 0; index < _destinationButtons.Length; index++)
                {
                    _destinationButtons[index].interactable = false;
                }

                return;
            }

            if (IsTutorialActive)
            {
                RefreshTutorialGuidance();
                return;
            }

            _holdButton.interactable = true;
            for (var index = 0; index < _destinationButtons.Length; index++)
            {
                _destinationButtons[index].interactable =
                    !_inputLocked && _session.CanSort((Destination)index);
            }
        }

        private void RefreshTutorialGuidance()
        {
            if (!IsTutorialActive || _tutorialCoach == null)
            {
                return;
            }

            var highlightedRule = -1;
            var highlightedDestination = -1;
            var holdEnabled = false;
            switch (_tutorialStage)
            {
                case TutorialStage.FirstVault:
                    highlightedRule = 0;
                    highlightedDestination = (int)Destination.Vault;
                    break;
                case TutorialStage.HoldDuplicateVault:
                    highlightedRule = 0;
                    holdEnabled = true;
                    break;
                case TutorialStage.FirstRepair:
                    highlightedRule = 1;
                    highlightedDestination = (int)Destination.Repair;
                    break;
                case TutorialStage.FirstStorage:
                    highlightedRule = 2;
                    highlightedDestination = (int)Destination.Storage;
                    break;
                case TutorialStage.SecondStorage:
                    highlightedRule = 2;
                    highlightedDestination = (int)Destination.Storage;
                    break;
                case TutorialStage.SecondRepair:
                    highlightedRule = 1;
                    highlightedDestination = (int)Destination.Repair;
                    break;
                case TutorialStage.FinalHeldVault:
                    highlightedRule = 0;
                    highlightedDestination = (int)Destination.Vault;
                    break;
            }

            _tutorialCoach.text = holdEnabled
                ? _localizer.Get(
                    "hold_required",
                    DestinationName(_session.CurrentResolution.Destination))
                : _localizer.Get("tutorial_goal");
            _ruleListText.text = RulesText(highlightedRule);
            _holdButton.interactable = !_inputLocked && holdEnabled;
            _holdHighlight.enabled = holdEnabled;
            for (var index = 0; index < _destinationButtons.Length; index++)
            {
                _destinationButtons[index].interactable =
                    !_inputLocked && !holdEnabled && _session.CanSort((Destination)index);
                _destinationHighlights[index].enabled = index == highlightedDestination;
            }
        }

        private void ShowTutorialDocketCompleteCard()
        {
            HideTutorialDocketCompleteCard();
            var card = CreatePanel(
                _screenRoot,
                "TutorialDocketCompleteCard",
                Wine,
                new Vector2(0.12f, 0.40f),
                new Vector2(0.88f, 0.56f));
            AddSurfaceChrome(card, Amber, 3f, 0.38f);
            CreateText(
                card,
                "TutorialDocketCompleteMessage",
                _localizer.Get("tutorial_first_docket_complete"),
                28,
                Paper,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.10f),
                new Vector2(0.95f, 0.90f),
                true);
            _tutorialDocketCompleteCard = card.gameObject;
        }

        private void HideTutorialDocketCompleteCard()
        {
            if (_tutorialDocketCompleteCard == null)
            {
                return;
            }

            _tutorialDocketCompleteCard.SetActive(false);
            Destroy(_tutorialDocketCompleteCard);
            _tutorialDocketCompleteCard = null;
        }

        private void CompleteTutorial()
        {
            _tutorialStage = TutorialStage.Complete;
            _save.tutorialCompleted = true;
            Save();
            _feedbackService.Play(PlayerFeedbackCue.ShiftComplete);
            ActiveScreen = AppScreen.Tutorial;
            var page = CreatePage("TutorialCompleteScreen");
            CreateText(page, "TutorialCompleteTitle", _localizer.Get("tutorial_complete_title"), 58, Amber, TextAlignmentOptions.Center, new Vector2(0.10f, 0.62f), new Vector2(0.90f, 0.76f), true, TextRole.Display);
            CreateText(page, "TutorialCompleteBody", _localizer.Get("tutorial_finish"), 30, Paper, TextAlignmentOptions.Center, new Vector2(0.14f, 0.39f), new Vector2(0.86f, 0.58f));
            CreateButton(page, "TutorialStartShiftButton", _localizer.Get("tutorial_start_shift"), new Vector2(0.18f, 0.20f), new Vector2(0.82f, 0.30f), Amber, Ink, () => StartNewShift(_seedProvider.CreateStandardSeed(_save.completedShifts)));
        }

        private void ShowSortFeedback(
            Artifact artifact,
            ArtifactContent content,
            SortOutcome outcome,
            bool playCue = true,
            bool playAnimation = true)
        {
            var wasCorrect = outcome.WasCorrect;
            var reason = RuleReason(artifact, outcome);
            _sortFeedbackPanel.color = wasCorrect
                ? DestinationColor(outcome.SelectedDestination)
                : DustyRose;
            _statusText.text = wasCorrect
                ? _localizer.Get("feedback_correct_label") + " · " + reason + "\n" + Resolution(content)
                : _localizer.Get("feedback_wrong_label") + " · " +
                  _localizer.Get("wrong", DestinationName(outcome.ExpectedDestination)) + "\n" + reason;
            _statusText.color = wasCorrect && outcome.SelectedDestination == Destination.Vault ? Ink : Paper;
            if (playCue)
            {
                _feedbackService.Play(wasCorrect ? PlayerFeedbackCue.Correct : PlayerFeedbackCue.Wrong);
            }
            if (!wasCorrect && playAnimation)
            {
                _feedbackAnimator?.PlayWrong();
            }

            var highlighted = wasCorrect ? outcome.SelectedDestination : outcome.ExpectedDestination;
            for (var index = 0; index < _destinationHighlights.Length; index++)
            {
                var outline = _destinationHighlights[index];
                outline.enabled = index == (int)highlighted;
            }
        }

        private string RuleReason(Artifact artifact, SortOutcome outcome)
        {
            var matchedIndex = -1;
            for (var index = 0; index < _activeRules.Count; index++)
            {
                if (_activeRules[index].Id == outcome.MatchedRuleId)
                {
                    matchedIndex = index;
                    break;
                }
            }

            if (matchedIndex < 0)
            {
                throw new InvalidOperationException("Sort outcome references an unknown active rule.");
            }

            var matched = _activeRules[matchedIndex];
            var destination = DestinationName(outcome.ExpectedDestination);
            if (matched.IsFallback)
            {
                return _localizer.Get("fallback_reason", destination);
            }

            var matchedTraits = matched.RequiredAll != ArtifactTraits.None
                ? matched.RequiredAll
                : matched.RequiredAny;
            var hasLowerMatch = false;
            for (var index = matchedIndex + 1; index < _activeRules.Count; index++)
            {
                if (!_activeRules[index].IsFallback && _activeRules[index].Matches(artifact))
                {
                    hasLowerMatch = true;
                    break;
                }
            }

            return _localizer.Get(
                hasLowerMatch ? "rule_priority_reason" : "rule_reason",
                TraitsText(matchedTraits),
                destination);
        }

        private static Color DestinationColor(Destination destination)
        {
            switch (destination)
            {
                case Destination.Repair: return DustyRose;
                case Destination.Vault: return Amber;
                default: return Sage;
            }
        }

        private void ShowBlockedFeedback()
        {
            RefreshDecisionMessage();
        }

        private void RefreshDecisionMessage()
        {
            var holdRequired = _session?.ShouldSuggestHold == true;
            _sortFeedbackPanel.color = holdRequired ? Wine : Color.clear;
            _statusText.text = holdRequired
                ? _localizer.Get(
                    "hold_required",
                    DestinationName(_session.CurrentResolution.Destination))
                : _localizer.Get("decision_prompt");
            _statusText.color = Paper;
            SetHoldPresentation(holdRequired);
            for (var index = 0; index < _destinationHighlights.Length; index++)
            {
                _destinationHighlights[index].enabled = false;
            }
        }

        private void SetHoldPresentation(bool required)
        {
            _holdButtonLabel.text = _localizer.Get(required ? "hold_for_next" : "hold");
            _holdHighlight.enabled = required;
        }

        private void RefreshShiftView(
            bool animateArtifact = true,
            bool refreshDecisionMessage = true)
        {
            if (_session?.CurrentArtifact == null)
            {
                return;
            }

            var content = _artifactById[_session.CurrentArtifact.Id];
            var artwork = VisualAssetLibrary.Artifact(content.Id);
            _artifactIllustration.sprite = artwork;
            _artifactIllustration.enabled = artwork != null;
            _currentSymbol.gameObject.SetActive(artwork == null);
            _currentSymbol.text = content.Symbol;
            _currentName.text = Name(content);
            _currentDescription.text = Description(content);
            _currentTraits.text = TraitsText(
                content.Traits,
                IsTutorialActive ? TutorialEmphasizedTraits() : ArtifactTraits.None);
            if (_session.HeldArtifact == null)
            {
                _heldText.text = _localizer.Get("hold") + "\n—";
                SetPreviewArtwork(_heldIllustration, null);
            }
            else
            {
                var heldContent = _artifactById[_session.HeldArtifact.Id];
                _heldText.text = _localizer.Get("hold") + "\n" + heldContent.Symbol + "  " + Name(heldContent);
                SetPreviewArtwork(_heldIllustration, heldContent.Id);
            }

            for (var index = 0; index < _nextTexts.Length; index++)
            {
                var nextArtifact = _session.PeekNextArtifact(index);
                if (nextArtifact != null)
                {
                    var nextContent = _artifactById[nextArtifact.Id];
                    _nextTexts[index].text = _localizer.Get("next") + " " + (index + 1) + "\n" + nextContent.Symbol + "  " + Name(nextContent);
                    SetPreviewArtwork(_nextIllustrations[index], nextContent.Id);
                }
                else
                {
                    _nextTexts[index].text = _localizer.Get("next") + " " + (index + 1) + "\n—";
                    SetPreviewArtwork(_nextIllustrations[index], null);
                }
            }

            for (var index = 0; index < _destinationButtons.Length; index++)
            {
                _destinationButtons[index].interactable =
                    !_inputLocked && _session.CanSort((Destination)index);
            }

            if (refreshDecisionMessage)
            {
                RefreshDecisionMessage();
            }
            if (_session.RequiredDockets > 0)
            {
                _docketProgress?.Refresh(
                    _session.CurrentDocket,
                    _session.CompletedDockets,
                    _session.RequiredDockets,
                    _localizer.Get("docket_empty"),
                    _localizer.Get("docket_complete"));
                _hudText.text = _localizer.Get(
                    "shift_hud",
                    _session.Hearts,
                    _session.CompletedDockets + 1,
                    _session.RequiredDockets,
                    _session.PristineDocketStreak,
                    _session.Coins);
            }
            else
            {
                _hudText.text = $"♥ {_session.Hearts}     {_localizer.Get("coins")} {_session.Coins}";
            }

            if (animateArtifact)
            {
                _feedbackAnimator?.PlayArtifactEntrance();
            }

            _feedbackAnimator?.SetIdleEnabled(true);
        }

        private void ShowResults()
        {
            ApplyResultOnce();
            ActiveScreen = AppScreen.Results;
            var page = CreatePage("ResultsScreen");
            var completed = _session.State == ShiftState.Completed;
            var resultTitle = CreateText(page, "ResultTitle", _localizer.Get(completed ? "complete" : "failed"), 54, completed ? Amber : DustyRose, TextAlignmentOptions.Center, new Vector2(0.08f, 0.87f), new Vector2(0.92f, 0.96f), true, TextRole.Display);
            CreateText(page, "ResultDocketsHeader", _localizer.Get("result_dockets"), 25, Amber, TextAlignmentOptions.Center, new Vector2(0.12f, 0.81f), new Vector2(0.88f, 0.86f), true);
            var resultRows = new List<CanvasGroup>(4);
            for (var docket = 0; docket < 4; docket++)
            {
                var hasCompletedDocket = docket < _session.CompletedDocketPristine.Count;
                var pristine = hasCompletedDocket && _session.CompletedDocketPristine[docket];
                var status = pristine
                    ? _localizer.Get("docket_pristine")
                    : _localizer.Get("docket_inked");
                var rowTop = 0.80f - docket * 0.045f;
                var row = CreateText(
                    page,
                    "ResultDocket" + docket,
                    (docket + 1) + " · " + status,
                    22,
                    pristine ? Sage : DustyRose,
                    TextAlignmentOptions.Center,
                    new Vector2(0.20f, rowTop - 0.04f),
                    new Vector2(0.80f, rowTop),
                    true);
                resultRows.Add(row.gameObject.AddComponent<CanvasGroup>());
            }

            var ledgerAnimator = page.gameObject.AddComponent<ResultLedgerAnimator>();
            ledgerAnimator.Configure(resultRows);
            ledgerAnimator.Play();

            var resultResolution = _lastCorrectArtifactId != null && _artifactById.TryGetValue(_lastCorrectArtifactId, out var finalContent)
                ? Resolution(finalContent)
                : _localizer.Get("result_waiting");
            CreateText(
                page,
                "ResultResolution",
                _localizer.Get("resolution_label") + "\n" + resultResolution,
                22,
                Paper,
                TextAlignmentOptions.Center,
                new Vector2(0.12f, 0.45f),
                new Vector2(0.88f, 0.61f),
                true,
                TextRole.Display);
            var resultScore = CreateText(page, "ResultScore", $"{_localizer.Get("score")}  {_session.Score}\n{_localizer.Get("coins")}  {_session.Coins}\n{_localizer.Get("result_correct_label")}  {_session.CorrectSorts}   ·   {_localizer.Get("result_mistakes_label")}  {_session.Mistakes}", 27, Paper, TextAlignmentOptions.Center, new Vector2(0.15f, 0.27f), new Vector2(0.85f, 0.43f), true);
            if (_isDailyShift && completed)
            {
                CreateText(page, "DailyResultStatus", _localizer.Get("daily_result_best", _save.dailyBestScore), 20, Amber, TextAlignmentOptions.Center, new Vector2(0.15f, 0.23f), new Vector2(0.85f, 0.27f), true);
            }
            var resultAnimator = page.gameObject.AddComponent<ShiftFeedbackAnimator>();
            resultAnimator.Configure(resultTitle.rectTransform, resultScore.rectTransform);
            resultAnimator.PlayArtifactEntrance();
            CreateText(page, "RewardedAdFeedback", string.IsNullOrEmpty(_rewardFeedbackKey) ? string.Empty : _localizer.Get(_rewardFeedbackKey), 20, DustyRose, TextAlignmentOptions.Center, new Vector2(0.12f, 0.20f), new Vector2(0.88f, 0.25f), true);
            CreateButton(page, "ResultsContinueButton", _localizer.Get("continue"), new Vector2(0.20f, 0.09f), new Vector2(0.80f, 0.17f), Amber, Ink, ReturnFromResults);
        }

        private void RequestReward(bool completed)
        {
            if (!CanShowRewarded || _session == null || _session.RewardClaimed)
            {
                return;
            }

            var placement = completed ? "shift_complete_double" : "shift_failed_revive";
            var completionHandled = false;
            _adService.ShowRewarded(placement, result =>
            {
                if (completionHandled)
                {
                    return;
                }

                completionHandled = true;
                if (result != RewardedAdResult.Earned)
                {
                    _rewardFeedbackKey = RewardFeedbackKey(result);
                    if (_screenRoot != null && ActiveScreen == AppScreen.Results)
                    {
                        ShowResults();
                    }

                    return;
                }

                _rewardFeedbackKey = null;
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
            if (_isDailyShift)
            {
                _progression.RecordDailyCompletion(_save, _dailyDateKey, _session.Score);
            }

            _resultApplied = true;
            _appliedResultCoins = _session.Coins;
            Save();
        }

        private string DailyButtonText()
        {
            var dateKey = _clock.LocalNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var status = string.Equals(_save.lastDailyCompletedDate, dateKey, StringComparison.Ordinal)
                ? _localizer.Get("daily_completed", _save.dailyBestScore)
                : _localizer.Get("daily_available");
            return $"{_localizer.Get("daily")}\n{dateKey} · {status}";
        }

        private void SelectCosmetic(CosmeticContent cosmetic)
        {
            var wasOwned = _save.unlockedCosmeticIds.Contains(cosmetic.Id);
            var changed = wasOwned
                ? _progression.TryEquipCosmetic(_save, cosmetic.Id)
                : _progression.TryUnlockCosmetic(_save, cosmetic.Id, cosmetic.Cost);
            if (changed)
            {
                Save();
                _cosmeticFeedback = _localizer.Get("cosmetic_equipped_feedback", CosmeticName(cosmetic));
            }
            else if (!wasOwned && _save.coins < cosmetic.Cost)
            {
                _cosmeticFeedback = _localizer.Get("insufficient");
            }

            _collectionTab = CollectionTab.Cosmetics;
            BuildCollectionScreen();
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
                else if (_screenRoot != null && ActiveScreen == AppScreen.Settings)
                {
                    ShowSettings();
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

        private static string RewardFeedbackKey(RewardedAdResult result)
        {
            switch (result)
            {
                case RewardedAdResult.Dismissed:
                    return "ad_dismissed";
                case RewardedAdResult.Failed:
                    return "ad_failed";
                default:
                    return "ad_unavailable";
            }
        }

        private void SetLocale(string locale)
        {
            _save.locale = locale == "ko" ? "ko" : "en";
            _localizer.SetLocale(_save.locale);
            Save();
            ShowSettings();
        }

        private string FeedbackToggleLabel(string key, bool enabled)
        {
            return _localizer.Get(key) + ": " + _localizer.Get(enabled ? "on" : "off");
        }

        private void ToggleSound()
        {
            _save.soundEnabled = !_save.soundEnabled;
            ConfigureFeedback();
            Save();
            ShowSettings();
        }

        private void ToggleHaptics()
        {
            _save.hapticsEnabled = !_save.hapticsEnabled;
            ConfigureFeedback();
            Save();
            ShowSettings();
        }

        private void ConfigureFeedback()
        {
            _feedbackService?.Configure(_save.soundEnabled, _save.hapticsEnabled);
        }

        private void Save()
        {
            if (_saveStore != null && _save != null)
            {
                _saveStore.Save(_save);
            }
        }

        private void EnsureDisplayCamera()
        {
            var displayCamera = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(camera => camera.isActiveAndEnabled);
            if (displayCamera == null)
            {
                var cameraObject = new GameObject("CurioClerkDisplayCamera", typeof(Camera));
                cameraObject.transform.SetParent(transform, false);
                displayCamera = cameraObject.GetComponent<Camera>();
                displayCamera.clearFlags = CameraClearFlags.SolidColor;
                displayCamera.backgroundColor = Plum;
                displayCamera.cullingMask = 0;
            }

            var hasActiveListener = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Any(listener => listener.isActiveAndEnabled);
            if (!hasActiveListener)
            {
                var listener = displayCamera.GetComponent<AudioListener>();
                if (listener == null)
                {
                    listener = displayCamera.gameObject.AddComponent<AudioListener>();
                }

                listener.enabled = true;
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
            var backgroundImage = background.GetComponent<Image>();
            var deskBackground = VisualAssetLibrary.DeskBackground;
            if (deskBackground != null)
            {
                backgroundImage.sprite = deskBackground;
                backgroundImage.color = Color.white;
            }

            backgroundImage.raycastTarget = false;
            var deskTint = CreatePanel(background, "DeskTint", new Color(Plum.r, Plum.g, Plum.b, 0.36f), Vector2.zero, Vector2.one);
            deskTint.GetComponent<Image>().raycastTarget = false;
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

        private static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, bool bold = false, TextRole role = TextRole.Interface)
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

            if (s_DisplayFont == null)
            {
                s_DisplayFont = Resources.Load<TMP_FontAsset>("Fonts/GowunBatang-Bold-Dynamic");
            }

            var selectedFont = role == TextRole.Display ? s_DisplayFont : s_InterfaceFont;
            if (selectedFont != null)
            {
                text.font = selectedFont;
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

        private static Image CreateArtworkImage(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, min, max);
            var image = gameObject.GetComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private void BuildDocketProgress(Transform parent)
        {
            var panel = CreatePanel(
                parent,
                "DocketProgress",
                new Color(Wine.r, Wine.g, Wine.b, 0.88f),
                new Vector2(0.05f, 0.85f),
                new Vector2(0.95f, 0.935f));
            AddSurfaceChrome(panel, Amber, 1.5f, 0.22f);
            CreateText(
                panel,
                "DocketLabel",
                _localizer.Get("docket"),
                19,
                Amber,
                TextAlignmentOptions.Center,
                new Vector2(0.02f, 0.08f),
                new Vector2(0.16f, 0.92f),
                true);
            var counter = CreateText(
                panel,
                "DocketCounter",
                string.Empty,
                22,
                Paper,
                TextAlignmentOptions.Center,
                new Vector2(0.16f, 0.08f),
                new Vector2(0.31f, 0.92f),
                true);
            var stampLabels = new TMP_Text[3];
            var stamps = new[]
            {
                CreateDocketStamp(panel, "DocketStampRepair", VisualAssetLibrary.RepairIcon,
                    DustyRose, new Vector2(0.34f, 0.14f), new Vector2(0.52f, 0.86f),
                    out stampLabels[0]),
                CreateDocketStamp(panel, "DocketStampStorage", VisualAssetLibrary.StorageIcon,
                    Sage, new Vector2(0.56f, 0.14f), new Vector2(0.74f, 0.86f),
                    out stampLabels[1]),
                CreateDocketStamp(panel, "DocketStampVault", VisualAssetLibrary.VaultIcon,
                    Amber, new Vector2(0.78f, 0.14f), new Vector2(0.96f, 0.86f),
                    out stampLabels[2])
            };

            _docketProgress = panel.gameObject.AddComponent<DocketProgressView>();
            _docketProgress.Configure(
                counter,
                stamps,
                stampLabels,
                new Color(Paper.r, Paper.g, Paper.b, 0.16f),
                new[]
                {
                    new Color(DustyRose.r, DustyRose.g, DustyRose.b, 0.72f),
                    new Color(Sage.r, Sage.g, Sage.b, 0.72f),
                    new Color(Amber.r, Amber.g, Amber.b, 0.72f)
                });
        }

        private static Image CreateDocketStamp(
            Transform parent,
            string name,
            Sprite iconSprite,
            Color iconColor,
            Vector2 min,
            Vector2 max,
            out TMP_Text statusLabel)
        {
            var stamp = CreatePanel(parent, name, Color.clear, min, max);
            AddSurfaceChrome(stamp, iconColor, 1f, 0.12f);
            var icon = CreateArtworkImage(stamp, name + "Icon", new Vector2(0.20f, 0.32f), new Vector2(0.80f, 0.94f));
            icon.sprite = iconSprite;
            icon.color = iconColor;
            statusLabel = CreateText(
                stamp,
                name + "Status",
                string.Empty,
                16,
                Paper,
                TextAlignmentOptions.Center,
                new Vector2(0.04f, 0.02f),
                new Vector2(0.96f, 0.32f),
                true);
            return stamp.GetComponent<Image>();
        }

        private static Image CreateArtifactPreview(Transform parent, string panelName, string artworkName, string labelName, Color accent, Vector2 min, Vector2 max, out TMP_Text label)
        {
            var panel = CreatePanel(parent, panelName, new Color(Wine.r, Wine.g, Wine.b, 0.88f), min, max);
            panel.GetComponent<Image>().raycastTarget = false;
            AddSurfaceChrome(panel, accent, 1f, 0.20f);
            var artwork = CreateArtworkImage(panel, artworkName, new Vector2(0.035f, 0.08f), new Vector2(0.29f, 0.92f));
            label = CreateText(panel, labelName, string.Empty, 20, accent, TextAlignmentOptions.Center, new Vector2(0.29f, 0.04f), new Vector2(0.98f, 0.96f), true);
            return artwork;
        }

        private void CreateEquippedCosmeticArtwork(Transform parent, CosmeticContent cosmetic, Vector2 min, Vector2 max, bool showLabel)
        {
            var panel = CreatePanel(parent, "EquippedDeskCharm", new Color(Wine.r, Wine.g, Wine.b, 0.78f), min, max);
            AddSurfaceChrome(panel, Hex(cosmetic.AccentHex), 1.5f, 0.20f);
            var artwork = CreateArtworkImage(panel, "EquippedDeskCharmArtwork", new Vector2(0.06f, showLabel ? 0.25f : 0.06f), new Vector2(0.94f, 0.94f));
            artwork.sprite = VisualAssetLibrary.Cosmetic(cosmetic.Id);
            artwork.enabled = artwork.sprite != null;
            if (showLabel)
            {
                CreateText(panel, "EquippedDeskCharmLabel", CosmeticName(cosmetic), 15, Paper, TextAlignmentOptions.Center, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.26f), true);
            }
        }

        private static void SetPreviewArtwork(Image preview, string artifactId)
        {
            var sprite = VisualAssetLibrary.Artifact(artifactId);
            preview.sprite = sprite;
            preview.enabled = sprite != null;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, Color background, Color foreground, UnityEngine.Events.UnityAction action, float labelSize = 26)
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
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            CreateText(rect, "Label", label, labelSize, foreground, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, true);
            AddSurfaceChrome(rect, foreground, 1.5f, 0.18f, false);
            return button;
        }

        private static void AddButtonIcon(Button button, string name, Sprite sprite, Color color)
        {
            var icon = CreateArtworkImage(button.transform, name, new Vector2(0.08f, 0.18f), new Vector2(0.30f, 0.82f));
            icon.sprite = sprite;
            icon.color = color;
            var label = button.transform.Find("Label").GetComponent<RectTransform>();
            SetAnchors(label, new Vector2(0.27f, 0), new Vector2(0.98f, 1));
        }

        private static void AddSurfaceChrome(RectTransform surface, Color edgeColor, float edgeDistance, float shadowAlpha, bool addOutline = true)
        {
            if (addOutline)
            {
                var outline = surface.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0.58f);
                outline.effectDistance = new Vector2(edgeDistance, -edgeDistance);
                outline.useGraphicAlpha = false;
            }

            var shadow = surface.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(Ink.r, Ink.g, Ink.b, shadowAlpha);
            shadow.effectDistance = new Vector2(0, -8f);
            shadow.useGraphicAlpha = false;
        }

        private static Outline CreateButtonHighlight(Button button)
        {
            var outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = Paper;
            outline.effectDistance = new Vector2(5f, -5f);
            outline.useGraphicAlpha = false;
            outline.enabled = false;
            return outline;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private string RulesText(int highlightedRule = -1)
        {
            var lines = new List<string>(_activeRules.Count);
            for (var index = 0; index < _activeRules.Count; index++)
            {
                var rule = _activeRules[index];
                if (rule.IsFallback)
                {
                    var fallbackLine = $"{index + 1}. {_localizer.Get("fallback")}";
                    lines.Add(index == highlightedRule ? HighlightRule(fallbackLine) : fallbackLine);
                    continue;
                }

                var conditions = rule.RequiredAll != ArtifactTraits.None ? TraitsText(rule.RequiredAll) : TraitsText(rule.RequiredAny);
                var joiner = rule.RequiredAny != ArtifactTraits.None && rule.RequiredAll == ArtifactTraits.None ? " / " : " + ";
                if (joiner == " / ")
                {
                    conditions = conditions.Replace(" · ", joiner);
                }

                var line = $"{index + 1}. {conditions}  →  {DestinationName(rule.Destination)}";
                lines.Add(index == highlightedRule ? HighlightRule(line) : line);
            }

            return string.Join("\n", lines);
        }

        private static string HighlightRule(string line) => "<color=#E0A24B><b>" + line + "</b></color>";

        private ArtifactTraits TutorialEmphasizedTraits()
        {
            var resolution = _session?.CurrentResolution;
            if (resolution == null || _activeRules == null)
            {
                return ArtifactTraits.None;
            }

            var rule = _activeRules.FirstOrDefault(candidate => candidate.Id == resolution.RuleId);
            if (rule == null || rule.IsFallback)
            {
                return ArtifactTraits.None;
            }

            var required = rule.RequiredAll != ArtifactTraits.None
                ? rule.RequiredAll
                : rule.RequiredAny;
            return required & _session.CurrentArtifact.Traits;
        }

        private string TraitsText(
            ArtifactTraits traits,
            ArtifactTraits emphasized = ArtifactTraits.None)
        {
            var labels = new List<string>(3);
            AddTrait(labels, traits, ArtifactTraits.Cursed, "trait_cursed", emphasized);
            AddTrait(labels, traits, ArtifactTraits.Fragile, "trait_fragile", emphasized);
            AddTrait(labels, traits, ArtifactTraits.Alive, "trait_alive", emphasized);
            AddTrait(labels, traits, ArtifactTraits.Temporal, "trait_temporal", emphasized);
            AddTrait(labels, traits, ArtifactTraits.Wet, "trait_wet", emphasized);
            AddTrait(labels, traits, ArtifactTraits.Metallic, "trait_metallic", emphasized);
            return string.Join(" · ", labels);
        }

        private void AddTrait(
            ICollection<string> labels,
            ArtifactTraits value,
            ArtifactTraits trait,
            string key,
            ArtifactTraits emphasized)
        {
            if ((value & trait) != 0)
            {
                var label = _localizer.Get(key);
                labels.Add((emphasized & trait) != 0
                    ? "<color=#E0A24B><b>" + label + "</b></color>"
                    : label);
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

        private string Resolution(ArtifactContent content) => _localizer.Locale == "ko" ? content.ResolutionKorean : content.ResolutionEnglish;

        private string CosmeticName(CosmeticContent content) => _localizer.Locale == "ko" ? content.NameKorean : content.NameEnglish;

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }
    }
}

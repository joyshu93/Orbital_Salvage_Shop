using System;
using System.Collections;
using System.Collections.Generic;
using CurioClerk.Content.Incidents;
using CurioClerk.Infrastructure.Feedback;
using CurioClerk.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class IncidentPresentationViewPlayModeTests
    {
        private GameObject _host;
        private readonly List<UnityEngine.Object> _ownedAssets = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                UnityEngine.Object.DestroyImmediate(_host);
            }

            foreach (var asset in _ownedAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            _ownedAssets.Clear();
        }

        [UnityTest]
        public IEnumerator NarrativeView_ShowsKoreanFirstBeatAndAdvancesExactlyOneBeat()
        {
            var view = CreateView(out var speaker, out var body, out var portrait, out var cue, out var button);
            var neutral = CreateSprite("neutral");
            var concerned = CreateSprite("concerned");
            var alert = CreateSprite("alert");
            var beats = new[]
            {
                Beat("First", "첫 문장", SeniorClerkMood.Neutral, IncidentVisualCue.None),
                Beat("Second", "둘째 문장", SeniorClerkMood.Concerned, IncidentVisualCue.Frost),
                Beat("Third", "셋째 문장", SeniorClerkMood.Alert, IncidentVisualCue.None)
            };

            view.Play(
                beats,
                "ko",
                mood => mood == SeniorClerkMood.Neutral
                    ? neutral
                    : mood == SeniorClerkMood.Concerned ? concerned : alert,
                () => { });
            yield return null;

            Assert.That(speaker.text, Is.EqualTo("선임 관리인"));
            Assert.That(body.text, Is.EqualTo("첫 문장"));
            Assert.That(portrait.sprite, Is.SameAs(neutral));
            Assert.That(portrait.enabled, Is.True);
            Assert.That(cue.enabled, Is.False);

            button.onClick.Invoke();
            yield return null;

            Assert.That(body.text, Is.EqualTo("둘째 문장"));
            Assert.That(body.text, Is.Not.EqualTo("셋째 문장"), "One tap must advance exactly one beat.");
            Assert.That(portrait.sprite, Is.SameAs(concerned));
            Assert.That(cue.enabled, Is.True, "A frost beat must reveal the configured cue surface.");
        }

        [UnityTest]
        public IEnumerator NarrativeView_UsesEnglishCopyAndChangesPortraitMood()
        {
            var view = CreateView(out var speaker, out var body, out var portrait, out _, out var button);
            var neutral = CreateSprite("neutral");
            var relieved = CreateSprite("relieved");
            var beats = new[]
            {
                Beat("The ledger is open.", "장부가 열렸습니다.", SeniorClerkMood.Neutral),
                Beat("Well handled.", "잘 처리했어요.", SeniorClerkMood.Relieved)
            };

            view.Play(beats, "en", mood => mood == SeniorClerkMood.Neutral ? neutral : relieved, () => { });
            yield return null;

            Assert.That(speaker.text, Is.EqualTo("Senior Clerk"));
            Assert.That(body.text, Is.EqualTo("The ledger is open."));
            Assert.That(portrait.sprite, Is.SameAs(neutral));

            button.onClick.Invoke();
            yield return null;

            Assert.That(body.text, Is.EqualTo("Well handled."));
            Assert.That(portrait.sprite, Is.SameAs(relieved));
        }

        [UnityTest]
        public IEnumerator NarrativeView_CompletesExactlyOnceAfterFinalBeat()
        {
            var view = CreateView(out _, out _, out _, out _, out var button);
            var completionCount = 0;
            view.Play(
                new[] { Beat("Only beat", "한 문장", SeniorClerkMood.Neutral) },
                "ko",
                _ => null,
                () => completionCount++);
            yield return null;

            button.onClick.Invoke();
            button.onClick.Invoke();
            yield return null;

            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(button.interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator NarrativeView_MissingPortraitKeepsSpeakerAndBodyReadable()
        {
            var view = CreateView(out var speaker, out var body, out var portrait, out _, out _);
            view.Play(
                new[] { Beat("Read the frost.", "서리를 읽으세요.", SeniorClerkMood.Alert) },
                "ko",
                _ => null,
                () => { });
            yield return null;

            Assert.That(portrait.sprite, Is.Null);
            Assert.That(portrait.enabled, Is.False);
            Assert.That(speaker.gameObject.activeInHierarchy, Is.True);
            Assert.That(body.gameObject.activeInHierarchy, Is.True);
            Assert.That(speaker.text, Is.EqualTo("선임 관리인"));
            Assert.That(body.text, Is.EqualTo("서리를 읽으세요."));
        }

        [UnityTest]
        public IEnumerator NarrativeView_DisableEnablePreservesProgressWithoutDuplicateCallbacks()
        {
            var view = CreateView(out _, out var body, out _, out _, out var button);
            var completionCount = 0;
            view.Play(
                new[]
                {
                    Beat("First", "첫째", SeniorClerkMood.Neutral),
                    Beat("Second", "둘째", SeniorClerkMood.Relieved)
                },
                "ko",
                _ => null,
                () => completionCount++);
            yield return null;

            _host.SetActive(false);
            _host.SetActive(true);
            _host.SetActive(false);
            _host.SetActive(true);
            yield return null;

            button.onClick.Invoke();
            yield return null;
            Assert.That(body.text, Is.EqualTo("둘째"));
            Assert.That(completionCount, Is.Zero, "Re-enabling must not add duplicate button listeners.");

            button.onClick.Invoke();
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));

            _host.SetActive(false);
            _host.SetActive(true);
            button.onClick.Invoke();
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator IncidentReaction_KeyMomentOwnsCardAndKeepsAuthoredLineReadable()
        {
            var feedback = new RecordingFeedbackService();
            var view = CreateReactionView(feedback, out var card, out var line, out _, out _);
            var restScale = card.localScale;
            var completionCount = 0;

            view.PlayKeyReaction(
                "얼음이 대답했다. 창고의 숨결이 한 박자 멎는다.",
                IncidentVisualCue.Frost,
                () => completionCount++);
            yield return new WaitForSecondsRealtime(0.16f);

            Assert.That(line.enabled, Is.True);
            Assert.That(line.text, Is.EqualTo("얼음이 대답했다. 창고의 숨결이 한 박자 멎는다."));
            Assert.That(Vector3.Distance(card.localScale, restScale), Is.GreaterThan(0.02f));
            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.KeyReaction }));

            yield return new WaitForSecondsRealtime(0.76f);
            Assert.That(completionCount, Is.Zero,
                "A key reaction must own the card for at least one second instead of flashing past it.");

            yield return new WaitForSecondsRealtime(0.46f);
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(line.enabled, Is.False);
            Assert.That(Vector3.Distance(card.localScale, restScale), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator IncidentReaction_IncidentCompleteWarmsScreenAndInvokesFeedbackOnce()
        {
            var feedback = new RecordingFeedbackService();
            var view = CreateReactionView(feedback, out _, out _, out _, out var warmth);
            var completionCount = 0;

            view.PlayIncidentComplete(() => completionCount++);
            yield return new WaitForSecondsRealtime(0.24f);

            Assert.That(warmth.enabled, Is.True);
            Assert.That(warmth.color.a, Is.GreaterThan(0.08f));
            Assert.That(feedback.Cues, Is.EqualTo(new[] { PlayerFeedbackCue.IncidentComplete }));

            yield return new WaitForSecondsRealtime(1.25f);

            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(warmth.enabled, Is.False);
            Assert.That(feedback.Cues, Has.Count.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator IncidentReaction_DisableThenEnableRestoresAndCompletesPendingMomentOnce()
        {
            var feedback = new RecordingFeedbackService();
            var view = CreateReactionView(feedback, out var card, out var line, out _, out var warmth);
            var restScale = card.localScale;
            var completionCount = 0;

            view.PlayKeyReaction("숨을 고르세요.", IncidentVisualCue.AmberWarmth, () => completionCount++);
            yield return null;
            _host.SetActive(false);

            Assert.That(Vector3.Distance(card.localScale, restScale), Is.LessThan(0.001f));
            Assert.That(line.enabled, Is.False);
            Assert.That(warmth.enabled, Is.False);
            Assert.That(completionCount, Is.Zero);

            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.That(feedback.Cues, Has.Count.EqualTo(1));

            _host.SetActive(false);
            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator IncidentReaction_FrostStateAndMistakeLineRemainAtmosphericAndReadable()
        {
            var view = CreateReactionView(
                new RecordingFeedbackService(),
                out var card,
                out var line,
                out var frost,
                out _);
            var restRotation = card.localRotation;

            view.SetFrosted(true);
            Assert.That(frost.enabled, Is.True);

            view.PlayMistake("서리가 장부 가장자리까지 번진다.");
            yield return new WaitForSecondsRealtime(0.12f);

            Assert.That(line.enabled, Is.True);
            Assert.That(line.text, Is.EqualTo("서리가 장부 가장자리까지 번진다."));
            Assert.That(Quaternion.Angle(card.localRotation, restRotation), Is.GreaterThan(0.2f));

            yield return new WaitForSecondsRealtime(0.58f);
            Assert.That(line.enabled, Is.False);
            Assert.That(frost.enabled, Is.True,
                "The incident's persistent frost must survive a transient mistake reaction.");
        }

        private NarrativeSequenceView CreateView(
            out TMP_Text speaker,
            out TMP_Text body,
            out Image portrait,
            out Image cue,
            out Button button)
        {
            _host = new GameObject("NarrativeHost", typeof(RectTransform));
            var view = _host.AddComponent<NarrativeSequenceView>();
            speaker = CreateText("Speaker");
            body = CreateText("Body");
            portrait = CreateImage("Portrait");
            cue = CreateImage("Cue");
            button = CreateButton("Continue");
            view.Configure(speaker, body, portrait, cue, button);
            return view;
        }

        private IncidentReactionView CreateReactionView(
            IPlayerFeedbackService feedback,
            out RectTransform card,
            out TMP_Text line,
            out Image frost,
            out Image warmth)
        {
            _host = new GameObject("IncidentReactionHost", typeof(RectTransform));
            card = CreateRect("ArtifactCard");
            card.localScale = new Vector3(0.96f, 1.02f, 1f);
            card.localRotation = Quaternion.Euler(0f, 0f, 1.5f);
            line = CreateText("ReactionLine");
            frost = CreateImage("FrostOverlay");
            frost.enabled = false;
            frost.color = new Color(0.58f, 0.82f, 0.95f, 0.38f);
            warmth = CreateImage("WarmthOverlay");
            warmth.enabled = false;
            warmth.color = new Color(0.96f, 0.68f, 0.25f, 0f);
            var view = _host.AddComponent<IncidentReactionView>();
            view.Configure(card, line, frost, warmth, feedback);
            return view;
        }

        private TMP_Text CreateText(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<TMP_Text>();
        }

        private Image CreateImage(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<Image>();
        }

        private Button CreateButton(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<Button>();
        }

        private RectTransform CreateRect(string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<RectTransform>();
        }

        private Sprite CreateSprite(string name)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = name + "-texture" };
            var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            sprite.name = name;
            _ownedAssets.Add(sprite);
            _ownedAssets.Add(texture);
            return sprite;
        }

        private static NarrativeBeat Beat(
            string english,
            string korean,
            SeniorClerkMood mood,
            IncidentVisualCue cue = IncidentVisualCue.None)
            => new NarrativeBeat(new LocalizedCopy(english, korean), mood, cue);

        private sealed class RecordingFeedbackService : IPlayerFeedbackService
        {
            public List<PlayerFeedbackCue> Cues { get; } = new List<PlayerFeedbackCue>();

            public void Configure(bool soundEnabled, bool hapticsEnabled)
            {
            }

            public void Play(PlayerFeedbackCue cue) => Cues.Add(cue);

            public void Dispose()
            {
            }
        }
    }
}

using System;
using System.Collections;
using CurioClerk.Content.Incidents;
using CurioClerk.Infrastructure.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class IncidentReactionView : MonoBehaviour
    {
        private const float KeyReactionDuration = 1.25f;
        private const float MistakeDuration = 0.52f;
        private const float IncidentCompleteDuration = 1.35f;

        private RectTransform _artifactCard;
        private TMP_Text _reactionText;
        private Image _reactionVeil;
        private Image _frostOverlay;
        private Image _warmthOverlay;
        private IPlayerFeedbackService _feedbackService;
        private Vector3 _cardRestScale;
        private Quaternion _cardRestRotation;
        private Vector2 _textRestPosition;
        private Vector3 _textRestScale;
        private Color _textRestColor;
        private Color _reactionVeilRestColor;
        private Color _frostRestColor;
        private Color _warmthRestColor;
        private Coroutine _reactionRoutine;
        private Action _completion;
        private bool _frosted;
        private bool _resumeCompletionOnEnable;

        public void Configure(
            RectTransform artifactCard,
            TMP_Text reactionText,
            Image reactionVeil,
            Image frostOverlay,
            Image warmthOverlay,
            IPlayerFeedbackService feedbackService)
        {
            _artifactCard = artifactCard ?? throw new ArgumentNullException(nameof(artifactCard));
            _reactionText = reactionText ?? throw new ArgumentNullException(nameof(reactionText));
            _reactionVeil = reactionVeil ?? throw new ArgumentNullException(nameof(reactionVeil));
            _frostOverlay = frostOverlay ?? throw new ArgumentNullException(nameof(frostOverlay));
            _warmthOverlay = warmthOverlay ?? throw new ArgumentNullException(nameof(warmthOverlay));
            _feedbackService = feedbackService;
            _cardRestScale = _artifactCard.localScale;
            _cardRestRotation = _artifactCard.localRotation;
            _textRestPosition = _reactionText.rectTransform.anchoredPosition;
            _textRestScale = _reactionText.rectTransform.localScale;
            _textRestColor = _reactionText.color;
            _reactionVeilRestColor = _reactionVeil.color;
            _frostRestColor = _frostOverlay.color;
            _warmthRestColor = _warmthOverlay.color;
            _frosted = _frostOverlay.enabled;
            RestoreState();
        }

        public void SetFrosted(bool frosted)
        {
            EnsureConfigured();
            _frosted = frosted;
            if (_reactionRoutine == null)
            {
                ApplyPersistentFrost();
            }
        }

        public void PlayKeyReaction(string text, IncidentVisualCue cue, Action completed)
        {
            EnsureConfigured();
            StartReaction(AnimateKeyReaction(text ?? string.Empty, cue), completed);
            _feedbackService?.Play(PlayerFeedbackCue.KeyReaction);
        }

        public void PlayMistake(string text)
        {
            EnsureConfigured();
            StartReaction(AnimateMistake(text ?? string.Empty), null);
        }

        public void PlayIncidentComplete(Action completed)
        {
            EnsureConfigured();
            StartReaction(AnimateIncidentComplete(), completed);
            _feedbackService?.Play(PlayerFeedbackCue.IncidentComplete);
        }

        private void StartReaction(IEnumerator animation, Action completed)
        {
            StopReaction(clearCompletion: true);
            RestoreState();
            _resumeCompletionOnEnable = false;
            _completion = completed;
            _reactionRoutine = StartCoroutine(animation);
        }

        private IEnumerator AnimateKeyReaction(string text, IncidentVisualCue cue)
        {
            ShowReactionText(text);
            var elapsed = 0f;
            while (elapsed < KeyReactionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / KeyReactionDuration);
                var arrive = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.18f));
                var depart = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.78f) / 0.22f));
                var presence = arrive * depart;
                var arrivalShock = Mathf.Sin(arrive * Mathf.PI);
                _artifactCard.localScale = _cardRestScale * (1f + presence * 0.12f + arrivalShock * 0.04f);
                _artifactCard.localRotation = _cardRestRotation * Quaternion.Euler(0f, 0f, presence * -2.4f);
                _reactionText.rectTransform.localScale = _textRestScale *
                                                         (Mathf.Lerp(0.72f, 1f, arrive) + arrivalShock * 0.20f);
                _reactionText.rectTransform.anchoredPosition = Vector2.Lerp(
                    _textRestPosition + Vector2.down * 18f,
                    _textRestPosition,
                    arrive);
                SetVeilAlpha(arrive * depart * 0.82f);
                SetTextAlpha(Mathf.Clamp01(presence * 1.35f));
                AnimateCue(cue, presence);
                yield return null;
            }

            CompleteReaction();
        }

        private IEnumerator AnimateMistake(string text)
        {
            ShowReactionText(text);
            var elapsed = 0f;
            while (elapsed < MistakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / MistakeDuration);
                var resistance = Mathf.Sin(progress * Mathf.PI * 3f) * (1f - progress);
                var visibility = Mathf.Sin(progress * Mathf.PI);
                _artifactCard.localRotation = _cardRestRotation * Quaternion.Euler(0f, 0f, resistance * 3.2f);
                _artifactCard.localScale = _cardRestScale * (1f - Mathf.Abs(resistance) * 0.025f);
                _reactionText.rectTransform.localScale = _textRestScale * (0.92f + visibility * 0.08f);
                SetVeilAlpha(Mathf.Sqrt(Mathf.Max(0f, visibility)) * 0.70f);
                SetTextAlpha(Mathf.Clamp01(visibility * 1.8f));
                yield return null;
            }

            CompleteReaction();
        }

        private IEnumerator AnimateIncidentComplete()
        {
            _warmthOverlay.enabled = true;
            var elapsed = 0f;
            while (elapsed < IncidentCompleteDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / IncidentCompleteDuration);
                var reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.42f));
                var settle = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.72f) / 0.28f));
                var glow = reveal * settle;
                _warmthOverlay.color = new Color(0.98f, 0.68f, 0.27f, glow * 0.34f);
                _artifactCard.localScale = _cardRestScale * (1f + glow * 0.035f);
                yield return null;
            }

            CompleteReaction();
        }

        private void AnimateCue(IncidentVisualCue cue, float presence)
        {
            if (cue == IncidentVisualCue.Frost)
            {
                _frostOverlay.enabled = true;
                var color = _frostRestColor;
                color.a = Mathf.Max(_frosted ? _frostRestColor.a : 0f, presence * 0.62f);
                _frostOverlay.color = color;
                return;
            }

            if (cue == IncidentVisualCue.AmberWarmth || cue == IncidentVisualCue.InkSeal)
            {
                _warmthOverlay.enabled = true;
                var warmth = cue == IncidentVisualCue.InkSeal
                    ? new Color(0.70f, 0.36f, 0.30f, presence * 0.20f)
                    : new Color(0.98f, 0.68f, 0.27f, presence * 0.24f);
                _warmthOverlay.color = warmth;
            }
        }

        private void ShowReactionText(string text)
        {
            _reactionText.text = text;
            _reactionText.enabled = true;
            _reactionVeil.enabled = true;
            _reactionText.rectTransform.anchoredPosition = _textRestPosition + Vector2.down * 18f;
            _reactionText.rectTransform.localScale = _textRestScale * 0.72f;
            SetVeilAlpha(0f);
            SetTextAlpha(0f);
        }

        private void SetVeilAlpha(float alpha)
        {
            var color = _reactionVeilRestColor;
            color.a = Mathf.Clamp01(alpha);
            _reactionVeil.color = color;
        }

        private void SetTextAlpha(float alpha)
        {
            var color = _textRestColor;
            color.a *= alpha;
            _reactionText.color = color;
        }

        private void CompleteReaction()
        {
            _reactionRoutine = null;
            RestoreState();
            var completed = _completion;
            _completion = null;
            completed?.Invoke();
        }

        private void RestoreState()
        {
            if (_artifactCard == null)
            {
                return;
            }

            _artifactCard.localScale = _cardRestScale;
            _artifactCard.localRotation = _cardRestRotation;
            _reactionText.rectTransform.anchoredPosition = _textRestPosition;
            _reactionText.rectTransform.localScale = _textRestScale;
            _reactionText.color = _textRestColor;
            _reactionText.enabled = false;
            _reactionVeil.color = _reactionVeilRestColor;
            _reactionVeil.enabled = false;
            _warmthOverlay.color = _warmthRestColor;
            _warmthOverlay.enabled = false;
            ApplyPersistentFrost();
        }

        private void ApplyPersistentFrost()
        {
            if (_frostOverlay == null)
            {
                return;
            }

            _frostOverlay.color = _frostRestColor;
            _frostOverlay.enabled = _frosted;
        }

        private void StopReaction(bool clearCompletion)
        {
            if (_reactionRoutine != null)
            {
                StopCoroutine(_reactionRoutine);
                _reactionRoutine = null;
            }

            if (clearCompletion)
            {
                _completion = null;
            }
        }

        private void EnsureConfigured()
        {
            if (_artifactCard == null ||
                _reactionText == null ||
                _reactionVeil == null ||
                _frostOverlay == null ||
                _warmthOverlay == null)
            {
                throw new InvalidOperationException("Configure the incident reaction view before using it.");
            }
        }

        private void OnDisable()
        {
            _resumeCompletionOnEnable = _reactionRoutine != null && _completion != null;
            StopReaction(clearCompletion: !_resumeCompletionOnEnable);
            RestoreState();
        }

        private void OnEnable()
        {
            if (!_resumeCompletionOnEnable)
            {
                return;
            }

            _resumeCompletionOnEnable = false;
            var completed = _completion;
            _completion = null;
            completed?.Invoke();
        }

        private void OnDestroy()
        {
            StopReaction(clearCompletion: true);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace CurioClerk.Presentation
{
    public sealed class ShiftFeedbackAnimator : MonoBehaviour
    {
        private const float EntranceDuration = 0.20f;
        private const float CorrectDuration = 0.42f;
        private const float WrongDuration = 0.28f;
        private const float HoldDuration = 0.34f;

        private RectTransform _artifactCard;
        private RectTransform _artwork;
        private RectTransform _feedbackPanel;
        private RectTransform _heldPreview;
        private TransformState _cardRest;
        private TransformState _artworkRest;
        private TransformState _feedbackRest;
        private TransformState _heldRest;
        private Coroutine _motionRoutine;
        private Coroutine _idleRoutine;
        private bool _idleEnabled;

        public void Configure(RectTransform artifactCard, RectTransform feedbackPanel)
        {
            Configure(artifactCard, artifactCard, feedbackPanel, feedbackPanel);
        }

        public void Configure(
            RectTransform artifactCard,
            RectTransform artwork,
            RectTransform feedbackPanel,
            RectTransform heldPreview)
        {
            _artifactCard = artifactCard ?? throw new ArgumentNullException(nameof(artifactCard));
            _artwork = artwork ?? throw new ArgumentNullException(nameof(artwork));
            _feedbackPanel = feedbackPanel ?? throw new ArgumentNullException(nameof(feedbackPanel));
            _heldPreview = heldPreview ?? throw new ArgumentNullException(nameof(heldPreview));
            _cardRest = TransformState.Capture(_artifactCard);
            _artworkRest = TransformState.Capture(_artwork);
            _feedbackRest = TransformState.Capture(_feedbackPanel);
            _heldRest = TransformState.Capture(_heldPreview);
        }

        public void PlayArtifactEntrance()
        {
            if (IsConfigured)
            {
                StartMotion(AnimateArtifactEntrance());
            }
        }

        public void PlayCorrect(Action completed)
        {
            if (!IsConfigured)
            {
                completed?.Invoke();
                return;
            }

            StartMotion(AnimateCorrect(completed));
        }

        public void PlayWrong()
        {
            PlayWrong(null);
        }

        public void PlayWrong(Action completed)
        {
            if (IsConfigured)
            {
                StartMotion(AnimateWrong(completed));
            }
            else
            {
                completed?.Invoke();
            }
        }

        public void PlayHold(Action completed)
        {
            if (!IsConfigured)
            {
                completed?.Invoke();
                return;
            }

            StartMotion(AnimateHold(completed));
        }

        public void SetIdleEnabled(bool enabled)
        {
            _idleEnabled = enabled;
            if (!enabled)
            {
                StopIdle();
                RestoreArtwork();
                return;
            }

            ResumeIdle();
        }

        private bool IsConfigured =>
            _artifactCard != null &&
            _artwork != null &&
            _feedbackPanel != null &&
            _heldPreview != null;

        private void StartMotion(IEnumerator animation)
        {
            StopMotion();
            StopIdle();
            RestoreAll();
            _motionRoutine = StartCoroutine(animation);
        }

        private IEnumerator AnimateArtifactEntrance()
        {
            var elapsed = 0f;
            while (elapsed < EntranceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / EntranceDuration));
                _artifactCard.anchoredPosition = Vector2.Lerp(
                    _cardRest.Position + Vector2.down * 18f,
                    _cardRest.Position,
                    progress);
                _artifactCard.localScale = Vector3.Lerp(
                    _cardRest.Scale * 0.94f,
                    _cardRest.Scale,
                    progress);
                yield return null;
            }

            RestoreCard();
            CompleteMotion(null);
        }

        private IEnumerator AnimateCorrect(Action completed)
        {
            var elapsed = 0f;
            while (elapsed < CorrectDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / CorrectDuration);
                var lift = Mathf.Sin(progress * Mathf.PI);
                _artwork.anchoredPosition = _artworkRest.Position + Vector2.up * (28f * lift);
                _artwork.localScale = _artworkRest.Scale * (1f + 0.045f * lift);
                _feedbackPanel.localScale = _feedbackRest.Scale * (1f + 0.075f * lift);
                yield return null;
            }

            RestoreArtwork();
            RestoreFeedback();
            CompleteMotion(completed);
        }

        private IEnumerator AnimateWrong(Action completed)
        {
            var elapsed = 0f;
            while (elapsed < WrongDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / WrongDuration);
                var offset = Mathf.Sin(progress * Mathf.PI * 6f) * (1f - progress) * 13f;
                _artifactCard.anchoredPosition = _cardRest.Position + Vector2.right * offset;
                _artifactCard.localRotation = Quaternion.Euler(0f, 0f, offset * -0.16f);
                yield return null;
            }

            RestoreCard();
            CompleteMotion(completed);
        }

        private IEnumerator AnimateHold(Action completed)
        {
            var worldTarget = _heldPreview.TransformPoint(_heldPreview.rect.center);
            var localTarget = (Vector2)_artwork.parent.InverseTransformPoint(worldTarget);
            var artworkLocal = (Vector2)_artwork.localPosition;
            var target = _artworkRest.Position + (localTarget - artworkLocal) * 0.68f;
            var elapsed = 0f;
            while (elapsed < HoldDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / HoldDuration));
                _artwork.anchoredPosition = Vector2.Lerp(_artworkRest.Position, target, progress);
                _artwork.localScale = Vector3.Lerp(_artworkRest.Scale, _artworkRest.Scale * 0.72f, progress);
                _artwork.localRotation = Quaternion.Slerp(
                    _artworkRest.Rotation,
                    Quaternion.Euler(0f, 0f, -5f),
                    progress);
                yield return null;
            }

            RestoreArtwork();
            CompleteMotion(completed);
        }

        private IEnumerator AnimateIdle()
        {
            var elapsed = 0f;
            while (_idleEnabled)
            {
                elapsed += Time.unscaledDeltaTime;
                var wave = Mathf.Sin(elapsed * 1.8f);
                _artwork.anchoredPosition = _artworkRest.Position + Vector2.up * (wave * 3f);
                _artwork.localRotation = Quaternion.Euler(0f, 0f, wave * 0.45f);
                yield return null;
            }

            RestoreArtwork();
            _idleRoutine = null;
        }

        private void CompleteMotion(Action completed)
        {
            _motionRoutine = null;
            completed?.Invoke();
            if (_motionRoutine == null)
            {
                ResumeIdle();
            }
        }

        private void ResumeIdle()
        {
            if (!_idleEnabled || !IsConfigured || _motionRoutine != null || _idleRoutine != null || !isActiveAndEnabled)
            {
                return;
            }

            _idleRoutine = StartCoroutine(AnimateIdle());
        }

        private void StopMotion()
        {
            if (_motionRoutine != null)
            {
                StopCoroutine(_motionRoutine);
                _motionRoutine = null;
            }
        }

        private void StopIdle()
        {
            if (_idleRoutine != null)
            {
                StopCoroutine(_idleRoutine);
                _idleRoutine = null;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _motionRoutine = null;
            _idleRoutine = null;
            RestoreAll();
        }

        private void RestoreAll()
        {
            RestoreCard();
            RestoreArtwork();
            RestoreFeedback();
            _heldRest.Restore(_heldPreview);
        }

        private void RestoreCard() => _cardRest.Restore(_artifactCard);

        private void RestoreArtwork() => _artworkRest.Restore(_artwork);

        private void RestoreFeedback() => _feedbackRest.Restore(_feedbackPanel);

        private readonly struct TransformState
        {
            public TransformState(Vector2 position, Vector3 scale, Quaternion rotation)
            {
                Position = position;
                Scale = scale;
                Rotation = rotation;
            }

            public Vector2 Position { get; }
            public Vector3 Scale { get; }
            public Quaternion Rotation { get; }

            public static TransformState Capture(RectTransform transform)
            {
                return new TransformState(
                    transform.anchoredPosition,
                    transform.localScale,
                    transform.localRotation);
            }

            public void Restore(RectTransform transform)
            {
                if (transform == null)
                {
                    return;
                }

                transform.anchoredPosition = Position;
                transform.localScale = Scale;
                transform.localRotation = Rotation;
            }
        }
    }
}

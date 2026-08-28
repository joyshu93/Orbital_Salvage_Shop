using System;
using System.Collections;
using UnityEngine;

namespace CurioClerk.Presentation
{
    public sealed class ShiftFeedbackAnimator : MonoBehaviour
    {
        private const float EntranceDuration = 0.20f;
        private const float CorrectDuration = 0.56f;
        private const float WrongDuration = 0.28f;
        private const float HoldDuration = 0.34f;

        private RectTransform _artifactCard;
        private RectTransform _artwork;
        private RectTransform _feedbackPanel;
        private RectTransform _heldPreview;
        private RectTransform _farewellSeal;
        private CanvasGroup _artworkGroup;
        private CanvasGroup _farewellSealGroup;
        private TransformState _cardRest;
        private TransformState _artworkRest;
        private TransformState _feedbackRest;
        private TransformState _heldRest;
        private Vector3 _farewellSealRestScale;
        private Coroutine _motionRoutine;
        private Coroutine _idleRoutine;
        private Action _motionCompletion;
        private bool _idleEnabled;
        private bool _resumeCompletionOnEnable;

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
            _artworkGroup = _artwork.GetComponent<CanvasGroup>();
        }

        public void ConfigureFarewell(RectTransform seal, CanvasGroup sealGroup)
        {
            _farewellSeal = seal ?? throw new ArgumentNullException(nameof(seal));
            _farewellSealGroup = sealGroup ?? throw new ArgumentNullException(nameof(sealGroup));
            _farewellSealRestScale = _farewellSeal.localScale;
            RestoreFarewell();
        }

        public void PlayArtifactEntrance()
        {
            if (IsConfigured)
            {
                StartMotion(AnimateArtifactEntrance(), null);
            }
        }

        public void PlayCorrect(Action completed)
        {
            PlayCorrect(null, completed);
        }

        public void PlayCorrect(RectTransform destinationTarget, Action completed)
        {
            if (!IsConfigured)
            {
                completed?.Invoke();
                return;
            }

            StartMotion(AnimateCorrect(destinationTarget), completed);
        }

        public void PlayWrong()
        {
            PlayWrong(null);
        }

        public void PlayWrong(Action completed)
        {
            if (IsConfigured)
            {
                StartMotion(AnimateWrong(), completed);
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

            StartMotion(AnimateHold(), completed);
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

        private void StartMotion(IEnumerator animation, Action completed)
        {
            StopMotion();
            StopIdle();
            RestoreAll();
            _motionCompletion = completed;
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
            CompleteMotion();
        }

        private IEnumerator AnimateCorrect(RectTransform destinationTarget)
        {
            var target = _artworkRest.Position;
            var targetRotation = _artworkRest.Rotation;
            if (destinationTarget != null)
            {
                var worldTarget = destinationTarget.TransformPoint(destinationTarget.rect.center);
                var localTarget = (Vector2)_artwork.parent.InverseTransformPoint(worldTarget);
                var artworkLocal = (Vector2)_artwork.localPosition;
                target = _artworkRest.Position + (localTarget - artworkLocal) * 0.76f;
                var direction = Mathf.Sign(localTarget.x - artworkLocal.x);
                targetRotation = Quaternion.Euler(0f, 0f, direction * -7f);
            }

            var elapsed = 0f;
            while (elapsed < CorrectDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / CorrectDuration);
                var liftProgress = Mathf.Clamp01(progress / 0.58f);
                var lift = Mathf.Sin(liftProgress * Mathf.PI);
                var exit = destinationTarget == null
                    ? 0f
                    : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.30f) / 0.70f));
                var liftedPosition = _artworkRest.Position + Vector2.up * (24f * lift);
                _artwork.anchoredPosition = Vector2.Lerp(liftedPosition, target, exit);
                _artwork.localScale = _artworkRest.Scale * (1f + 0.045f * lift) * (1f - 0.52f * exit);
                _artwork.localRotation = Quaternion.Slerp(_artworkRest.Rotation, targetRotation, exit);
                _feedbackPanel.localScale = _feedbackRest.Scale * (1f + 0.075f * lift);
                if (_artworkGroup != null)
                {
                    _artworkGroup.alpha = 1f - 0.82f * exit;
                }

                AnimateFarewellSeal(progress);
                yield return null;
            }

            RestoreArtwork();
            RestoreFeedback();
            RestoreFarewell();
            CompleteMotion();
        }

        private IEnumerator AnimateWrong()
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
            CompleteMotion();
        }

        private IEnumerator AnimateHold()
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
                var arrivalPulse = Mathf.Sin(progress * Mathf.PI);
                _heldPreview.localScale = _heldRest.Scale * (1f + arrivalPulse * 0.10f);
                yield return null;
            }

            RestoreArtwork();
            _heldRest.Restore(_heldPreview);
            CompleteMotion();
        }

        private void AnimateFarewellSeal(float progress)
        {
            if (_farewellSeal == null || _farewellSealGroup == null)
            {
                return;
            }

            var reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.25f));
            var fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.72f) / 0.28f));
            _farewellSealGroup.alpha = reveal * fade;
            _farewellSeal.localScale = _farewellSealRestScale * Mathf.Lerp(0.55f, 1.08f, reveal);
            _farewellSeal.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-12f, -2f, reveal));
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

        private void CompleteMotion()
        {
            var completed = _motionCompletion;
            _motionCompletion = null;
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

            _motionCompletion = null;
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
            _resumeCompletionOnEnable = _motionRoutine != null && _motionCompletion != null;
            StopAllCoroutines();
            _motionRoutine = null;
            _idleRoutine = null;
            if (!_resumeCompletionOnEnable)
            {
                _motionCompletion = null;
            }
            RestoreAll();
        }

        private void OnEnable()
        {
            if (_resumeCompletionOnEnable)
            {
                _resumeCompletionOnEnable = false;
                var completed = _motionCompletion;
                _motionCompletion = null;
                completed?.Invoke();
            }

            ResumeIdle();
        }

        private void RestoreAll()
        {
            RestoreCard();
            RestoreArtwork();
            RestoreFeedback();
            _heldRest.Restore(_heldPreview);
            RestoreFarewell();
        }

        private void RestoreCard() => _cardRest.Restore(_artifactCard);

        private void RestoreArtwork()
        {
            _artworkRest.Restore(_artwork);
            if (_artworkGroup != null)
            {
                _artworkGroup.alpha = 1f;
            }
        }

        private void RestoreFeedback() => _feedbackRest.Restore(_feedbackPanel);

        private void RestoreFarewell()
        {
            if (_farewellSeal == null || _farewellSealGroup == null)
            {
                return;
            }

            _farewellSeal.localScale = _farewellSealRestScale;
            _farewellSeal.localRotation = Quaternion.identity;
            _farewellSealGroup.alpha = 0f;
        }

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

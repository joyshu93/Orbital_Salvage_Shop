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
        private RectTransform _curioResponseSurface;
        private RectTransform _curioResponseText;
        private RectTransform _curioResponseSeal;
        private CanvasGroup _artworkGroup;
        private CanvasGroup _farewellSealGroup;
        private CanvasGroup _curioResponseGroup;
        private TransformState _cardRest;
        private TransformState _artworkRest;
        private TransformState _feedbackRest;
        private TransformState _heldRest;
        private RectTransform _impactTarget;
        private TransformState _impactTargetRest;
        private Vector3 _farewellSealRestScale;
        private TransformState _curioResponseSurfaceRest;
        private TransformState _curioResponseTextRest;
        private TransformState _curioResponseSealRest;
        private Coroutine _motionRoutine;
        private Coroutine _idleRoutine;
        private Coroutine _heldResonanceRoutine;
        private Action _motionCompletion;
        private bool _idleEnabled;
        private bool _heldResonanceEnabled;
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

        public void ConfigureCurioResponse(
            RectTransform responseSurface,
            RectTransform responseText,
            RectTransform responseSeal,
            CanvasGroup responseGroup)
        {
            _curioResponseSurface = responseSurface ?? throw new ArgumentNullException(nameof(responseSurface));
            _curioResponseText = responseText ?? throw new ArgumentNullException(nameof(responseText));
            _curioResponseSeal = responseSeal ?? throw new ArgumentNullException(nameof(responseSeal));
            _curioResponseGroup = responseGroup ?? throw new ArgumentNullException(nameof(responseGroup));
            _curioResponseSurfaceRest = TransformState.Capture(_curioResponseSurface);
            _curioResponseTextRest = TransformState.Capture(_curioResponseText);
            _curioResponseSealRest = TransformState.Capture(_curioResponseSeal);
            RestoreCurioResponse();
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

        public void SetHeldResonanceEnabled(bool enabled)
        {
            _heldResonanceEnabled = enabled;
            if (!enabled)
            {
                StopHeldResonance();
                _heldRest.Restore(_heldPreview);
                return;
            }

            ResumeHeldResonance();
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
            var targetDirection = 0f;
            if (destinationTarget != null)
            {
                _impactTarget = destinationTarget;
                _impactTargetRest = TransformState.Capture(destinationTarget);
                var worldTarget = destinationTarget.TransformPoint(destinationTarget.rect.center);
                var localTarget = (Vector2)_artwork.parent.InverseTransformPoint(worldTarget);
                var artworkLocal = (Vector2)_artwork.localPosition;
                target = _artworkRest.Position + (localTarget - artworkLocal) * 0.76f;
                targetDirection = Mathf.Sign(localTarget.x - artworkLocal.x);
                targetRotation = Quaternion.Euler(0f, 0f, targetDirection * -7f);
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
                var impact = Mathf.Sin(Mathf.Clamp01(progress / 0.42f) * Mathf.PI);
                var decisionImpact = Mathf.Sin(Mathf.Clamp01(progress / 0.22f) * Mathf.PI);
                var liftedPosition = _artworkRest.Position + Vector2.up * (24f * lift);
                _artifactCard.localScale = _cardRest.Scale * (1f - 0.06f * decisionImpact);
                _artifactCard.localRotation = _cardRest.Rotation *
                                              Quaternion.Euler(0f, 0f, -0.8f * decisionImpact);
                _artwork.anchoredPosition = Vector2.Lerp(liftedPosition, target, exit);
                _artwork.localScale = _artworkRest.Scale * (1f + 0.045f * lift) * (1f - 0.52f * exit);
                _artwork.localRotation = Quaternion.Slerp(_artworkRest.Rotation, targetRotation, exit);
                _feedbackPanel.localScale = _feedbackRest.Scale * (1f + 0.14f * lift + 0.09f * impact);
                if (_impactTarget != null)
                {
                    _impactTarget.localScale = _impactTargetRest.Scale * (1f + 0.20f * impact);
                    _impactTarget.localRotation = _impactTargetRest.Rotation *
                                                  Quaternion.Euler(0f, 0f, targetDirection * -4.2f * impact);
                }
                if (_artworkGroup != null)
                {
                    _artworkGroup.alpha = 1f - 0.82f * exit;
                }

                AnimateFarewellSeal(progress);
                AnimateCurioResponse(progress);
                yield return null;
            }

            RestoreCard();
            RestoreArtwork();
            RestoreFeedback();
            RestoreFarewell();
            RestoreCurioResponse();
            RestoreImpactTarget();
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
                var impact = Mathf.Sin(Mathf.Clamp01(progress / 0.65f) * Mathf.PI);
                _artifactCard.anchoredPosition = _cardRest.Position + Vector2.right * offset;
                _artifactCard.localRotation = Quaternion.Euler(0f, 0f, offset * -0.16f);
                _artifactCard.localScale = _cardRest.Scale * (1f - Mathf.Abs(offset) * 0.0025f);
                _feedbackPanel.localScale = _feedbackRest.Scale * (1f + impact * 0.16f);
                yield return null;
            }

            RestoreCard();
            RestoreFeedback();
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
                if (_artworkGroup != null)
                {
                    _artworkGroup.alpha = 1f - 0.68f * progress;
                }
                var arrivalPulse = Mathf.Sin(progress * Mathf.PI);
                _heldPreview.localScale = _heldRest.Scale * (1f + arrivalPulse * 0.18f);
                _feedbackPanel.localScale = _feedbackRest.Scale * (1f + arrivalPulse * 0.08f);
                yield return null;
            }

            RestoreArtwork();
            _heldRest.Restore(_heldPreview);
            RestoreFeedback();
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

        private void AnimateCurioResponse(float progress)
        {
            if (_curioResponseSurface == null ||
                _curioResponseText == null ||
                _curioResponseSeal == null ||
                _curioResponseGroup == null)
            {
                return;
            }

            var reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.10f) / 0.28f));
            var sealReveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.10f) / 0.25f));
            var textReveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.24f) / 0.26f));
            var fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.80f) / 0.20f));
            _curioResponseGroup.alpha = reveal * fade;
            _curioResponseSurface.localScale = _curioResponseSurfaceRest.Scale *
                                               Mathf.Lerp(0.84f, 1.035f, reveal);
            _curioResponseSeal.localScale = _curioResponseSealRest.Scale *
                                            (Mathf.Lerp(0.35f, 1f, sealReveal) +
                                             Mathf.Sin(sealReveal * Mathf.PI) * 0.55f);
            _curioResponseSeal.localRotation = _curioResponseSealRest.Rotation *
                                               Quaternion.Euler(0f, 0f, Mathf.Lerp(-32f, 0f, sealReveal));
            _curioResponseText.anchoredPosition = _curioResponseTextRest.Position +
                                                  Vector2.Lerp(Vector2.down * 18f, Vector2.up * 4f, textReveal);
            _curioResponseText.localScale = _curioResponseTextRest.Scale * Mathf.Lerp(0.68f, 1.12f, textReveal);
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

        private IEnumerator AnimateHeldResonance()
        {
            var elapsed = 0f;
            while (_heldResonanceEnabled)
            {
                elapsed += Time.unscaledDeltaTime;
                var wave = Mathf.Sin(elapsed * 8f);
                var beat = Mathf.Abs(wave);
                _heldPreview.anchoredPosition = _heldRest.Position +
                                                new Vector2(wave * 0.9f, beat * 1.4f);
                _heldPreview.localScale = _heldRest.Scale * (1f + beat * 0.025f);
                _heldPreview.localRotation = _heldRest.Rotation *
                                             Quaternion.Euler(0f, 0f, wave * 1.2f);
                yield return null;
            }

            _heldRest.Restore(_heldPreview);
            _heldResonanceRoutine = null;
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

        private void ResumeHeldResonance()
        {
            if (!_heldResonanceEnabled || !IsConfigured || _heldResonanceRoutine != null || !isActiveAndEnabled)
            {
                return;
            }

            _heldResonanceRoutine = StartCoroutine(AnimateHeldResonance());
        }

        private void StopHeldResonance()
        {
            if (_heldResonanceRoutine == null)
            {
                return;
            }

            StopCoroutine(_heldResonanceRoutine);
            _heldResonanceRoutine = null;
        }

        private void OnDisable()
        {
            _resumeCompletionOnEnable = _motionRoutine != null && _motionCompletion != null;
            StopAllCoroutines();
            _motionRoutine = null;
            _idleRoutine = null;
            _heldResonanceRoutine = null;
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
            ResumeHeldResonance();
        }

        private void RestoreAll()
        {
            RestoreCard();
            RestoreArtwork();
            RestoreFeedback();
            _heldRest.Restore(_heldPreview);
            RestoreFarewell();
            RestoreCurioResponse();
            RestoreImpactTarget();
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

        private void RestoreCurioResponse()
        {
            if (_curioResponseSurface == null ||
                _curioResponseText == null ||
                _curioResponseSeal == null ||
                _curioResponseGroup == null)
            {
                return;
            }

            _curioResponseSurfaceRest.Restore(_curioResponseSurface);
            _curioResponseTextRest.Restore(_curioResponseText);
            _curioResponseSealRest.Restore(_curioResponseSeal);
            _curioResponseGroup.alpha = 0f;
        }

        private void RestoreImpactTarget()
        {
            if (_impactTarget == null)
            {
                return;
            }

            _impactTargetRest.Restore(_impactTarget);
            _impactTarget = null;
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

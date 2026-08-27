using System.Collections;
using UnityEngine;

namespace CurioClerk.Presentation
{
    public sealed class ShiftFeedbackAnimator : MonoBehaviour
    {
        private const float EntranceDuration = 0.18f;
        private const float FeedbackDuration = 0.22f;
        private RectTransform _artifactCard;
        private RectTransform _feedbackPanel;
        private Vector2 _artifactRestPosition;
        private Vector2 _feedbackRestPosition;
        private Coroutine _artifactRoutine;
        private Coroutine _feedbackRoutine;

        public void Configure(RectTransform artifactCard, RectTransform feedbackPanel)
        {
            _artifactCard = artifactCard;
            _feedbackPanel = feedbackPanel;
            _artifactRestPosition = artifactCard.anchoredPosition;
            _feedbackRestPosition = feedbackPanel.anchoredPosition;
        }

        public void PlayArtifactEntrance()
        {
            if (_artifactCard == null)
            {
                return;
            }

            if (_artifactRoutine != null)
            {
                StopCoroutine(_artifactRoutine);
            }

            _artifactCard.anchoredPosition = _artifactRestPosition + Vector2.down * 18f;
            _artifactCard.localScale = Vector3.one * 0.94f;
            _artifactRoutine = StartCoroutine(AnimateArtifactEntrance());
        }

        public void PlayCorrect()
        {
            StartFeedbackAnimation(false, 0.045f);
        }

        public void PlayDocketComplete()
        {
            StartFeedbackAnimation(false, 0.11f);
        }

        public void PlayWrong()
        {
            StartFeedbackAnimation(true, 0f);
        }

        private IEnumerator AnimateArtifactEntrance()
        {
            var elapsed = 0f;
            while (elapsed < EntranceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / EntranceDuration));
                _artifactCard.anchoredPosition = Vector2.Lerp(
                    _artifactRestPosition + Vector2.down * 18f,
                    _artifactRestPosition,
                    progress);
                _artifactCard.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, progress);
                yield return null;
            }

            ResetArtifact();
            _artifactRoutine = null;
        }

        private void StartFeedbackAnimation(bool shake, float pulseScale)
        {
            if (_feedbackPanel == null)
            {
                return;
            }

            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
            }

            _feedbackRoutine = StartCoroutine(AnimateFeedback(shake, pulseScale));
        }

        private IEnumerator AnimateFeedback(bool shake, float pulseScale)
        {
            var elapsed = 0f;
            while (elapsed < FeedbackDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / FeedbackDuration);
                if (shake)
                {
                    var offset = Mathf.Sin(progress * Mathf.PI * 6f) * (1f - progress) * 12f;
                    _feedbackPanel.anchoredPosition = _feedbackRestPosition + Vector2.right * offset;
                    _feedbackPanel.localScale = Vector3.one;
                }
                else
                {
                    var pulse = Mathf.Sin(progress * Mathf.PI) * pulseScale;
                    _feedbackPanel.localScale = Vector3.one * (1f + pulse);
                    _feedbackPanel.anchoredPosition = _feedbackRestPosition;
                }

                yield return null;
            }

            ResetFeedback();
            _feedbackRoutine = null;
        }

        private void OnDisable()
        {
            ResetArtifact();
            ResetFeedback();
        }

        private void ResetArtifact()
        {
            if (_artifactCard == null)
            {
                return;
            }

            _artifactCard.anchoredPosition = _artifactRestPosition;
            _artifactCard.localScale = Vector3.one;
        }

        private void ResetFeedback()
        {
            if (_feedbackPanel == null)
            {
                return;
            }

            _feedbackPanel.anchoredPosition = _feedbackRestPosition;
            _feedbackPanel.localScale = Vector3.one;
        }
    }
}

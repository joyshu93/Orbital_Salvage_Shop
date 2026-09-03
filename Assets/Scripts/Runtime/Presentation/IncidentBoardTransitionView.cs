using System;
using System.Collections;
using CurioClerk.Content;
using UnityEngine;

namespace CurioClerk.Presentation
{
    public sealed class IncidentBoardTransitionView : MonoBehaviour
    {
        private RectTransform _current;
        private CanvasGroup _currentGroup;
        private RectTransform _resolved;
        private CanvasGroup _resolvedGroup;
        private CanvasGroup _rainVeil;
        private Coroutine _routine;

        public void Configure(RectTransform current, CanvasGroup currentGroup, RectTransform resolved, CanvasGroup resolvedGroup, CanvasGroup rainVeil)
        {
            _current = current ?? throw new ArgumentNullException(nameof(current));
            _currentGroup = currentGroup ?? throw new ArgumentNullException(nameof(currentGroup));
            _resolved = resolved ?? throw new ArgumentNullException(nameof(resolved));
            _resolvedGroup = resolvedGroup ?? throw new ArgumentNullException(nameof(resolvedGroup));
            _rainVeil = rainVeil ?? throw new ArgumentNullException(nameof(rainVeil));
            ApplyFinalState();
        }

        public void Play(bool reveal, IncidentPresentationProfile profile)
        {
            StopAndSettle();
            if (!reveal || !isActiveAndEnabled) return;
            _routine = StartCoroutine(Animate(Mathf.Clamp(profile == null ? 1f : profile.TransitionStrength, 0.5f, 1.5f)));
        }

        private IEnumerator Animate(float strength)
        {
            var duration = Mathf.Lerp(1.2f, 1.8f, (strength - 0.5f) / 1f);
            _current.localScale = Vector3.one * 0.94f;
            _current.anchoredPosition += Vector2.down * 26f;
            _currentGroup.alpha = 0f;
            _resolved.localScale = Vector3.one * 1.05f;
            _resolved.anchoredPosition += Vector2.down * 14f;
            _resolvedGroup.alpha = 0.55f;
            _rainVeil.alpha = 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                _currentGroup.alpha = Mathf.SmoothStep(0f, 1f, progress);
                _current.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, progress);
                _resolvedGroup.alpha = Mathf.Lerp(0.55f, 1f, progress);
                _resolved.localScale = Vector3.one * Mathf.Lerp(1.05f, 1f, progress);
                _resolved.anchoredPosition = Vector2.Lerp(Vector2.down * 14f, Vector2.zero, progress);
                _rainVeil.alpha = Mathf.Sin(progress * Mathf.PI) * 0.24f;
                yield return null;
            }
            _routine = null;
            ApplyFinalState();
        }

        private void StopAndSettle()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            ApplyFinalState();
        }

        private void ApplyFinalState()
        {
            if (_current != null) { _current.localScale = Vector3.one; _current.anchoredPosition = Vector2.zero; }
            if (_resolved != null) { _resolved.localScale = Vector3.one; _resolved.anchoredPosition = Vector2.zero; }
            if (_currentGroup != null) _currentGroup.alpha = 1f;
            if (_resolvedGroup != null) _resolvedGroup.alpha = 1f;
            if (_rainVeil != null) _rainVeil.alpha = 0f;
        }

        private void OnDisable() => StopAndSettle();
        private void OnDestroy() => StopAndSettle();
    }
}

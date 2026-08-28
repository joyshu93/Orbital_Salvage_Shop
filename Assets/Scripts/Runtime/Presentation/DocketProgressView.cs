using System;
using System.Collections;
using System.Collections.Generic;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class DocketProgressView : MonoBehaviour
    {
        private TMP_Text _counter;
        private Image[] _stamps;
        private TMP_Text[] _labels;
        private Color _openColor;
        private Color[] _stampedColors;
        private Color[] _completionRestColors;
        private Vector3[] _completionRestScales;
        private Coroutine _completionRoutine;
        private Action _completionCallback;
        private bool _hasCompletionRestState;
        private bool _resumeCompletionOnEnable;

        private const float CompletionDuration = 0.70f;

        public void Configure(
            TMP_Text counter,
            IReadOnlyList<Image> stamps,
            IReadOnlyList<TMP_Text> labels,
            Color openColor,
            IReadOnlyList<Color> stampedColors)
        {
            if (counter == null)
            {
                throw new ArgumentNullException(nameof(counter));
            }

            if (stamps == null || stamps.Count != 3)
            {
                throw new ArgumentException("A docket view requires three destination stamps.", nameof(stamps));
            }

            if (labels == null || labels.Count != 3)
            {
                throw new ArgumentException("A docket view requires three destination labels.", nameof(labels));
            }

            if (stampedColors == null || stampedColors.Count != 3)
            {
                throw new ArgumentException("A docket view requires three destination colors.", nameof(stampedColors));
            }

            _counter = counter;
            _stamps = new Image[stamps.Count];
            _labels = new TMP_Text[labels.Count];
            _stampedColors = new Color[stampedColors.Count];
            for (var index = 0; index < stamps.Count; index++)
            {
                _stamps[index] = stamps[index] ??
                    throw new ArgumentException("Docket stamps cannot contain null images.", nameof(stamps));
                _labels[index] = labels[index] ??
                    throw new ArgumentException("Docket labels cannot contain null text.", nameof(labels));
                _stampedColors[index] = stampedColors[index];
            }

            _openColor = openColor;
            _completionRestColors = new Color[_stamps.Length];
            _completionRestScales = new Vector3[_stamps.Length];
        }

        public void Refresh(
            DocketState docket,
            int completedDockets,
            int requiredDockets,
            string openLabel,
            string completedLabel)
        {
            if (_counter == null || _stamps == null || _labels == null || _stampedColors == null)
            {
                throw new InvalidOperationException("Configure the docket view before refreshing it.");
            }

            var visibleDocket = Math.Min(completedDockets + 1, requiredDockets);
            _counter.text = $"{visibleDocket} / {requiredDockets}";
            for (var index = 0; index < _stamps.Length; index++)
            {
                var stamped = docket != null && docket.IsStamped((Destination)index);
                _stamps[index].color = stamped ? _stampedColors[index] : _openColor;
                _labels[index].text = stamped ? completedLabel : openLabel;
            }
        }

        public void PlayComplete(Action completed)
        {
            if (_stamps == null)
            {
                completed?.Invoke();
                return;
            }

            if (_completionRoutine != null)
            {
                StopCompletion();
                RestoreCompletionState();
                _hasCompletionRestState = false;
            }

            for (var index = 0; index < _stamps.Length; index++)
            {
                _completionRestColors[index] = _stamps[index].color;
                _completionRestScales[index] = _stamps[index].rectTransform.localScale;
            }

            _hasCompletionRestState = true;
            _completionCallback = completed;
            _completionRoutine = StartCoroutine(AnimateComplete());
        }

        private IEnumerator AnimateComplete()
        {
            var elapsed = 0f;
            while (elapsed < CompletionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / CompletionDuration);
                var pulse = Mathf.Sin(progress * Mathf.PI);
                for (var index = 0; index < _stamps.Length; index++)
                {
                    _stamps[index].rectTransform.localScale =
                        _completionRestScales[index] * (1f + pulse * 0.13f);
                    _stamps[index].color = Color.Lerp(
                        _completionRestColors[index],
                        Color.white,
                        pulse * 0.32f);
                }

                yield return null;
            }

            RestoreCompletionState();
            _hasCompletionRestState = false;
            _completionRoutine = null;
            var completion = _completionCallback;
            _completionCallback = null;
            completion?.Invoke();
        }

        private void OnDisable()
        {
            _resumeCompletionOnEnable = _completionRoutine != null && _completionCallback != null;
            if (_completionRoutine != null)
            {
                StopCoroutine(_completionRoutine);
                _completionRoutine = null;
            }

            if (!_resumeCompletionOnEnable)
            {
                _completionCallback = null;
            }
            RestoreCompletionState();
            _hasCompletionRestState = false;
        }

        private void OnEnable()
        {
            if (!_resumeCompletionOnEnable)
            {
                return;
            }

            _resumeCompletionOnEnable = false;
            var completion = _completionCallback;
            _completionCallback = null;
            completion?.Invoke();
        }

        private void StopCompletion()
        {
            if (_completionRoutine == null)
            {
                return;
            }

            StopCoroutine(_completionRoutine);
            _completionRoutine = null;
            _completionCallback = null;
        }

        private void RestoreCompletionState()
        {
            if (_stamps == null || _completionRestColors == null || _completionRestScales == null)
            {
                return;
            }

            for (var index = 0; index < _stamps.Length; index++)
            {
                if (_stamps[index] == null)
                {
                    continue;
                }

                if (_hasCompletionRestState)
                {
                    _stamps[index].rectTransform.localScale = _completionRestScales[index];
                    _stamps[index].color = _completionRestColors[index];
                }
            }
        }
    }
}

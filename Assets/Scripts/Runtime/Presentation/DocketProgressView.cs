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
        private bool[] _stamped;
        private Color[] _resonanceRestColors;
        private Vector3[] _resonanceRestScales;
        private Coroutine _completionRoutine;
        private Coroutine _resonanceRoutine;
        private Action _completionCallback;
        private bool _hasCompletionRestState;
        private bool _hasResonanceRestState;
        private bool _resumeCompletionOnEnable;
        private Image _completionSigil;
        private Color _completionSigilRestColor;
        private Vector3 _completionSigilRestScale;
        private bool _completionSigilRestEnabled;

        private const float CompletionDuration = 0.70f;
        private const float ResonanceDuration = 0.56f;

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
            _stamped = new bool[_stamps.Length];
            _resonanceRestColors = new Color[_stamps.Length];
            _resonanceRestScales = new Vector3[_stamps.Length];
        }

        public void ConfigureCompletionSigil(Image completionSigil)
        {
            _completionSigil = completionSigil ?? throw new ArgumentNullException(nameof(completionSigil));
        }

        public void Refresh(
            DocketState docket,
            int completedDockets,
            int requiredDockets,
            string openLabel,
            string completedLabel)
        {
            StopResonance();
            if (_counter == null || _stamps == null || _labels == null || _stampedColors == null)
            {
                throw new InvalidOperationException("Configure the docket view before refreshing it.");
            }

            var visibleDocket = Math.Min(completedDockets + 1, requiredDockets);
            _counter.text = $"{visibleDocket} / {requiredDockets}";
            for (var index = 0; index < _stamps.Length; index++)
            {
                var stamped = docket != null && docket.IsStamped((Destination)index);
                _stamped[index] = stamped;
                _stamps[index].color = stamped ? _stampedColors[index] : _openColor;
                _labels[index].text = stamped ? completedLabel : openLabel;
            }
        }

        public void PlayResonancePulse()
        {
            if (_stamps == null || _stamped == null)
            {
                return;
            }

            StopResonance();
            for (var index = 0; index < _stamps.Length; index++)
            {
                _resonanceRestColors[index] = _stamps[index].color;
                _resonanceRestScales[index] = _stamps[index].rectTransform.localScale;
            }

            _hasResonanceRestState = true;
            _resonanceRoutine = StartCoroutine(AnimateResonancePulse());
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

            if (_completionSigil != null)
            {
                _completionSigilRestColor = _completionSigil.color;
                _completionSigilRestScale = _completionSigil.rectTransform.localScale;
                _completionSigilRestEnabled = _completionSigil.enabled;
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


                AnimateCompletionSigil(progress);

                yield return null;
            }

            RestoreCompletionState();
            _hasCompletionRestState = false;
            _completionRoutine = null;
            var completion = _completionCallback;
            _completionCallback = null;
            completion?.Invoke();
        }

        private IEnumerator AnimateResonancePulse()
        {
            var elapsed = 0f;
            while (elapsed < ResonanceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / ResonanceDuration);
                var pulse = Mathf.Abs(Mathf.Sin(progress * Mathf.PI * 2f));
                for (var index = 0; index < _stamps.Length; index++)
                {
                    if (!_stamped[index])
                    {
                        continue;
                    }

                    _stamps[index].rectTransform.localScale =
                        _resonanceRestScales[index] * (1f + pulse * 0.15f);
                    _stamps[index].color = Color.Lerp(
                        _resonanceRestColors[index],
                        Color.white,
                        pulse * 0.38f);
                }

                yield return null;
            }

            RestoreResonanceState();
            _hasResonanceRestState = false;
            _resonanceRoutine = null;
        }

        private void OnDisable()
        {
            StopResonance();
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

        private void StopResonance()
        {
            if (_resonanceRoutine != null)
            {
                StopCoroutine(_resonanceRoutine);
                _resonanceRoutine = null;
            }

            RestoreResonanceState();
            _hasResonanceRestState = false;
        }

        private void RestoreResonanceState()
        {
            if (!_hasResonanceRestState || _stamps == null || _resonanceRestScales == null)
            {
                return;
            }

            for (var index = 0; index < _stamps.Length; index++)
            {
                if (_stamps[index] == null || _resonanceRestScales[index] == default)
                {
                    continue;
                }

                _stamps[index].rectTransform.localScale = _resonanceRestScales[index];
                _stamps[index].color = _resonanceRestColors[index];
            }
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


            if (_hasCompletionRestState && _completionSigil != null)
            {
                _completionSigil.color = _completionSigilRestColor;
                _completionSigil.rectTransform.localScale = _completionSigilRestScale;
                _completionSigil.enabled = _completionSigilRestEnabled;
            }
        }

        private void AnimateCompletionSigil(float progress)
        {
            if (_completionSigil == null)
            {
                return;
            }

            var reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.38f));
            var fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progress - 0.70f) / 0.30f));
            var visibility = reveal * fade;
            var transparentRest = _completionSigilRestColor;
            transparentRest.a = 0f;
            var litColor = new Color(1f, 0.79f, 0.39f, 1f);
            _completionSigil.enabled = true;
            _completionSigil.color = Color.Lerp(transparentRest, litColor, visibility);
            _completionSigil.rectTransform.localScale =
                _completionSigilRestScale * Mathf.Lerp(0.58f, 1.10f, reveal);
        }
    }
}

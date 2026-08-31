using System;
using System.Collections.Generic;
using CurioClerk.Content.Incidents;
using CurioClerk.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class NarrativeSequenceView : MonoBehaviour
    {
        private TMP_Text _speaker;
        private TMP_Text _body;
        private Image _portrait;
        private Image _cueSurface;
        private Button _continueButton;
        private IReadOnlyList<NarrativeBeat> _beats;
        private Func<SeniorClerkMood, Sprite> _portraitResolver;
        private Action _completed;
        private string _locale = "en";
        private int _beatIndex;
        private bool _isPlaying;
        private bool _listenerAttached;

        public void Configure(
            TMP_Text speaker,
            TMP_Text body,
            Image portrait,
            Image cueSurface,
            Button continueButton)
        {
            DetachContinueListener();
            _speaker = speaker ?? throw new ArgumentNullException(nameof(speaker));
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _portrait = portrait ?? throw new ArgumentNullException(nameof(portrait));
            _cueSurface = cueSurface ?? throw new ArgumentNullException(nameof(cueSurface));
            _continueButton = continueButton ?? throw new ArgumentNullException(nameof(continueButton));
            AttachContinueListener();
        }

        public void Play(
            IReadOnlyList<NarrativeBeat> beats,
            string locale,
            Func<SeniorClerkMood, Sprite> portraitResolver,
            Action completed)
        {
            EnsureConfigured();
            if (beats == null)
            {
                throw new ArgumentNullException(nameof(beats));
            }

            for (var index = 0; index < beats.Count; index++)
            {
                if (beats[index]?.Copy == null)
                {
                    throw new ArgumentException("Narrative beats cannot contain null copy.", nameof(beats));
                }
            }

            _beats = beats;
            _locale = locale == "ko" ? "ko" : "en";
            _portraitResolver = portraitResolver;
            _completed = completed;
            _beatIndex = 0;
            _isPlaying = beats.Count > 0;
            _continueButton.interactable = _isPlaying;

            if (!_isPlaying)
            {
                _speaker.text = new Localizer(_locale).Get("senior_clerk");
                _body.text = string.Empty;
                _portrait.sprite = null;
                _portrait.enabled = false;
                HideCue();
                Complete();
                return;
            }

            RefreshBeat();
        }

        private void Advance()
        {
            if (!_isPlaying)
            {
                return;
            }

            if (_beatIndex + 1 < _beats.Count)
            {
                _beatIndex++;
                RefreshBeat();
                return;
            }

            Complete();
        }

        private void RefreshBeat()
        {
            var beat = _beats[_beatIndex];
            _speaker.text = new Localizer(_locale).Get("senior_clerk");
            _body.text = beat.Copy.ForLocale(_locale);

            var portrait = _portraitResolver?.Invoke(beat.Mood);
            _portrait.sprite = portrait;
            _portrait.enabled = portrait != null;

            switch (beat.VisualCue)
            {
                case IncidentVisualCue.Frost:
                    var frost = VisualAssetLibrary.FrostOverlay;
                    _cueSurface.sprite = frost;
                    _cueSurface.color = Color.white;
                    _cueSurface.enabled = frost != null;
                    return;
                case IncidentVisualCue.InkSeal:
                    ShowColorCue(new Color(0.35f, 0.10f, 0.20f, 0.16f));
                    return;
                case IncidentVisualCue.AmberWarmth:
                    ShowColorCue(new Color(0.92f, 0.55f, 0.18f, 0.14f));
                    return;
                case IncidentVisualCue.Rain:
                    ShowColorCue(new Color(0.31f, 0.48f, 0.63f, 0.13f));
                    return;
                default:
                    HideCue();
                    return;
            }
        }

        private void Complete()
        {
            if (!_isPlaying && _completed == null)
            {
                return;
            }

            _isPlaying = false;
            _continueButton.interactable = false;
            var completed = _completed;
            _completed = null;
            completed?.Invoke();
        }

        private void HideCue()
        {
            _cueSurface.sprite = null;
            _cueSurface.color = Color.white;
            _cueSurface.enabled = false;
        }

        private void ShowColorCue(Color color)
        {
            _cueSurface.sprite = null;
            _cueSurface.color = color;
            _cueSurface.enabled = true;
        }

        private void EnsureConfigured()
        {
            if (_speaker == null ||
                _body == null ||
                _portrait == null ||
                _cueSurface == null ||
                _continueButton == null)
            {
                throw new InvalidOperationException("Configure the narrative view before playing it.");
            }
        }

        private void AttachContinueListener()
        {
            if (_continueButton == null || _listenerAttached)
            {
                return;
            }

            _continueButton.onClick.AddListener(Advance);
            _listenerAttached = true;
        }

        private void DetachContinueListener()
        {
            if (_continueButton == null || !_listenerAttached)
            {
                return;
            }

            _continueButton.onClick.RemoveListener(Advance);
            _listenerAttached = false;
        }

        private void OnEnable() => AttachContinueListener();

        private void OnDisable() => DetachContinueListener();

        private void OnDestroy() => DetachContinueListener();
    }
}

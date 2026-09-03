using System;
using CurioClerk.Content;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class IncidentCardView : MonoBehaviour
    {
        private static readonly Color FallbackSurface = new Color(0.357f, 0.161f, 0.267f, 0.91f);
        private static readonly Color FallbackAccent = new Color(0.878f, 0.635f, 0.294f, 1f);
        private Image _surface;
        private Image _artwork;
        private TMP_Text _status;
        private TMP_Text _title;
        private TMP_Text _clue;
        private Button _actionButton;
        private TMP_Text _actionLabel;
        private TMP_Text _waitingState;

        public void Configure(Image surface, Image artwork, TMP_Text status, TMP_Text title, TMP_Text clue, Button actionButton, TMP_Text actionLabel)
        {
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            _artwork = artwork ?? throw new ArgumentNullException(nameof(artwork));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _clue = clue ?? throw new ArgumentNullException(nameof(clue));
            _actionButton = actionButton ?? throw new ArgumentNullException(nameof(actionButton));
            _actionLabel = actionLabel ?? throw new ArgumentNullException(nameof(actionLabel));
        }

        public void ConfigureWaitingState(TMP_Text waitingState)
            => _waitingState = waitingState ?? throw new ArgumentNullException(nameof(waitingState));

        public void Bind(IncidentCardState state, IncidentPresentationProfile profile, Sprite artwork, UnityAction action)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_surface == null) throw new InvalidOperationException("Configure the incident card before binding it.");
            var accent = profile == null ? FallbackAccent : profile.AccentColor;
            _surface.color = profile == null ? FallbackSurface : profile.SurfaceColor;
            _status.color = accent;
            _status.text = state.Status ?? string.Empty;
            _title.text = state.Title ?? string.Empty;
            _clue.text = state.Clue ?? string.Empty;
            _artwork.sprite = artwork;
            _artwork.enabled = artwork != null;
            _actionButton.onClick.RemoveAllListeners();
            var actionable = state.Action != IncidentCardAction.None;
            _actionButton.gameObject.SetActive(actionable);
            if (actionable)
            {
                _actionLabel.text = state.ActionLabel ?? string.Empty;
                if (action != null) _actionButton.onClick.AddListener(action);
            }
            if (_waitingState != null)
            {
                _waitingState.gameObject.SetActive(!actionable);
                _waitingState.text = actionable ? string.Empty : state.Status ?? string.Empty;
            }
        }
    }
}

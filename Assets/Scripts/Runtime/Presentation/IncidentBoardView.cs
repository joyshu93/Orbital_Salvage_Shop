using System;
using System.Collections.Generic;
using CurioClerk.Content;
using UnityEngine;
using UnityEngine.Events;

namespace CurioClerk.Presentation
{
    public sealed class IncidentBoardView : MonoBehaviour
    {
        private IncidentCardView _current;
        private readonly List<IncidentCardView> _resolved = new List<IncidentCardView>();

        public void Configure(IncidentCardView current, IReadOnlyList<IncidentCardView> resolved)
        {
            _current = current;
            _resolved.Clear();
            if (resolved == null) return;
            for (var index = 0; index < resolved.Count; index++)
            {
                if (resolved[index] != null) _resolved.Add(resolved[index]);
            }
        }

        public void Bind(
            IncidentBoardState state,
            Func<string, IncidentPresentationProfile> profile,
            Func<string, Sprite> artwork,
            UnityAction startCurrent,
            Func<string, UnityAction> replay)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_current != null)
            {
                _current.gameObject.SetActive(state.Current != null);
                if (state.Current != null)
                    _current.Bind(state.Current, profile?.Invoke(state.Current.IncidentId), artwork?.Invoke(state.Current.LeadArtifactId), startCurrent);
            }
            for (var index = 0; index < _resolved.Count; index++)
            {
                var card = _resolved[index];
                var cardState = index < state.Resolved.Count ? state.Resolved[index] : null;
                card.gameObject.SetActive(cardState != null);
                if (cardState != null)
                    card.Bind(cardState, profile?.Invoke(cardState.IncidentId), artwork?.Invoke(cardState.LeadArtifactId), replay?.Invoke(cardState.IncidentId));
            }
        }
    }
}

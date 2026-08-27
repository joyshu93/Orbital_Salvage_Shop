using System;
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
        private Color _openColor;
        private Color _stampedColor;

        public void Configure(
            TMP_Text counter,
            IReadOnlyList<Image> stamps,
            Color openColor,
            Color stampedColor)
        {
            if (counter == null)
            {
                throw new ArgumentNullException(nameof(counter));
            }

            if (stamps == null || stamps.Count != 3)
            {
                throw new ArgumentException("A docket view requires three destination stamps.", nameof(stamps));
            }

            _counter = counter;
            _stamps = new Image[stamps.Count];
            for (var index = 0; index < stamps.Count; index++)
            {
                _stamps[index] = stamps[index] ??
                    throw new ArgumentException("Docket stamps cannot contain null images.", nameof(stamps));
            }

            _openColor = openColor;
            _stampedColor = stampedColor;
        }

        public void Refresh(DocketState docket, int completedDockets, int requiredDockets)
        {
            if (_counter == null || _stamps == null)
            {
                throw new InvalidOperationException("Configure the docket view before refreshing it.");
            }

            var visibleDocket = Math.Min(completedDockets + 1, requiredDockets);
            _counter.text = $"{visibleDocket} / {requiredDockets}";
            for (var index = 0; index < _stamps.Length; index++)
            {
                var stamped = docket != null && docket.IsStamped((Destination)index);
                _stamps[index].color = stamped ? _stampedColor : _openColor;
            }
        }
    }
}

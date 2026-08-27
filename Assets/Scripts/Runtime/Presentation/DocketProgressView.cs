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
        private TMP_Text[] _labels;
        private Color _openColor;
        private Color[] _stampedColors;

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
    }
}

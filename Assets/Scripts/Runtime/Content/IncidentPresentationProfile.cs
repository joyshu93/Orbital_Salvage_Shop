using System;
using CurioClerk.Content.Incidents;
using UnityEngine;

namespace CurioClerk.Content
{
    [CreateAssetMenu(menuName = "Curio Clerk/Incident Presentation Profile", fileName = "IncidentPresentation")]
    public sealed class IncidentPresentationProfile : ScriptableObject
    {
        [SerializeField] private string incidentId;
        [SerializeField] private Color accentColor = Color.white;
        [SerializeField] private Color surfaceColor = Color.black;
        [Range(0.5f, 1.5f), SerializeField] private float transitionStrength = 1f;

        public string IncidentId => incidentId;

        public Color AccentColor => accentColor;

        public Color SurfaceColor => surfaceColor;

        public float TransitionStrength => transitionStrength;

        public void Configure(IncidentPresentationStyleContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrWhiteSpace(content.IncidentId))
            {
                throw new ArgumentException("Incident presentation profiles require an incident ID.", nameof(content));
            }

            if (!TryParseColor(content.AccentHex, out var parsedAccent))
            {
                throw new ArgumentException("Incident presentation profiles require a valid accent color.", nameof(content));
            }

            if (!TryParseColor(content.SurfaceHex, out var parsedSurface))
            {
                throw new ArgumentException("Incident presentation profiles require a valid surface color.", nameof(content));
            }

            incidentId = content.IncidentId;
            accentColor = parsedAccent;
            surfaceColor = parsedSurface;
            transitionStrength = Mathf.Clamp(content.TransitionStrength, 0.5f, 1.5f);
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = default;
            return !string.IsNullOrWhiteSpace(hex) &&
                   ColorUtility.TryParseHtmlString("#" + hex, out color);
        }
    }
}

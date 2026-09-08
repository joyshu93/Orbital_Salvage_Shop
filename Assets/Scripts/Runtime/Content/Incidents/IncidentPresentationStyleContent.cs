namespace CurioClerk.Content.Incidents
{
    public sealed class IncidentPresentationStyleContent
    {
        public IncidentPresentationStyleContent(
            string incidentId,
            string accentHex,
            string surfaceHex,
            float transitionStrength)
        {
            IncidentId = incidentId;
            AccentHex = accentHex;
            SurfaceHex = surfaceHex;
            TransitionStrength = transitionStrength;
        }

        public string IncidentId { get; }

        public string AccentHex { get; }

        public string SurfaceHex { get; }

        public float TransitionStrength { get; }
    }
}

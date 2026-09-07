using System;

namespace CurioClerk.Content.Incidents
{
    public static class ThirdIncidentCatalog
    {
        public static IncidentDefinition CreatePreview()
        {
            return new IncidentDefinition(
                "one-minute-ahead",
                new LocalizedCopy("One Minute Ahead", "1분 앞선 저녁"),
                leadArtifactId: "backward-candle",
                boardVisualCue: IncidentVisualCue.AmberWarmth,
                completesWhenAllStagesCompleted: false,
                awaitingContentClue: new LocalizedCopy(
                    "The moss grows toward 2:17.",
                    "이끼가 2시 17분을 향해 자라고 있다."),
                stages: Array.Empty<IncidentStageDefinition>());
        }
    }
}

using System;

namespace CurioClerk.Infrastructure.Feedback
{
    public enum PlayerFeedbackCue
    {
        Hold,
        Correct,
        Wrong,
        ShiftComplete,
        KeyReaction,
        IncidentComplete
    }

    public interface IPlayerFeedbackService : IDisposable
    {
        void Configure(bool soundEnabled, bool hapticsEnabled);

        void Play(PlayerFeedbackCue cue);
    }
}

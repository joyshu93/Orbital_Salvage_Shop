using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurioClerk.Infrastructure.Feedback
{
    public sealed class UnityPlayerFeedbackService : IPlayerFeedbackService
    {
        private const int SampleRate = 22050;
        private readonly AudioSource _source;
        private readonly Dictionary<PlayerFeedbackCue, AudioClip> _clips;
        private bool _soundEnabled;
        private bool _hapticsEnabled;
        private bool _disposed;

        public UnityPlayerFeedbackService(GameObject host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
            _source.volume = 0.55f;
            _clips = new Dictionary<PlayerFeedbackCue, AudioClip>
            {
                [PlayerFeedbackCue.Hold] = CreateTone("HoldTone", 0.07f, 330f, 390f, 0.16f),
                [PlayerFeedbackCue.Correct] = CreateTone("CorrectTone", 0.12f, 490f, 680f, 0.20f),
                [PlayerFeedbackCue.Wrong] = CreateTone("WrongTone", 0.15f, 210f, 145f, 0.18f),
                [PlayerFeedbackCue.ShiftComplete] = CreateTone("ShiftCompleteTone", 0.22f, 440f, 720f, 0.18f)
            };
        }

        public void Configure(bool soundEnabled, bool hapticsEnabled)
        {
            _soundEnabled = soundEnabled;
            _hapticsEnabled = hapticsEnabled;
        }

        public void Play(PlayerFeedbackCue cue)
        {
            if (_disposed)
            {
                return;
            }

            if (_soundEnabled && _clips.TryGetValue(cue, out var clip))
            {
                _source.PlayOneShot(clip);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (_hapticsEnabled && cue == PlayerFeedbackCue.Wrong)
            {
                Handheld.Vibrate();
            }
#endif
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var clip in _clips.Values)
            {
                if (clip != null)
                {
                    UnityEngine.Object.Destroy(clip);
                }
            }

            _clips.Clear();
        }

        private static AudioClip CreateTone(
            string name,
            float durationSeconds,
            float startFrequency,
            float endFrequency,
            float amplitude)
        {
            var sampleCount = Mathf.CeilToInt(durationSeconds * SampleRate);
            var samples = new float[sampleCount];
            var phase = 0f;
            for (var index = 0; index < sampleCount; index++)
            {
                var progress = index / (float)Math.Max(1, sampleCount - 1);
                var attack = Mathf.Clamp01(progress / 0.08f);
                var release = 1f - Mathf.SmoothStep(0f, 1f, progress);
                var envelope = attack * release * release;
                var frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                phase += 2f * Mathf.PI * frequency / SampleRate;
                var fundamental = Mathf.Sin(phase);
                var warmHarmonic = 0.16f * Mathf.Sin(phase * 2f);
                samples[index] = (fundamental + warmHarmonic) * amplitude * envelope;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

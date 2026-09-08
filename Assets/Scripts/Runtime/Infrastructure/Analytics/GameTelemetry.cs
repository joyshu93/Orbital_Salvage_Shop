using System;
using System.Collections.Generic;
using System.Globalization;
using CurioClerk.Infrastructure.Time;

namespace CurioClerk.Infrastructure.Analytics
{
    public sealed class GameTelemetry
    {
        private readonly IAnalyticsService _analytics;
        private readonly IClock _clock;
        private DateTime? _shiftStartedAt;

        public GameTelemetry(IAnalyticsService analytics, IClock clock)
        {
            _analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void TutorialStarted()
        {
            _analytics.Track(AnalyticsEvents.TutorialStarted);
        }

        public void TutorialCompleted()
        {
            _analytics.Track(AnalyticsEvents.TutorialCompleted);
        }

        public void ShiftStarted(int band)
        {
            _shiftStartedAt = _clock.LocalNow;
            _analytics.Track(AnalyticsEvents.ShiftStarted, BandParameters(band));
        }

        public void ShiftFailed(int band, int sortedCount)
        {
            _shiftStartedAt = null;
            _analytics.Track(AnalyticsEvents.ShiftFailed, new Dictionary<string, string>
            {
                { "band", BandValue(band) },
                { "sorted_bucket", SortedCountBucket(sortedCount) }
            });
        }

        public void ShiftCompleted(int band)
        {
            var elapsedSeconds = _shiftStartedAt.HasValue
                ? Math.Max(0d, (_clock.LocalNow - _shiftStartedAt.Value).TotalSeconds)
                : 0d;
            _shiftStartedAt = null;
            _analytics.Track(AnalyticsEvents.ShiftCompleted, new Dictionary<string, string>
            {
                { "band", BandValue(band) },
                { "duration_bucket", DurationBucket(elapsedSeconds) }
            });
        }

        public void RewardOfferShown(string placement)
        {
            _analytics.Track(AnalyticsEvents.RewardOfferShown, new Dictionary<string, string>
            {
                { "placement", placement }
            });
        }

        public void RewardResult(string placement, string result)
        {
            _analytics.Track(AnalyticsEvents.RewardResult, new Dictionary<string, string>
            {
                { "placement", placement },
                { "result", result }
            });
        }

        public void CosmeticUnlocked(string cosmeticId)
        {
            _analytics.Track(AnalyticsEvents.CosmeticUnlocked, new Dictionary<string, string>
            {
                { "cosmetic_id", cosmeticId }
            });
        }

        public static string SortedCountBucket(int count)
        {
            return count <= 3 ? "0_3" : count <= 7 ? "4_7" : "8_12";
        }

        public static string DurationBucket(double seconds)
        {
            return seconds < 60d ? "under_60" : seconds < 120d ? "60_119" : "120_plus";
        }

        private static IReadOnlyDictionary<string, string> BandParameters(int band)
        {
            return new Dictionary<string, string> { { "band", BandValue(band) } };
        }

        private static string BandValue(int band)
        {
            return band.ToString(CultureInfo.InvariantCulture);
        }
    }
}

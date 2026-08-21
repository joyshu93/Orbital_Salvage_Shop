using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CurioClerk.Infrastructure.Analytics
{
    public static class AnalyticsEvents
    {
        public const string TutorialStarted = "tutorial_started";
        public const string TutorialCompleted = "tutorial_completed";
        public const string ShiftStarted = "shift_started";
        public const string ShiftFailed = "shift_failed";
        public const string ShiftCompleted = "shift_completed";
        public const string RewardOfferShown = "reward_offer_shown";
        public const string RewardResult = "reward_result";
        public const string CosmeticUnlocked = "cosmetic_unlocked";

        private static readonly ReadOnlyCollection<string> EventNames = Array.AsReadOnly(new[]
        {
            TutorialStarted,
            TutorialCompleted,
            ShiftStarted,
            ShiftFailed,
            ShiftCompleted,
            RewardOfferShown,
            RewardResult,
            CosmeticUnlocked
        });

        private static readonly ReadOnlyCollection<string> ParameterNames = Array.AsReadOnly(new[]
        {
            "band",
            "sorted_bucket",
            "duration_bucket",
            "placement",
            "result",
            "cosmetic_id"
        });

        private static readonly IReadOnlyDictionary<string, string[]> EventParameters =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { TutorialStarted, Array.Empty<string>() },
                { TutorialCompleted, Array.Empty<string>() },
                { ShiftStarted, new[] { "band" } },
                { ShiftFailed, new[] { "band", "sorted_bucket" } },
                { ShiftCompleted, new[] { "band", "duration_bucket" } },
                { RewardOfferShown, new[] { "placement" } },
                { RewardResult, new[] { "placement", "result" } },
                { CosmeticUnlocked, new[] { "cosmetic_id" } }
            };

        private static readonly IReadOnlyDictionary<string, HashSet<string>> ParameterValues =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
            {
                { "band", Values("1", "2", "3", "4", "5") },
                { "sorted_bucket", Values("0_3", "4_7", "8_12") },
                { "duration_bucket", Values("under_60", "60_119", "120_plus") },
                { "placement", Values("shift_failed_revive", "shift_complete_double") },
                { "result", Values("earned", "dismissed", "failed", "unavailable") },
                { "cosmetic_id", Values("brass-lamp", "moth-mobile", "plum-runner", "moon-mug", "fern-familiar", "amber-window") }
            };

        public static IReadOnlyCollection<string> All => EventNames;

        public static IReadOnlyCollection<string> AllowedParameterNames => ParameterNames;

        public static bool IsValid(
            string eventName,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(eventName) ||
                !EventParameters.TryGetValue(eventName, out var expectedNames))
            {
                return false;
            }

            var suppliedCount = parameters?.Count ?? 0;
            if (suppliedCount != expectedNames.Length)
            {
                return false;
            }

            for (var index = 0; index < expectedNames.Length; index++)
            {
                var parameterName = expectedNames[index];
                if (parameters == null ||
                    !parameters.TryGetValue(parameterName, out var value) ||
                    !ParameterValues[parameterName].Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        private static HashSet<string> Values(params string[] values)
        {
            return new HashSet<string>(values, StringComparer.Ordinal);
        }
    }
}

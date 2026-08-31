using System;
using System.Collections.Generic;
using System.Globalization;

namespace CurioClerk.Core.Progression
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public const int CurrentVersion = 4;

        private const string DefaultIncidentId = "unmelting-ice";

        public int version = CurrentVersion;
        public int coins;
        public int completedShifts;
        public bool tutorialCompleted;
        public string locale = "en";
        public bool analyticsConsent;
        public bool crashReportingConsent;
        public bool soundEnabled = true;
        public bool hapticsEnabled = true;
        public List<string> discoveredArtifactIds = new List<string>();
        public List<string> unlockedCosmeticIds = new List<string>();
        public string equippedCosmeticId = string.Empty;
        public string lastDailyCompletedDate = string.Empty;
        public int dailyBestScore;
        public string activeIncidentId = DefaultIncidentId;
        public int activeIncidentStage;
        public List<IncidentStageRecord> incidentStageRecords = new List<IncidentStageRecord>();
        public List<string> completedIncidentIds = new List<string>();

        public void Sanitize()
        {
            if (version < 2)
            {
                soundEnabled = true;
                hapticsEnabled = true;
            }

            version = CurrentVersion;
            coins = Math.Max(0, coins);
            completedShifts = Math.Max(0, completedShifts);
            locale = locale == "ko" ? "ko" : "en";
            discoveredArtifactIds = discoveredArtifactIds ?? new List<string>();
            unlockedCosmeticIds = unlockedCosmeticIds ?? new List<string>();
            equippedCosmeticId = equippedCosmeticId ?? string.Empty;
            dailyBestScore = Math.Max(0, dailyBestScore);
            if (!DateTime.TryParseExact(
                    lastDailyCompletedDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dailyDate))
            {
                lastDailyCompletedDate = string.Empty;
                dailyBestScore = 0;
            }
            else
            {
                lastDailyCompletedDate = dailyDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            activeIncidentId = string.IsNullOrWhiteSpace(activeIncidentId) ? DefaultIncidentId : activeIncidentId;
            activeIncidentStage = Math.Max(0, activeIncidentStage);
            incidentStageRecords = SanitizeIncidentStageRecords(incidentStageRecords);
            completedIncidentIds = SanitizeCompletedIncidentIds(completedIncidentIds);
        }

        private static List<IncidentStageRecord> SanitizeIncidentStageRecords(List<IncidentStageRecord> records)
        {
            var sanitized = new List<IncidentStageRecord>();
            if (records == null)
            {
                return sanitized;
            }

            foreach (var record in records)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.stageId))
                {
                    continue;
                }

                var safeQuality = Math.Max(0, Math.Min(2, record.bestQuality));
                IncidentStageRecord existing = null;
                foreach (var sanitizedRecord in sanitized)
                {
                    if (string.Equals(sanitizedRecord.stageId, record.stageId, StringComparison.Ordinal))
                    {
                        existing = sanitizedRecord;
                        break;
                    }
                }

                if (existing != null)
                {
                    existing.bestQuality = Math.Max(existing.bestQuality, safeQuality);
                    continue;
                }

                var sanitizedRecord = new IncidentStageRecord
                {
                    stageId = record.stageId,
                    bestQuality = safeQuality
                };
                sanitized.Add(sanitizedRecord);
            }

            return sanitized;
        }

        private static List<string> SanitizeCompletedIncidentIds(List<string> incidentIds)
        {
            var sanitized = new List<string>();
            var known = new HashSet<string>(StringComparer.Ordinal);
            if (incidentIds == null)
            {
                return sanitized;
            }

            foreach (var incidentId in incidentIds)
            {
                if (!string.IsNullOrWhiteSpace(incidentId) && known.Add(incidentId))
                {
                    sanitized.Add(incidentId);
                }
            }

            return sanitized;
        }
    }

    [Serializable]
    public sealed class IncidentStageRecord
    {
        public string stageId = string.Empty;
        public int bestQuality;
    }
}

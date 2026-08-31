using System;
using System.Collections.Generic;
using System.Globalization;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Shifts;

namespace CurioClerk.Core.Progression
{
    public sealed class ProgressionService
    {
        private const string DefaultIncidentId = "unmelting-ice";

        public void ApplyShift(PlayerSaveData save, ShiftResult result, IEnumerable<string> discoveredArtifactIds)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.State != ShiftState.Completed)
            {
                return;
            }

            save.Sanitize();
            save.coins += Math.Max(0, result.Coins);
            save.completedShifts++;
            if (discoveredArtifactIds == null)
            {
                return;
            }

            var known = new HashSet<string>(save.discoveredArtifactIds, StringComparer.Ordinal);
            foreach (var artifactId in discoveredArtifactIds)
            {
                if (!string.IsNullOrWhiteSpace(artifactId) && known.Add(artifactId))
                {
                    save.discoveredArtifactIds.Add(artifactId);
                }
            }
        }

        public bool TryUnlockCosmetic(PlayerSaveData save, string cosmeticId, int cost)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                throw new ArgumentException("Cosmetic id is required.", nameof(cosmeticId));
            }

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            save.Sanitize();
            if (save.unlockedCosmeticIds.Contains(cosmeticId) || save.coins < cost)
            {
                return false;
            }

            save.coins -= cost;
            save.unlockedCosmeticIds.Add(cosmeticId);
            save.equippedCosmeticId = cosmeticId;

            return true;
        }

        public bool TryEquipCosmetic(PlayerSaveData save, string cosmeticId)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                throw new ArgumentException("Cosmetic id is required.", nameof(cosmeticId));
            }

            save.Sanitize();
            if (!save.unlockedCosmeticIds.Contains(cosmeticId))
            {
                return false;
            }

            save.equippedCosmeticId = cosmeticId;
            return true;
        }

        public void RecordDailyCompletion(PlayerSaveData save, string localDateKey, int score)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (!DateTime.TryParseExact(
                    localDateKey,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var localDate))
            {
                throw new ArgumentException("Daily date must use yyyy-MM-dd.", nameof(localDateKey));
            }

            save.Sanitize();
            var normalizedDate = localDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var safeScore = Math.Max(0, score);
            if (string.Equals(save.lastDailyCompletedDate, normalizedDate, StringComparison.Ordinal))
            {
                save.dailyBestScore = Math.Max(save.dailyBestScore, safeScore);
                return;
            }

            save.lastDailyCompletedDate = normalizedDate;
            save.dailyBestScore = safeScore;
        }

        public IncidentRunner RestoreIncident(PlayerSaveData save, string incidentId, IReadOnlyList<string> stageIds)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            var hasKnownIncidentState = string.Equals(incidentId, DefaultIncidentId, StringComparison.Ordinal)
                && string.Equals(save.activeIncidentId, DefaultIncidentId, StringComparison.Ordinal);
            save.Sanitize();
            if (!hasKnownIncidentState)
            {
                save.activeIncidentId = DefaultIncidentId;
                save.activeIncidentStage = 0;
            }

            var completedBoundary = Math.Min(5, stageIds == null ? 0 : stageIds.Count);
            save.activeIncidentStage = Math.Min(save.activeIncidentStage, completedBoundary);
            return new IncidentRunner(DefaultIncidentId, stageIds, save.activeIncidentStage);
        }

        public void ApplyIncidentStage(PlayerSaveData save, IncidentStageCompletion completion)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (completion == null)
            {
                throw new ArgumentNullException(nameof(completion));
            }

            if (string.IsNullOrWhiteSpace(completion.IncidentId))
            {
                throw new ArgumentException("Incident id is required.", nameof(completion));
            }

            if (string.IsNullOrWhiteSpace(completion.StageId))
            {
                throw new ArgumentException("Stage id is required.", nameof(completion));
            }

            if (!Enum.IsDefined(typeof(IncidentQuality), completion.Quality))
            {
                throw new ArgumentOutOfRangeException(nameof(completion));
            }

            save.Sanitize();
            save.activeIncidentId = completion.IncidentId;
            save.activeIncidentStage = Math.Max(save.activeIncidentStage, Math.Min(5, Math.Max(0, completion.NextStageIndex)));

            IncidentStageRecord record = null;
            foreach (var candidate in save.incidentStageRecords)
            {
                if (string.Equals(candidate.stageId, completion.StageId, StringComparison.Ordinal))
                {
                    record = candidate;
                    break;
                }
            }

            if (record == null)
            {
                save.incidentStageRecords.Add(new IncidentStageRecord
                {
                    stageId = completion.StageId,
                    bestQuality = (int)completion.Quality
                });
            }
            else
            {
                record.bestQuality = Math.Max(record.bestQuality, (int)completion.Quality);
            }

            if (completion.IncidentCompleted && !save.completedIncidentIds.Contains(completion.IncidentId))
            {
                save.completedIncidentIds.Add(completion.IncidentId);
            }
        }
    }
}

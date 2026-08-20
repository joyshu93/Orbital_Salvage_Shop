using System;
using System.Collections.Generic;
using CurioClerk.Core.Shifts;

namespace CurioClerk.Core.Progression
{
    public sealed class ProgressionService
    {
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
            if (string.IsNullOrEmpty(save.equippedCosmeticId))
            {
                save.equippedCosmeticId = cosmeticId;
            }

            return true;
        }
    }
}


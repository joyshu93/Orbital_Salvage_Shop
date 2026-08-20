using System;
using System.Collections.Generic;

namespace CurioClerk.Core.Progression
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int coins;
        public int completedShifts;
        public bool tutorialCompleted;
        public string locale = "en";
        public bool analyticsConsent;
        public bool crashReportingConsent;
        public List<string> discoveredArtifactIds = new List<string>();
        public List<string> unlockedCosmeticIds = new List<string>();
        public string equippedCosmeticId = string.Empty;

        public void Sanitize()
        {
            version = CurrentVersion;
            coins = Math.Max(0, coins);
            completedShifts = Math.Max(0, completedShifts);
            locale = locale == "ko" ? "ko" : "en";
            discoveredArtifactIds = discoveredArtifactIds ?? new List<string>();
            unlockedCosmeticIds = unlockedCosmeticIds ?? new List<string>();
            equippedCosmeticId = equippedCosmeticId ?? string.Empty;
        }
    }
}


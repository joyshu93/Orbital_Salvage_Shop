using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Progression;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ProgressionContractTests
    {
        [Test]
        public void ApplyShift_AddsCoinsCompletionAndDistinctDiscoveries()
        {
            var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData");
            var serviceType = Require("CurioClerk.Core.Progression.ProgressionService");
            var resultType = Require("CurioClerk.Core.Shifts.ShiftResult");
            var stateType = Require("CurioClerk.Core.Shifts.ShiftState");
            var save = Activator.CreateInstance(saveType);
            var service = Activator.CreateInstance(serviceType);
            var result = Activator.CreateInstance(resultType, Enum.Parse(stateType, "Completed"), 500, 35, 10, 2);

            serviceType.GetMethod("ApplyShift").Invoke(service, new object[]
            {
                save,
                result,
                new[] { "clockwork-moth", "rain-jar", "clockwork-moth" }
            });

            Assert.That(Field<int>(save, "coins"), Is.EqualTo(35));
            Assert.That(Field<int>(save, "completedShifts"), Is.EqualTo(1));
            Assert.That(((IList)saveType.GetField("discoveredArtifactIds").GetValue(save)).Count, Is.EqualTo(2));
        }

        [Test]
        public void CosmeticUnlock_ChargesOnceAndRejectsInsufficientCoins()
        {
            var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData");
            var serviceType = Require("CurioClerk.Core.Progression.ProgressionService");
            var save = Activator.CreateInstance(saveType);
            var service = Activator.CreateInstance(serviceType);
            saveType.GetField("coins").SetValue(save, 90);
            var method = serviceType.GetMethod("TryUnlockCosmetic");

            Assert.That((bool)method.Invoke(service, new object[] { save, "brass-lamp", 100 }), Is.False);
            saveType.GetField("coins").SetValue(save, 150);
            Assert.That((bool)method.Invoke(service, new object[] { save, "brass-lamp", 100 }), Is.True);
            Assert.That(Field<int>(save, "coins"), Is.EqualTo(50));
            Assert.That(Field<string>(save, "equippedCosmeticId"), Is.EqualTo("brass-lamp"));
            Assert.That((bool)method.Invoke(service, new object[] { save, "brass-lamp", 100 }), Is.False);
            Assert.That(Field<int>(save, "coins"), Is.EqualTo(50));
        }

        [Test]
        public void CosmeticEquip_RequiresOwnershipAndChangesSelection()
        {
            var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData");
            var serviceType = Require("CurioClerk.Core.Progression.ProgressionService");
            var save = Activator.CreateInstance(saveType);
            var service = Activator.CreateInstance(serviceType);
            var unlocked = (IList)saveType.GetField("unlockedCosmeticIds").GetValue(save);
            unlocked.Add("brass-lamp");
            unlocked.Add("moth-mobile");
            var method = serviceType.GetMethod("TryEquipCosmetic");

            Assert.That(method, Is.Not.Null, "Progression must expose a guarded cosmetic equip operation.");
            Assert.That((bool)method.Invoke(service, new object[] { save, "moon-mug" }), Is.False);
            Assert.That((bool)method.Invoke(service, new object[] { save, "moth-mobile" }), Is.True);
            Assert.That(Field<string>(save, "equippedCosmeticId"), Is.EqualTo("moth-mobile"));
        }

        [Test]
        public void DailyCompletion_KeepsHighestScoreForSameDate()
        {
            var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData");
            var serviceType = Require("CurioClerk.Core.Progression.ProgressionService");
            var save = Activator.CreateInstance(saveType);
            var service = Activator.CreateInstance(serviceType);
            var method = serviceType.GetMethod("RecordDailyCompletion");

            Assert.That(method, Is.Not.Null, "Progression must record a completed daily challenge.");
            method.Invoke(service, new object[] { save, "2026-08-26", 440 });
            method.Invoke(service, new object[] { save, "2026-08-26", 310 });

            Assert.That(Field<string>(save, "lastDailyCompletedDate"), Is.EqualTo("2026-08-26"));
            Assert.That(Field<int>(save, "dailyBestScore"), Is.EqualTo(440));
        }

        [Test]
        public void DailyCompletion_NewDateStartsANewBestScore()
        {
            var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData");
            var serviceType = Require("CurioClerk.Core.Progression.ProgressionService");
            var save = Activator.CreateInstance(saveType);
            var service = Activator.CreateInstance(serviceType);
            var method = serviceType.GetMethod("RecordDailyCompletion");

            Assert.That(method, Is.Not.Null, "Progression must record a completed daily challenge.");
            method.Invoke(service, new object[] { save, "2026-08-26", 440 });
            method.Invoke(service, new object[] { save, "2026-08-27", 250 });

            Assert.That(Field<string>(save, "lastDailyCompletedDate"), Is.EqualTo("2026-08-27"));
            Assert.That(Field<int>(save, "dailyBestScore"), Is.EqualTo(250));
        }

        [Test]
        public void ApplyIncidentStage_KeepsTheBestQualityAndAdvancesOnce()
        {
            var save = new PlayerSaveData();
            var service = new ProgressionService();

            service.ApplyIncidentStage(save, Completion("ice-01-crack", IncidentQuality.Precise, 1, false));
            service.ApplyIncidentStage(save, Completion("ice-01-crack", IncidentQuality.Stable, 1, false));

            Assert.That(save.activeIncidentStage, Is.EqualTo(1));
            Assert.That(save.incidentStageRecords.Single().bestQuality, Is.EqualTo((int)IncidentQuality.Precise));
        }

        [Test]
        public void ApplyIncidentStage_SuppressesDuplicateCompletedIncidents()
        {
            var save = new PlayerSaveData();
            var service = new ProgressionService();

            service.ApplyIncidentStage(save, Completion("ice-05-farewell", IncidentQuality.Resonant, 5, true));
            service.ApplyIncidentStage(save, Completion("ice-05-farewell", IncidentQuality.Stable, 5, true));

            Assert.That(save.completedIncidentIds, Is.EqualTo(new[] { "unmelting-ice" }));
            Assert.That(save.activeIncidentStage, Is.EqualTo(5));
        }

        [Test]
        public void RestoreIncident_UnknownSaveProgressStartsTheUnmeltingIceAtStageZero()
        {
            var save = new PlayerSaveData
            {
                activeIncidentId = "another-incident",
                activeIncidentStage = 4
            };

            var restored = new ProgressionService().RestoreIncident(
                save,
                "unmelting-ice",
                new[] { "ice-01-crack", "ice-02-glow", "ice-03-echo", "ice-04-frozen-seal", "ice-05-farewell" });

            Assert.That(restored.IncidentId, Is.EqualTo("unmelting-ice"));
            Assert.That(restored.CurrentStageIndex, Is.Zero);
        }

        [Test]
        public void RestoreIncident_BlankSavedIncidentStartsAtStageZero()
        {
            var save = new PlayerSaveData
            {
                activeIncidentId = string.Empty,
                activeIncidentStage = 4
            };

            var restored = new ProgressionService().RestoreIncident(
                save,
                "unmelting-ice",
                new[] { "ice-01-crack", "ice-02-glow", "ice-03-echo", "ice-04-frozen-seal", "ice-05-farewell" });

            Assert.That(restored.CurrentStageIndex, Is.Zero);
        }

        [Test]
        public void RestoreIncident_ClampsPastFinalStageToCompletedBoundary()
        {
            var save = new PlayerSaveData
            {
                activeIncidentId = "unmelting-ice",
                activeIncidentStage = 99
            };

            var restored = new ProgressionService().RestoreIncident(
                save,
                "unmelting-ice",
                new[] { "ice-01-crack", "ice-02-glow", "ice-03-echo", "ice-04-frozen-seal", "ice-05-farewell" });

            Assert.That(restored.CurrentStageIndex, Is.EqualTo(5));
            Assert.That(restored.IsComplete, Is.True);
        }

        private static IncidentStageCompletion Completion(string stageId, IncidentQuality quality, int nextStageIndex, bool incidentCompleted)
        {
            var stageIds = incidentCompleted
                ? new[] { stageId, "next-stage", "later-stage", "fourth-stage", "fifth-stage" }
                : new[] { stageId, "next-stage", "later-stage", "fourth-stage", "fifth-stage", "sixth-stage" };
            var runner = new IncidentRunner(
                "unmelting-ice",
                stageIds,
                nextStageIndex - 1);
            return runner.CompleteCurrentStage(quality);
        }

        private static T Field<T>(object instance, string name)
        {
            return (T)instance.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance).GetValue(instance);
        }

        private static Type Require(string fullName)
        {
            var assembly = fullName.Contains("Core.") ? "CurioClerk.Core" : "CurioClerk.Runtime";
            var type = Type.GetType($"{fullName}, {assembly}");
            Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
            return type;
        }
    }
}

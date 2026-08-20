using System;
using System.Collections;
using System.Reflection;
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
            Assert.That((bool)method.Invoke(service, new object[] { save, "brass-lamp", 100 }), Is.False);
            Assert.That(Field<int>(save, "coins"), Is.EqualTo(50));
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


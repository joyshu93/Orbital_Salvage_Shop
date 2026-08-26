using System;
using System.IO;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class SaveStoreContractTests
    {
        private string _directory;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "curio-clerk-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _path = Path.Combine(_directory, "save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void SaveAndLoad_RoundTripsPlayerProgress()
        {
            var api = SaveApi.Create(_path);
            api.Set("coins", 125);
            api.Set("completedShifts", 4);

            api.Save();
            var loaded = api.Load();

            Assert.That(api.Int(loaded, "coins"), Is.EqualTo(125));
            Assert.That(api.Int(loaded, "completedShifts"), Is.EqualTo(4));
        }

        [Test]
        public void SaveAndLoad_RoundTripsDailyChallengeProgress()
        {
            var api = SaveApi.Create(_path);
            api.Set("lastDailyCompletedDate", "2026-08-26");
            api.Set("dailyBestScore", 525);

            api.Save();
            var loaded = api.Load();

            Assert.That(api.String(loaded, "lastDailyCompletedDate"), Is.EqualTo("2026-08-26"));
            Assert.That(api.Int(loaded, "dailyBestScore"), Is.EqualTo(525));
        }

        [Test]
        public void Load_UsesBackupWhenPrimarySaveIsCorrupt()
        {
            var api = SaveApi.Create(_path);
            api.Set("coins", 40);
            api.Save();
            api.Set("coins", 90);
            api.Save();
            File.WriteAllText(_path, "{ definitely not valid json");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "coins"), Is.EqualTo(40));
        }

        [Test]
        public void Load_ReturnsSafeDefaultsWhenPrimaryAndBackupAreCorrupt()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "bad");
            File.WriteAllText(_path + ".bak", "also bad");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(3));
            Assert.That(api.Int(loaded, "coins"), Is.Zero);
        }

        [Test]
        public void Load_MigratesVersionOneFeedbackPreferencesToEnabledDefaults()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "{\"version\":1,\"coins\":12}");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(3));
            Assert.That(api.Bool(loaded, "soundEnabled"), Is.True,
                "Players upgrading from v1 must not silently lose sound because missing JSON booleans deserialize false.");
            Assert.That(api.Bool(loaded, "hapticsEnabled"), Is.True);
        }

        [Test]
        public void Load_PreservesDisabledVersionTwoFeedbackPreferences()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "{\"version\":2,\"soundEnabled\":false,\"hapticsEnabled\":false}");

            var loaded = api.Load();

            Assert.That(api.Bool(loaded, "soundEnabled"), Is.False,
                "A deliberate v2 opt-out must survive load and sanitization.");
            Assert.That(api.Bool(loaded, "hapticsEnabled"), Is.False);
        }

        [Test]
        public void Load_MigratesVersionTwoDailyProgressToSafeDefaults()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "{\"version\":2,\"coins\":12,\"soundEnabled\":false,\"hapticsEnabled\":false}");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(3));
            Assert.That(api.String(loaded, "lastDailyCompletedDate"), Is.Empty);
            Assert.That(api.Int(loaded, "dailyBestScore"), Is.Zero);
            Assert.That(api.Bool(loaded, "soundEnabled"), Is.False);
            Assert.That(api.Bool(loaded, "hapticsEnabled"), Is.False);
        }

        private sealed class SaveApi
        {
            private readonly Type _saveType;
            private readonly Type _storeType;
            private readonly object _store;
            private readonly object _save;

            private SaveApi(Type saveType, Type storeType, object store, object save)
            {
                _saveType = saveType;
                _storeType = storeType;
                _store = store;
                _save = save;
            }

            public static SaveApi Create(string path)
            {
                var saveType = Require("CurioClerk.Core.Progression.PlayerSaveData", "CurioClerk.Core");
                var storeType = Require("CurioClerk.Infrastructure.Save.JsonFileSaveStore", "CurioClerk.Runtime");
                var store = Activator.CreateInstance(storeType, path);
                var save = Activator.CreateInstance(saveType);
                return new SaveApi(saveType, storeType, store, save);
            }

            public void Set(string field, int value) => _saveType.GetField(field).SetValue(_save, value);

            public void Set(string field, string value) => _saveType.GetField(field).SetValue(_save, value);

            public void Save() => _storeType.GetMethod("Save").Invoke(_store, new[] { _save });

            public object Load() => _storeType.GetMethod("LoadOrDefault").Invoke(_store, null);

            public int Int(object save, string field) => (int)_saveType.GetField(field).GetValue(save);

            public bool Bool(object save, string field) => (bool)_saveType.GetField(field).GetValue(save);

            public string String(object save, string field) => (string)_saveType.GetField(field).GetValue(save);

            private static Type Require(string fullName, string assembly)
            {
                var type = Type.GetType($"{fullName}, {assembly}");
                Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
                return type;
            }
        }
    }
}

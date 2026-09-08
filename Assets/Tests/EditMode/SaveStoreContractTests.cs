using System;
using System.Collections;
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

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(4));
            Assert.That(api.Int(loaded, "coins"), Is.Zero);
        }

        [Test]
        public void Load_MigratesVersionOneFeedbackPreferencesToEnabledDefaults()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "{\"version\":1,\"coins\":12}");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(4));
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

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(4));
            Assert.That(api.String(loaded, "lastDailyCompletedDate"), Is.Empty);
            Assert.That(api.Int(loaded, "dailyBestScore"), Is.Zero);
            Assert.That(api.Bool(loaded, "soundEnabled"), Is.False);
            Assert.That(api.Bool(loaded, "hapticsEnabled"), Is.False);
        }

        [Test]
        public void Load_MigratesVersionThreeIncidentProgressToVersionFourDefaults()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path, "{\"version\":3,\"coins\":12,\"locale\":\"ko\",\"soundEnabled\":false,\"hapticsEnabled\":false}");

            var loaded = api.Load();

            Assert.That(api.Int(loaded, "version"), Is.EqualTo(4));
            Assert.That(api.String(loaded, "activeIncidentId"), Is.EqualTo("unmelting-ice"));
            Assert.That(api.Int(loaded, "activeIncidentStage"), Is.Zero);
            Assert.That(api.List(loaded, "incidentStageRecords"), Is.Empty);
            Assert.That(api.List(loaded, "completedIncidentIds"), Is.Empty);
            Assert.That(api.Int(loaded, "coins"), Is.EqualTo(12));
            Assert.That(api.String(loaded, "locale"), Is.EqualTo("ko"));
            Assert.That(api.Bool(loaded, "soundEnabled"), Is.False);
            Assert.That(api.Bool(loaded, "hapticsEnabled"), Is.False);
        }

        [Test]
        public void SaveAndLoad_RoundTripsIncidentStageRecords()
        {
            var api = SaveApi.Create(_path);
            api.Set("activeIncidentId", "unmelting-ice");
            api.Set("activeIncidentStage", 2);
            api.List("incidentStageRecords").Add(api.StageRecord("ice-02-glow", 2));
            api.List("completedIncidentIds").Add("unmelting-ice-prologue");

            api.Save();
            var loaded = api.Load();
            var stageRecord = api.List(loaded, "incidentStageRecords")[0];

            Assert.That(api.String(loaded, "activeIncidentId"), Is.EqualTo("unmelting-ice"));
            Assert.That(api.Int(loaded, "activeIncidentStage"), Is.EqualTo(2));
            Assert.That(api.String(stageRecord, "stageId"), Is.EqualTo("ice-02-glow"));
            Assert.That(api.Int(stageRecord, "bestQuality"), Is.EqualTo(2));
            Assert.That(api.List(loaded, "completedIncidentIds"), Is.EqualTo(new[] { "unmelting-ice-prologue" }));
        }

        [Test]
        public void Load_RecoversInvalidIncidentDataWithoutResettingUnrelatedProgress()
        {
            var api = SaveApi.Create(_path);
            File.WriteAllText(_path,
                "{\"version\":4,\"coins\":75,\"locale\":\"ko\",\"soundEnabled\":false,\"hapticsEnabled\":false,\"activeIncidentId\":\"\",\"activeIncidentStage\":-4,\"incidentStageRecords\":[{\"stageId\":\"\",\"bestQuality\":2},{\"stageId\":\"ice-01-crack\",\"bestQuality\":99},{\"stageId\":\"ice-01-crack\",\"bestQuality\":1}],\"completedIncidentIds\":[\"unmelting-ice\",\"unmelting-ice\",\"\"]}");

            var loaded = api.Load();
            var records = api.List(loaded, "incidentStageRecords");

            Assert.That(api.String(loaded, "activeIncidentId"), Is.EqualTo("unmelting-ice"));
            Assert.That(api.Int(loaded, "activeIncidentStage"), Is.Zero);
            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(api.String(records[0], "stageId"), Is.EqualTo("ice-01-crack"));
            Assert.That(api.Int(records[0], "bestQuality"), Is.EqualTo(2));
            Assert.That(api.List(loaded, "completedIncidentIds"), Is.EqualTo(new[] { "unmelting-ice" }));
            Assert.That(api.Int(loaded, "coins"), Is.EqualTo(75));
            Assert.That(api.String(loaded, "locale"), Is.EqualTo("ko"));
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

            public IList List(string field) => List(_save, field);

            public IList List(object save, string field) => (IList)save.GetType().GetField(field).GetValue(save);

            public object StageRecord(string stageId, int bestQuality)
            {
                var recordType = Require("CurioClerk.Core.Progression.IncidentStageRecord", "CurioClerk.Core");
                var record = Activator.CreateInstance(recordType);
                recordType.GetField("stageId").SetValue(record, stageId);
                recordType.GetField("bestQuality").SetValue(record, bestQuality);
                return record;
            }

            public void Save() => _storeType.GetMethod("Save").Invoke(_store, new[] { _save });

            public object Load() => _storeType.GetMethod("LoadOrDefault").Invoke(_store, null);

            public int Int(object save, string field) => (int)save.GetType().GetField(field).GetValue(save);

            public bool Bool(object save, string field) => (bool)save.GetType().GetField(field).GetValue(save);

            public string String(object save, string field) => (string)save.GetType().GetField(field).GetValue(save);

            private static Type Require(string fullName, string assembly)
            {
                var type = Type.GetType($"{fullName}, {assembly}");
                Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
                return type;
            }
        }
    }
}

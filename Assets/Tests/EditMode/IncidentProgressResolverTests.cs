using System.Collections.Generic;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Progression;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class IncidentProgressResolverTests
    {
        [Test]
        public void Resolve_FirstIncompleteIncidentIsAvailableAndSecondIsLocked()
        {
            var snapshot = new IncidentProgressResolver().Resolve(new PlayerSaveData(), Definitions());

            Assert.That(snapshot.Current.Definition.Id, Is.EqualTo("unmelting-ice"));
            Assert.That(snapshot.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.Available));
            Assert.That(snapshot.Current.NextStageIndex, Is.Zero);
            Assert.That(snapshot.Find("remembering-rain").Lifecycle, Is.EqualTo(IncidentLifecycle.Locked));
        }

        [Test]
        public void Resolve_FirstResolvedUnlocksSecondAtStageZero()
        {
            var save = new PlayerSaveData();
            save.completedIncidentIds.Add("unmelting-ice");

            var snapshot = new IncidentProgressResolver().Resolve(save, Definitions());

            Assert.That(snapshot.Find("unmelting-ice").Lifecycle, Is.EqualTo(IncidentLifecycle.Resolved));
            Assert.That(snapshot.Current.Definition.Id, Is.EqualTo("remembering-rain"));
            Assert.That(snapshot.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.Available));
            Assert.That(snapshot.Current.NextStageIndex, Is.Zero);
        }

        [Test]
        public void Resolve_OpenEndedIncidentWithAllAuthoredStagesCompleteAwaitsContent()
        {
            var save = SaveWithStage("rain-01-voices");

            var snapshot = new IncidentProgressResolver().Resolve(save, Definitions());

            Assert.That(snapshot.Current.Definition.Id, Is.EqualTo("remembering-rain"));
            Assert.That(snapshot.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.AwaitingContent));
            Assert.That(snapshot.Current.NextStageIndex, Is.EqualTo(1));
            Assert.That(save.completedIncidentIds, Does.Not.Contain("remembering-rain"));
        }

        [Test]
        public void Resolve_AppendedStageMakesWaitingSaveAvailableWithoutReset()
        {
            var save = SaveWithStage("rain-01-voices");
            var resolver = new IncidentProgressResolver();

            var waiting = resolver.Resolve(save, Definitions(rainStages: new[] { "rain-01-voices" }));
            Assert.That(waiting.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.AwaitingContent));

            var resumed = resolver.Resolve(
                save,
                Definitions(rainStages: new[] { "rain-01-voices", "rain-02-window" }));
            Assert.That(resumed.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.Available));
            Assert.That(resumed.Current.NextStageIndex, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_UnknownLegacyIncidentFallsBackWithoutLosingOtherSaveData()
        {
            var save = new PlayerSaveData
            {
                activeIncidentId = "retired-incident",
                activeIncidentStage = 99,
                coins = 73,
                locale = "ko"
            };
            save.discoveredArtifactIds.Add("moon-umbrella");
            save.incidentStageRecords.Add(new IncidentStageRecord
            {
                stageId = "retired-stage",
                bestQuality = (int)IncidentQuality.Resonant
            });

            var snapshot = new IncidentProgressResolver().Resolve(save, Definitions());

            Assert.That(snapshot.Current.Definition.Id, Is.EqualTo("unmelting-ice"));
            Assert.That(snapshot.Current.NextStageIndex, Is.Zero);
            Assert.That(save.coins, Is.EqualTo(73));
            Assert.That(save.locale, Is.EqualTo("ko"));
            Assert.That(save.discoveredArtifactIds, Is.EqualTo(new[] { "moon-umbrella" }));
            Assert.That(save.incidentStageRecords, Has.Count.EqualTo(1));
            Assert.That(save.incidentStageRecords[0].stageId, Is.EqualTo("retired-stage"));
            Assert.That(save.incidentStageRecords[0].bestQuality, Is.EqualTo((int)IncidentQuality.Resonant));
        }

        private static PlayerSaveData SaveWithStage(string stageId)
        {
            var save = new PlayerSaveData { activeIncidentId = "remembering-rain", activeIncidentStage = 1 };
            save.completedIncidentIds.Add("unmelting-ice");
            save.incidentStageRecords.Add(new IncidentStageRecord { stageId = stageId, bestQuality = 1 });
            return save;
        }

        private static IReadOnlyList<IncidentProgressDefinition> Definitions(string[] rainStages = null)
        {
            return new[]
            {
                new IncidentProgressDefinition(
                    "unmelting-ice",
                    new[] { "ice-01-crack", "ice-02-spread" },
                    completesWhenAllStagesCompleted: true),
                new IncidentProgressDefinition(
                    "remembering-rain",
                    rainStages ?? new[] { "rain-01-voices" },
                    completesWhenAllStagesCompleted: false)
            };
        }
    }
}

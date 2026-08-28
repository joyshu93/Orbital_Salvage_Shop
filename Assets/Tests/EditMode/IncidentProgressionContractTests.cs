using System;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class IncidentProgressionContractTests
    {
        [Test]
        public void StageRun_MistakesProduceStable()
        {
            var run = new IncidentStageRun("ice-01-crack", "mossy-watch");

            Assert.That(run.Evaluate(CompletedResult(mistakes: 1)), Is.EqualTo(IncidentQuality.Stable));
        }

        [Test]
        public void StageRun_ZeroMistakesProducePrecise()
        {
            var run = new IncidentStageRun("ice-01-crack", "mossy-watch");

            Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Precise));
        }

        [Test]
        public void StageRun_ResonatesOnlyAfterTheAuthoredHold()
        {
            var run = new IncidentStageRun("ice-04-frozen-seal", "mossy-watch");
            run.RecordHold("unmelting-ice");
            Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Precise));

            run.RecordHold("mossy-watch");
            Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Resonant));
        }

        [Test]
        public void StageRun_HoldingAnotherArtifactDoesNotQualify()
        {
            var run = new IncidentStageRun("ice-04-frozen-seal", "mossy-watch");
            run.RecordHold("unmelting-ice");

            Assert.That(run.ResonanceConditionMet, Is.False);
            Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Precise));
        }

        [Test]
        public void StageRun_FailedResultsCannotBeEvaluated()
        {
            var run = new IncidentStageRun("ice-01-crack", "mossy-watch");

            Assert.Throws<InvalidOperationException>(() => run.Evaluate(
                new ShiftResult(ShiftState.Failed, score: 0, coins: 0, correctSorts: 0, mistakes: 3)));
        }

        [Test]
        public void Runner_CompletingStagesAdvancesExactlyOnce()
        {
            var runner = new IncidentRunner("unmelting-ice", new[] { "ice-01", "ice-02" }, 0);

            var completion = runner.CompleteCurrentStage(IncidentQuality.Stable);

            Assert.That(completion.IncidentId, Is.EqualTo("unmelting-ice"));
            Assert.That(completion.StageId, Is.EqualTo("ice-01"));
            Assert.That(completion.CompletedStageIndex, Is.Zero);
            Assert.That(completion.Quality, Is.EqualTo(IncidentQuality.Stable));
            Assert.That(completion.NextStageIndex, Is.EqualTo(1));
            Assert.That(completion.IncidentCompleted, Is.False);
            Assert.That(runner.CurrentStageIndex, Is.EqualTo(1));
            Assert.That(runner.CurrentStageId, Is.EqualTo("ice-02"));

            Assert.Throws<InvalidOperationException>(() => runner.CompleteCurrentStage(IncidentQuality.Precise));
            Assert.That(runner.CurrentStageIndex, Is.EqualTo(1));
        }

        [Test]
        public void Runner_FifthCompletionMarksIncidentComplete()
        {
            var runner = new IncidentRunner("unmelting-ice", new[] { "ice-01", "ice-02", "ice-03", "ice-04", "ice-05" }, 0);

            for (var index = 0; index < 5; index++)
            {
                var completion = runner.CompleteCurrentStage(IncidentQuality.Precise);
                Assert.That(completion.CompletedStageIndex, Is.EqualTo(index));
            }

            Assert.That(runner.IsComplete, Is.True);
            Assert.That(runner.CurrentStageIndex, Is.EqualTo(5));
            Assert.That(runner.CurrentStageId, Is.Null);
        }

        private static ShiftResult CompletedResult(int mistakes)
            => new ShiftResult(ShiftState.Completed, score: 100, coins: 5, correctSorts: 3, mistakes: mistakes);
    }
}

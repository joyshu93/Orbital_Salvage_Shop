using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Content.Incidents;
using CurioClerk.Content.Workbench;
using CurioClerk.Core.Workbench;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class WorkbenchCatalogTests
    {
        private static readonly string[] StageIds =
        {
            "ice-01-crack", "ice-02-spread", "ice-03-tomorrow", "ice-04-frozen-seal", "ice-05-thaw",
            "rain-01-voices", "rain-02-names-under-water", "rain-03-unsent-letter", "rain-04-dry-order",
            "rain-05-testimony"
        };

        [Test]
        public void Catalog_CoversBothExistingCasesWithoutChangingStableStageIds()
        {
            var existing = ContentCatalog.CreateIncidents().Where(incident => incident.CompletesWhenAllStagesCompleted)
                .SelectMany(incident => incident.Stages).Select(stage => stage.Id).ToArray();
            Assert.That(existing, Is.EqualTo(StageIds));
            Assert.That(WorkbenchCatalog.All.Select(scene => scene.StageId), Is.EqualTo(existing));
            Assert.That(WorkbenchCatalog.Find("unpublished-stage"), Is.Null);
        }

        [TestCaseSource(nameof(StageIds))]
        public void EveryScene_HasBilingualPhysicalTargetsToolsAndConsequences(string stageId)
        {
            var scene = WorkbenchCatalog.Find(stageId);
            Assert.That(scene, Is.Not.Null, stageId);
            var artifactIds = ContentCatalog.CreateArtifacts().Select(artifact => artifact.Id).ToHashSet();
            Assert.That(artifactIds, Does.Contain(scene.ArtifactId), stageId);
            AssertCopy(scene.Title, stageId);
            AssertCopy(scene.Objective, stageId);
            AssertCopy(scene.Introduction, stageId);
            AssertCopy(scene.Ending, stageId);
            AssertCopy(scene.Discovery, stageId);
            Assert.That(scene.Targets.Count, Is.InRange(2, 4), stageId);
            Assert.That(scene.Tools.Count, Is.InRange(2, 4), stageId);
            Assert.That(scene.Actions.Count, Is.InRange(2, 3), stageId);
            Assert.That(scene.Targets.Select(target => target.Id).Distinct().Count(), Is.EqualTo(scene.Targets.Count));
            Assert.That(scene.Tools.Select(tool => tool.Id).Distinct().Count(), Is.EqualTo(scene.Tools.Count));
            Assert.That(scene.Actions.Select(action => action.Step.Id).Distinct().Count(), Is.EqualTo(scene.Actions.Count));

            foreach (var target in scene.Targets)
            {
                AssertCopy(target.Label, stageId + "/" + target.Id);
                AssertCopy(target.Observation, stageId + "/" + target.Id);
                Assert.That(target.X, Is.InRange(0.05f, 0.95f));
                Assert.That(target.Y, Is.InRange(0.05f, 0.95f));
                if (target.RevealAfterStep != null)
                    Assert.That(scene.Actions.Select(action => action.Step.Id), Does.Contain(target.RevealAfterStep));
            }

            foreach (var tool in scene.Tools)
            {
                AssertCopy(tool.Label, stageId + "/" + tool.Id);
                AssertCopy(tool.Description, stageId + "/" + tool.Id);
                Assert.That(scene.Actions.Select(action => action.Step.ToolId), Does.Contain(tool.Id), "No filler tools.");
            }

            foreach (var action in scene.Actions)
            {
                AssertCopy(action.Result, stageId + "/" + action.Step.Id);
                AssertCopy(action.Hint, stageId + "/" + action.Step.Id);
                Assert.That(scene.Tools.Select(tool => tool.Id), Does.Contain(action.Step.ToolId));
                Assert.That(scene.Targets.Select(target => target.Id), Does.Contain(action.Step.TargetId));
                Assert.That(new[] { "frost", "repair", "reveal", "rain", "warmth", "turn" }, Does.Contain(action.Effect));
                if (action.RevealedArtifactId != null)
                    Assert.That(artifactIds, Does.Contain(action.RevealedArtifactId));
            }
        }

        [TestCaseSource(nameof(StageIds))]
        public void EveryScene_IsSolvableThroughVisibleCluesAndRecoversFromWrongExperiments(string stageId)
        {
            var scene = WorkbenchCatalog.Find(stageId);
            Assert.That(scene, Is.Not.Null, stageId);
            var session = new WorkbenchSession(scene.Puzzle);
            var finished = new HashSet<string>(StringComparer.Ordinal);

            foreach (var action in scene.Actions)
            {
                foreach (var target in scene.Targets.Where(target => target.RevealAfterStep == null ||
                             finished.Contains(target.RevealAfterStep)))
                    session.Observe(target.Id);

                Assert.That(action.Step.RequiredObservations, Is.Not.Empty,
                    stageId + "/" + action.Step.Id + " must depend on inspecting a clue.");
                var visible = scene.Targets.Where(target => target.RevealAfterStep == null ||
                    finished.Contains(target.RevealAfterStep)).Select(target => target.Id).ToArray();
                Assert.That(visible, Does.Contain(action.Step.TargetId), "Target cannot unlock itself.");
                Assert.That(action.Step.RequiredObservations.All(visible.Contains), Is.True);
                Assert.That(action.Step.RequiredSteps.All(finished.Contains), Is.True);
                if (finished.Count > 0)
                    Assert.That(action.Step.RequiredSteps, Is.Not.Empty, "Later actions must have a physical prerequisite.");

                Assert.That(session.Apply("unknown-tool", action.Step.TargetId).Kind, Is.EqualTo(WorkbenchOutcomeKind.NoMatch));
                Assert.That(session.IsComplete, Is.False, "Wrong experiments cannot bypass the puzzle.");
                var result = session.Apply(action.Step.ToolId, action.Step.TargetId);
                Assert.That(result.Kind, Is.EqualTo(WorkbenchOutcomeKind.Applied)
                    .Or.EqualTo(WorkbenchOutcomeKind.Completed), stageId);
                Assert.That(result.StepId, Is.EqualTo(action.Step.Id));
                finished.Add(action.Step.Id);
            }

            Assert.That(session.IsComplete, Is.True, stageId);
        }

        private static void AssertCopy(LocalizedCopy copy, string context)
        {
            Assert.That(copy, Is.Not.Null, context);
            Assert.That(string.IsNullOrWhiteSpace(copy.English), Is.False, context);
            Assert.That(string.IsNullOrWhiteSpace(copy.Korean), Is.False, context);
            Assert.That(copy.ForLocale("ko"), Is.EqualTo(copy.Korean));
            Assert.That(copy.ForLocale("en"), Is.EqualTo(copy.English));
        }
    }
}

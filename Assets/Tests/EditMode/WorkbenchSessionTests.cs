using System;
using System.Collections.Generic;
using CurioClerk.Core.Workbench;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class WorkbenchSessionTests
    {
        [Test]
        public void Observe_RecordsOnlyKnownTargetsAndDoesNotRepeat()
        {
            var session = new WorkbenchSession(IcePuzzle());

            Assert.That(session.Observe("crack"), Is.True);
            Assert.That(session.Observe("crack"), Is.False);
            Assert.That(session.HasObserved("crack"), Is.True);
            Assert.That(session.ObservedTargets, Is.EquivalentTo(new[] { "crack" }));
            Assert.That(session.CompletedSteps, Is.Empty);
            Assert.That(session.IsComplete, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("missing")]
        [TestCase("Crack")]
        public void Observe_UnknownIdsDoNotCreateFacts(string targetId)
        {
            var session = new WorkbenchSession(IcePuzzle());

            Assert.That(session.Observe(targetId), Is.False);
            Assert.That(session.HasObserved(targetId), Is.False);
            Assert.That(session.HasCompleted(targetId), Is.False);
            Assert.That(session.ObservedTargets, Is.Empty);
        }

        [Test]
        public void Apply_RequiresAuthoredObservationBeforeChangingTheObject()
        {
            var session = new WorkbenchSession(IcePuzzle());
            var blocked = session.Apply("cloth", "crack");

            Assert.That(blocked.Kind, Is.EqualTo(WorkbenchOutcomeKind.NeedsObservation));
            Assert.That(blocked.StepId, Is.EqualTo("seal-cold"));
            Assert.That(session.CompletedSteps, Is.Empty);
            session.Observe("crack");

            var applied = session.Apply("cloth", "crack");
            Assert.That(applied.Kind, Is.EqualTo(WorkbenchOutcomeKind.Applied));
            Assert.That(applied.StepId, Is.EqualTo("seal-cold"));
            Assert.That(session.HasCompleted("seal-cold"), Is.True);
            Assert.That(session.IsComplete, Is.False);
        }

        [Test]
        public void Apply_ChecksEveryAuthoredObservationRatherThanJustTheDestination()
        {
            var session = new WorkbenchSession(new WorkbenchPuzzle("paired-clue",
                new[] { "lock", "label" }, new[]
                {
                    new WorkbenchStep("open", "key", "lock", new[] { "lock", "label" })
                }));
            session.Observe("lock");

            Assert.That(session.Apply("key", "lock").Kind, Is.EqualTo(WorkbenchOutcomeKind.NeedsObservation));
            session.Observe("label");
            Assert.That(session.Apply("key", "lock").Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
        }

        [Test]
        public void Apply_BlockedDependencyCanBeResolvedWithoutRestarting()
        {
            var session = new WorkbenchSession(IcePuzzle());
            session.Observe("drawer");

            var blocked = session.Apply("handle", "drawer");
            Assert.That(blocked.Kind, Is.EqualTo(WorkbenchOutcomeKind.NeedsPreviousStep));
            Assert.That(blocked.StepId, Is.EqualTo("open-drawer"));
            Assert.That(session.CompletedSteps, Is.Empty);

            session.Observe("crack");
            session.Apply("cloth", "crack");
            var completed = session.Apply("handle", "drawer");
            Assert.That(completed.Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
            Assert.That(completed.StepId, Is.EqualTo("open-drawer"));
            Assert.That(session.IsComplete, Is.True);
            Assert.That(session.CompletedSteps, Is.EquivalentTo(new[] { "seal-cold", "open-drawer" }));
        }

        [TestCase("dry", "brush")]
        [TestCase("brush", "dry")]
        public void Apply_BranchedPrerequisitesCanCompleteInEitherOrder(string first, string second)
        {
            var session = new WorkbenchSession(new WorkbenchPuzzle("letter", new[] { "page" }, new[]
            {
                new WorkbenchStep("read", "lens", "page", requiredSteps: new[] { "dry", "brush" }),
                new WorkbenchStep("dry", "dry", "page"),
                new WorkbenchStep("brush", "brush", "page")
            }));

            Assert.That(session.Apply(first, "page").Kind, Is.EqualTo(WorkbenchOutcomeKind.Applied));
            Assert.That(session.Apply("lens", "page").Kind, Is.EqualTo(WorkbenchOutcomeKind.NeedsPreviousStep));
            Assert.That(session.Apply(second, "page").Kind, Is.EqualTo(WorkbenchOutcomeKind.Applied));
            Assert.That(session.Apply("lens", "page").Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
            Assert.That(session.CompletedSteps.Count, Is.EqualTo(3));
        }

        [TestCase("wrong", "crack")]
        [TestCase("cloth", "drawer")]
        [TestCase("cloth", "missing")]
        [TestCase(null, "crack")]
        [TestCase("cloth", null)]
        [TestCase("", "crack")]
        public void Apply_WrongExperimentsPreserveFactsAndProgressAndRemainRecoverable(string tool, string target)
        {
            var session = new WorkbenchSession(IcePuzzle());
            session.Observe("crack");
            session.Observe("drawer");
            session.Apply("cloth", "crack");

            var outcome = session.Apply(tool, target);
            Assert.That(outcome.Kind, Is.EqualTo(WorkbenchOutcomeKind.NoMatch));
            Assert.That(outcome.StepId, Is.Null);
            Assert.That(session.ObservedTargets, Is.EquivalentTo(new[] { "crack", "drawer" }));
            Assert.That(session.CompletedSteps, Is.EquivalentTo(new[] { "seal-cold" }));
            Assert.That(session.Apply("handle", "drawer").Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
        }

        [Test]
        public void Apply_RepeatedActionsNeverEmitCompletionTwice()
        {
            var session = new WorkbenchSession(IcePuzzle());
            session.Observe("crack");
            session.Observe("drawer");
            session.Apply("cloth", "crack");

            Assert.That(session.Apply("cloth", "crack").Kind, Is.EqualTo(WorkbenchOutcomeKind.AlreadyApplied));
            Assert.That(session.IsComplete, Is.False);
            Assert.That(session.Apply("handle", "drawer").Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
            var repeated = session.Apply("handle", "drawer");
            Assert.That(repeated.Kind, Is.EqualTo(WorkbenchOutcomeKind.AlreadyApplied));
            Assert.That(repeated.StepId, Is.EqualTo("open-drawer"));
            Assert.That(session.Apply("cloth", "crack").Kind, Is.EqualTo(WorkbenchOutcomeKind.AlreadyApplied));
            Assert.That(session.Apply("wrong", "drawer").Kind, Is.EqualTo(WorkbenchOutcomeKind.NoMatch));
            Assert.That(session.CompletedSteps.Count, Is.EqualTo(2));
            Assert.That(session.IsComplete, Is.True);
        }

        [Test]
        public void NewSession_StartsFreshWithoutChangingACompletedSession()
        {
            var puzzle = IcePuzzle();
            var first = new WorkbenchSession(puzzle);
            first.Observe("crack");
            first.Observe("drawer");
            first.Apply("cloth", "crack");
            first.Apply("handle", "drawer");
            var retry = new WorkbenchSession(puzzle);

            Assert.That(first.IsComplete, Is.True);
            Assert.That(retry.Puzzle, Is.SameAs(puzzle));
            Assert.That(retry.ObservedTargets, Is.Empty);
            Assert.That(retry.CompletedSteps, Is.Empty);
            Assert.That(retry.IsComplete, Is.False);
            Assert.That(retry.Apply("cloth", "crack").Kind, Is.EqualTo(WorkbenchOutcomeKind.NeedsObservation));
        }

        [Test]
        public void Definitions_CopyCallerArraysSoLaterEditsCannotAlterThePuzzle()
        {
            var observations = new[] { "crack" };
            var previous = new[] { "seal" };
            var targets = new[] { "crack", "drawer" };
            var steps = new[]
            {
                new WorkbenchStep("seal", "cloth", "crack", observations),
                new WorkbenchStep("open", "handle", "drawer", requiredSteps: previous)
            };
            var puzzle = new WorkbenchPuzzle("ice", targets, steps);
            observations[0] = "missing";
            previous[0] = "missing";
            targets[0] = "missing";
            steps[0] = new WorkbenchStep("replacement", "other", "crack");
            var session = new WorkbenchSession(puzzle);

            Assert.That(session.Observe("crack"), Is.True);
            Assert.That(session.Apply("cloth", "crack").Kind, Is.EqualTo(WorkbenchOutcomeKind.Applied));
            Assert.That(session.Apply("handle", "drawer").Kind, Is.EqualTo(WorkbenchOutcomeKind.Completed));
        }

        [Test]
        public void ExposedCollections_CannotBeEditedOrChangePastSnapshots()
        {
            var puzzle = IcePuzzle();
            var session = new WorkbenchSession(puzzle);
            Assert.Throws<NotSupportedException>(() => ((IList<string>)puzzle.TargetIds)[0] = "missing");
            Assert.Throws<NotSupportedException>(() => ((IList<WorkbenchStep>)puzzle.Steps)[0] = null);
            Assert.Throws<NotSupportedException>(() => ((IList<string>)puzzle.Steps[0].RequiredObservations)[0] = "missing");
            Assert.Throws<NotSupportedException>(() => ((IList<string>)puzzle.Steps[1].RequiredSteps)[0] = "missing");

            var beforeObservation = session.ObservedTargets;
            session.Observe("crack");
            var observed = session.ObservedTargets;
            var beforeApplication = session.CompletedSteps;
            session.Apply("cloth", "crack");
            var completed = session.CompletedSteps;
            Assert.That(beforeObservation, Is.Empty);
            Assert.That(beforeApplication, Is.Empty);
            Assert.Throws<NotSupportedException>(() => ((ICollection<string>)observed).Add("drawer"));
            Assert.Throws<NotSupportedException>(() => ((ICollection<string>)completed).Add("open-drawer"));
            Assert.That(session.HasObserved("drawer"), Is.False);
            Assert.That(session.HasCompleted("open-drawer"), Is.False);
            session.Observe("drawer");
            session.Apply("handle", "drawer");
            Assert.That(observed, Is.EquivalentTo(new[] { "crack" }));
            Assert.That(completed, Is.EquivalentTo(new[] { "seal-cold" }));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Definitions_RejectBlankIdentityFields(string invalid)
        {
            Assert.Throws<ArgumentException>(() => new WorkbenchStep(invalid, "cloth", "crack"));
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", invalid, "crack"));
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", "cloth", invalid));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle(invalid, new[] { "crack" },
                new[] { new WorkbenchStep("seal", "cloth", "crack") }));
        }

        [Test]
        public void Definitions_RejectBlankOrDuplicatePrerequisites()
        {
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", "cloth", "crack", new[] { " " }));
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", "cloth", "crack", new[] { "crack", "crack" }));
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", "cloth", "crack", requiredSteps: new string[] { null }));
            Assert.Throws<ArgumentException>(() => new WorkbenchStep("seal", "cloth", "crack", requiredSteps: new[] { "dry", "dry" }));
        }

        [Test]
        public void Puzzle_RejectsMissingOrDuplicateTargets()
        {
            var steps = new[] { new WorkbenchStep("seal", "cloth", "crack") };
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", null, steps));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", Array.Empty<string>(), steps));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack", "crack" }, steps));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack", " " }, steps));
        }

        [Test]
        public void Puzzle_RejectsMissingOrDuplicateSteps()
        {
            var step = new WorkbenchStep("seal", "cloth", "crack");
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, null));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, Array.Empty<WorkbenchStep>()));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new WorkbenchStep[] { null }));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                step, new WorkbenchStep("seal", "other-tool", "crack")
            }));
        }

        [Test]
        public void Puzzle_RejectsAmbiguousToolAndTargetPairs()
        {
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("first", "cloth", "crack"),
                new WorkbenchStep("second", "cloth", "crack", requiredSteps: new[] { "first" })
            }));
        }

        [Test]
        public void Puzzle_RejectsUnknownTargetsAndPrerequisites()
        {
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("seal", "cloth", "missing")
            }));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("seal", "cloth", "crack", new[] { "missing" })
            }));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("seal", "cloth", "crack", requiredSteps: new[] { "missing" })
            }));
        }

        [Test]
        public void Puzzle_RejectsSelfDependenciesAndDisconnectedCycles()
        {
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("seal", "cloth", "crack", requiredSteps: new[] { "seal" })
            }));
            Assert.Throws<ArgumentException>(() => new WorkbenchPuzzle("ice", new[] { "crack" }, new[]
            {
                new WorkbenchStep("start", "cloth", "crack"),
                new WorkbenchStep("cycle-a", "brush", "crack", requiredSteps: new[] { "cycle-b" }),
                new WorkbenchStep("cycle-b", "key", "crack", requiredSteps: new[] { "cycle-a" })
            }));
        }

        [Test]
        public void Session_RejectsMissingPuzzle()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkbenchSession(null));
        }

        private static WorkbenchPuzzle IcePuzzle()
            => new WorkbenchPuzzle("ice", new[] { "crack", "drawer" }, new[]
            {
                new WorkbenchStep("seal-cold", "cloth", "crack", new[] { "crack" }),
                new WorkbenchStep("open-drawer", "handle", "drawer", new[] { "drawer" }, new[] { "seal-cold" })
            });
    }
}

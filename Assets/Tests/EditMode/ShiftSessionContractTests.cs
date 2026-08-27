using System;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ShiftSessionContractTests
    {
        [Test]
        public void DuplicateCorrectDestination_IsBlockedWithoutAdvancingOrChargingAHeart()
        {
            var session = CreateReferenceSession();
            session.Sort(Destination.Vault);
            var before = session.CurrentArtifact.Id;

            var outcome = session.Sort(Destination.Vault);

            Assert.That(outcome.Disposition, Is.EqualTo(SortDisposition.Blocked));
            Assert.That(session.CurrentArtifact.Id, Is.EqualTo(before));
            Assert.That(session.Hearts, Is.EqualTo(3));
            Assert.That(session.CurrentDocket.StampCount, Is.EqualTo(1));
            Assert.That(session.CanSort(Destination.Vault), Is.False);
            Assert.That(session.ShouldSuggestHold, Is.True);
        }

        [Test]
        public void WrongSort_LosesAHeartButKeepsTheArtifactAndDocket()
        {
            var session = CreateReferenceSession();
            var before = session.CurrentArtifact.Id;

            var outcome = session.Sort(Destination.Storage);

            Assert.That(outcome.Disposition, Is.EqualTo(SortDisposition.Wrong));
            Assert.That(session.CurrentArtifact.Id, Is.EqualTo(before));
            Assert.That(session.Hearts, Is.EqualTo(2));
            Assert.That(session.Mistakes, Is.EqualTo(1));
            Assert.That(session.CurrentDocket.IsPristine, Is.False);
            Assert.That(session.CurrentDocket.StampCount, Is.Zero);
        }

        [Test]
        public void ReferenceFlow_CompletesFourPristineDocketsWithTwoHolds()
        {
            var session = CreateReferenceSession();

            session.Sort(Destination.Vault);
            Assert.That(session.Hold(), Is.True);
            session.Sort(Destination.Repair);
            session.Sort(Destination.Storage);
            session.Sort(Destination.Repair);
            session.Sort(Destination.Storage);
            session.Sort(Destination.Vault);
            session.Sort(Destination.Storage);
            Assert.That(session.Hold(), Is.True);
            session.Sort(Destination.Vault);
            session.Sort(Destination.Repair);
            session.Sort(Destination.Vault);
            session.Sort(Destination.Repair);
            session.Sort(Destination.Storage);

            Assert.That(session.State, Is.EqualTo(ShiftState.Completed));
            Assert.That(session.CompletedDockets, Is.EqualTo(4));
            Assert.That(session.CorrectSorts, Is.EqualTo(12));
            Assert.That(session.Score, Is.EqualTo(2800));
            Assert.That(session.Coins, Is.EqualTo(100));
            Assert.That(session.CompletedDocketPristine,
                Is.EqualTo(new[] { true, true, true, true }));
        }

        [Test]
        public void Hold_CannotRepeatUntilSortAndPreviewIncludesHeldTail()
        {
            var session = CreateSession("VRS");

            Assert.That(session.CurrentArtifact.Id, Is.EqualTo("artifact-0"));
            Assert.That(session.PeekNextArtifact(0).Id, Is.EqualTo("artifact-1"));
            Assert.That(session.PeekNextArtifact(1).Id, Is.EqualTo("artifact-2"));
            Assert.That(session.Hold(), Is.True);
            Assert.That(session.CurrentArtifact.Id, Is.EqualTo("artifact-1"));
            Assert.That(session.HeldArtifact.Id, Is.EqualTo("artifact-0"));
            Assert.That(session.PeekNextArtifact(0).Id, Is.EqualTo("artifact-2"));
            Assert.That(session.Hold(), Is.False);

            session.Sort(Destination.Repair);
            Assert.That(session.CurrentArtifact.Id, Is.EqualTo("artifact-2"));
            Assert.That(session.PeekNextArtifact(0).Id, Is.EqualTo("artifact-0"));
            session.Sort(Destination.Storage);
            Assert.That(session.CurrentArtifact.Id, Is.EqualTo("artifact-0"));
            Assert.That(session.PeekNextArtifact(0), Is.Null);
            session.Sort(Destination.Vault);

            Assert.That(session.State, Is.EqualTo(ShiftState.Completed));
        }

        [Test]
        public void ThreeMistakes_FailWithoutAdvancingAndOnlyOneReviveCanBeClaimed()
        {
            var session = CreateSession("VRS");

            session.Sort(Destination.Storage);
            session.Sort(Destination.Storage);
            session.Sort(Destination.Storage);

            Assert.That(session.CurrentArtifact.Id, Is.EqualTo("artifact-0"));
            Assert.That(session.Hearts, Is.Zero);
            Assert.That(session.State, Is.EqualTo(ShiftState.Failed));
            Assert.That(session.TryRevive(), Is.True);
            Assert.That(session.Hearts, Is.EqualTo(1));
            Assert.That(session.State, Is.EqualTo(ShiftState.Active));
            Assert.That(session.TryRevive(), Is.False);
            Assert.That(session.RewardClaimed, Is.True);
        }

        [Test]
        public void CompletedDocketShift_CanDoubleCoinsOnlyOnce()
        {
            var session = CreateSession("VRS");
            session.Sort(Destination.Vault);
            session.Sort(Destination.Repair);
            session.Sort(Destination.Storage);

            Assert.That(session.State, Is.EqualTo(ShiftState.Completed));
            Assert.That(session.Coins, Is.EqualTo(40));
            Assert.That(session.TryDoubleCoins(), Is.True);
            Assert.That(session.Coins, Is.EqualTo(80));
            Assert.That(session.TryDoubleCoins(), Is.False);
            Assert.That(session.RewardClaimed, Is.True);
        }

        [Test]
        public void Constructor_RejectsQueueThatCannotFormCompleteDockets()
        {
            var artifacts = new[]
            {
                new Artifact("one", ArtifactTraits.Cursed),
                new Artifact("two", ArtifactTraits.Fragile)
            };

            Assert.Throws<ArgumentException>(() => new ShiftSession(artifacts, CreateRules()));
        }

        private static ShiftSession CreateReferenceSession()
            => CreateSession("VVRSRSVSSRVR");

        private static ShiftSession CreateSession(string destinations)
        {
            var artifacts = new Artifact[destinations.Length];
            for (var index = 0; index < destinations.Length; index++)
            {
                artifacts[index] = new Artifact($"artifact-{index}", TraitFor(destinations[index]));
            }

            return new ShiftSession(artifacts, CreateRules());
        }

        private static SortingRule[] CreateRules()
        {
            return new[]
            {
                new SortingRule("cursed-vault", ArtifactTraits.Cursed, ArtifactTraits.None,
                    Destination.Vault, false),
                new SortingRule("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None,
                    Destination.Repair, false),
                new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                    Destination.Storage, true)
            };
        }

        private static ArtifactTraits TraitFor(char destination)
        {
            switch (destination)
            {
                case 'V': return ArtifactTraits.Cursed;
                case 'R': return ArtifactTraits.Fragile;
                case 'S': return ArtifactTraits.Metallic;
                default: throw new ArgumentOutOfRangeException(nameof(destination));
            }
        }
    }
}

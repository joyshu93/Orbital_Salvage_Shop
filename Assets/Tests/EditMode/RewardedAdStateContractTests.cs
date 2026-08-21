using System;
using System.Collections.Generic;
using CurioClerk.Infrastructure.Ads;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class RewardedAdStateContractTests
    {
        [TestCase(true, false, RewardedAdResult.Earned)]
        [TestCase(false, true, RewardedAdResult.Failed)]
        public void RewardedCallback_CompletesExactlyOnceWhenSdkSendsDuplicateTerminalCallbacks(
            bool rewardFirst,
            bool failureSecond,
            RewardedAdResult expected)
        {
            var fake = new FakeRewardedClient();
            var service = new RewardedAdService(fake, "test-unit");
            service.SetRequestPermission(true);
            var results = new List<RewardedAdResult>();

            service.ShowRewarded("shift_complete_double", results.Add);
            fake.Emit(rewardFirst ? RewardedAdResult.Earned : RewardedAdResult.Failed);
            if (failureSecond)
            {
                fake.Emit(RewardedAdResult.Failed);
            }
            else
            {
                fake.Emit(RewardedAdResult.Dismissed);
            }

            Assert.That(results, Is.EqualTo(new[] { expected }));
        }

        [Test]
        public void PermissionFalse_NeverLoadsAndReturnsUnavailable()
        {
            var fake = new FakeRewardedClient();
            var service = new RewardedAdService(fake, "test-unit");
            var results = new List<RewardedAdResult>();

            service.SetRequestPermission(false);
            service.ShowRewarded("shift_complete_double", results.Add);

            Assert.That(fake.LoadRequests, Is.Zero);
            Assert.That(fake.ShowRequests, Is.Zero);
            Assert.That(results, Is.EqualTo(new[] { RewardedAdResult.Unavailable }));
        }

        [Test]
        public void InvalidPlacement_ReturnsUnavailableWithoutShowing()
        {
            var fake = new FakeRewardedClient();
            var service = new RewardedAdService(fake, "test-unit");
            var results = new List<RewardedAdResult>();
            service.SetRequestPermission(true);

            service.ShowRewarded("unapproved_placement", results.Add);

            Assert.That(fake.ShowRequests, Is.Zero);
            Assert.That(results, Is.EqualTo(new[] { RewardedAdResult.Unavailable }));
        }

        [Test]
        public void CloseWithoutEarnedReward_ReturnsDismissed()
        {
            var fake = new FakeRewardedClient();
            var service = ReadyService(fake);
            var results = new List<RewardedAdResult>();

            service.ShowRewarded("shift_failed_revive", results.Add);
            fake.Emit(RewardedAdResult.Dismissed);

            Assert.That(results, Is.EqualTo(new[] { RewardedAdResult.Dismissed }));
        }

        [Test]
        public void LoadOrShowFailure_ReturnsFailed()
        {
            var fake = new FakeRewardedClient();
            var service = ReadyService(fake);
            var results = new List<RewardedAdResult>();

            service.ShowRewarded("shift_complete_double", results.Add);
            fake.Emit(RewardedAdResult.Failed);

            Assert.That(results, Is.EqualTo(new[] { RewardedAdResult.Failed }));
        }

        [Test]
        public void EarnedReward_LoadsReplacementWhenPermissionRemainsTrue()
        {
            var fake = new FakeRewardedClient();
            var service = ReadyService(fake);

            service.ShowRewarded("shift_complete_double", _ => { });
            fake.Emit(RewardedAdResult.Earned);

            Assert.That(fake.LoadRequests, Is.EqualTo(2));
            Assert.That(service.IsRewardedReady, Is.True);
        }

        [TestCase("shift_failed_revive")]
        [TestCase("shift_complete_double")]
        public void ApprovedPlacement_IsAccepted(string placement)
        {
            var fake = new FakeRewardedClient();
            var service = ReadyService(fake);

            service.ShowRewarded(placement, _ => { });

            Assert.That(fake.ShowRequests, Is.EqualTo(1));
        }

        private static RewardedAdService ReadyService(FakeRewardedClient fake)
        {
            var service = new RewardedAdService(fake, "test-unit");
            service.SetRequestPermission(true);
            return service;
        }

        private sealed class FakeRewardedClient : IRewardedAdClient
        {
            private Action<RewardedAdResult> _completed;

            public bool IsReady { get; private set; }

            public int LoadRequests { get; private set; }

            public int ShowRequests { get; private set; }

            public void SetRequestPermission(bool allowed)
            {
                IsReady = allowed;
                if (allowed)
                {
                    LoadRequests++;
                }
            }

            public void Show(Action<RewardedAdResult> completed)
            {
                ShowRequests++;
                IsReady = false;
                _completed = completed;
            }

            public void Emit(RewardedAdResult result)
            {
                _completed?.Invoke(result);
            }
        }
    }
}

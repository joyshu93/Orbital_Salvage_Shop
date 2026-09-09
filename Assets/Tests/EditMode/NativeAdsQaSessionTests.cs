using System;
using System.Reflection;
using CurioClerk.Infrastructure.Ads;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class NativeAdsQaSessionTests
    {
        private object Create(DeferredAd ad)
        {
            var type = typeof(RewardedAdService).Assembly.GetType("CurioClerk.Qa.NativeAdsQaSession");
            Assert.That(type, Is.Not.Null, "QA reward session must exist without any save-store dependency.");
            return Activator.CreateInstance(type, ad);
        }

        private static object Call(object target, string name, params object[] args)
            => target.GetType().GetMethod(name).Invoke(target, args);
        private static T Value<T>(object target, string name)
            => (T)target.GetType().GetProperty(name).GetValue(target);

        [Test]
        public void Earned_DoublesRealShiftCoinsOnceAndRejectsAnotherShow()
        {
            var ad = new DeferredAd();
            var qa = Create(ad);
            Call(qa, "Begin", false);
            Assert.That(Value<int>(qa, "Coins"), Is.EqualTo(40));
            Call(qa, "SetPermission", true);
            Call(qa, "Show");
            ad.Callback(RewardedAdResult.Earned);
            ad.Callback(RewardedAdResult.Earned);
            Call(qa, "Show");
            Assert.That(Value<int>(qa, "Coins"), Is.EqualTo(80));
            Assert.That(Value<int>(qa, "Awards"), Is.EqualTo(1));
            Assert.That(ad.Shows, Is.EqualTo(1));
        }

        [TestCase(RewardedAdResult.Dismissed)]
        [TestCase(RewardedAdResult.Failed)]
        [TestCase(RewardedAdResult.Unavailable)]
        public void NonEarned_LeavesCoinsAndRewardClaimUnchanged(RewardedAdResult result)
        {
            var ad = new DeferredAd();
            var qa = Create(ad);
            Call(qa, "Begin", false);
            Call(qa, "SetPermission", true);
            Call(qa, "Show");
            ad.Callback(result);
            Assert.That(Value<int>(qa, "Coins"), Is.EqualTo(40));
            Assert.That(Value<int>(qa, "Awards"), Is.Zero);
            Assert.That(Value<bool>(qa, "Pending"), Is.False);
        }

        [Test]
        public void Pending_RejectsConcurrentShowAndSessionReplacement()
        {
            var ad = new DeferredAd();
            var qa = Create(ad);
            Call(qa, "Begin", false);
            Call(qa, "SetPermission", true);
            Call(qa, "Show");
            Call(qa, "Show");
            Assert.That(Call(qa, "Begin", true), Is.EqualTo(false));
            Assert.That(ad.Shows, Is.EqualTo(1));
            Assert.That(Value<int>(qa, "Coins"), Is.EqualTo(40));
        }

        [Test]
        public void WithdrawPermission_RejectsLateEarnedAndFutureShows()
        {
            var ad = new DeferredAd();
            var qa = Create(ad);
            Call(qa, "Begin", false);
            Call(qa, "Show");
            Assert.That(ad.Shows, Is.Zero);
            Call(qa, "SetPermission", true);
            Call(qa, "Show");
            var late = ad.Callback;
            Call(qa, "SetPermission", false);
            late(RewardedAdResult.Earned);
            Call(qa, "Show");
            Assert.That(Value<int>(qa, "Coins"), Is.EqualTo(40));
            Assert.That(Value<int>(qa, "Awards"), Is.Zero);
            Assert.That(ad.Shows, Is.EqualTo(1));
        }

        [Test]
        public void PreviousSessionCallback_CannotRewardNewReviveSession()
        {
            var ad = new DeferredAd();
            var qa = Create(ad);
            Call(qa, "Begin", false);
            Call(qa, "SetPermission", true);
            Call(qa, "Show");
            var old = ad.Callback;
            old(RewardedAdResult.Dismissed);
            Call(qa, "Begin", true);
            Call(qa, "Show");
            old(RewardedAdResult.Earned);
            Assert.That(Value<int>(qa, "Hearts"), Is.Zero);
            ad.Callback(RewardedAdResult.Earned);
            ad.Callback(RewardedAdResult.Earned);
            Assert.That(Value<int>(qa, "Hearts"), Is.EqualTo(1));
            Assert.That(Value<int>(qa, "Awards"), Is.EqualTo(1));
        }

        private sealed class DeferredAd : IAdService
        {
            public bool IsRewardedReady => true;
            public int Shows;
            public Action<RewardedAdResult> Callback;
            public void SetRequestPermission(bool allowed) { }
            public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
            {
                Assert.That(placement, Is.EqualTo("shift_complete_double").Or.EqualTo("shift_failed_revive"));
                Shows++;
                Callback = completed;
            }
        }
    }
}

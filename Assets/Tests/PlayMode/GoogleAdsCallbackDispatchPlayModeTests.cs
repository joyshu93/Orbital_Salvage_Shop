#if UNITY_EDITOR || (UNITY_ANDROID && !CURIO_OFFLINE_QA)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CurioClerk.Infrastructure.Ads;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class GoogleAdsCallbackDispatchPlayModeTests
    {
        [UnityTest]
        public IEnumerator ConsentCallback_DispatchesBeforeAnyNativeAdsInitialization()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var ads = Type.GetType("GoogleMobileAds.Api.MobileAds, GoogleMobileAds", true);
            var executor = Type.GetType("GoogleMobileAds.Common.MobileAdsEventExecutor, GoogleMobileAds.Common", true);
            var dispatchFlag = ads.GetProperty("RaiseAdEventsOnUnityMainThread", flags);
            var sdkInstance = ads.GetField("instance", flags);
            var queue = executor.GetField("adEventsQueue", flags);
            var empty = executor.GetField("adEventsQueueEmpty", flags);
            var threadId = executor.GetField("UnityMainThreadId", flags);
            var executorInstance = executor.GetField("instance", flags);
            var oldFlag = dispatchFlag.GetValue(null);
            var oldSdkInstance = sdkInstance.GetValue(null);
            var oldQueue = queue.GetValue(null);
            var oldEmpty = empty.GetValue(null);
            var oldThread = threadId.GetValue(null);
            var hadExecutor = (bool)executor.GetMethod("IsActive", flags).Invoke(null, null);
            void DestroyExecutors()
            {
                foreach (var obj in Resources.FindObjectsOfTypeAll(executor))
                    if (obj is Component component) UnityEngine.Object.DestroyImmediate(component.gameObject);
                executorInstance.SetValue(null, null);
            }
            try
            {
                DestroyExecutors();
                queue.SetValue(null, new List<Action>());
                empty.SetValue(null, true);
                threadId.SetValue(null, -1);
                var helper = typeof(RewardedAdService).Assembly.GetType("CurioClerk.Infrastructure.Ads.GoogleAdsCallbackDispatcher", true);
                var initialize = helper.GetMethod("Initialize", flags);
                initialize.Invoke(null, null);
                var expectedThread = Thread.CurrentThread.ManagedThreadId;
                var callbackThread = -1;
                var callbacks = 0;
                Exception workerFailure = null;
                Action callback = () =>
                {
                    callbackThread = Thread.CurrentThread.ManagedThreadId;
                    Interlocked.Increment(ref callbacks);
                };
                var raise = ads.GetMethod("RaiseAction", flags);
                var worker = new Thread(() =>
                {
                    try { raise.Invoke(null, new object[] { callback }); }
                    catch (Exception exception) { workerFailure = exception; }
                });
                worker.Start();
                Assert.That(worker.Join(2000), Is.True, "SDK RaiseAction must return after queueing.");
                Assert.That(workerFailure, Is.Null);
                for (var frame = 0; frame < 10 && Volatile.Read(ref callbacks) == 0; frame++) yield return null;
                Assert.That(callbacks, Is.EqualTo(1), "Actual SDK callback queue must drain before MobileAds.Initialize.");
                Assert.That(callbackThread, Is.EqualTo(expectedThread));
                Assert.That(ReferenceEquals(sdkInstance.GetValue(null), oldSdkInstance), Is.True, "Dispatcher must not initialize ads.");
                var firstExecutor = executorInstance.GetValue(null);
                initialize.Invoke(null, null);
                Assert.That(executorInstance.GetValue(null), Is.SameAs(firstExecutor));
                yield return null;
                Assert.That(callbacks, Is.EqualTo(1));
            }
            finally
            {
                DestroyExecutors();
                queue.SetValue(null, oldQueue);
                empty.SetValue(null, oldEmpty);
                threadId.SetValue(null, oldThread);
                dispatchFlag.SetValue(null, oldFlag);
                if (hadExecutor) executor.GetMethod("Initialize", flags).Invoke(null, null);
            }
        }
    }
}
#endif

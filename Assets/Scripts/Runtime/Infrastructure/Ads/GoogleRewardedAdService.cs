#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using GoogleMobileAds.Api;

namespace CurioClerk.Infrastructure.Ads
{
    public sealed class GoogleRewardedAdService : IAdService
    {
        private readonly RewardedAdService _service;

        public GoogleRewardedAdService(string rewardedAdUnitId)
        {
            ConfigureMainThreadCallbacks();
            _service = new RewardedAdService(
                new GoogleRewardedAdClient(rewardedAdUnitId),
                rewardedAdUnitId);
        }

        public bool IsRewardedReady => _service.IsRewardedReady;

        public void SetRequestPermission(bool allowed)
        {
            _service.SetRequestPermission(allowed);
        }

        public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
        {
            _service.ShowRewarded(placement, completed);
        }

        private static void ConfigureMainThreadCallbacks()
        {
            // Pinned GMA 11.3.0 routes RaiseAction callbacks through Unity's update executor when true.
#pragma warning disable 0618
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore 0618
        }

        private sealed class GoogleRewardedAdClient : IRewardedAdClient
        {
            private readonly string _rewardedAdUnitId;
            private RewardedAd _rewardedAd;
            private Action<RewardedAdResult> _activeRequest;
            private bool _requestAllowed;
            private bool _initialized;
            private bool _initializationPending;
            private bool _loadPending;
            private bool _loadFailed;
            private bool _rewardEarned;
            private int _loadGeneration;

            public GoogleRewardedAdClient(string rewardedAdUnitId)
            {
                _rewardedAdUnitId = rewardedAdUnitId;
            }

            public bool IsReady =>
                _requestAllowed &&
                _rewardedAd != null &&
                _rewardedAd.CanShowAd();

            public bool ConsumeLoadFailure()
            {
                var failed = _loadFailed;
                _loadFailed = false;
                return failed;
            }

            public void SetRequestPermission(bool allowed)
            {
                _requestAllowed = allowed;
                if (!allowed)
                {
                    _loadGeneration++;
                    _loadPending = false;
                    _loadFailed = false;
                    _activeRequest = null;
                    DestroyLoadedAd();
                    return;
                }

                if (_initialized)
                {
                    LoadIfNeeded();
                    return;
                }

                if (_initializationPending)
                {
                    return;
                }

                _initializationPending = true;
                MobileAds.Initialize(_ =>
                {
                    _initializationPending = false;
                    _initialized = true;
                    LoadIfNeeded();
                });
            }

            public void Show(Action<RewardedAdResult> completed)
            {
                if (!IsReady)
                {
                    completed?.Invoke(RewardedAdResult.Unavailable);
                    return;
                }

                var ad = _rewardedAd;
                _activeRequest = completed;
                _rewardEarned = false;
                ad.OnAdFullScreenContentClosed += () => Complete(
                    ad,
                    _rewardEarned ? RewardedAdResult.Earned : RewardedAdResult.Dismissed);
                ad.OnAdFullScreenContentFailed += _ => Complete(ad, RewardedAdResult.Failed);
                try
                {
                    ad.Show(_ =>
                    {
                        if (ReferenceEquals(_rewardedAd, ad) && _activeRequest != null)
                        {
                            _rewardEarned = true;
                        }
                    });
                }
                catch
                {
                    Complete(ad, RewardedAdResult.Failed);
                }
            }

            private void LoadIfNeeded()
            {
                if (!_requestAllowed ||
                    _activeRequest != null ||
                    _loadPending ||
                    _loadFailed ||
                    IsReady ||
                    string.IsNullOrWhiteSpace(_rewardedAdUnitId))
                {
                    return;
                }

                DestroyLoadedAd();
                _loadPending = true;
                var generation = ++_loadGeneration;
                try
                {
                    RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), (ad, error) =>
                    {
                        if (generation != _loadGeneration || !_requestAllowed)
                        {
                            ad?.Destroy();
                            return;
                        }

                        _loadPending = false;
                        _loadGeneration++;
                        if (error != null || ad == null)
                        {
                            _loadFailed = true;
                            ad?.Destroy();
                            return;
                        }

                        _loadFailed = false;
                        _rewardedAd = ad;
                    });
                }
                catch
                {
                    if (generation == _loadGeneration && _requestAllowed)
                    {
                        _loadPending = false;
                        _loadGeneration++;
                        _loadFailed = true;
                    }
                }
            }

            private void Complete(RewardedAd source, RewardedAdResult result)
            {
                if (!ReferenceEquals(_rewardedAd, source) || _activeRequest == null)
                {
                    return;
                }

                var completed = _activeRequest;
                _activeRequest = null;
                _rewardEarned = false;
                _rewardedAd = null;
                source.Destroy();
                completed(result);
            }

            private void DestroyLoadedAd()
            {
                var ad = _rewardedAd;
                _rewardedAd = null;
                ad?.Destroy();
            }
        }
    }
}
#endif

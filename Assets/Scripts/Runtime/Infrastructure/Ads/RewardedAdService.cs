using System;
using System.Collections.Generic;

namespace CurioClerk.Infrastructure.Ads
{
    public sealed class RewardedAdService : IAdService
    {
        private static readonly HashSet<string> AllowedPlacements = new HashSet<string>(StringComparer.Ordinal)
        {
            "shift_failed_revive",
            "shift_complete_double"
        };

        private readonly IRewardedAdClient _client;
        private readonly bool _isConfigured;
        private bool _requestAllowed;
        private bool _requestActive;
        private Action<RewardedAdResult> _activeRequest;

        public RewardedAdService(IRewardedAdClient client, string rewardedAdUnitId)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _isConfigured = !string.IsNullOrWhiteSpace(rewardedAdUnitId);
        }

        public bool IsRewardedReady =>
            _isConfigured &&
            _requestAllowed &&
            !_requestActive &&
            _client.IsReady;

        public void SetRequestPermission(bool allowed)
        {
            _requestAllowed = allowed && _isConfigured;
            if (_requestAllowed)
            {
                _client.SetRequestPermission(true);
                return;
            }

            var pending = TakeActiveRequest();
            _client.SetRequestPermission(false);
            pending?.Invoke(RewardedAdResult.Unavailable);
        }

        public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
        {
            if (!AllowedPlacements.Contains(placement) ||
                !_isConfigured ||
                !_requestAllowed ||
                _requestActive)
            {
                completed?.Invoke(RewardedAdResult.Unavailable);
                return;
            }

            if (_client.ConsumeLoadFailure())
            {
                try
                {
                    completed?.Invoke(RewardedAdResult.Failed);
                }
                finally
                {
                    if (_requestAllowed)
                    {
                        _client.SetRequestPermission(true);
                    }
                }

                return;
            }

            if (!_client.IsReady)
            {
                completed?.Invoke(RewardedAdResult.Unavailable);
                return;
            }

            _requestActive = true;
            _activeRequest = completed;
            try
            {
                _client.Show(CompleteActiveRequest);
            }
            catch
            {
                CompleteActiveRequest(RewardedAdResult.Failed);
            }
        }

        private void CompleteActiveRequest(RewardedAdResult result)
        {
            var completed = TakeActiveRequest();
            if (completed == null)
            {
                return;
            }

            try
            {
                completed(result);
            }
            finally
            {
                if (_requestAllowed)
                {
                    _client.SetRequestPermission(true);
                }
            }
        }

        private Action<RewardedAdResult> TakeActiveRequest()
        {
            if (!_requestActive)
            {
                return null;
            }

            var completed = _activeRequest;
            _requestActive = false;
            _activeRequest = null;
            return completed ?? IgnoreResult;
        }

        private static void IgnoreResult(RewardedAdResult _)
        {
        }
    }
}

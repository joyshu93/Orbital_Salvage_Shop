#if UNITY_EDITOR || (UNITY_ANDROID && DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA && !CURIO_OFFLINE_QA)
using System;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Infrastructure.Ads;

namespace CurioClerk.Qa
{
    // A real domain session, deliberately without a save/progression boundary.
    public sealed class NativeAdsQaSession
    {
        private readonly IAdService _ads;
        private ShiftSession _shift;
        private bool _allowed;
        private bool _revive;
        private long _version;
        public NativeAdsQaSession(IAdService ads) => _ads = ads ?? throw new ArgumentNullException(nameof(ads));
        public event Action<string> Recorded;
        public bool Pending { get; private set; }
        public int Awards { get; private set; }
        public int Completions { get; private set; }
        public int Coins => _shift?.Coins ?? 0;
        public int Hearts => _shift?.Hearts ?? 0;
        public bool Claimed => _shift?.RewardClaimed ?? false;
        public string LastResult { get; private set; } = "none";

        public bool Begin(bool revive)
        {
            if (Pending) return false;
            _version++;
            _revive = revive;
            _shift = new ShiftSession(new[] {
                new Artifact("qa-vault", ArtifactTraits.Cursed),
                new Artifact("qa-repair", ArtifactTraits.Fragile),
                new Artifact("qa-storage", ArtifactTraits.None)
            }, new[] {
                new SortingRule("qa-vault", ArtifactTraits.Cursed, ArtifactTraits.None, Destination.Vault, false),
                new SortingRule("qa-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair, false),
                new SortingRule("qa-storage", ArtifactTraits.None, ArtifactTraits.None, Destination.Storage, true)
            });
            if (revive)
            {
                for (var i = 0; i < 3; i++) _shift.Sort(Destination.Storage);
            }
            else
            {
                _shift.Sort(Destination.Vault);
                _shift.Sort(Destination.Repair);
                _shift.Sort(Destination.Storage);
            }
            Awards = Completions = 0;
            LastResult = "none";
            Recorded?.Invoke($"session begin revive={revive} coins={Coins} hearts={Hearts}");
            return true;
        }

        public void SetPermission(bool allowed)
        {
            _allowed = allowed;
            if (!allowed)
            {
                _version++;
                Pending = false;
            }
            _ads.SetRequestPermission(allowed);
            Recorded?.Invoke($"permission={allowed}");
        }

        public void Show()
        {
            if (!_allowed || Pending || _shift == null || Claimed)
            {
                Recorded?.Invoke($"show rejected allowed={_allowed} pending={Pending} claimed={Claimed}");
                return;
            }
            Pending = true;
            var version = ++_version;
            var shift = _shift;
            Recorded?.Invoke($"show request={version} coins={Coins} hearts={Hearts}");
            _ads.ShowRewarded(_revive ? "shift_failed_revive" : "shift_complete_double", result =>
            {
                if (version != _version || !Pending || !_allowed) return;
                Pending = false;
                Completions++;
                LastResult = result.ToString();
                if (result == RewardedAdResult.Earned && (_revive ? shift.TryRevive() : shift.TryDoubleCoins())) Awards++;
                Recorded?.Invoke($"result request={version} result={result} completions={Completions} awards={Awards} coins={Coins} hearts={Hearts}");
            });
        }
    }
}
#endif

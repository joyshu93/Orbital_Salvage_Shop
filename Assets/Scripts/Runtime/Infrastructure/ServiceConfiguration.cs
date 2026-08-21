using UnityEngine;

namespace CurioClerk.Infrastructure
{
    public sealed class ServiceConfiguration : ScriptableObject
    {
        [SerializeField] private string _androidRewardedAdUnitId;

        public string AndroidRewardedAdUnitId => _androidRewardedAdUnitId;
    }
}

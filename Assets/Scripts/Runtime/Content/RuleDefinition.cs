using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using UnityEngine;

namespace CurioClerk.Content
{
    [CreateAssetMenu(menuName = "Curio Clerk/Sorting Rule", fileName = "SortingRule")]
    public sealed class RuleDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private ArtifactTraits requiredAll;
        [SerializeField] private ArtifactTraits requiredAny;
        [SerializeField] private Destination destination;

        public string Id => id;

        public void Configure(SortingRule rule)
        {
            id = rule.Id;
            requiredAll = rule.RequiredAll;
            requiredAny = rule.RequiredAny;
            destination = rule.Destination;
        }

        public SortingRule ToRule() => new SortingRule(id, requiredAll, requiredAny, destination, false);
    }
}


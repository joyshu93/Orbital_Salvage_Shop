using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class CorePresenceTests
    {
        [Test]
        public void RuleEngine_IsAvailableFromTheCoreAssembly()
        {
            var type = System.Type.GetType("CurioClerk.Core.Rules.RuleEngine, CurioClerk.Core");

            Assert.That(type, Is.Not.Null, "The production rule engine has not been implemented yet.");
        }
    }
}

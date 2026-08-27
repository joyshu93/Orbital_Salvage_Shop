using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class DocketStateContractTests
    {
        [Test]
        public void ThreeUniqueStampsCompleteAPristineDocket()
        {
            var docket = new DocketState();

            Assert.That(docket.TryStamp(Destination.Vault), Is.True);
            Assert.That(docket.TryStamp(Destination.Vault), Is.False);
            Assert.That(docket.TryStamp(Destination.Repair), Is.True);
            Assert.That(docket.TryStamp(Destination.Storage), Is.True);

            Assert.That(docket.StampCount, Is.EqualTo(3));
            Assert.That(docket.IsComplete, Is.True);
            Assert.That(docket.IsPristine, Is.True);
        }

        [Test]
        public void MarkMistakeOnlyClearsPristineState()
        {
            var docket = new DocketState();
            docket.TryStamp(Destination.Storage);

            docket.MarkMistake();

            Assert.That(docket.IsPristine, Is.False);
            Assert.That(docket.IsStamped(Destination.Storage), Is.True);
            Assert.That(docket.StampCount, Is.EqualTo(1));
        }
    }
}

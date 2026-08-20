using System;
using System.Reflection;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ServiceBoundaryContractTests
    {
        [TestCase("CurioClerk.Infrastructure.Privacy.IPrivacyService")]
        [TestCase("CurioClerk.Infrastructure.Diagnostics.ICrashReporter")]
        public void RequiredConsentAwareBoundaryExists(string fullName)
        {
            Assert.That(FindType(fullName), Is.Not.Null, $"Missing service boundary: {fullName}");
        }

        [Test]
        public void ServiceFactory_CreatesAllReplaceableRuntimeServices()
        {
            var factory = FindType("CurioClerk.Infrastructure.ServiceFactory");
            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.GetMethod("CreateAdService", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(factory.GetMethod("CreateAnalyticsService", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(factory.GetMethod("CreatePrivacyService", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(factory.GetMethod("CreateCrashReporter", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}

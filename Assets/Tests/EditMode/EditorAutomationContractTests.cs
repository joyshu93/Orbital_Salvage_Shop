using System;
using System.Reflection;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class EditorAutomationContractTests
    {
        [Test]
        public void ProjectBuilder_ExposesBatchEntryPoints()
        {
            var type = FindType("CurioClerk.Editor.ProjectBuilder");

            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("BuildAll", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
            Assert.That(type.GetMethod("BuildAndroid", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        }

        [Test]
        public void ContentValidator_ExposesAThrowingBuildGate()
        {
            var type = FindType("CurioClerk.Editor.ContentValidator");

            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetMethod("ValidateOrThrow", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
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

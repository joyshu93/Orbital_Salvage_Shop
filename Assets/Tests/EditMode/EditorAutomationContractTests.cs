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

        [Test]
        public void ReleaseConfiguration_PinsGalaxyStoreVersionAndAndroidContract()
        {
            var type = FindType("CurioClerk.Editor.ReleaseConfiguration");
            Assert.That(type, Is.Not.Null);
            Assert.That(type.GetField("VersionName").GetRawConstantValue(), Is.EqualTo("1.0.0"));
            Assert.That(type.GetField("VersionCode").GetRawConstantValue(), Is.EqualTo(10000));
            Assert.That(type.GetField("PackageId").GetRawConstantValue(),
                Is.EqualTo("com.joyshu93.curioclerknightshift"));
            Assert.That(type.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
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

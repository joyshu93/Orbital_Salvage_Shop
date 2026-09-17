using System.Collections;
using CurioClerk.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class WorkbenchAtmosphereTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        [UnityTest]
        public IEnumerator Turn_MovesTheObjectThenDisablingRestoresItsAuthoredTransform()
        {
            var view = CreateView(out var artifact, out var frost, out var warmth);
            artifact.anchoredPosition = new Vector2(14, -9);
            artifact.localScale = new Vector3(.8f, .9f, 1);
            artifact.localRotation = Quaternion.Euler(0, 0, 4);
            var position = artifact.anchoredPosition;
            var scale = artifact.localScale;
            var rotation = artifact.localRotation;
            view.Configure(artifact, frost, warmth, false);
            view.Apply("turn", .5f, false);

            var deadline = Time.realtimeSinceStartup + 1;
            while (Quaternion.Angle(artifact.localRotation, rotation) < .1f && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(Quaternion.Angle(artifact.localRotation, rotation), Is.GreaterThan(.1f));
            view.enabled = false;
            Assert.That(Quaternion.Angle(artifact.localRotation, rotation), Is.LessThan(.001f));
            Assert.That(Vector2.Distance(artifact.anchoredPosition, position), Is.LessThan(.001f));
            Assert.That(Vector3.Distance(artifact.localScale, scale), Is.LessThan(.001f));
            view.enabled = true;
            yield return null;
            Assert.That(Quaternion.Angle(artifact.localRotation, rotation), Is.LessThan(.001f));
        }

        [UnityTest]
        public IEnumerator Repair_SettlesAfterItsShortMotionWithoutRequiringAnotherInput()
        {
            var view = CreateView(out var artifact, out var frost, out var warmth);
            var position = artifact.anchoredPosition;
            view.Configure(artifact, frost, warmth, false);
            view.Apply("repair", .4f, false);

            var deadline = Time.realtimeSinceStartup + 1;
            while (Vector2.Distance(artifact.anchoredPosition, position) < .1f && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(Vector2.Distance(artifact.anchoredPosition, position), Is.GreaterThan(.1f));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(Vector2.Distance(artifact.anchoredPosition, position), Is.LessThan(.001f));
            Assert.That(frost.color.a, Is.EqualTo(.27f).Within(.005f));
        }

        [UnityTest]
        public IEnumerator CompletingWhileDisabledSettlesVisualsWithoutReplayingTheReactionOnEnable()
        {
            var view = CreateView(out var artifact, out var frost, out var warmth);
            view.Configure(artifact, frost, warmth, true);
            view.enabled = false;
            view.Apply("reveal", .5f, true);

            Assert.That(frost.color.a, Is.Zero.Within(.001f));
            Assert.That(warmth.color.a, Is.GreaterThan(0));
            view.enabled = true;
            yield return null;
            Assert.That(frost.color.a, Is.Zero.Within(.001f));
            foreach (var image in artifact.GetComponentsInChildren<Image>())
                if (image.transform != artifact)
                    Assert.That(!image.isActiveAndEnabled || image.color.a < .001f, Is.True,
                        "A completed umbrella must remain dry after re-enabling the view.");
        }

        [UnityTest]
        public IEnumerator ReconfigureAndDestroyLeaveNoRainObjectsOrInputBlockingDecorations()
        {
            var view = CreateView(out var artifact, out var frost, out var warmth);
            view.Configure(artifact, frost, warmth, true);
            view.Configure(artifact, frost, warmth, true);
            yield return null;

            Assert.That(artifact.childCount, Is.EqualTo(1), "Reconfiguration must replace its owned rain group.");
            Assert.That(artifact.GetComponentsInChildren<Image>(true).Length, Is.GreaterThan(1));
            foreach (var image in artifact.GetComponentsInChildren<Image>(true))
                Assert.That(image.raycastTarget, Is.False);
            Assert.That(frost.raycastTarget, Is.False);
            Assert.That(warmth.raycastTarget, Is.False);

            Object.Destroy(view);
            yield return null;
            yield return null;
            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact.childCount, Is.Zero, "Destroying just the component must clean up its rain group.");
        }

        private WorkbenchAtmosphere CreateView(out RectTransform artifact, out Image frost, out Image warmth)
        {
            _host = new GameObject("AtmosphereTest", typeof(RectTransform));
            artifact = CreateImage("Artifact").rectTransform;
            artifact.sizeDelta = new Vector2(600, 600);
            frost = CreateImage("Frost");
            frost.color = new Color(.7f, .87f, 1, .45f);
            warmth = CreateImage("Warmth");
            warmth.color = new Color(1, .64f, .19f, 0);
            return _host.AddComponent<WorkbenchAtmosphere>();
        }

        private Image CreateImage(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<Image>();
        }
    }
}

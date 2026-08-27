using System.Collections;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class ShiftPresentationViewPlayModeTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }
        }

        [UnityTest]
        public IEnumerator DocketProgress_LabelsEmptyAndCompletedStamps()
        {
            var view = CreateDocketView(out var labels, out var surfaces);
            var docket = new DocketState();
            docket.TryStamp(Destination.Repair);

            view.Refresh(docket, 0, 4, "EMPTY", "COMPLETE");
            yield return null;

            Assert.That(labels[0].text, Is.EqualTo("COMPLETE"));
            Assert.That(labels[1].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[2].text, Is.EqualTo("EMPTY"));
            Assert.That(surfaces[0].color, Is.Not.EqualTo(surfaces[1].color));
        }

        [UnityTest]
        public IEnumerator DocketProgress_KeepsFinalCounterAtRequiredTotal()
        {
            var view = CreateDocketView(out var labels, out _);

            view.Refresh(null, 4, 4, "EMPTY", "COMPLETE");
            yield return null;

            Assert.That(_host.transform.Find("Counter").GetComponent<TMP_Text>().text, Is.EqualTo("4 / 4"));
            Assert.That(labels[0].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[1].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[2].text, Is.EqualTo("EMPTY"));
        }

        private DocketProgressView CreateDocketView(out TMP_Text[] labels, out Image[] surfaces)
        {
            _host = new GameObject("DocketProgressViewTestHost", typeof(RectTransform));
            var counter = CreateText("Counter");
            labels = new[]
            {
                CreateText("RepairStatus"),
                CreateText("StorageStatus"),
                CreateText("VaultStatus")
            };
            surfaces = new[]
            {
                CreateImage("RepairSurface"),
                CreateImage("StorageSurface"),
                CreateImage("VaultSurface")
            };

            var view = _host.AddComponent<DocketProgressView>();
            view.Configure(
                counter,
                surfaces,
                labels,
                new Color(0.15f, 0.12f, 0.14f, 1f),
                new[]
                {
                    new Color(0.71f, 0.43f, 0.47f, 1f),
                    new Color(0.44f, 0.54f, 0.42f, 1f),
                    new Color(0.88f, 0.64f, 0.29f, 1f)
                });
            return view;
        }

        private TMP_Text CreateText(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<TMP_Text>();
        }

        private Image CreateImage(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<Image>();
        }
    }
}

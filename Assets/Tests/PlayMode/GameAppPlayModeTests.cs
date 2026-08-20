using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class GameAppPlayModeTests
    {
        [UnityTest]
        public IEnumerator App_StartsAtMenuAndBuildsAPlayableShiftLayout()
        {
            var appType = Type.GetType("CurioClerk.Presentation.GameApp, CurioClerk.Runtime");
            Assert.That(appType, Is.Not.Null, "Missing production type: CurioClerk.Presentation.GameApp");
            var host = new GameObject("GameAppTestHost");
            var app = host.AddComponent(appType);
            yield return null;

            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Menu"));
            Assert.That(GameObject.Find("CurioClerkCanvas"), Is.Not.Null);
            Assert.That(GameObject.Find("StartShiftButton"), Is.Not.Null);
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            var titleText = GameObject.Find("Title").GetComponent(textType);
            var titleFont = textType.GetProperty("font").GetValue(titleText) as UnityEngine.Object;
            Assert.That(titleFont, Is.Not.Null);
            Assert.That(titleFont.name, Does.StartWith("NotoSansKR"));

            appType.GetMethod("StartNewShift").Invoke(app, new object[] { 4242 });
            yield return null;

            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Shift"));
            Assert.That(GameObject.Find("CurrentArtifactCard"), Is.Not.Null);
            Assert.That(GameObject.Find("NextPreview0"), Is.Not.Null);
            Assert.That(GameObject.Find("NextPreview1"), Is.Not.Null);
            Assert.That(GameObject.Find("HoldButton"), Is.Not.Null);
            Assert.That(GameObject.Find("RepairButton"), Is.Not.Null);
            Assert.That(GameObject.Find("StorageButton"), Is.Not.Null);
            Assert.That(GameObject.Find("VaultButton"), Is.Not.Null);
            var dragType = Type.GetType("CurioClerk.Presentation.ArtifactDragHandler, CurioClerk.Runtime");
            Assert.That(dragType, Is.Not.Null, "Missing card drag interaction type.");
            Assert.That(GameObject.Find("CurrentArtifactCard").GetComponent(dragType), Is.Not.Null,
                "The current artifact card must support drag-to-sort.");

            UnityEngine.Object.Destroy(host);
            yield return null;
        }
    }
}

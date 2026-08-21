using System;
using System.Collections;
using System.Reflection;
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

        [UnityTest]
        public IEnumerator CompletedShift_IsSavedBeforeLeavingResults_AndOnlyCorrectSortsAreDiscovered()
        {
            var appType = Type.GetType("CurioClerk.Presentation.GameApp, CurioClerk.Runtime");
            var ruleEngineType = Type.GetType("CurioClerk.Core.Rules.RuleEngine, CurioClerk.Core");
            var destinationType = Type.GetType("CurioClerk.Core.Rules.Destination, CurioClerk.Core");
            Assert.That(appType, Is.Not.Null);
            Assert.That(ruleEngineType, Is.Not.Null);
            Assert.That(destinationType, Is.Not.Null);

            var host = new GameObject("GameAppResultPersistenceTestHost");
            var app = host.AddComponent(appType);
            yield return null;

            var save = appType.GetProperty("SaveData").GetValue(app);
            var saveType = save.GetType();
            var completedField = saveType.GetField("completedShifts");
            var discovered = (IList)saveType.GetField("discoveredArtifactIds").GetValue(save);
            discovered.Clear();
            var completedBefore = (int)completedField.GetValue(save);

            appType.GetMethod("StartNewShift").Invoke(app, new object[] { 4242 });
            var sessionField = appType.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);
            var rulesField = appType.GetField("_activeRules", BindingFlags.Instance | BindingFlags.NonPublic);
            var choose = appType.GetMethod("ChooseDestination");
            var ruleEngine = Activator.CreateInstance(ruleEngineType);
            var resolve = ruleEngineType.GetMethod("Resolve");

            for (var index = 0; index < 12; index++)
            {
                var session = sessionField.GetValue(app);
                var artifact = session.GetType().GetProperty("CurrentArtifact").GetValue(session);
                var expected = resolve.Invoke(ruleEngine, new[] { artifact, rulesField.GetValue(app) });
                var selected = index == 0
                    ? Enum.ToObject(destinationType, ((int)expected + 1) % 3)
                    : expected;
                choose.Invoke(app, new[] { selected });
            }

            yield return null;
            Assert.That(appType.GetProperty("ActiveScreen").GetValue(app).ToString(), Is.EqualTo("Results"));
            Assert.That((int)completedField.GetValue(save), Is.EqualTo(completedBefore + 1),
                "A completed result must be durable before the player leaves the results screen.");
            Assert.That(discovered.Count, Is.EqualTo(11),
                "The incorrectly sorted artifact must remain undiscovered.");

            var coinsField = saveType.GetField("coins");
            var coinsBeforeReward = (int)coinsField.GetValue(save);
            var completedSession = sessionField.GetValue(app);
            var baseShiftCoins = (int)completedSession.GetType().GetProperty("Coins").GetValue(completedSession);
            appType.GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, new object[] { true });
            Assert.That((int)coinsField.GetValue(save), Is.EqualTo(coinsBeforeReward + baseShiftCoins),
                "A successful double-coins reward must persist only the bonus delta immediately.");

            UnityEngine.Object.Destroy(host);
            yield return null;
        }
    }
}

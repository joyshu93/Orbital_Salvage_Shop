using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ReleaseAssetContractTests
    {
        [Test]
        public void ReleaseAssets_ContainNoEmojiOneAndNoDefaultTmpSpriteAsset()
        {
            var emojiPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.IndexOf("EmojiOne", System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            Assert.That(emojiPaths, Is.Empty);

            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            Assert.That(settings, Is.Not.Null);
            var serialized = new SerializedObject(settings);
            Assert.That(serialized.FindProperty("m_defaultSpriteAsset").objectReferenceValue, Is.Null);
        }
    }
}

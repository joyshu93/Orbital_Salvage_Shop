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

        [Test]
        public void ReleaseAssets_ContainBodyAndDisplayTmpFonts()
        {
            var body = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Dynamic");
            var display = Resources.Load<TMP_FontAsset>("Fonts/GowunBatang-Bold-Dynamic");

            Assert.That(body, Is.Not.Null);
            Assert.That(display, Is.Not.Null);
            Assert.That(body.name, Does.StartWith("NotoSansKR"));
            Assert.That(display.name, Does.StartWith("GowunBatang-Bold"));
            Assert.That(body.sourceFontFile, Is.Not.Null, "The body font must retain its dynamic glyph source.");
            Assert.That(display.sourceFontFile, Is.Not.Null, "The display font must retain its dynamic glyph source.");
            Assert.That(body.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic));
            Assert.That(display.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic));
        }
    }
}

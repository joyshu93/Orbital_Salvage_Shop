using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Localization;
using NUnit.Framework;
using UnityEngine;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ContentCatalogContractTests
    {
        [Test]
        public void ArtifactCatalog_ContainsTwentyFourUniqueBilingualArtifactsWithOneToThreeTraits()
        {
            var catalog = RequireCatalog();
            var artifacts = Items(catalog.GetMethod("CreateArtifacts").Invoke(null, null));

            Assert.That(artifacts, Has.Count.EqualTo(24));
            Assert.That(artifacts.Select(item => String(item, "Id")).Distinct().Count(), Is.EqualTo(24));
            foreach (var artifact in artifacts)
            {
                Assert.That(String(artifact, "NameEnglish"), Is.Not.Empty);
                Assert.That(String(artifact, "NameKorean"), Is.Not.Empty);
                Assert.That(String(artifact, "DescriptionEnglish"), Is.Not.Empty);
                Assert.That(String(artifact, "DescriptionKorean"), Is.Not.Empty);
                Assert.That(String(artifact, "ResolutionEnglish"), Is.Not.Empty, String(artifact, "Id"));
                Assert.That(String(artifact, "ResolutionKorean"), Is.Not.Empty, String(artifact, "Id"));
                var bits = Convert.ToInt32(Property(artifact, "Traits"));
                var traitCount = 0;
                while (bits != 0)
                {
                    traitCount += bits & 1;
                    bits >>= 1;
                }

                Assert.That(traitCount, Is.InRange(1, 3), String(artifact, "Id"));
            }
        }

        [Test]
        public void ArtifactDefinition_ConfigureCopiesBilingualResolutionCopy()
        {
            var content = ContentCatalog.CreateArtifacts()[0];
            var definition = ScriptableObject.CreateInstance<ArtifactDefinition>();
            try
            {
                definition.Configure(content);
                var englishProperty = typeof(ArtifactDefinition).GetProperty("ResolutionEnglish");
                var koreanProperty = typeof(ArtifactDefinition).GetProperty("ResolutionKorean");

                Assert.That(englishProperty, Is.Not.Null);
                Assert.That(koreanProperty, Is.Not.Null);
                Assert.That(englishProperty.GetValue(definition),
                    Is.EqualTo(String(content, "ResolutionEnglish")));
                Assert.That(koreanProperty.GetValue(definition),
                    Is.EqualTo(String(content, "ResolutionKorean")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void RuleAndDocketCatalog_HasAuthoredBalancedGameplayStructures()
        {
            Assert.That(ContentCatalog.ContentVersion, Is.EqualTo(2));

            var ruleTemplates = ContentCatalog.CreateRuleTemplates();
            Assert.That(ruleTemplates.Count, Is.EqualTo(10));
            Assert.That(ruleTemplates.Select(rule => rule.Id).Distinct().Count(), Is.EqualTo(10));

            var artifacts = ContentCatalog.CreateArtifacts()
                .Select(content => content.ToArtifact())
                .ToArray();
            var packs = ContentCatalog.CreateRulePacks();
            Assert.That(packs.Select(pack => pack.Id),
                Is.EqualTo(new[] { "pack-cursed-fragile", "pack-temporal-wet" }));
            foreach (var pack in packs)
            {
                Assert.That(pack.Rules, Has.Count.EqualTo(3), pack.Id);
                Assert.That(pack.Rules.Take(2).All(rule => !rule.IsFallback), Is.True, pack.Id);
                Assert.That(pack.Rules[2].IsFallback, Is.True, pack.Id);
                Assert.That(pack.Rules[2].Destination, Is.EqualTo(Destination.Storage), pack.Id);

                var engine = new RuleEngine();
                var destinations = artifacts
                    .Select(artifact => engine.Resolve(artifact, pack.Rules))
                    .ToArray();
                Assert.That(destinations.Count(value => value == Destination.Repair),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
                Assert.That(destinations.Count(value => value == Destination.Storage),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
                Assert.That(destinations.Count(value => value == Destination.Vault),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
            }

            var templates = ContentCatalog.CreateShiftTemplates();
            Assert.That(templates.Select(template => template.Id),
                Is.EqualTo(new[] { "docket-band-1", "docket-band-2", "docket-band-3" }));
            Assert.That(templates.Select(template => Pattern(template.Destinations)),
                Is.EqualTo(new[]
                {
                    "RRSVRSVRSVSV",
                    "VVRSRSVSSRVR",
                    "RRSVSSVRRVSV"
                }));
            Assert.That(templates.Select(template => template.MinimumRequiredHolds),
                Is.EqualTo(new[] { 1, 2, 3 }));
            var analyzer = new DocketSequenceAnalyzer();
            foreach (var template in templates)
            {
                Assert.That(analyzer.MinimumHolds(template.Destinations),
                    Is.EqualTo(template.MinimumRequiredHolds), template.Id);
            }
        }

        [Test]
        public void CosmeticCatalog_HasSixProgressivelyPricedUnlocks()
        {
            var catalog = RequireCatalog();
            var cosmetics = Items(catalog.GetMethod("CreateCosmetics").Invoke(null, null));
            var costs = cosmetics.Select(item => Convert.ToInt32(Property(item, "Cost"))).ToArray();

            Assert.That(cosmetics, Has.Count.EqualTo(6));
            Assert.That(costs, Is.Ordered.Ascending);
            Assert.That(costs[0], Is.GreaterThan(0));
        }

        [Test]
        public void IncidentCatalog_ExposesTwoOrderedIncidentsWithSixTotalStages()
        {
            var incidents = ContentCatalog.CreateIncidents();

            Assert.That(incidents.Count, Is.EqualTo(2));
            Assert.That(incidents[0].Id, Is.EqualTo("unmelting-ice"));
            Assert.That(incidents[0].Stages.Select(stage => stage.Id), Is.EqualTo(new[]
            {
                "ice-01-crack",
                "ice-02-spread",
                "ice-03-tomorrow",
                "ice-04-frozen-seal",
                "ice-05-thaw"
            }));
            Assert.That(incidents[1].Id, Is.EqualTo("remembering-rain"));
            Assert.That(incidents[1].Stages.Select(stage => stage.Id), Is.EqualTo(new[] { "rain-01-voices" }));
            Assert.That(incidents.Sum(incident => incident.Stages.Count), Is.EqualTo(6));
            Assert.That(ContentCatalog.CreateArtifacts().All(artifact =>
                (artifact.Traits & CurioClerk.Core.Artifacts.ArtifactTraits.Frosted) == 0), Is.True,
                "Frosted is an incident-stage modifier and must not mutate base catalog traits.");
        }

        [Test]
        public void IncidentPresentationStyles_HaveOneDeterministicStylePerIncident()
        {
            var styles = ContentCatalog.CreateIncidentPresentationStyles();

            Assert.That(styles.Count, Is.EqualTo(2));
            Assert.That(styles.Select(style => style.IncidentId),
                Is.EqualTo(new[] { "unmelting-ice", "remembering-rain" }));
            Assert.That(styles.Select(style => style.AccentHex), Is.EqualTo(new[] { "D6A85F", "8094B8" }));
            Assert.That(styles.Select(style => style.SurfaceHex), Is.EqualTo(new[] { "6E334F", "343B57" }));
            Assert.That(styles.Select(style => style.TransitionStrength), Is.EqualTo(new[] { 0.85f, 1.00f }));
        }

        [Test]
        public void IncidentInterfaceCopy_IsCompleteBilingualAndKeepsMatchingPlaceholders()
        {
            var keys = new[]
            {
                "incident_begin", "incident_continue", "incident_stage", "incident_complete", "incident_replay",
                "incident_next_teaser", "free_shift", "senior_clerk", "narrative_continue",
                "retry_stage", "next_stage", "quality_stable", "quality_precise", "quality_resonant",
                "quality_stable_body", "quality_precise_body", "quality_resonant_body", "trait_frosted",
                "calm_streak", "incident_hold_protect", "incident_failed_body", "processing",
                "incident_first_investigation", "incident_in_progress", "incident_first_clue", "incident_waiting",
                "incident_resolved", "incident_replay_case", "incident_return_board"
            };
            var english = Localizer.Entries("en").ToDictionary(entry => entry.Key, entry => entry.Value);
            var korean = Localizer.Entries("ko").ToDictionary(entry => entry.Key, entry => entry.Value);

            foreach (var key in keys)
            {
                Assert.That(english.ContainsKey(key), Is.True, $"Missing English key: {key}");
                Assert.That(korean.ContainsKey(key), Is.True, $"Missing Korean key: {key}");
                Assert.That(english[key], Is.Not.Empty.And.Not.EqualTo(key), key);
                Assert.That(korean[key], Is.Not.Empty.And.Not.EqualTo(key), key);
                Assert.That(FormatPlaceholders(korean[key]), Is.EqualTo(FormatPlaceholders(english[key])), key);
            }

            Assert.That(korean["incident_begin"], Is.EqualTo("사건 시작"));
            Assert.That(english["incident_continue"], Is.EqualTo("Continue Investigation · {0}/{1}"));
            Assert.That(korean["incident_continue"], Is.EqualTo("조사 계속 · {0}/{1}"));
            Assert.That(english["incident_stage"], Is.EqualTo("Investigation {0}/{1}"));
            Assert.That(korean["incident_stage"], Is.EqualTo("조사 {0}/{1}"));
            Assert.That(korean["incident_complete"], Is.EqualTo("첫 사건 해결"));
            Assert.That(korean["incident_replay"], Is.EqualTo("사건 다시보기"));
            Assert.That(korean["incident_next_teaser"], Is.EqualTo("다음 사건 · 실내에서 비를 맞은 우산"));
            Assert.That(korean["free_shift"], Is.EqualTo("자유 교대"));
            Assert.That(korean["senior_clerk"], Is.EqualTo("선임 관리인"));
            Assert.That(korean["narrative_continue"], Is.EqualTo("계속"));
            Assert.That(korean["retry_stage"], Is.EqualTo("같은 교대 다시 하기"));
            Assert.That(korean["next_stage"], Is.EqualTo("다음 교대"));
            Assert.That(korean["quality_stable"], Is.EqualTo("안정"));
            Assert.That(korean["quality_precise"], Is.EqualTo("정교"));
            Assert.That(korean["quality_resonant"], Is.EqualTo("공명"));
            Assert.That(korean["trait_frosted"], Is.EqualTo("서리 묻음"));
            Assert.That(korean["calm_streak"], Is.EqualTo("손길이 안정되었습니다"));
            Assert.That(korean["incident_hold_protect"], Is.EqualTo("보호 보류"));
            Assert.That(english["incident_first_investigation"], Is.EqualTo("Begin First Investigation"));
            Assert.That(korean["incident_first_investigation"], Is.EqualTo("첫 조사 시작"));
            Assert.That(english["incident_in_progress"], Is.EqualTo("Investigation in progress"));
            Assert.That(korean["incident_in_progress"], Is.EqualTo("조사 진행 중"));
            Assert.That(english["incident_first_clue"], Is.EqualTo("FIRST CLUE"));
            Assert.That(korean["incident_first_clue"], Is.EqualTo("첫 단서"));
            Assert.That(english["incident_waiting"], Is.EqualTo("Next shift in preparation"));
            Assert.That(korean["incident_waiting"], Is.EqualTo("다음 교대 준비 중"));
            Assert.That(english["incident_resolved"], Is.EqualTo("CASE RESOLVED"));
            Assert.That(korean["incident_resolved"], Is.EqualTo("사건 해결"));
            Assert.That(english["incident_replay_case"], Is.EqualTo("Replay Case"));
            Assert.That(korean["incident_replay_case"], Is.EqualTo("사건 다시보기"));
            Assert.That(english["incident_return_board"], Is.EqualTo("Return to Incident Board"));
            Assert.That(korean["incident_return_board"], Is.EqualTo("사건 보드로 돌아가기"));
        }

        private static Type RequireCatalog()
        {
            var type = Type.GetType("CurioClerk.Content.ContentCatalog, CurioClerk.Runtime");
            Assert.That(type, Is.Not.Null, "Missing production type: CurioClerk.Content.ContentCatalog");
            return type;
        }

        private static List<object> Items(object enumerable)
        {
            var result = new List<object>();
            foreach (var item in (IEnumerable)enumerable)
            {
                result.Add(item);
            }

            return result;
        }

        private static object Property(object item, string name) => item.GetType().GetProperty(name).GetValue(item);

        private static string String(object item, string name) => (string)Property(item, name);

        private static string[] FormatPlaceholders(string value)
        {
            return System.Text.RegularExpressions.Regex.Matches(value, @"\{\d+\}")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(match => match.Value)
                .ToArray();
        }

        private static string Pattern(IEnumerable<Destination> destinations)
        {
            return string.Concat(destinations.Select(destination =>
            {
                switch (destination)
                {
                    case Destination.Repair: return "R";
                    case Destination.Storage: return "S";
                    case Destination.Vault: return "V";
                    default: throw new ArgumentOutOfRangeException(nameof(destination));
                }
            }));
        }
    }
}

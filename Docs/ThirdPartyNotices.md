# Third-party notices register

Emulator QA note (2026-09-08): validation reports, regression tests, the bilingual fallback-destination label correction, case-specific ending presentation using existing artwork/copy, and local screenshots/logs use the existing project and installed SDKs. No new third-party assets, packages, or licenses are introduced. Local evidence is not store media; existing SDK and art notices continue to apply.

Last reviewed: 2026-09-08 (KST)

This is the working inventory for externally sourced material. It is not yet the final in-app notice. Before an RC build, remove unused material, resolve every open item, and make required notices easily viewable in the distributed product or its accompanying materials.

## Included assets

| Component | Repository evidence | License/attribution | Current release decision |
| --- | --- | --- | --- |
| Noto Sans KR Variable | `Assets/Fonts/NotoSansKR/NotoSansKR-Variable.ttf` | SIL Open Font License 1.1. Copyright 2014–2021 Adobe; Reserved Font Name `Source`. Full text: `Assets/Fonts/NotoSansKR/OFL.txt`. | Approved for bundling only while the copyright notice and OFL text are retained and made human-readable with the distribution. Do not rename a modified version with a reserved name. |
| Gowun Batang Bold | `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf` | SIL Open Font License 1.1. Copyright 2021 The Gowun Batang Project Authors. Full text: `Assets/Fonts/GowunBatang/OFL.txt`. | Approved for prototype bundling without modification while the copyright notice and OFL text are retained and made human-readable with the distribution. |
| Liberation Sans | `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` and generated TMP font resources | SIL Open Font License 1.1. Copyright 2010 Google Corporation and 2012 Red Hat, Inc.; reserved names are stated in `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. | Repository copy is permitted under the bundled OFL terms. Confirm whether it is included in the final player and include its notice if shipped. |
| EmojiOne TMP sample | Removed from `Assets/TextMesh Pro/Sprites` and detached from `Assets/TextMesh Pro/Resources/TMP Settings.asset`. | The removed sample had incomplete provenance, so no EmojiOne material is approved or shipped. | **Removed from release.** `ContentValidator` and the EditMode release-asset contract reject any future `EmojiOne` asset. |
| TextMesh Pro essential resources and shaders | `Assets/TextMesh Pro/**`; source package: `com.unity.ugui` 2.0.0, pinned in `Packages/packages-lock.json`. | Unity Companion License. Verbatim package license: `Docs/Licenses/uGUI-2.0.0-LICENSE.md`; source and package fingerprint: `Docs/Licenses/uGUI-2.0.0-source.md`. Separately listed Liberation Sans notice remains applicable where shipped. | **Resolved for the recorded uGUI 2.0.0 essential resources and shaders.** Retain the license notice with distribution and re-audit if the package version or copied resources change. |
| Unity packages | `Packages/manifest.json` and `Packages/packages-lock.json` | Unity registry/built-in packages under their package metadata, Unity Companion License where applicable, and Unity terms. Direct versions include Input System 1.20.0, Localization 1.5.8, URP 17.3.0, Test Framework 1.6.0, uGUI 2.0.0, and Visual Studio Editor 2.0.26. | Version lock is the source of truth. Export and review package notices from the resolved RC environment; do not assume one license covers every transitive package. |
| Google Mobile Ads Unity plugin | Google-authored `com.google.ads.mobile` 11.3.0 in `Packages/manifest.json`; official Google release: https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.3.0; distributed through the community OpenUPM registry, which is not operated by Google. | Apache License 2.0; retain and review the package license from the human-resolved package cache before the RC notice audit. | **Pinned; Unity resolution pending.** Use only this UPM installation, and do not import an Asset-package copy. |
| External Dependency Manager for Unity | `GooglePackages/com.google.external-dependency-manager-1.2.188.tgz`; official archive: https://dl.google.com/games/registry/unity/com.google.external-dependency-manager/com.google.external-dependency-manager-1.2.188.tgz; SHA-256: `250B3AD3191C20E703799F612F642D8D734042A06E4CCAD636F926854101A482`. | Apache License 2.0 in archive path `package/LICENSE.md`; package identity and license must be rechecked after any archive replacement. | **Pinned and archive-verified.** Embedded `package/package.json` declares `com.google.external-dependency-manager` 1.2.188; Unity resolution remains pending. |
| bundletool 1.18.3 (development-only) | Official release: https://github.com/google/bundletool/releases/tag/1.18.3; ignored local jar SHA-256: `A099CFA1543F55593BC2ED16A70A7C67FE54B1747BB7301F37FDFD6D91028E29`; local instructions: `tools/bundletool/README.md`. | Apache License 2.0 in the official `google/bundletool` repository. | **Approved as a local validation tool only; not bundled in the game or committed to Git.** Recheck source, version, hash, and license before replacement. |
| Procedural interaction and incident feedback tones | Generated at runtime by repository-owned C# under `Assets/Scripts/Runtime/Infrastructure/Feedback`; provenance entries `AUDIO-SYNTH-001` and `AUDIO-SYNTH-002` in `Docs/AIAssetProvenance.md`. | No third-party audio or reference recording; Unity built-in audio APIs only. | **No third-party notice required.** Prototype listening, dialogue balance, and device-volume review remain open before RC. |
| Phase 3 bilingual settings copy | `Assets/Scripts/Runtime/Localization/Localizer.cs`; provenance entry `TEXT-UI-001` in `Docs/AIAssetProvenance.md`. | Project-authored functional English/Korean labels; no third-party prose or attribution identified. | **No third-party notice required.** Bilingual visual review remains open before RC. |
| Remaining curio catalog illustrations | Sixteen selected ImageGen PNGs recorded with hashes as `ART-CATALOG-001` in `Docs/AIAssetProvenance.md`; normalized prompts and correction history are in `Docs/VisualSlicePrompts.md`. | Project-owned reference sprites and original text prompts only; no third-party asset or reference was supplied. | **No third-party attribution identified. Prototype only; final similarity/trademark and human release review remain open.** Update this row if any third-party element is later introduced. |
| Desk charm illustration set | Six selected ImageGen PNGs recorded with hashes as `ART-COSMETICS-001` in `Docs/AIAssetProvenance.md`; normalized prompts are retained in `Docs/VisualSlicePrompts.md`. | Original text prompts only; no third-party asset or reference was supplied. | **No third-party attribution identified. Prototype only; final similarity/trademark and human release review remain open.** Update this row if any external element is introduced. |
| Casebook and cosmetics bilingual copy | `Assets/Scripts/Runtime/Localization/Localizer.cs`; provenance entry `TEXT-UI-002` in `Docs/AIAssetProvenance.md`. | Project-authored functional English/Korean labels; no third-party prose or attribution identified. | **No third-party notice required.** Bilingual visual review remains open before RC. |
| Three-Seal bilingual gameplay copy | `Assets/Scripts/Runtime/Localization/Localizer.cs`, `Assets/Scripts/Runtime/Content/ContentCatalog.cs`, generated localization tables, and generated Artifact assets; provenance entry `TEXT-GAMEPLAY-003` in `Docs/AIAssetProvenance.md`. | Existing project-authored names/descriptions and the approved Three-Seal design only; no third-party prose or attribution identified. | **No third-party notice required.** Human bilingual review remains required before RC. |
| Unmelting Ice five-stage bilingual narrative script | `Assets/Scripts/Runtime/Content/Incidents/FirstIncidentCatalog.cs`; provenance entry `TEXT-NARRATIVE-004` in `Docs/AIAssetProvenance.md`. | Developer-approved incident specification, fixed authored beats, repository artifact fiction, and the approved 2026-09-03 frost-trail, temporal-priority, protective-Hold, and mastery-finale revisions only; no third-party prose or reference text was used. | **No third-party notice required.** Developer bilingual and in-context review remains required before RC. |
| Multi-incident board and Remembering Rain five-stage bilingual narrative | `Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md`; `Docs/superpowers/plans/2026-09-03-multi-incident-remembering-rain.md`; `Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs`; provenance entries `DESIGN-NARRATIVE-002`, `TEXT-NARRATIVE-005`, and `TEXT-NARRATIVE-006` in `Docs/AIAssetProvenance.md`. | Developer-approved product direction, current repository architecture and artifact fiction, original English/Korean prose, and references to existing Unity systems only; no third-party art, audio, prose, or reference input is bundled. | **No third-party notice required.** All English/Korean prose is prototype copy requiring bilingual and in-context review before RC. |
| Full-campaign story master blueprint | `Docs/superpowers/specs/2026-09-04-curio-clerk-story-master-design.md`; provenance entry `DESIGN-NARRATIVE-003` in `Docs/AIAssetProvenance.md`. | Developer-approved story direction, repository-owned artifact fiction and mechanics, and original campaign structure and prose only; no third-party art, audio, prose, film, game, or style reference is bundled. | **No third-party notice required.** Internal review draft only; developer continuity approval is required before campaign-canon use. |
| Remembering Rain full-incident implementation plan | `Docs/superpowers/plans/2026-09-04-remembering-rain-full-incident.md`; provenance entry `DESIGN-NARRATIVE-004` in `Docs/AIAssetProvenance.md`. | Approved campaign blueprint, current repository architecture, repository-owned artifact fiction and mechanics, and original English/Korean draft prose only; no third-party art, audio, prose, film, game, or style reference is bundled. | **No third-party notice required.** Internal implementation material only; runtime copy still requires bilingual and in-context review. |
| Narrative occupational puzzle design reference | `Docs/superpowers/specs/2026-08-28-narrative-occupational-puzzle-design.md` and `Docs/Design/NarrativeOccupationalPuzzle/curio-narrative-ux-board.html`; provenance entry `DESIGN-NARRATIVE-001`. | Developer-approved product direction, repository content, original prose, and CSS shapes only; no third-party art, prose, font, or reference input is bundled. | **No third-party notice required.** Internal design material only; promotional reuse requires a separate audit. |
| First-incident senior clerk portraits and frost overlay | Five selected ImageGen PNGs recorded with hashes as `ART-NARRATIVE-001` and `ART-EFFECT-001`; exact prompts and selection notes are in `Docs/NarrativeSlicePrompts.md`. | Original text prompts and project-owned generated portrait variants only; no third-party image or style reference was supplied. | **No third-party attribution identified. Approved for prototype integration; final similarity/trademark and human release review remain open.** |

### Gowun Batang Bold

- Source: https://github.com/google/fonts/tree/main/ofl/gowunbatang
- File: `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf`
- License: SIL Open Font License 1.1
- Use: Curio names, titles, and short resolution copy.

No Asset Store art, stock art, commercial audio, or third-party gameplay package has been approved or recorded as of the review date.

## Android QA resolution evidence (2026-09-08)

GMA Unity 11.3.0 and EDM4U 1.2.188 are now resolved in Unity's package cache. This supersedes the resolution-pending status in the original intake rows above. They remain one UPM installation each; the generated linker file and Gradle templates under `Assets` do not vendor another plugin copy.

- GMA package fingerprint: `d3eae59ba596620a68df5b2b2ae25fdf6f7eb20c`; upstream revision `1c594958ab28f726feae74fe88734953999f2fba`. Verbatim license: `Docs/Licenses/GoogleMobileAds-11.3.0-LICENSE.md`, SHA-256 `EB5D0724B2AE76A94AE804C44B3D6CFAACA822B7D42907AEFA9256CB93DB6FE0`.
- EDM4U package fingerprint: `3dd580bc51c69c0bd5fa20bb03ff79da`. Verbatim license: `Docs/Licenses/EDM4U-1.2.188-LICENSE.md`, SHA-256 `F76F18185C04EC80175330572B38E40772DCCDA80F4BF0868E042E4E5B0B58A5`. The original archive hash above remains unchanged.
- Pinned plugin dependency XML requests Android GMA `com.google.android.gms:play-services-ads:25.4.0`, UMP `com.google.android.ump:user-messaging-platform:4.0.0`, ConstraintLayout 2.1.4, Fragment 1.7.1 and Lifecycle Process 2.6.2. EDM4U's generated Gradle blocks match these requests. Inspect the final Gradle dependency graph for selected transitive versions before RC.
- The Unity plugin and EDM4U licenses do not cover all native Google libraries. Native GMA/UMP are subject to the [Google Mobile Ads SDK terms](https://developers.google.com/admob/terms); AndroidX source is governed by its [Android open source licenses](https://source.android.com/docs/setup/about/licenses). Final native/transitive notices and human-readable distribution packaging remain RC work.
- UMP runtime code is already integrated and is included in the ordinary Android QA player. The dedicated `CURIO_OFFLINE_QA` route excludes the Google runtime service implementations. Sample IDs do not disable SDK networking or replace the consent/privacy review.

## Planned but not installed

The following services are planned. They are not yet part of the repository or player, so their notices, versions, data behavior, and licenses are not claimed as complete:

- any purchased music, sound effects, illustration, icon, or font package.

Firebase App, Analytics, and Crashlytics were evaluated and then removed under the dated 2026-08-21 v1 no-remote-telemetry decision. They are not included, vendored, resolved, or approved for the v1 player. Reintroduction requires a new privacy decision, notices, declarations, tests, and removal of the repository exclusion gate.

When one is installed, add the exact package/version, source URL, license file path, shipped files, attribution requirement, privacy/Data Safety impact, and purchase-receipt reference in the same commit.

## Intake rules

Before adding third-party material:

1. Record the original creator, canonical source URL, exact version, acquisition date, and intended in-game/store use.
2. Save the license text and required attribution in the repository when redistribution permits it. Store receipts privately and record only a non-sensitive reference.
3. Confirm commercial mobile-game use, modification, redistribution inside an AAB, advertising use, and AI-input rights separately. A right to use an asset in a game does not automatically grant a right to upload it to an AI service.
4. Reject assets with missing provenance, unclear authorship, copied franchise material, non-commercial-only terms, or attribution requirements the product cannot satisfy.
5. If an asset is modified, keep the original license and record the modification; do not remove authorship or rights notices.

## RC audit

- [ ] Compare `Assets`, `Packages/manifest.json`, and `Packages/packages-lock.json` with this register.
- [x] Remove the unresolved EmojiOne TMP sample and enforce its absence at validation time.
- [ ] Verify notices for every font, audio file, image, SDK, native library, and transitive package actually shipped.
- [ ] Confirm every purchased asset receipt and license is archived outside the public repository.
- [ ] Expose required notices from Settings or bundle an equally human-readable notice accepted by the license.
- [ ] Reconcile this file with `Docs/AIAssetProvenance.md`; an asset can require both AI disclosure and third-party attribution.
- [ ] Have the human developer sign off the final inventory and date this document.

## Official references

- SIL Open Font License 1.1: https://openfontlicense.org/open-font-license-official-text/
- Unity Terms of Service: https://unity.com/legal/terms-of-service
- Unity legal information: https://unity.com/legal

## Native SDK QA — 2026-09-09

QA-only English/Korean diagnostic labels are original project functional text (TEXT-QA-20260909). No additional third-party asset or dependency is introduced. Existing pinned GMA/UMP and font notices continue to apply. Google sample ad creatives and UMP forms are fetched by their native SDKs, are not project-authored content, and are retained only as local QA evidence.

## Internal playtest integration — 2026-09-09

TEXT-PLAYTEST-20260909 covers original functional English/Korean Hold guidance and internal QA/playtest documents. Layout adjustments reuse existing art and fonts. No new third-party dependency, media, or license is added; existing artwork, font, GMA/UMP, and package notices remain applicable. Captured device screens and SDK logs are local validation evidence and are not bundled as new game assets.

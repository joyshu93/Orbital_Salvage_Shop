# Third-party notices register

Last reviewed: 2026-08-21 (KST)

This is the working inventory for externally sourced material. It is not yet the final in-app notice. Before an RC build, remove unused material, resolve every open item, and make required notices easily viewable in the distributed product or its accompanying materials.

## Included assets

| Component | Repository evidence | License/attribution | Current release decision |
| --- | --- | --- | --- |
| Noto Sans KR Variable | `Assets/Fonts/NotoSansKR/NotoSansKR-Variable.ttf` | SIL Open Font License 1.1. Copyright 2014–2021 Adobe; Reserved Font Name `Source`. Full text: `Assets/Fonts/NotoSansKR/OFL.txt`. | Approved for bundling only while the copyright notice and OFL text are retained and made human-readable with the distribution. Do not rename a modified version with a reserved name. |
| Liberation Sans | `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` and generated TMP font resources | SIL Open Font License 1.1. Copyright 2010 Google Corporation and 2012 Red Hat, Inc.; reserved names are stated in `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. | Repository copy is permitted under the bundled OFL terms. Confirm whether it is included in the final player and include its notice if shipped. |
| EmojiOne TMP sample | Removed from `Assets/TextMesh Pro/Sprites` and detached from `Assets/TextMesh Pro/Resources/TMP Settings.asset`. | The removed sample had incomplete provenance, so no EmojiOne material is approved or shipped. | **Removed from release.** `ContentValidator` and the EditMode release-asset contract reject any future `EmojiOne` asset. |
| TextMesh Pro essential resources and shaders | `Assets/TextMesh Pro/**`; source package: `com.unity.ugui` 2.0.0, pinned in `Packages/packages-lock.json`. | Unity Companion License. Verbatim package license: `Docs/Licenses/uGUI-2.0.0-LICENSE.md`; source and package fingerprint: `Docs/Licenses/uGUI-2.0.0-source.md`. Separately listed Liberation Sans notice remains applicable where shipped. | **Resolved for the recorded uGUI 2.0.0 essential resources and shaders.** Retain the license notice with distribution and re-audit if the package version or copied resources change. |
| Unity packages | `Packages/manifest.json` and `Packages/packages-lock.json` | Unity registry/built-in packages under their package metadata, Unity Companion License where applicable, and Unity terms. Direct versions include Input System 1.20.0, Localization 1.5.8, URP 17.3.0, Test Framework 1.6.0, uGUI 2.0.0, and Visual Studio Editor 2.0.26. | Version lock is the source of truth. Export and review package notices from the resolved RC environment; do not assume one license covers every transitive package. |

No Asset Store art, stock art, commercial audio, or third-party gameplay package has been approved or recorded as of the review date.

## Planned but not installed

The following services are planned. They are not yet part of the repository or player, so their notices, versions, data behavior, and licenses are not claimed as complete:

- Google Mobile Ads Unity plugin and User Messaging Platform integration;
- Firebase Analytics and Firebase Crashlytics Unity SDKs;
- any purchased music, sound effects, illustration, icon, or font package.

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

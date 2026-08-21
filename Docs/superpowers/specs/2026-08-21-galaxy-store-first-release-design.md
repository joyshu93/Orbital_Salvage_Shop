# Galaxy Store first-release design

Date: 2026-08-21
Product: **Curio Clerk: Night Shift / 기묘한 분실물 야간반**
Decision status: approved direction, pending implementation-plan review

## Context

The project is developed by one person. The developer has no Google Play Console account and will not recruit external testers. The primary objective is to complete a genuine commercial-store release with minimal cash spending and turn the full release/operation process into credible Unity career evidence.

The previous Google Play plan is not executable under these constraints because a newly created personal Play account would require at least 12 continuously opted-in closed testers for 14 days before production access. A Steam pivot would avoid that requirement but would add a US$100 fee and require substantial PC, premium-pricing, input, layout, and content redesign.

## Decision

Release the existing portrait Android game on **Samsung Galaxy Store first**.

- Keep Unity `6000.3.21f1`, Android, portrait orientation, touch controls, offline play, English/Korean localization, and the existing package ID.
- Launch in South Korea first to constrain policy, support, localization, and device risk for a solo developer.
- Use Galaxy Store's staged rollout for the new app: 10% → 50% → 100%, with at least 24 hours and a health check between stages. If the eligible audience is too small for percentages to produce useful evidence, advance only after a clean installation, launch, play-through, and crash review.
- Expand to English-language regions only after version 1.0.1 is stable and the English listing has been rechecked.
- Defer Google Play until testers become available or its eligibility rules change. Defer Steam until the mobile release proves the concept warrants a larger premium edition.

Samsung's current registration documentation supports AAB upload. Galaxy Store manages the signing key and generates a universal APK from the AAB. The project will retain a securely backed-up developer-controlled key where Seller Portal permits it; no signing key, password, bank document, or identity document may enter Git.

## Cost boundary

The release-path cash target is **KRW 0**, excluding hardware, connectivity, taxes, banking charges, or identity documents the developer already needs independently.

- Do not buy a Google Play or Steam account for v1.
- Do not buy assets, fonts, audio, testers, reviews, followers, or user acquisition.
- Use only self-created assets or assets with recorded commercial licenses.
- Use free service tiers and free static hosting for the support page and privacy policy.
- Samsung's public setup guide does not list a per-app registration fee; confirm the actual Seller Portal terms before accepting commercial seller status.
- No expense is authorized merely because it fits the old KRW 1,000,000 ceiling. Any new cash expense requires a separate developer decision.

## Store and account responsibilities

The human developer owns all identity- and money-bearing actions:

1. Create or verify a Samsung account with the correct South Korea region before Seller Portal registration; the region cannot be changed in Seller Portal afterward.
2. Register with Seller Portal and apply for commercial seller status. Free and paid distribution both require that status.
3. Supply government identity, financial, tax, address, support-email, and privacy-policy information directly to Samsung or Google as required.
4. Create AdMob and Firebase projects, retain credentials privately, and provide only non-secret IDs needed for repository configuration.
5. Create and back up the Android signing material outside the repository.
6. Run Unity Editor, Unity tests, asset generation, and release builds under the project's no-MCP workflow.
7. Review and submit the final Seller Portal declarations.

Codex may prepare repository code, tests, build scripts, store copy, data-safety drafts, release checklists, provenance records, and diagnosis of human-supplied build/test output. Codex must not operate Unity without later-authorized official Unity agentic access.

## Product and monetization

Version 1 remains a free, offline-capable game with rewarded ads only.

- Keep exactly two optional rewarded placements: failed-shift revive and successful-shift double coins.
- Allow at most one successful reward per shift.
- No banner, interstitial, app-open ad, energy system, loot box, paid currency, or IAP.
- Unavailable, rejected, failed, or unapproved ads must never block play or remove the base reward.
- Use the Google Mobile Ads Unity plugin and UMP only after their exact versions and licenses are recorded.
- Keep Analytics and Crashlytics collection off until the applicable in-game consent is granted; withdrawal must stop future collection.
- Link the public Galaxy Store listing to AdMob after publication for app-readiness review. Until approval or during no-fill, the game remains fully usable and simply hides reward offers.

AdMob currently recognizes Samsung Galaxy Store as a supported third-party Android store. The store package name must exactly match the AdMob app record.

## Testing without external testers

No pre-release retention or usability claim will be made. Automated and device testing reduce technical risk but do not substitute for independent human feedback.

The release candidate requires:

- all existing EditMode and PlayMode suites run by the human developer with the output retained;
- deterministic content validation and a human-run release AAB build;
- a complete first-launch → tutorial → three shifts → reward result → save/relaunch play-through by the developer;
- pause/resume, force-stop recovery, corrupt-save recovery, language switching, consent withdrawal, ad no-fill/failure/duplicate callback, and offline-mode checks;
- at least one owned Samsung Android device;
- Samsung Remote Test Lab checks on at least three additional device/OS/screen profiles;
- API target >=33 and at least one 64-bit binary, while retaining the project's API 36 target and ARM64/IL2CPP configuration;
- no P0/P1 issue, no known reproducible crash, no release credential in Git, and a complete Samsung self-check list.

The game requires no account, so no reviewer login is needed. Add concise review-team instructions that explain the tutorial, reward-ad test path, privacy options, offline behavior, and how to reach each major screen. Supply a self-test video only if certification requests additional evidence.

## Rights, AI, and store declarations

Release remains blocked until:

- the current ImageGen application icon is replaced or receives a documented human creative pass, similarity review, and explicit release approval;
- every shipped AI-assisted asset has a complete entry in `Docs/AIAssetProvenance.md`;
- the unresolved EmojiOne sample is removed or its exact commercial redistribution terms are established;
- the exact TextMesh Pro essential-resource source package/version and license evidence are recorded;
- all store images and video have individual AI/provenance decisions;
- `Docs/ThirdPartyNotices.md`, the Samsung Data Safety form, privacy policy, and actual SDK behavior agree.

Do not imply endorsement by Samsung, Google, OpenAI, Unity, an artist, a franchise, or a brand.

## Release flow

1. **Account gate:** complete Samsung account, Seller Portal, and commercial seller verification before spending time on final store art.
2. **Technical RC:** close gameplay/save/privacy defects, integrate pinned service SDKs, and produce a human-validated AAB and symbols.
3. **Content freeze:** finish EN/KO copy, art, audio, provenance, notices, rating answers, support page, privacy policy, and Data Safety.
4. **Certification:** upload the AAB, select supported devices, add review notes, and respond to Samsung findings with reproducible fixes.
5. **Korea rollout:** 10% → 50% → 100% when health gates allow.
6. **First maintenance release:** ship version 1.0.1 within 14 calendar days after version 1.0.0 reaches 100% rollout. Use real crash, review, and usability evidence when available; if no issue appears, select and verify one small maintenance improvement from the documented post-release audit. This proves update and maintenance experience rather than a one-time upload.
7. **Expansion review:** decide whether to expand Galaxy regions, pursue Google Play later, or design a separate Steam edition.

## Career evidence package

The release is useful for employment only when the developer's contribution can be evaluated quickly. Produce and retain:

- a public Galaxy Store product link and release/version history;
- a 30–60 second gameplay video with no concept-only footage;
- a one-page project page naming Unity, C#, Android, architecture, tests, persistence, localization, privacy, ads, analytics, crash reporting, and the exact solo responsibilities;
- an architecture diagram and selected sanitized code/tests, without service secrets;
- a certification case study describing one rejection or pre-submit defect, its evidence, fix, and verification when such a case exists;
- post-release metrics reported honestly: installs, crash-free users, tutorial completion, shift completion, retention, and reward-ad results; percentage metrics are reported only after at least 30 unique users have launched the relevant release, while smaller samples are shown as raw counts and explicitly labelled too small for a pass/fail conclusion;
- a concise postmortem covering scope decisions, failures, policy work, AI provenance, and what changed in 1.0.1.

Do not claim commercial success without numbers. The accurate resume claim after completion is: **Designed, implemented, certified, released, and maintained a solo Unity Android game on Samsung Galaxy Store.**

## Success criteria

The first-release objective is achieved only when all of the following are true:

- version 1.0.0 is publicly downloadable from Galaxy Store in South Korea;
- version 1.0.1 is submitted and published through the normal update path within 14 calendar days after version 1.0.0 reaches 100% rollout;
- P0/P1 defects are zero at each release decision;
- once at least 30 unique users have launched the relevant release, crash-free users are at least 99%, with the raw sample size shown beside the percentage; below that threshold, raw crash and user counts are recorded without claiming that the gate passed or failed;
- no advertisement reward is known to be missing or duplicated;
- the store listing, privacy/Data Safety declarations, licenses, and AI provenance match the shipped build;
- the career evidence package above is publicly or privately reviewable without requiring a Samsung device for its core video and technical explanation.

Revenue and retention remain learning metrics for this first title, not conditions for calling the release experience complete.

## Principal risks

| Risk | Control |
| --- | --- |
| Low Galaxy Store discovery and revenue | Treat v1 as a release/operations credential; spend nothing on acquisition; expand only on evidence. |
| No independent pre-release feedback | Keep scope small, use deterministic tests and device lab coverage, disclose the limitation, and prioritize early post-release fixes. |
| Strict or inconsistent certification | Follow Samsung's self-check list, retain test evidence, provide clear review notes, and budget schedule rather than money for resubmission. |
| AdMob readiness delay or no-fill | Preserve offline play and hide unavailable reward offers; never make ad revenue a functional dependency. |
| AI or third-party rights uncertainty | Block unresolved assets from RC through the provenance and notice ledgers. |
| Portfolio link inaccessible to non-Samsung reviewers | Lead with a short gameplay video, technical project page, and sanitized code samples; use the store link as verification, not the only artifact. |

## Official references

- Galaxy Store preparation and commercial seller status: https://developer.samsung.com/galaxy-store/prepare.html
- Seller Portal registration, AAB, staged rollout, Data Safety, and review: https://developer.samsung.com/galaxy-store/launch.html
- Galaxy Store AAB/API/64-bit FAQ: https://developer.samsung.com/galaxy-store/faq.html
- Galaxy Store self-check list: https://developer.samsung.com/galaxy-store/self-check-list-galaxy.html?lang=en
- Galaxy Store distribution policy: https://developer.samsung.com/galaxy-store/distribution-guide.html?lang=en
- AdMob supported third-party stores and readiness review: https://support.google.com/admob/answer/9989980?hl=en-gb
- Google Play new-personal-account testing rule: https://support.google.com/googleplay/android-developer/answer/14151465?hl=en
- Unity Terms of Service and agentic access boundary: https://unity.com/legal/terms-of-service

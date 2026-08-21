# Release checklist

This checklist is the Samsung Galaxy Store-first release gate for v1. It does not replace the developer's direct Seller Portal review or certification requirements.

## Samsung account and commercial seller

- [ ] Samsung Seller Portal account is registered for South Korea as a commercial seller.
- [ ] Identity, business, tax, and financial verification evidence is submitted directly in Seller Portal and is not copied to Git.
- [ ] Seller verification status is recorded with a date in `Docs/Store/SamsungSellerSetup.md`, without identity-document contents.
- [ ] Package ID `com.joyshu93.curioclerknightshift`, English and Korean titles, public developer name, support email, and public privacy-policy URL are confirmed before submission.
- [ ] Store icon, phone screenshots, and EN/KO short and full descriptions are complete and match the shipped build.

## AI and rights

- [ ] Every AI-created or AI-edited asset has a completed entry in `Docs/AIAssetProvenance.md`.
- [ ] Every store asset has a current store submission decision and supporting rationale.
- [ ] Current ImageGen application icon is replaced or receives a documented human creative pass, similarity review, and human release approval.
- [ ] Named-artist/studio styles, protected characters, brands, logos, unauthorized likenesses, and unexplained signatures/watermarks are absent.
- [ ] `Docs/ThirdPartyNotices.md` matches the RC asset/package inventory and all required notices are human-readable in the distribution.
- [ ] The unresolved EmojiOne TMP sample is removed or its exact version and commercial redistribution terms are documented.

## Build and services

- [ ] Human developer runs `scripts/test-unity.ps1`; supplied report passes EditMode and PlayMode.
- [ ] Human developer runs `scripts/build-android.ps1`; supplied output includes a signed RC AAB and public symbols zip.
- [ ] API 29 minimum, API 36 target, ARM64-only, IL2CPP, portrait, and package ID are verified.
- [ ] UMP launch flow and privacy-options re-entry are tested where the final SDK configuration requires them.
- [ ] AdMob live IDs replace test IDs only in the signed release configuration.
- [ ] Analytics and Crashlytics remain off before consent and stop after withdrawal.
- [ ] Crashlytics test event and IL2CPP symbolication are verified if Crashlytics is enabled for release.
- [ ] The public privacy policy matches actual AdMob/Firebase collection and the final SDK configuration.

## Owned-device and Remote Test Lab matrix

- [ ] Owned Android devices cover at least two aspect ratios and the supported Android-version range.
- [ ] Samsung Remote Test Lab covers representative Galaxy devices and current Android versions unavailable among owned devices.
- [ ] Pause/resume, force-stop restore, corrupt-save recovery, and English/Korean switching pass on the matrix.
- [ ] Reward success, close, load failure, no-fill, and duplicate callback preserve base rewards on the matrix.
- [ ] P0/P1 defects: 0; release-blocking crash or data-loss regressions: 0.

## Certification

- [ ] Seller Portal metadata, content classification, privacy disclosures, pricing, and distribution country are complete for South Korea.
- [ ] The signed RC AAB and required symbols are uploaded to Seller Portal.
- [ ] Certification feedback is resolved or explicitly accepted by the developer before rollout.
- [ ] Release documentation gate passes in Repository mode; Submission mode passes only after real public identity fields are supplied.

## Rollout

- [ ] Roll out to 10%, wait at least 24 hours, and inspect crashes, reviews, and support requests.
- [ ] Roll out to 50%, wait at least 24 hours, and inspect crashes, reviews, and support requests.
- [ ] Roll out to 100% only while crash-free users remain at least 99.5%.
- [ ] If D1 is below 20%, stop content/ad expansion and repair the core loop.

## 1.0.1 readiness

- [ ] Triage launch feedback, crashes, and store reviews into a dated 1.0.1 candidate list.
- [ ] Re-run affected device matrix, consent, save, rewarded-ad, and localization checks before submitting 1.0.1.
- [ ] Update privacy, service, rights, and provenance records for any SDK, asset, or store-listing change.

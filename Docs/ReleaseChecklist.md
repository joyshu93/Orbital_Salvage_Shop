# Release checklist

Current staffing note (2026-08-21): this is a solo project and **0 external testers are currently recruited**. If the Play Console account is a personal account created after 2023-11-13, production access remains blocked until at least 12 testers stay opted in to the closed test continuously for 14 days. Target 15 recruits to absorb dropouts; do not use paid review or fake-tester services.

## Accounts and listing

- [ ] Play Console personal account registered, US$25 paid, and Android device verified
- [ ] Confirm whether the account is subject to the new-personal-account closed-test requirement
- [ ] App name and package ID availability rechecked immediately before permanent app creation
- [ ] Developer display name, support email, and public privacy-policy URL finalized
- [ ] External tester pool increased from 0 to 15 so at least 12 can remain opted in for 14 continuous days
- [ ] Tester feedback channel and test instructions prepared; consent/feedback records contain no unnecessary personal data
- [ ] Store icon, feature graphic, phone screenshots, EN/KO short and full descriptions complete

Official testing requirement: https://support.google.com/googleplay/android-developer/answer/14151465?hl=en

## AI and rights gates

- [ ] Every AI-created or AI-edited asset has a completed entry in `Docs/AIAssetProvenance.md`
- [ ] Current ImageGen application icon is replaced or receives a documented human creative pass, similarity review, and human release approval
- [ ] Each uploaded Store listing, Promotional content, and YouTube visual has an individual Play Console AI-declaration decision
- [ ] The no-runtime-generation classification is still accurate; any new AI/UGC feature has been reassessed against current Play policy
- [ ] Named-artist/studio styles, protected characters, brands, logos, unauthorized likenesses, and unexplained signatures/watermarks are absent
- [ ] `Docs/ThirdPartyNotices.md` matches the RC asset/package inventory and all required notices are human-readable in the distribution
- [ ] The unresolved EmojiOne TMP sample is removed or its exact version and commercial redistribution terms are documented

Official AI declaration guidance: https://support.google.com/googleplay/android-developer/answer/17262077?hl=en

## Build and services

- [ ] Human developer runs `scripts/test-unity.ps1`; supplied report passes EditMode and PlayMode
- [ ] Human developer runs `scripts/build-android.ps1`; supplied output includes a signed RC AAB and public symbols zip
- [ ] API 29 minimum, API 36 target, ARM64-only, IL2CPP, portrait verified
- [ ] UMP launch flow and privacy-options re-entry tested
- [ ] AdMob live IDs replace test IDs only in the signed release configuration
- [ ] Analytics and Crashlytics remain off before consent and stop after withdrawal
- [ ] Crashlytics test event and IL2CPP symbolication verified
- [ ] Data Safety matches actual AdMob/Firebase collection

## Test gates

- [ ] API 29, 33, 35, and 36 emulator smoke tests
- [ ] Two physical Android devices with different aspect ratios
- [ ] Pause/resume, force-stop restore, corrupt-save recovery, and language switching
- [ ] Reward success, close, load failure, no-fill, and duplicate callback
- [ ] At least 8 closed-test users finish the tutorial
- [ ] At least 6 complete three shifts
- [ ] At least 4 return the next day
- [ ] P0/P1 defects: 0; crash-free users: 99%+

## Rollout

- [ ] Submit production access only after the 12-person/14-day requirement is satisfied
- [ ] Roll out 10%, wait at least 24 hours, inspect crashes/reviews
- [ ] Roll out 50%, wait at least 24 hours, inspect crashes/reviews
- [ ] Roll out 100% only while crash-free users remain at least 99.5%
- [ ] If D1 is below 20%, stop content/ad expansion and repair the core loop

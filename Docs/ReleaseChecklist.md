# Release checklist

## Accounts and listing

- [ ] Play Console personal account registered, US$25 paid, and Android device verified
- [ ] App name and package ID availability rechecked immediately before permanent app creation
- [ ] Developer display name, support email, and public privacy-policy URL finalized
- [ ] 15 real testers recruited so at least 12 remain opted in for 14 continuous days
- [ ] Store icon, feature graphic, phone screenshots, EN/KO short and full descriptions complete

## Build and services

- [ ] `scripts/test-unity.ps1` passes EditMode and PlayMode
- [ ] `scripts/build-android.ps1` produces a signed RC AAB and public symbols zip
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

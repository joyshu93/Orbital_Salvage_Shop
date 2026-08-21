# AdMob, UMP, Firebase, and privacy setup

Service credentials are intentionally excluded from Git. The game currently uses consent-aware local/no-op adapters, so unavailable SDKs never block play or base rewards.

## Store release boundary

Samsung Galaxy Store v1 account, identity, financial verification, signing custody, and listing setup are tracked in `Docs/Store/SamsungSellerSetup.md`. Do not add Seller Portal credentials, verification evidence, signing keys, or public identity placeholders to this repository. Public values belong in `Docs/PrivacyPolicy.md` only after the developer supplies them.

## AdMob and UMP

1. Register the Android app in AdMob with package `com.joyshu93.curioclerknightshift`.
2. Install the official Google Mobile Ads Unity plugin. The validated planning baseline is v11.3.0; use one installation method only (OpenUPM or the official `.unitypackage`).
3. Enter the Android AdMob app ID under `Assets > Google Mobile Ads > Settings`.
4. Create one rewarded unit. During development use Google's Android rewarded test unit, never a live unit.
5. In AdMob Privacy & messaging, create the required UMP messages.
6. On every launch, call consent `Update`, then `LoadAndShowConsentFormIfRequired`. Initialize/load ads only when `CanRequestAds()` is true. Expose `ShowPrivacyOptionsForm()` from Settings when required.
7. Implement only the two placements `failed_revive` and `success_double`. One successful placement locks the other for that shift. Failed/closed ads do not remove base rewards.

Official references:

- https://developers.google.com/admob/unity/quick-start
- https://developers.google.com/admob/unity/privacy
- https://support.google.com/admob/answer/7313578

## Firebase Analytics and Crashlytics

1. Create a Firebase Android app with the same package ID.
2. Download the current official Unity SDK. Planning baseline: Firebase Unity SDK 13.14.0.
3. Import `FirebaseAnalytics.unitypackage` and `FirebaseCrashlytics.unitypackage` from `dotnet4`.
4. Put `google-services.json` in `Assets` locally. It is ignored by Git.
5. Disable Analytics and Crashlytics collection by default. Enable each only after its in-game consent toggle is on; withdrawal must disable future collection.
6. Send no artifact description, free-form text, exact local date, advertising identifier, email, or other PII as custom parameters.
7. Force one test non-fatal/crash in an internal build, verify it in Firebase, then remove the trigger.
8. Upload the public IL2CPP symbols produced beside the AAB with `firebase crashlytics:symbols:upload --app=<FIREBASE_APP_ID> <SYMBOLS_PATH>`.

Official references:

- https://firebase.google.com/docs/unity/setup
- https://firebase.google.com/docs/crashlytics/unity/get-started

## Required analytics events

- `tutorial_started`, `tutorial_completed`
- `shift_started` with difficulty band only
- `shift_failed` with band and sorted-count bucket
- `shift_completed` with band and duration bucket
- `reward_offer_shown`, `reward_result` with placement and result
- `cosmetic_unlocked` with cosmetic ID

Do not start paid acquisition until organic D1 is at least 25% and D7 at least 8% in the first meaningful cohort.

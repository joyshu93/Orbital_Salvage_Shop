# AdMob, UMP, Firebase, and privacy setup

Service credentials are intentionally excluded from Git. The game currently uses consent-aware local/no-op adapters, so unavailable SDKs never block play or base rewards.

## Store release boundary

Samsung Galaxy Store v1 account, identity, financial verification, signing custody, and listing setup are tracked in `Docs/Store/SamsungSellerSetup.md`. Do not add Seller Portal credentials, verification evidence, signing keys, or public identity placeholders to this repository. Public values belong in `Docs/PrivacyPolicy.md` only after the developer supplies them.

## AdMob and UMP

1. Register the Android app in AdMob with package `com.joyshu93.curioclerknightshift`.
2. The official Google-authored Google Mobile Ads Unity plugin is pinned as `com.google.ads.mobile` 11.3.0 and distributed through the community OpenUPM registry configured in `Packages/manifest.json`; OpenUPM is not a Google-operated registry. Do not also import the official `.unitypackage` or copy plugin files under `Assets`.
3. After the human package-resolution checkpoint below, enter the Android AdMob app ID under `Assets > Google Mobile Ads > Settings`. The generated local settings asset is ignored by Git.
4. Create one rewarded unit. During development use Google's Android rewarded test unit, never a live unit.
5. In AdMob Privacy & messaging, create the required UMP messages.
6. On every launch, call consent `Update`, then `LoadAndShowConsentFormIfRequired`. Initialize/load ads only when `CanRequestAds()` is true. Expose `ShowPrivacyOptionsForm()` from Settings when required.
7. Implement only the two placements `shift_failed_revive` and `shift_complete_double`. One successful placement locks the other for that shift. Failed/closed ads do not remove base rewards.

Official references:

- https://developers.google.com/admob/unity/quick-start
- https://developers.google.com/admob/unity/privacy
- https://support.google.com/admob/answer/7313578

## Firebase Analytics and Crashlytics

1. Create a Firebase Android app with the same package ID.
2. Firebase App, Analytics, and Crashlytics are pinned as official local UPM tarballs at version 13.15.0, with EDM4U pinned at 1.2.188. Their exact source URLs and SHA-256 values are recorded in `Docs/ThirdPartyNotices.md`. Do not import the corresponding `.unitypackage` files or copy SDK folders under `Assets`.
3. Put `google-services.json` in `Assets` locally. It is ignored by Git; never commit it or any service credential.
4. `Assets/Plugins/Android/AndroidManifest.xml` disables Analytics and Crashlytics collection before Firebase runtime initialization. Enable each only after its in-game consent toggle is on; withdrawal must disable future collection.
5. Send no artifact description, free-form text, exact local date, advertising identifier, email, or other PII as custom parameters.
6. Force one test non-fatal/crash in an internal build, verify it in Firebase, then remove the trigger.
7. Upload the public IL2CPP symbols produced beside the AAB with `firebase crashlytics:symbols:upload --app=<FIREBASE_APP_ID> <SYMBOLS_PATH>`.

Official references:

- https://firebase.google.com/docs/unity/setup
- https://firebase.google.com/docs/crashlytics/unity/get-started

## Human package-resolution checkpoint

`Packages/packages-lock.json` must be produced by Unity, not edited by hand to impersonate resolution:

1. Open the project in Unity `6000.3.21f1`.
2. Wait for Package Manager and External Dependency Manager to finish.
3. Confirm the Console has no compilation or Android dependency-resolution error.
4. Confirm the resolved graph contains Google Mobile Ads 11.3.0, Firebase App/Analytics/Crashlytics 13.15.0, and EDM4U 1.2.188.
5. Confirm there is no Asset-package copy under `Assets/Firebase` or `Assets/ExternalDependencyManager`, close Unity, and retain the Unity-generated `Packages/packages-lock.json` change.

## Required analytics events

- `tutorial_started`, `tutorial_completed`
- `shift_started` with difficulty band only
- `shift_failed` with band and sorted-count bucket
- `shift_completed` with band and duration bucket
- `reward_offer_shown`, `reward_result` with placement and result
- `cosmetic_unlocked` with cosmetic ID

Do not start paid acquisition until organic D1 is at least 25% and D7 at least 8% in the first meaningful cohort.

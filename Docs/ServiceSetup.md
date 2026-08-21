# AdMob, UMP, and v1 privacy setup

Service credentials are intentionally excluded from Git. Version 1 ships Google Mobile Ads and UMP for optional rewarded ads, but it ships no Firebase App, Analytics, Crashlytics, or other remote gameplay-telemetry transport.

## Store release boundary

Samsung Galaxy Store v1 account, identity, financial verification, signing custody, and listing setup are tracked in `Docs/Store/SamsungSellerSetup.md`. Do not add Seller Portal credentials, verification evidence, signing keys, or public identity placeholders to this repository. Public values belong in `Docs/PrivacyPolicy.md` only after the developer supplies them.

## AdMob and UMP

1. Register the Android app in AdMob with package `com.joyshu93.curioclerknightshift`.
2. The official Google-authored Google Mobile Ads Unity plugin is pinned as `com.google.ads.mobile` 11.3.0 and distributed through the community OpenUPM registry configured in `Packages/manifest.json`; OpenUPM is not a Google-operated registry. Do not also import the official `.unitypackage` or copy plugin files under `Assets`.
3. After the human package-resolution checkpoint below, open and save `Assets > Google Mobile Ads > Settings` once so the local settings asset exists. Do not commit it or put a live ID in source; the release builder injects the environment-supplied app ID into this ignored asset.
4. Create one rewarded unit. During development use Google's Android rewarded test unit, never a live unit.
5. In AdMob Privacy & messaging, create the required UMP messages.
6. On every launch, call consent `Update`, then `LoadAndShowConsentFormIfRequired`. Initialize/load ads only when `CanRequestAds()` is true. Expose `ShowPrivacyOptionsForm()` from Settings when required.
7. Implement only the two placements `shift_failed_revive` and `shift_complete_double`. One successful placement locks the other for that shift. Failed/closed ads do not remove base rewards.

Official references:

- https://developers.google.com/admob/unity/quick-start
- https://developers.google.com/admob/unity/privacy
- https://support.google.com/admob/answer/7313578

## Version 1 no-remote-telemetry boundary

The 2026-08-21 v1 decision excludes Firebase and remote gameplay/crash telemetry from the shipped player:

- `Packages/manifest.json`, the runtime asmdef, runtime source, vendored packages, and Android plugins must contain no Firebase shipping dependency.
- `ServiceFactory` always supplies the local `ConsentAwareAnalyticsService` and `ConsentAwareCrashReporter`; these retain only their local enabled flag and do not transmit, log, cache, or persist event/report payloads.
- `AnalyticsEvents` and `GameTelemetry` remain pure allowlist and bucketing logic for local behavior and tests. They do not create a transport.
- Run `scripts/check-no-remote-telemetry.ps1` for every release candidate. A Firebase package, assembly reference, adapter, tgz, SDK symbol, or manifest entry is a release-blocking failure.
- Defensive `google-services.json` ignore rules remain so credentials cannot be accidentally committed, but a local file must not be added to a v1 build.

## Human package-resolution checkpoint

`Packages/packages-lock.json` must be produced by Unity, not edited by hand to impersonate resolution:

1. Open the project in Unity `6000.3.21f1`.
2. Wait for Package Manager and External Dependency Manager to finish.
3. Confirm the Console has no compilation or Android dependency-resolution error.
4. Confirm the resolved graph contains Google Mobile Ads 11.3.0 and EDM4U 1.2.188, with no `com.google.firebase.*` package.
5. Confirm there is no Asset-package copy under `Assets/Firebase` or `Assets/ExternalDependencyManager`, close Unity, and retain the Unity-generated `Packages/packages-lock.json` change.

## Human-owned release configuration and build

The release build reads six values from the current terminal process. Never put the values in Git, a checked-in script, a screenshot, or a support log:

```powershell
$env:CURIO_ADMOB_APP_ID = '<live AdMob Android app ID>'
$env:CURIO_ADMOB_REWARDED_ID = '<live rewarded unit ID>'
$env:CURIO_ANDROID_KEYSTORE_PATH = '<existing keystore path>'
$env:CURIO_ANDROID_KEYSTORE_PASS = '<keystore password>'
$env:CURIO_ANDROID_KEY_ALIAS = '<key alias>'
$env:CURIO_ANDROID_KEY_PASS = '<key password>'
```

The build first verifies that the running Editor is exactly Unity `6000.3.21f1` and independently runs the Release-mode no-remote-telemetry gate, so invoking the Unity menu or batch entry point cannot bypass the wrapper preflight. The gate child process is hidden and receives no AdMob or signing environment values. The build then validates the live ID shapes, rejects Google's sample IDs, writes the rewarded unit only to the ignored `Assets/Resources/ServiceConfiguration.asset`, and writes the app ID only to the ignored Google Mobile Ads settings asset. Signing values are applied in memory immediately before `BuildPipeline.BuildPlayer` and cleared afterward. The committed build manifest contains exactly the approved public release metadata and the AAB SHA-256.

After Unity has resolved the pinned GMA/EDM4U packages, the human developer downloads the official `bundletool-all-1.18.3.jar` from:

- https://github.com/google/bundletool/releases/download/1.18.3/bundletool-all-1.18.3.jar

Keep it at `tools/bundletool/bundletool-all-1.18.3.jar`; the jar is ignored. The inspection script executes `bundletool version` and requires the actual normalized output to equal `1.18.3`; renaming another jar is insufficient. Then run:

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
.\scripts\test-unity.ps1
.\scripts\build-android.ps1
.\scripts\inspect-aab.ps1 -AabPath .\Builds\Android\CurioClerk.aab -BundletoolPath .\tools\bundletool\bundletool-all-1.18.3.jar
```

Expected local outputs are the signed AAB, one general IL2CPP symbols zip, `CurioClerk-build.json`, and a sanitized `inspection.txt` under the ignored `Builds/Android` directory. Confirm Git status contains no settings asset, keystore, identifier, password, jar, or build output before release handoff.

## Local coarse event vocabulary

The pure local schema retains these names for deterministic tests and future product analysis design; version 1 does not transmit them:

- `tutorial_started`, `tutorial_completed`
- `shift_started` with difficulty band only
- `shift_failed` with band and sorted-count bucket
- `shift_completed` with band and duration bucket
- `reward_offer_shown`, `reward_result` with placement and result
- `cosmetic_unlocked` with cosmetic ID

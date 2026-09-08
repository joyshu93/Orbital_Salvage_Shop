# AdMob, UMP, and v1 privacy setup

Service credentials are intentionally excluded from Git. Version 1 ships Google Mobile Ads and UMP for optional rewarded ads, but it ships no Firebase App, Analytics, Crashlytics, or other remote gameplay-telemetry transport.

## Store release boundary

Samsung Galaxy Store v1 account, identity, financial verification, signing custody, and listing setup are tracked in `Docs/Store/SamsungSellerSetup.md`. Do not add Seller Portal credentials, verification evidence, signing keys, or public identity placeholders to this repository. Public values belong in `Docs/PrivacyPolicy.md` only after the developer supplies them.

## AdMob and UMP

1. Register the Android app in AdMob with package `com.joyshu93.curioclerknightshift`.
2. The official Google-authored Google Mobile Ads Unity plugin is pinned as `com.google.ads.mobile` 11.3.0 and distributed through the community OpenUPM registry configured in `Packages/manifest.json`; OpenUPM is not a Google-operated registry. The runtime asmdef explicitly references the plugin's `GoogleMobileAds.Core.dll`, `GoogleMobileAds.dll`, and `GoogleMobileAds.Ump.dll` because it uses `overrideReferences`. Do not also import the official `.unitypackage` or copy plugin files under `Assets`.
3. After the package-resolution validation below, the QA and release builders create the ignored Google Mobile Ads settings asset through the pinned plugin's own `LoadInstance` path when it is absent. Do not commit it or put a live ID in source; the release builder injects the environment-supplied app ID into this ignored asset.
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

## Package-resolution validation

`Packages/packages-lock.json` must be produced by Unity, not edited by hand to impersonate resolution:

The developer authorizes automated Unity validation under `AGENTS.md`. The 2026-09-08 local audit confirmed resolved GMA 11.3.0 and EDM4U 1.2.188 and retained their package licenses in `Docs/Licenses`. The following checks also apply when reproducing the environment:

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

The build first verifies the pinned Unity project/editor and either its bundled Android tools or the isolated personal toolchain without launching Unity. The default external locations are `%LOCALAPPDATA%\Android\Sdk` for SDK Platform 36, Build Tools 36.0.0, Command-line Tools 16.0, CMake 3.22.1, and NDK `27.2.12479018`, plus `%USERPROFILE%\UnityPersonal\OpenJDK17` for a Java 17 distribution. Custom locations can be supplied through `CURIO_ANDROID_SDK_ROOT`, `CURIO_ANDROID_NDK_ROOT`, and `CURIO_ANDROID_JDK_ROOT`; all three must be complete when any override is used. No system `PATH`, Unity Hub installation, or company project setting needs to change.

The preflight and Unity builder independently select the same complete toolchain from inherited overrides, bundled tools, or the personal external locations. The wrappers do not synthesize missing overrides or change the caller environment. `ProjectBuilder` temporarily applies those paths through Unity's Android external-tool settings and restores the previous values after success or failure. The build then independently runs the Release-mode no-remote-telemetry gate, so invoking the Unity menu or batch entry point cannot bypass the wrapper preflight. The gate child process is hidden and receives no AdMob or signing environment values. The build validates the live ID shapes, rejects Google's sample IDs, writes the rewarded unit only to the ignored `Assets/Resources/ServiceConfiguration.asset`, and writes the app ID only to the ignored Google Mobile Ads settings asset. Signing values are applied in memory immediately before `BuildPipeline.BuildPlayer` and cleared afterward. The committed build manifest contains exactly the approved public release metadata and the AAB SHA-256.

Run the execution-policy-free diagnostic before preparing any service IDs or signing values:

```powershell
.\scripts\check-android-toolchain.cmd
```

`READY` means the pinned editor plus its bundled or isolated external toolchain has the components needed to attempt an Android build. `BLOCKED` lists every missing component without printing an absolute machine path. On a company-managed machine, keep the managed Unity Hub and editor unchanged; the isolated external SDK, NDK, and JDK are sufficient for this project.

## Zero-credential QA APK

Before creating AdMob units or a release keystore, close the Unity Editor and run:

```powershell
.\scripts\build-android-dev.cmd
```

This creates `Builds/Android/CurioClerk-qa.apk` with Unity debug signing, `DEVELOPMENT_BUILD`, and Google's official Android sample app/rewarded identifiers. It does not read live AdMob IDs or release-signing environment variables. The builder temporarily applies the sample identifiers and debug signing, then restores the prior service, signing, app-bundle, and Android-tool settings after success or failure. The APK is for owned-device QA only and must never be submitted to a store.

After Unity has resolved the pinned GMA/EDM4U packages, the human developer downloads the official `bundletool-all-1.18.3.jar` from:

- https://github.com/google/bundletool/releases/download/1.18.3/bundletool-all-1.18.3.jar

Keep it at `tools/bundletool/bundletool-all-1.18.3.jar`; the jar is ignored. The official file downloaded on 2026-08-26 has SHA-256 `A099CFA1543F55593BC2ED16A70A7C67FE54B1747BB7301F37FDFD6D91028E29`. The inspection script executes `bundletool version` and requires the actual normalized output to equal `1.18.3`; renaming another jar is insufficient. Then run:

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
.\scripts\test-unity.cmd
.\scripts\build-android.cmd
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

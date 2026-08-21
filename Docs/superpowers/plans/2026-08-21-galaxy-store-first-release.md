# Galaxy Store First Release Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the existing Curio Clerk vertical slice into a consent-aware, rights-cleared Samsung Galaxy Store 1.0.0 release, then publish a verified 1.0.1 maintenance update and retain credible solo-development career evidence.

**Architecture:** Keep deterministic gameplay in `CurioClerk.Core`, isolate AdMob/UMP behind runtime interfaces, and ship local non-transport analytics/crash boundaries. Centralize immutable Android release values in one Editor configuration class, generate sensitive build-only ad configuration from process environment values, and make every release decision traceable through repository checks and human-retained evidence. Unity remains human-operated; agents edit files and diagnose output supplied by the developer.

**Tech Stack:** Unity `6000.3.21f1`, C#, uGUI/TMP, Unity Test Framework, Android AAB/IL2CPP/ARM64, Google Mobile Ads Unity `11.3.0`, UMP, bundletool `1.18.3`, PowerShell, Git LFS. Firebase and remote gameplay/crash telemetry are excluded from v1.

**Spec:** `Docs/superpowers/specs/2026-08-21-galaxy-store-first-release-design.md`

## Superseding v1 no-remote-telemetry decision — 2026-08-21

This section is authoritative and the executable tasks below have been rewritten to match it.

- **Task 5:** keep GMA 11.3.0 and EDM4U 1.2.188 only. Firebase dependencies, tgz archives, manifest metadata, notices, and resolution checks are void for v1.
- **Task 7:** the Firebase adapter implementation is replaced atomically by local non-transport `ConsentAwareAnalyticsService` and `ConsentAwareCrashReporter` on every platform. Keep only the pure event allowlist/bucketing logic and exclusion tests/gate.
- **Task 8:** gameplay event objects may support local tests, but the shipped service is a no-op transport. Do not add Firebase wiring or describe Analytics/Crashlytics toggles as transmitting data; privacy UI covers AdMob/UMP only.
- **Task 9:** build configuration may contain AdMob values only. Do not add Firebase configuration, credentials, Crashlytics symbol upload, or remote-telemetry build steps. General IL2CPP symbols may still be retained for developer/store diagnostics.
- **Task 10:** reviewer notes and Data Safety must describe actual AdMob/UMP behavior and explicitly exclude Firebase/remote gameplay telemetry; do not declare Firebase analytics or diagnostics.
- **Task 11:** replace Firebase opt-in, test-event, upload, and symbolication checks with `scripts/check-no-remote-telemetry.ps1` plus verification that local services do not transmit or persist payloads.
- **Final gate:** run `scripts/check-no-remote-telemetry.ps1` in addition to the other release gates. Any Firebase package, assembly, adapter, archive, manifest entry, declaration, or shipping SDK symbol blocks release.

Reintroducing remote telemetry requires a new approved privacy design and synchronized code, tests, notices, policy, and store declarations.

## Global Constraints

- Release Android first through Samsung Galaxy Store, South Korea only for version 1.0.0.
- Keep Unity `6000.3.21f1`, package ID `com.joyshu93.curioclerknightshift`, API 29 minimum, API 36 target, ARM64, IL2CPP, AAB, portrait, and 60 fps.
- The game must remain fully playable offline without an account, telemetry consent, or an available advertisement.
- Allow only optional rewarded ads: `shift_failed_revive` and `shift_complete_double`; at most one successful reward per shift.
- Ship no remote gameplay Analytics or Crashlytics transport; local boundaries do not transmit or persist payloads.
- Do not commit Samsung/service credentials, signing files, passwords, identity documents, bank documents, live service-account files, or tester personal data.
- Use only English and Korean player-facing copy and update both languages in the same change.
- Do not use Unity MCP or launch/control Unity from an agent. The developer runs Unity generation, tests, builds, and device checks.
- Release-path cash target is KRW 0. No paid assets, testers, reviews, user acquisition, Google Play account, or Steam submission for version 1.
- Percentage metrics require at least 30 unique users on the relevant release; smaller samples are reported as raw counts only.

## File Structure

| Area | Files | Responsibility |
| --- | --- | --- |
| Release truth | `Assets/Scripts/Editor/ReleaseConfiguration.cs`, `Assets/Scripts/Editor/ProjectBuilder.cs` | Own version, package, Android settings, build metadata, and release validation. |
| Service configuration | `Assets/Scripts/Runtime/Infrastructure/ServiceConfiguration.cs`, ignored generated asset under `Assets/Resources` | Carry the rewarded unit ID into a build without storing it in Git. |
| Ads and consent | `Assets/Scripts/Runtime/Infrastructure/Ads/*`, `Assets/Scripts/Runtime/Infrastructure/Privacy/*` | Wrap Google rewarded ads and UMP; resolve each callback once. |
| No remote telemetry | `scripts/check-no-remote-telemetry.ps1`, local `Analytics/*`, `Diagnostics/*` | Exclude Firebase/remote transports and retain pure local schema/bucketing only. |
| Telemetry schema | `Assets/Scripts/Runtime/Infrastructure/Analytics/GameTelemetry.cs`, `AnalyticsEvents.cs` | Define local allowlisted coarse events and parameters without transmission. |
| Presentation | `Assets/Scripts/Runtime/Presentation/GameApp.cs`, `Assets/Scripts/Runtime/Localization/Localizer.cs` | Connect gameplay, privacy controls, reward results, and bilingual status copy. |
| Tests | `Assets/Tests/EditMode/*`, `Assets/Tests/PlayMode/GameAppPlayModeTests.cs` | Lock release configuration, rights gates, service state machines, telemetry, and UI flow. |
| Android | `scripts/build-android.ps1`, `scripts/inspect-aab.ps1` | Build the AAB, validate manifest/ABI, and retain hashes. |
| Store and operations | `Docs/Store/*`, `Docs/ReleaseEvidence/*`, `Docs/Operations/*` | Hold listing copy, declarations, device evidence, rollout log, metrics, postmortem, and portfolio case study. |

---

## Phase A — Release Foundation

### Task 1: Replace the Play-only release workflow with a Samsung release gate

**Files:**
- Create: `scripts/check-release-docs.ps1`
- Create: `Docs/Store/SamsungSellerSetup.md`
- Modify: `README.md`
- Modify: `Docs/ReleaseChecklist.md`
- Modify: `Docs/PrivacyPolicy.md`
- Modify: `Docs/ServiceSetup.md`
- Modify: `Docs/AIAssetProvenance.md`

**Interfaces:**
- Consumes: approved Galaxy Store design and package ID `com.joyshu93.curioclerknightshift`.
- Produces: `scripts/check-release-docs.ps1` with Repository mode for version-controlled readiness and stricter Submission mode requiring resolved public identity fields.

- [ ] **Step 1: Add a failing documentation gate**

Create `scripts/check-release-docs.ps1` with this behavior:

```powershell
param(
    [ValidateSet('Repository', 'Submission')]
    [string]$Mode = 'Repository'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$required = @(
    'README.md',
    'Docs/ReleaseChecklist.md',
    'Docs/PrivacyPolicy.md',
    'Docs/Store/SamsungSellerSetup.md'
)

foreach ($relative in $required) {
    $path = Join-Path $projectRoot $relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing release document: $relative"
    }
}

$releaseChecklist = Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/ReleaseChecklist.md') -Raw
foreach ($forbidden in @('12 testers', 'Play Console personal account', 'Submit production access')) {
    if ($releaseChecklist.Contains($forbidden)) {
        throw "Legacy Play-only release instruction remains: $forbidden"
    }
}

$allPublicDocs = @(
    Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/PrivacyPolicy.md') -Raw
    Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/Store/SamsungSellerSetup.md') -Raw
) -join "`n"
if ($Mode -eq 'Submission' -and $allPublicDocs -match '\[(EFFECTIVE_DATE|DEVELOPER_DISPLAY_NAME|SUPPORT_EMAIL)\]') {
    throw 'Public identity fields are unresolved.'
}

Write-Host 'Release documentation gate passed.'
```

- [ ] **Step 2: Run the gate and verify the current repository fails**

Run:

```powershell
.\scripts\check-release-docs.ps1
```

Expected: FAIL because `Docs/Store/SamsungSellerSetup.md` is missing and `Docs/ReleaseChecklist.md` still contains Play-only tester requirements.

- [ ] **Step 3: Write the Samsung account record and rewrite release documents**

`Docs/Store/SamsungSellerSetup.md` must contain this non-secret structure:

```markdown
# Samsung Seller setup record

- Account region: South Korea
- Seller type: commercial seller
- Distribution country for 1.0.0: South Korea
- Package ID: `com.joyshu93.curioclerknightshift`
- App title: `Curio Clerk: Night Shift`
- Korean title: `기묘한 분실물 야간반`
- Signing custody: managed in Seller Portal; no key material is stored in Git
- Identity and financial evidence: submitted directly by the developer; not copied to this repository
- Seller verification status: record the dated status without identity-document contents
- Public developer name, support email, and privacy URL: record the actual public values after the developer supplies them
```

Rewrite `Docs/ReleaseChecklist.md` around these sections: Samsung account/commercial seller, AI and rights, build and services, owned-device plus Remote Test Lab matrix, certification, 10%/50%/100% rollout, and 1.0.1. Remove tester-count gates. Update `README.md` so Galaxy Store is the current v1 route and Google Play/Steam are deferred. Change Play-specific declaration wording in `Docs/AIAssetProvenance.md` to a store-neutral “store submission decision” field while retaining historic Play guidance as a reference. Leave the three bracketed identity fields in `Docs/PrivacyPolicy.md` until the developer supplies real public values; Repository mode permits them while Submission mode rejects them.

- [ ] **Step 4: Run both documentation modes**

Run:

```powershell
.\scripts\check-release-docs.ps1
.\scripts\check-release-docs.ps1 -Mode Submission
```

Expected: Repository mode prints `Release documentation gate passed.` Submission mode continues to fail until the developer enters the real effective date, public developer name, and support email. That submission failure is an external account/public-identity gate, not a reason to block the repository commit in Step 5.

- [ ] **Step 5: Commit the release-path migration**

```powershell
git add README.md Docs/ReleaseChecklist.md Docs/PrivacyPolicy.md Docs/ServiceSetup.md Docs/AIAssetProvenance.md Docs/Store/SamsungSellerSetup.md scripts/check-release-docs.ps1
git commit -m "docs: adopt Galaxy Store release workflow"
```

### Task 2: Centralize and test Android release configuration

**Files:**
- Create: `Assets/Scripts/Editor/ReleaseConfiguration.cs`
- Create: `Assets/Scripts/Editor/ReleaseConfiguration.cs.meta`
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs`
- Modify: `Assets/Tests/EditMode/EditorAutomationContractTests.cs`
- Modify after human generation: `ProjectSettings/ProjectSettings.asset`

**Interfaces:**
- Consumes: no earlier runtime code; exact Android constants from Global Constraints.
- Produces: `ReleaseConfiguration.Apply()` and constants `VersionName`, `VersionCode`, `PackageId`, `MinimumApi`, and `TargetApi` used by build and tests.

- [ ] **Step 1: Write the failing configuration contract**

Add this test to `EditorAutomationContractTests.cs`:

```csharp
[Test]
public void ReleaseConfiguration_PinsGalaxyStoreVersionAndAndroidContract()
{
    var type = FindType("CurioClerk.Editor.ReleaseConfiguration");
    Assert.That(type, Is.Not.Null);
    Assert.That(type.GetField("VersionName").GetRawConstantValue(), Is.EqualTo("1.0.0"));
    Assert.That(type.GetField("VersionCode").GetRawConstantValue(), Is.EqualTo(10000));
    Assert.That(type.GetField("PackageId").GetRawConstantValue(),
        Is.EqualTo("com.joyshu93.curioclerknightshift"));
    Assert.That(type.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
}
```

- [ ] **Step 2: Ask the developer to run EditMode tests and confirm the new test fails**

Human-run command:

```powershell
.\scripts\test-unity.ps1
```

Expected: the release-configuration test fails because the type does not exist.

- [ ] **Step 3: Implement the single release source of truth**

Create `ReleaseConfiguration.cs`:

```csharp
using Unity.Android.Types;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace CurioClerk.Editor
{
    public static class ReleaseConfiguration
    {
        public const string ProductName = "Curio Clerk: Night Shift";
        public const string PackageId = "com.joyshu93.curioclerknightshift";
        public const string VersionName = "1.0.0";
        public const int VersionCode = 10000;
        public const int MinimumApi = 29;
        public const int TargetApi = 36;

        public static void Apply()
        {
            PlayerSettings.companyName = "joyshu93";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.buildAppBundle = true;
            UserBuildSettings.DebugSymbols.level = DebugSymbolLevel.SymbolTable;
            UserBuildSettings.DebugSymbols.format = DebugSymbolFormat.Zip | DebugSymbolFormat.LegacyExtensions;
        }
    }
}
```

Replace the duplicated constants in `ProjectBuilder.ConfigurePlayer()` with `ReleaseConfiguration.Apply()` followed by the existing Input System configuration. Remove the fallback path containing Unity `6000.2.7f2`; Android tooling must come from the running `6000.3.21f1` editor installation and fail clearly if its child modules are absent.

- [ ] **Step 4: Ask the developer to regenerate settings and run both test suites**

Human actions:

1. Open Unity `6000.3.21f1`.
2. Run `Tools > Curio Clerk > Generate Project Assets`.
3. Close Unity.
4. Run:

```powershell
.\scripts\test-unity.ps1
```

Expected: EditMode and PlayMode both report `Passed`, and `ProjectSettings/ProjectSettings.asset` contains version `1.0.0`, code `10000`, API 29/36, and the unchanged package ID.

- [ ] **Step 5: Commit release configuration**

```powershell
git add Assets/Scripts/Editor/ReleaseConfiguration.cs Assets/Scripts/Editor/ReleaseConfiguration.cs.meta Assets/Scripts/Editor/ProjectBuilder.cs Assets/Tests/EditMode/EditorAutomationContractTests.cs ProjectSettings/ProjectSettings.asset
git commit -m "build: pin Galaxy Store 1.0.0 configuration"
```

### Task 3: Close the known EmojiOne and TextMesh Pro rights gaps

**Files:**
- Create: `Docs/Licenses/uGUI-2.0.0-LICENSE.md`
- Create: `Docs/Licenses/uGUI-2.0.0-source.md`
- Create: `Assets/Tests/EditMode/ReleaseAssetContractTests.cs`
- Create: `Assets/Tests/EditMode/ReleaseAssetContractTests.cs.meta`
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs`
- Modify: `Assets/TextMesh Pro/Resources/TMP Settings.asset`
- Modify: `Docs/ThirdPartyNotices.md`
- Delete: `Assets/TextMesh Pro/Sprites/EmojiOne.png` and `.meta`
- Delete: `Assets/TextMesh Pro/Sprites/EmojiOne.json` and `.meta`
- Delete: `Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt` and `.meta`

**Interfaces:**
- Consumes: uGUI package `2.0.0`, fingerprint `e20f1880fa043157d6b6ff0eb6d12f604094c9b4`, from the pinned Unity package cache.
- Produces: `ContentValidator.ValidateOrThrow()` rejects a default TMP sprite asset or any remaining `EmojiOne` file.

- [ ] **Step 1: Write the failing release-asset test**

Create `ReleaseAssetContractTests.cs`:

```csharp
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ReleaseAssetContractTests
    {
        [Test]
        public void ReleaseAssets_ContainNoEmojiOneAndNoDefaultTmpSpriteAsset()
        {
            var emojiPaths = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.IndexOf("EmojiOne", System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            Assert.That(emojiPaths, Is.Empty);

            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            Assert.That(settings, Is.Not.Null);
            var serialized = new SerializedObject(settings);
            Assert.That(serialized.FindProperty("m_defaultSpriteAsset").objectReferenceValue, Is.Null);
        }
    }
}
```

- [ ] **Step 2: Ask the developer to run EditMode tests and verify failure**

Human-run command:

```powershell
.\scripts\test-unity.ps1
```

Expected: FAIL because EmojiOne files exist and TMP Settings references the default sprite asset.

- [ ] **Step 3: Remove the sample and make regeneration preserve the cleared reference**

Delete the exact EmojiOne files listed above. Add this call after `EnsureTextMeshProResources()` in `ConfigureFontAssets()`:

```csharp
var tmpSettings = Resources.Load<TMP_Settings>("TMP Settings");
var serializedSettings = new SerializedObject(tmpSettings);
var defaultSpriteAsset = serializedSettings.FindProperty("m_defaultSpriteAsset");
if (defaultSpriteAsset == null)
{
    throw new InvalidOperationException("TMP default sprite setting could not be inspected.");
}

defaultSpriteAsset.objectReferenceValue = null;
serializedSettings.ApplyModifiedPropertiesWithoutUndo();
EditorUtility.SetDirty(tmpSettings);
```

Add the same two release rules to `ContentValidator.ValidateOrThrow()` so a build fails even when the test suite was skipped.

- [ ] **Step 4: Archive exact uGUI/TMP license evidence**

Copy the installed package’s `LICENSE.md` text into `Docs/Licenses/uGUI-2.0.0-LICENSE.md`. In `uGUI-2.0.0-source.md`, record version `2.0.0`, fingerprint `e20f1880fa043157d6b6ff0eb6d12f604094c9b4`, package name `com.unity.ugui`, its role as the source of TextMesh Pro essential resources, acquisition date, and the Unity Companion License URL. Mark the EmojiOne row removed and the TMP row resolved in `Docs/ThirdPartyNotices.md`.

- [ ] **Step 5: Regenerate and verify**

Human actions:

1. Run `Tools > Curio Clerk > Generate Project Assets` in Unity.
2. Close Unity.
3. Run `.\scripts\test-unity.ps1`.

Expected: both suites pass; `rg -n "EmojiOne" Assets` returns no result; TMP Settings has a null default sprite asset.

- [ ] **Step 6: Commit rights cleanup**

```powershell
git add Assets/Scripts/Editor/ProjectBuilder.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Tests/EditMode/ReleaseAssetContractTests.cs Assets/Tests/EditMode/ReleaseAssetContractTests.cs.meta "Assets/TextMesh Pro/Resources/TMP Settings.asset" Docs/Licenses Docs/ThirdPartyNotices.md
git add -u "Assets/TextMesh Pro/Sprites"
git commit -m "chore: remove unresolved TMP sample assets"
```

### Task 4: Create an explicit human art-approval gate

**Files:**
- Create: `Docs/ArtReleaseReview.md`
- Modify: `Docs/AIAssetProvenance.md`
- Modify: `Docs/ReleaseChecklist.md`
- Replace after human work: `Assets/Art/Brand/AppIcon.png`

**Interfaces:**
- Consumes: provenance entry `ART-BRAND-001` and the current ImageGen concept icon.
- Produces: a signed human decision for every release visual; no visual may enter RC with status `Prototype only`.

- [ ] **Step 1: Add the art review record**

Create `Docs/ArtReleaseReview.md` with one row per shipped visual and these columns:

```markdown
| Asset ID | Repository/store path | Source type | Human changes | Before/after evidence | Similarity/brand review | Release decision | Reviewer/date |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ART-BRAND-001 | `Assets/Art/Brand/AppIcon.png` | AI concept requiring human creative work | Record composition, silhouette, palette, line, and cleanup decisions | Record both retained files or Git commits | Record search method and result | Blocked until the developer writes `Approved for release` | Developer signs and dates |
```

Add rejection rules for named-artist imitation, protected characters, brand marks, watermarks, signatures, and visually confusing store icons.

- [ ] **Step 2: Have the developer perform and document the creative pass**

The developer must directly choose and change at least the composition/silhouette, palette, and line/shape cleanup; Unity import/resizing alone does not count. Retain the before state in Git history and write the exact human decisions in both `Docs/ArtReleaseReview.md` and `ART-BRAND-001`.

- [ ] **Step 3: Verify the art gate textually and visually**

Run:

```powershell
rg -n "Prototype only|Blocked until|Approved for release" Docs/AIAssetProvenance.md Docs/ArtReleaseReview.md
```

Expected before approval: blocked language remains. Expected after the developer’s visual review: `ART-BRAND-001` and the art-review row both state `Approved for release`, identify the reviewer/date, and point to before/after evidence.

- [ ] **Step 4: Commit only after human approval**

```powershell
git add Assets/Art/Brand/AppIcon.png Docs/AIAssetProvenance.md Docs/ArtReleaseReview.md Docs/ReleaseChecklist.md
git commit -m "art: approve release application icon"
```

---

## Phase B — Consent-Aware Services

### Task 5: Pin Google Mobile Ads and EDM4U reproducibly

**Files:**
- Create: `GooglePackages/com.google.external-dependency-manager-1.2.188.tgz`
- Modify: `.gitattributes`
- Modify: `.gitignore`
- Modify: `Packages/manifest.json`
- Modify after human Unity resolution: `Packages/packages-lock.json`
- Modify: `Docs/ThirdPartyNotices.md`
- Modify: `Docs/ServiceSetup.md`

**Interfaces:**
- Consumes: official Google Mobile Ads Unity package `com.google.ads.mobile` `11.3.0` and EDM4U `1.2.188`.
- Produces: one UPM-based GMA/UMP dependency graph with no Firebase package, archive, Android manifest metadata, or runtime assembly.

- [ ] **Step 1: Define the dependency boundary**

Keep the existing Unity dependencies and add only:

```json
"scopedRegistries": [
  {
    "name": "Google OpenUPM",
    "url": "https://package.openupm.com",
    "scopes": ["com.google"]
  }
],
"dependencies": {
  "com.google.ads.mobile": "11.3.0",
  "com.google.external-dependency-manager": "file:../GooglePackages/com.google.external-dependency-manager-1.2.188.tgz"
}
```

Do not add a Firebase dependency or mix these UPM packages with `.unitypackage` imports.

- [ ] **Step 2: Pin the exact EDM4U archive**

Download the official EDM4U `1.2.188` archive, confirm its internal package name/version, and record its SHA-256 in `Docs/ThirdPartyNotices.md`:

```powershell
Get-FileHash .\GooglePackages\com.google.external-dependency-manager-1.2.188.tgz -Algorithm SHA256
```

Keep `*.tgz filter=lfs diff=lfs merge=lfs -text` in `.gitattributes`. No Firebase archive belongs under `GooglePackages` or elsewhere in the repository.

- [ ] **Step 3: Keep local service configuration out of source control**

Keep the generated GMA settings and service configuration asset ignored:

```text
Assets/Resources/ServiceConfiguration.asset
Assets/Resources/ServiceConfiguration.asset.meta
Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset
Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset.meta
```

Do not add `google-services.json`, a Firebase-only Android manifest, or any service credential.

- [ ] **Step 4: Ask the developer to let Unity resolve GMA and EDM4U**

Human actions:

1. Open the project in Unity `6000.3.21f1`.
2. Wait for Package Manager and External Dependency Manager to finish.
3. Confirm the Console has no compilation or Android dependency-resolution error.
4. Close Unity and retain `Packages/packages-lock.json`.

Expected: GMA resolves at `11.3.0`, EDM4U resolves at `1.2.188`, and the resolved graph contains no Firebase package or assembly. This Unity checkpoint is human-only.

- [ ] **Step 5: Run repository dependency checks**

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
Get-Content .\Packages\manifest.json | ConvertFrom-Json | Out-Null
Get-Content .\Packages\packages-lock.json | ConvertFrom-Json | Out-Null
git status --short
```

Expected: the no-remote gate passes, JSON parses, GMA/EDM remain pinned, and no credential is staged.

- [ ] **Step 6: Commit the pinned ad dependencies and notices**

```powershell
git add .gitattributes .gitignore GooglePackages/com.google.external-dependency-manager-1.2.188.tgz Packages/manifest.json Packages/packages-lock.json Docs/ThirdPartyNotices.md Docs/ServiceSetup.md
git commit -m "build: pin rewarded ad dependencies"
```

### Task 6: Implement rewarded-ad and UMP adapters with once-only callbacks

**Files:**
- Create: `Assets/Scripts/Runtime/Infrastructure/Ads/RewardedAdResult.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Ads/IRewardedAdClient.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Ads/RewardedAdService.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Ads/GoogleRewardedAdService.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Privacy/GoogleUmpPrivacyService.cs`
- Create: corresponding `.meta` files
- Modify: `Assets/Scripts/Runtime/Infrastructure/Ads/IAdService.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/Ads/DefaultAdService.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/Privacy/IPrivacyService.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs`
- Modify: `Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef`
- Create: `Assets/Tests/EditMode/RewardedAdStateContractTests.cs`
- Create: corresponding `.meta` file

**Interfaces:**
- Consumes: GMA `RewardedAd.Load`, `RewardedAd.CanShowAd`, `RewardedAd.Show`, UMP `ConsentInformation.Update`, `ConsentForm.LoadAndShowConsentFormIfRequired`, and `ConsentForm.ShowPrivacyOptionsForm`.
- Produces: `IRewardedAdClient`, tested `RewardedAdService`, Android `GoogleRewardedAdService`, `IAdService.SetRequestPermission(bool)`, `ShowRewarded(string, Action<RewardedAdResult>)`, and an IPrivacyService callback that returns current `ConsentInformation.CanRequestAds()`.

- [ ] **Step 1: Write failing state-machine tests**

Define these required cases in `RewardedAdStateContractTests.cs` using a fake Google-client boundary:

```csharp
[TestCase(true, false, RewardedAdResult.Earned)]
[TestCase(false, true, RewardedAdResult.Failed)]
public void RewardedCallback_CompletesExactlyOnceWhenSdkSendsDuplicateTerminalCallbacks(
    bool rewardFirst, bool failureSecond, RewardedAdResult expected)
{
    var fake = new FakeRewardedClient();
    var service = new RewardedAdService(fake, "test-unit");
    service.SetRequestPermission(true);
    var results = new List<RewardedAdResult>();
    service.ShowRewarded("shift_complete_double", results.Add);
    fake.Emit(rewardFirst ? RewardedAdResult.Earned : RewardedAdResult.Failed);
    if (failureSecond) fake.Emit(RewardedAdResult.Failed);
    else fake.Emit(RewardedAdResult.Dismissed);
    Assert.That(results, Is.EqualTo(new[] { expected }));
}
```

Also test: permission false never loads, invalid placement returns `Unavailable`, close without earned reward returns `Dismissed`, load failure returns `Failed`, earned reward reloads the next ad, and only the two allowlisted placements are accepted.

- [ ] **Step 2: Ask the developer to run tests and verify failure**

Human-run command: `.\scripts\test-unity.ps1`

Expected: compile/test failure because `RewardedAdResult`, `RewardedAdService`, and `SetRequestPermission` do not exist.

- [ ] **Step 3: Update the ad contract**

Use this result enum and interface:

```csharp
public enum RewardedAdResult
{
    Earned,
    Dismissed,
    Failed,
    Unavailable
}

public interface IAdService
{
    bool IsRewardedReady { get; }
    void SetRequestPermission(bool allowed);
    void ShowRewarded(string placement, Action<RewardedAdResult> completed);
}
```

`RewardedAdService` must own one active request callback, clear it before invoking user code, destroy the consumed `RewardedAd`, and load a replacement only when permission remains true. `GoogleRewardedAdService` adapts the official static/load/full-screen callbacks to this tested state machine. Keep `DefaultAdService` as an Editor-only deterministic earned reward and a non-Editor unavailable fallback.

Use this SDK-free client boundary so EditMode tests can force every terminal callback order:

```csharp
public interface IRewardedAdClient
{
    bool IsReady { get; }
    void SetRequestPermission(bool allowed);
    void Show(Action<RewardedAdResult> completed);
}
```

- [ ] **Step 4: Implement UMP on every launch and privacy-options re-entry**

`GoogleUmpPrivacyService.RequestConsent` must call:

```csharp
ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
{
    ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
        completed?.Invoke(ConsentInformation.CanRequestAds()));
});
```

Even when update/form returns an error, return the current official `CanRequestAds()` value; do not infer consent from a locally saved string. `PrivacyOptionsRequired` is true only for `PrivacyOptionsRequirementStatus.Required`. `ShowPrivacyOptions` calls `ConsentForm.ShowPrivacyOptionsForm` and returns the updated `CanRequestAds()` result.

- [ ] **Step 5: Wire production adapters and assembly references**

`ServiceFactory` returns Google adapters on Android player builds and deterministic defaults in the Editor. Add the exact GMA runtime assembly references exposed by the installed package to `CurioClerk.Runtime.asmdef`. Do not use reflection for runtime SDK calls.

- [ ] **Step 6: Ask the developer to run tests and an Editor smoke check**

Human actions:

1. Run `.\scripts\test-unity.ps1` and confirm both suites pass.
2. Open the Main scene in Editor Play Mode.
3. Confirm gameplay remains available and the Editor reward simulation grants only one reward.

- [ ] **Step 7: Commit ads and consent**

```powershell
git add Assets/Scripts/Runtime/Infrastructure/Ads Assets/Scripts/Runtime/Infrastructure/Privacy Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef Assets/Tests/EditMode/RewardedAdStateContractTests.cs Assets/Tests/EditMode/RewardedAdStateContractTests.cs.meta
git commit -m "feat: integrate consent-aware rewarded ads"
```

### Task 7: Enforce local no-remote telemetry and a pure coarse schema

**Files:**
- Create: `scripts/check-no-remote-telemetry.ps1`
- Create: `scripts/test-no-remote-telemetry-gate.ps1`
- Create: `Assets/Scripts/Runtime/Infrastructure/Analytics/AnalyticsEvents.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Analytics/GameTelemetry.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs`
- Create: `Assets/Scripts/Runtime/Infrastructure/Diagnostics/ConsentAwareCrashReporter.cs`
- Create: corresponding `.meta` files
- Modify: `Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs`
- Modify: `Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef`
- Create: `Assets/Tests/EditMode/TelemetryContractTests.cs`
- Create: corresponding `.meta` file
- Modify: release privacy/setup/checklist documents and this active plan/spec

**Interfaces:**
- Consumes: `IAnalyticsService.SetConsent/Track`, `ICrashReporter.SetConsent/Log/Record`, and `IClock.LocalNow`.
- Produces: local synchronous non-transport services, a pure allowlist/bucket helper, and an executable release gate that blocks remote gameplay/crash telemetry.

- [ ] **Step 1: Write the gate contract and capture controlled RED**

Create a self-cleaning test fixture outside shipping paths. It must start from a minimal allowed repository, then introduce all three realistic rewires:

- an Android-only analytics implementation using a direct network API;
- a target-conditional analytics/crash factory branch;
- a nested first-party `AndroidManifest.xml` collection component or metadata entry.

Run the production gate against the mutated fixture and require it to fail with actionable messages. Never claim a Unity-test RED without a human Unity run.

- [ ] **Step 2: Implement the no-remote release gate**

The gate has `Repository` (default) and `Release` modes. Both parse `Packages/manifest.json`, `Packages/packages-lock.json`, and `CurioClerk.Runtime.asmdef`; preserve exact GMA/UMP/EDM dependencies; and reject Firebase/telemetry packages, precompiled references, adapters, archives, and collection metadata. Repository mode reports that the required lock entries still await human Unity resolution. Release mode fails until the resolved lock contains the exact required GMA and EDM entries.

It must also:

- recursively enforce an explicit v1 allowlist for runtime Analytics and Diagnostics files;
- content-scan every approved Analytics/Diagnostics source for executable file/stream/storage/serialization/debug/console/logger calls that could persist or log a payload;
- scan first-party runtime C# after removing comments and literals for documented direct network and common telemetry/crash SDK markers, leaving the GMA/UMP adapter path intact;
- require `ServiceFactory` to construct only the approved local analytics and crash implementations with no target-conditional branch;
- recursively inspect every first-party `AndroidManifest.xml` below `Assets`.

Any newly added surface fails closed until this gate is deliberately reviewed.

- [ ] **Step 3: Keep the production services local**

`ConsentAwareAnalyticsService` and `ConsentAwareCrashReporter` may maintain only an in-memory enabled flag. `Track`, `Log`, and `Record` perform no network call and persist no report. `ServiceFactory` directly constructs these same implementations on every platform.

The runtime asmdef contains GMA/UMP references only. There is no Firebase runtime folder, adapter, package, archive, precompiled reference, configuration, or Firebase-only Android manifest.

- [ ] **Step 4: Keep the event schema pure and coarse**

`AnalyticsEvents` accepts only the documented event and parameter names. `GameTelemetry` creates only allowlisted in-memory event objects and deterministic coarse buckets:

```csharp
public static string SortedCountBucket(int count) => count <= 3 ? "0_3" : count <= 7 ? "4_7" : "8_12";
public static string DurationBucket(double seconds) => seconds < 60 ? "under_60" : seconds < 120 ? "60_119" : "120_plus";
```

Reject unknown names/keys and values that could carry free text, artifact descriptions/IDs, locale-derived personal data, device/account identifiers, exact timestamps, seeds, or exact user paths. No event is uploaded or persisted.

- [ ] **Step 5: Test the exclusion and local behavior**

EditMode contracts name the privacy breaks they prevent: factory reintroduction of a transport, pre-consent persistence, post-withdrawal transmission, schema escape, and identity/free-text values. Ask the developer to run Unity tests as a human checkpoint.

Run the repository checks directly:

```powershell
.\scripts\test-no-remote-telemetry-gate.ps1
.\scripts\check-no-remote-telemetry.ps1
Get-Content .\Packages\manifest.json | ConvertFrom-Json | Out-Null
Get-Content .\Packages\packages-lock.json | ConvertFrom-Json | Out-Null
Get-Content .\Assets\Scripts\Runtime\CurioClerk.Runtime.asmdef | ConvertFrom-Json | Out-Null
```

Expected: controlled mutation RED is observed internally, then the clean fixture and production repository pass. Unity execution remains human-only.

- [ ] **Step 6: Commit the local boundary and gate**

```powershell
git add scripts/check-no-remote-telemetry.ps1 scripts/test-no-remote-telemetry-gate.ps1 Assets/Scripts/Runtime/Infrastructure/Analytics Assets/Scripts/Runtime/Infrastructure/Diagnostics Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef Assets/Tests/EditMode/TelemetryContractTests.cs Assets/Tests/EditMode/TelemetryContractTests.cs.meta Docs
git commit -m "fix: harden no-telemetry release guard"
```

### Task 8: Connect rewarded-ad results and AdMob/UMP privacy UI

**Files:**
- Create: `Assets/Scripts/Runtime/AssemblyInfo.cs`
- Create: `Assets/Scripts/Runtime/AssemblyInfo.cs.meta`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs`
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`
- Modify: `Assets/Tests/PlayMode/CurioClerk.PlayModeTests.asmdef`

**Interfaces:**
- Consumes: `RewardedAdResult`, `IAdService.SetRequestPermission`, and optional pure `GameTelemetry` event construction.
- Produces: idempotent reward behavior, a user-visible non-blocking ad result, UMP privacy-options re-entry, and test-only service injection. It does not produce remote gameplay/crash telemetry or collection toggles.

- [x] **Step 1: Write failing PlayMode reward contracts**

Use a deferred fake ad service to prove failed/dismissed/unavailable results never grant or remove coins, an earned result grants once, and duplicate terminal callbacks cannot grant twice. A recording analytics fake may inspect pure allowlisted event objects in tests, but production remains the local non-transport service.

Under `UNITY_INCLUDE_TESTS`, `ServiceFactory.SetTestServices(...)` and `ResetTestServices()` expose only the test injection point. `GameApp` guards every reward request with a local once-only completion flag.

- [ ] **Step 2: Ask the developer to run tests and verify failure**

Human-run command: `.\scripts\test-unity.ps1`

Expected: compile or assertion failure because `GameApp` still uses the old reward callback and does not handle every terminal result idempotently.

- [x] **Step 3: Wire UMP consent to ad initialization**

In both `RequestAdConsent` and `ShowAdPrivacyOptions`, call:

```csharp
_adService.SetRequestPermission(_canRequestAds);
```

Do this after computing `_canRequestAds`. A false result prevents requests and leaves gameplay enabled. Privacy UI concerns AdMob/UMP only; do not create Analytics or Crashlytics consent toggles.

- [x] **Step 4: Wire reward results and player feedback**

Map results to `earned`, `dismissed`, `failed`, or `unavailable`; call `TryDoubleCoins`/`TryRevive` only for `Earned`. Add EN/KO keys `ad_dismissed` and `ad_failed`, display the message without removing base rewards, and rely on the ad service's once-only completion.

If local gameplay event objects are useful for tests, create only the allowlisted `reward_offer_shown` and `reward_result` values. The shipping local service discards them synchronously without upload or persistence.

- [x] **Step 5: Keep optional lifecycle events local**

Optional tutorial, shift, and cosmetic events may be constructed through `GameTelemetry` solely as pure coarse values. Do not transmit or persist them, and do not attach score, coins, exact timestamps, seeds, artifact identifiers/descriptions, user paths, or free text.

- [ ] **Step 6: Regenerate localization and run human Unity checks**

Human actions:

1. Run `Tools > Curio Clerk > Generate Project Assets`.
2. Close Unity.
3. Run `.\scripts\test-unity.ps1`.

Expected: all tests pass, the EN/KO assets contain `ad_dismissed` and `ad_failed`, gameplay remains available without ads, and there are no Analytics/Crashlytics controls.

- [x] **Step 7: Run the release boundary and commit gameplay integration**

```powershell
.\scripts\check-no-remote-telemetry.ps1
git add Assets/Scripts/Runtime/AssemblyInfo.cs Assets/Scripts/Runtime/AssemblyInfo.cs.meta Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs Assets/Tests/PlayMode/CurioClerk.PlayModeTests.asmdef Assets/Localization
git commit -m "feat: connect consent-aware rewards"
```

---

## Phase C — Certification Package

### Task 9: Make AdMob-only release builds reproducible and inspectable

**Files:**
- Create: `Assets/Scripts/Runtime/Infrastructure/ServiceConfiguration.cs`
- Create: corresponding `.meta` file
- Modify: `Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs`
- Create: `Assets/Scripts/Editor/ReleaseBuildManifest.cs`
- Create: corresponding `.meta` file
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs`
- Modify: `scripts/build-android.ps1`
- Create: `scripts/inspect-aab.ps1`
- Create: `tools/bundletool/.gitkeep`
- Modify: `.gitignore`
- Modify: `Docs/ServiceSetup.md`

**Interfaces:**
- Consumes: environment values `CURIO_ADMOB_APP_ID`, `CURIO_ADMOB_REWARDED_ID`, `CURIO_ANDROID_KEYSTORE_PATH`, `CURIO_ANDROID_KEYSTORE_PASS`, `CURIO_ANDROID_KEY_ALIAS`, and `CURIO_ANDROID_KEY_PASS` supplied only to the human-run release process.
- Produces: `Builds/Android/CurioClerk.aab`, public symbols zip, and `Builds/Android/CurioClerk-build.json` containing non-secret build metadata and SHA-256.

- [x] **Step 1: Add failing Editor contract tests**

Extend `EditorAutomationContractTests` to require `ProjectBuilder.ValidateServiceIds(string, string)`, `ProjectBuilder.ValidateReleaseEnvironment()`, `ProjectBuilder.BuildAndroid()`, and `ReleaseBuildManifest.Write(string)`. Call only the pure `ValidateServiceIds` method in tests and assert that it rejects blank, malformed, or Google sample IDs without starting a Unity build.

- [x] **Step 2: Implement local service configuration generation**

`ServiceConfiguration` contains one serialized `AndroidRewardedAdUnitId` and is loaded from `Resources`. `ProjectBuilder.ValidateReleaseEnvironment` reads the two AdMob values, validates `ca-app-pub-<digits>~<digits>` for the app and `ca-app-pub-<digits>/<digits>` for rewarded, rejects Google sample IDs, writes the rewarded ID to the ignored Resources asset, and uses `SerializedObject` at `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` to set `adMobAndroidAppId`. Never log either full ID.

In non-development Android builds, `ServiceFactory` loads the generated `ServiceConfiguration` and constructs `GoogleRewardedAdService` only from its validated rewarded ID. Development builds retain Google's official sample rewarded ID. Missing or invalid release configuration returns the unavailable local service; no live or sample identifier is hard-coded as a release fallback.

Configure the keystore from environment only for the release build and clear the in-memory password fields in a `finally` block after `BuildPipeline.BuildPlayer` returns.

- [x] **Step 3: Write the build manifest**

After a successful AAB build, compute SHA-256 and serialize:

```json
{
  "product": "Curio Clerk: Night Shift",
  "packageId": "com.joyshu93.curioclerknightshift",
  "versionName": "1.0.0",
  "versionCode": 10000,
  "unityVersion": "6000.3.21f1",
  "minimumApi": 29,
  "targetApi": 36,
  "architecture": "ARM64",
  "backend": "IL2CPP",
  "aabSha256": "64 uppercase hexadecimal characters"
}
```

No filesystem username, service ID, keystore path, or password may appear.

- [x] **Step 4: Harden the PowerShell build wrapper**

`scripts/build-android.ps1` must run `scripts/check-no-remote-telemetry.ps1 -Mode Release` and fail before Unity starts if the telemetry boundary fails or any of the six environment values are absent. It must not print secret values. It accepts AdMob configuration only and has no remote-telemetry configuration or upload step. After build, require the AAB, general IL2CPP debugging archive, and JSON manifest; compare the manifest SHA-256 with `Get-FileHash`.

- [ ] **Step 5: Pin bundletool and implement AAB inspection**

Download official `bundletool-all-1.18.3.jar` to ignored `tools/bundletool/`. `scripts/inspect-aab.ps1` accepts the AAB and jar paths, then runs:

```powershell
java -jar $BundletoolPath validate --bundle=$AabPath
java -jar $BundletoolPath dump manifest --bundle=$AabPath --module=base
```

The script must assert package ID, version name/code, min SDK 29, target SDK 36, and the presence of `arm64-v8a` native libraries when native libraries exist. Save sanitized output under `Builds/Android/inspection.txt`.

- [ ] **Step 6: Ask the developer to run tests and a release build**

Human-run commands after exporting the six environment values in the current terminal:

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
.\scripts\test-unity.ps1
.\scripts\build-android.ps1
.\scripts\inspect-aab.ps1 -AabPath .\Builds\Android\CurioClerk.aab -BundletoolPath .\tools\bundletool\bundletool-all-1.18.3.jar
```

Expected: tests pass, all three build artifacts exist, SHA-256 values match, bundletool validation succeeds, and no secret appears in Git status or logs.

- [x] **Step 7: Commit build automation without generated artifacts or secrets**

```powershell
git add Assets/Scripts/Runtime/Infrastructure/ServiceConfiguration.cs Assets/Scripts/Runtime/Infrastructure/ServiceConfiguration.cs.meta Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs Assets/Scripts/Editor/ReleaseBuildManifest.cs Assets/Scripts/Editor/ReleaseBuildManifest.cs.meta Assets/Scripts/Editor/ProjectBuilder.cs scripts/build-android.ps1 scripts/inspect-aab.ps1 tools/bundletool/.gitkeep .gitignore Docs/ServiceSetup.md
git commit -m "build: validate signed Galaxy Store bundles"
```

### Task 10: Create AdMob/UMP store declarations, review notes, and release checks

**Files:**
- Create: `Docs/Store/GalaxyStoreListing.ko.md`
- Create: `Docs/Store/GalaxyStoreListing.en.md`
- Create: `Docs/Store/ReviewNotes.md`
- Create: `Docs/Store/DataSafety.md`
- Create: `Docs/Store/RatingAnswers.md`
- Create: `Docs/Store/AssetInventory.md`
- Create: `Docs/ReleaseEvidence/1.0.0/README.md`
- Modify: `scripts/check-release-docs.ps1`
- Modify: `Docs/ReleaseChecklist.md`

**Interfaces:**
- Consumes: actual SDK behavior, privacy policy, AI/third-party ledgers, package ID, and approved EN/KO product copy.
- Produces: one internally consistent Seller Portal submission package and an evidence folder containing no personal documents.

- [ ] **Step 1: Expand the release-doc gate before creating store files**

Require every file listed above and make Submission mode fail when any contains unresolved bracketed identity/date tokens, claims an account/backend/IAP, omits rewarded-ad disclosure, claims collection of the absent remote gameplay/crash telemetry, or describes AdMob/UMP data types that disagree with `Docs/PrivacyPolicy.md`. The repository no-remote gate remains a separate required release check.

- [ ] **Step 2: Run the gate and verify failure**

Run `.\scripts\check-release-docs.ps1 -Mode Submission`.

Expected: FAIL because the six store documents and evidence README do not exist.

- [ ] **Step 3: Write synchronized Korean and English listing copy**

Both listings must state: warm occult rule-sorting puzzle, portrait one-hand play, 12-item shifts, Repair/Storage/Vault, Hold, casebook, desk charms, offline gameplay, optional rewarded ads, no account, and no IAP. Do not claim awards, rankings, revenue, multiplayer, cloud save, or features absent from the build.

- [ ] **Step 4: Write exact reviewer instructions**

`ReviewNotes.md` must explain this path:

1. Launch without an account.
2. Complete the tutorial.
3. Finish or fail a shift.
4. Observe that base progression works without an ad.
5. If a rewarded ad is available from the certification build's release configuration, exercise revive or double coins once and verify the other option is locked. If no ad is available, verify that base progression remains usable and no reward is removed.
6. Open Settings, change language, and open UMP privacy options when required.
7. Force-stop/relaunch to confirm local save recovery.

- [ ] **Step 5: Reconcile Data Safety and rights inventories**

`DataSafety.md` must list actual AdMob/UMP device identifier, advertising, diagnostics, consent, and approximate network-derived location behavior exactly as configured, and must not declare absent Firebase gameplay analytics/crash collection. `AssetInventory.md` maps every uploaded icon, screenshot, and video to `Docs/AIAssetProvenance.md` and `Docs/ArtReleaseReview.md`. `RatingAnswers.md` records the developer’s final questionnaire answers without copying identity information.

- [ ] **Step 6: Run the gate and commit the store package**

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
.\scripts\check-release-docs.ps1 -Mode Submission
git add Docs/Store Docs/ReleaseEvidence/1.0.0/README.md Docs/ReleaseChecklist.md scripts/check-release-docs.ps1
git commit -m "docs: prepare Galaxy Store submission package"
```

Expected for agent-completable work: Repository mode passes. Submission mode remains intentionally blocked until the developer supplies the real public identity/date/privacy-hosting values and the separate Release no-remote gate passes after human Unity package resolution.

### Task 11: Execute the no-tester technical validation matrix

**Files:**
- Create: `Docs/ReleaseEvidence/1.0.0/automated-tests.md`
- Create: `Docs/ReleaseEvidence/1.0.0/owned-device.md`
- Create: `Docs/ReleaseEvidence/1.0.0/remote-test-lab.md`
- Create: `Docs/ReleaseEvidence/1.0.0/service-validation.md`
- Create: `Docs/ReleaseEvidence/1.0.0/rc-decision.md`
- Modify: `Docs/ReleaseChecklist.md`

**Interfaces:**
- Consumes: signed RC AAB, inspection output, one owned Samsung phone, and at least three available Samsung Remote Test Lab profiles.
- Produces: dated evidence for RC acceptance; no pre-release retention/usability claim.

- [ ] **Step 1: Record automated-test evidence**

The developer runs `.\scripts\test-unity.ps1` and records date, Git SHA, Unity version, EditMode pass/total, PlayMode pass/total, and the retained local XML/log paths. Do not commit machine-absolute paths or logs containing account details.

- [ ] **Step 2: Complete the owned-device matrix**

On one owned Samsung device, record model, Android/API level, screen resolution/aspect, install source, AAB-derived build version, and pass/fail for: first launch, tutorial, three shifts, drag/buttons/Hold, offline mode, pause/resume, force-stop recovery, corrupt-save recovery, EN/KO change, UMP grant/deny/privacy-options changes, ad earned/dismissed/no-fill/failure/duplicate callback, and relaunch.

- [ ] **Step 3: Complete three Remote Test Lab profiles**

Choose one available Galaxy A-series slab, one Galaxy S-series slab, and one Galaxy Fold profile spanning at least two Android major versions and two aspect classes. Run install/launch/tutorial/one-shift/language/safe-area/pause-resume checks. Record the exact models chosen, lab date, OS, and results; do not imply these are independent human usability tests.

- [ ] **Step 4: Validate only the shipped AdMob/UMP service path and no-remote boundary**

With the developer's own UMP choices and test devices:

- run `scripts/check-no-remote-telemetry.ps1 -Mode Release` against the exact RC source after human Unity package resolution;
- verify UMP update occurs every launch;
- verify no ad request precedes `CanRequestAds()`;
- verify an unavailable ad leaves base progression intact;
- verify one earned reward and zero duplicate grants;
- inspect app-attributed traffic, confirm that no gameplay-event or crash-report endpoint/payload appears, and classify any observed third-party SDK service traffic as AdMob/UMP only;
- verify the resolved package/player graph contains no remote gameplay/crash telemetry transport;
- verify local analytics/crash services do not transmit, log, cache, or persist payloads.

- [ ] **Step 5: Make and sign the RC decision**

`rc-decision.md` records Git SHA, AAB SHA-256, version, unresolved defect count by severity, rights-gate status, store-doc gate status, test matrix status, and the developer’s dated `Accept RC` or `Reject RC` decision. Acceptance requires P0/P1 = 0 and every rights/privacy/build gate complete.

- [ ] **Step 6: Commit sanitized evidence**

```powershell
git add Docs/ReleaseEvidence/1.0.0 Docs/ReleaseChecklist.md
git commit -m "test: record Galaxy Store 1.0.0 release evidence"
```

---

## Phase D — Publication, Maintenance, and Career Evidence

### Task 12: Submit and operate the staged 1.0.0 rollout

**Files:**
- Create: `Docs/Operations/RolloutLog.md`
- Create: `Docs/Operations/MetricsSnapshot.md`
- Create: `Docs/Operations/CertificationLog.md`
- Modify: `Docs/ReleaseChecklist.md`

**Interfaces:**
- Consumes: Samsung commercial seller approval, accepted RC, Seller Portal content ID retained outside public docs when desired, and Samsung review results.
- Produces: dated certification and 10% → 50% → 100% rollout decisions with at least 24 hours between healthy stages.

- [ ] **Step 1: Register and submit through Seller Portal manually**

The developer uploads the accepted AAB, supplies signing material directly to Seller Portal, enters the prepared listing/declarations, selects South Korea, and chooses manual publication with 10% staged rollout. No agent handles identity, bank, signing, or Seller Portal credential data.

- [ ] **Step 2: Record certification findings**

For every Samsung finding, record: submission version, date, Samsung category, reproducible symptom, affected device, repository issue/fix commit, human verification, resubmission date, and result. If certification passes first time, record the pre-submit defect chosen for the career case study instead.

- [ ] **Step 3: Apply rollout gates**

At 10% and 50%, wait at least 24 hours and record installs, unique users, raw crashes, review count, reward anomalies, and known P0/P1 defects. Advance only with P0/P1 = 0, no reproducible crash, no known missing/duplicate reward, and declarations still matching the build. Pause and fix otherwise.

- [ ] **Step 4: Report metrics without overstating small samples**

If unique users are below 30, record raw counts and the phrase `sample too small for a percentage gate`. At 30 or more, compute crash-free users as:

```text
100 × (unique users − users with at least one crash) / unique users
```

Require at least 99% for the release health statement.

- [ ] **Step 5: Commit public-safe operational records**

```powershell
git add Docs/Operations Docs/ReleaseChecklist.md
git commit -m "docs: record Galaxy Store 1.0.0 rollout"
```

### Task 13: Publish the 1.0.1 maintenance release within 14 days

**Files:**
- Modify: `Assets/Scripts/Editor/ReleaseConfiguration.cs`
- Modify: `Assets/Tests/EditMode/EditorAutomationContractTests.cs`
- Create: `Docs/ReleaseEvidence/1.0.1/README.md`
- Create: `Docs/Operations/CHANGELOG.md`
- Modify: relevant production/test files for one evidence-backed fix or maintenance improvement

**Interfaces:**
- Consumes: 1.0.0 rollout evidence or the documented post-release audit.
- Produces: version `1.0.1`, code `10001`, normal Samsung update review, and published maintenance evidence no later than 14 calendar days after 1.0.0 reaches 100%.

- [ ] **Step 1: Select one bounded maintenance change**

Prefer a real crash, certification, review, localization, safe-area, or usability issue. If none exists, choose one small verified improvement from the post-release audit, such as correcting one misleading EN/KO label or improving one reproducible accessibility/readability issue. Record the evidence and acceptance criterion before code changes.

- [ ] **Step 2: Write the failing regression test**

Add the narrowest EditMode or PlayMode test that reproduces the selected issue. Run `.\scripts\test-unity.ps1`; expected result is failure for the recorded symptom.

- [ ] **Step 3: Implement the minimal fix and bump version**

Change:

```csharp
public const string VersionName = "1.0.1";
public const int VersionCode = 10001;
```

Update the configuration contract test to the same values and implement only the selected fix.

- [ ] **Step 4: Verify the full release path again**

Human-run sequence:

```powershell
.\scripts\test-unity.ps1
.\scripts\check-release-docs.ps1 -Mode Submission
.\scripts\build-android.ps1
.\scripts\inspect-aab.ps1 -AabPath .\Builds\Android\CurioClerk.aab -BundletoolPath .\tools\bundletool\bundletool-all-1.18.3.jar
```

Repeat owned-device smoke tests and any device profile affected by the change. Record AAB SHA-256, test results, fix evidence, and RC decision in `Docs/ReleaseEvidence/1.0.1/README.md`.

- [ ] **Step 5: Submit, publish, and commit 1.0.1 evidence**

Submit through the normal Samsung update path and record publication date and listing URL. Commit source and sanitized evidence:

```powershell
git add Assets/Scripts Assets/Tests Docs/ReleaseEvidence/1.0.1 Docs/Operations/CHANGELOG.md ProjectSettings
git commit -m "release: prepare Curio Clerk 1.0.1"
```

### Task 14: Assemble the career evidence package and postmortem

**Files:**
- Create: `Docs/Portfolio/ProjectCaseStudy.md`
- Create: `Docs/Portfolio/Architecture.md`
- Create: `Docs/Portfolio/ReleaseCaseStudy.md`
- Create: `Docs/Portfolio/Postmortem.md`
- Create: `Docs/Portfolio/MediaInventory.md`
- Modify: `README.md`

**Interfaces:**
- Consumes: public Galaxy Store listing/version history, 1.0.0/1.0.1 evidence, architecture, tests, certification record, metrics, and approved media.
- Produces: a reviewable solo Unity release case study whose core explanation does not require a Samsung device.

- [ ] **Step 1: Write the one-page project case study**

Include product, role, scope, Unity/C#/Android stack, deterministic rule engine, resilient JSON save, EN/KO localization, optional rewarded ads, consent architecture, the v1 no-remote-telemetry boundary, test counts, Galaxy certification, staged rollout, and the 1.0.1 update. State the exact solo responsibilities and AI-assistance/provenance controls. Do not claim commercial success without numbers.

- [ ] **Step 2: Write the architecture document**

Document the dependency direction:

```text
Presentation -> Runtime interfaces/adapters -> external SDKs
Presentation -> Core
Editor/build -> Runtime content + release configuration
Core -> no Unity or external SDK dependency
```

Link to small sanitized code/test examples for rule priority, save recovery, reward idempotency, consent gating, and build validation.

- [ ] **Step 3: Produce approved media**

Create a 30–60 second video using only shipped gameplay and approved assets: menu, rule reading, drag sort, Hold, success/failure, casebook, and settings/privacy. Record every visible AI-assisted asset in `MediaInventory.md`; do not use concept-only footage, unlicensed music, personal notifications, device identifiers, or service dashboards containing account data.

- [ ] **Step 4: Write the release case study and postmortem**

Describe one Samsung rejection or pre-submit defect with evidence → root cause → fix → verification. The postmortem covers scope cuts, no-external-tester limitation, zero-cash choices, AI/license controls, actual metrics with sample size, what changed in 1.0.1, and the next-store decision.

- [ ] **Step 5: Update README and commit**

Add links to the public Galaxy Store page, gameplay video, and portfolio docs. The resume-safe claim is:

```text
Designed, implemented, certified, released, and maintained a solo Unity Android game on Samsung Galaxy Store.
```

Commit:

```powershell
git add README.md Docs/Portfolio
git commit -m "docs: publish Curio Clerk release case study"
```

## Final Verification Gate

The release program is complete only after fresh evidence confirms all of the following:

```powershell
.\scripts\check-no-remote-telemetry.ps1 -Mode Release
.\scripts\check-release-docs.ps1 -Mode Submission
.\scripts\test-unity.ps1
.\scripts\build-android.ps1
.\scripts\inspect-aab.ps1 -AabPath .\Builds\Android\CurioClerk.aab -BundletoolPath .\tools\bundletool\bundletool-all-1.18.3.jar
git status --short
```

- Human-supplied output shows all EditMode and PlayMode tests passed.
- The AAB inspection matches the currently submitted version and SHA-256.
- `git status --short` is empty and no credential is tracked.
- Samsung Galaxy Store South Korea publicly serves 1.0.0 and then 1.0.1.
- 1.0.1 publication is within 14 calendar days after 1.0.0 reached 100% rollout.
- P0/P1 defects are zero and no reward loss/duplication is known.
- The Release-mode no-remote gate passes against the human-resolved package graph.
- Rights, privacy policy, and store declarations match actual AdMob/UMP behavior and do not declare absent gameplay/crash telemetry.
- Metrics below 30 unique users are raw counts; at 30 or more, crash-free users are at least 99%.
- The gameplay video, technical case study, and postmortem are reviewable independently of the store app.

## Official Implementation References

- Google Mobile Ads Unity setup/version: https://developers.google.com/admob/unity/quick-start
- Google Mobile Ads Unity `11.3.0` release: https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.3.0
- UMP Unity flow: https://developers.google.com/admob/unity/privacy
- EDM4U package information (`1.2.188`): https://developers.google.com/unity/packages
- bundletool `1.18.3`: https://github.com/google/bundletool/releases/tag/1.18.3
- Galaxy Store registration/staged rollout/Data Safety: https://developer.samsung.com/galaxy-store/launch.html
- Galaxy Store API/AAB/64-bit requirements: https://developer.samsung.com/galaxy-store/faq.html
- Galaxy Store self-check list: https://developer.samsung.com/galaxy-store/self-check-list-galaxy.html?lang=en

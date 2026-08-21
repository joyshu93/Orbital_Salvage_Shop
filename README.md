# Curio Clerk: Night Shift

**기묘한 분실물 야간반** is a portrait, one-handed Android sorting puzzle set in a warm occult lost-and-found office.

## Development baseline

- Unity `6000.3.21f1` (Unity 6.3 LTS)
- Android 10 / API 29 minimum, API 36 target
- ARM64, IL2CPP, Android App Bundle
- English primary language with Korean localization
- Offline play; no account, backend, energy system, or forced advertising

Open the repository root as the Unity project. Generated content, localization, rendering assets, and the two scenes can be rebuilt with `Tools > Curio Clerk > Generate Project Assets`.

## Current vertical slice

- 24 bilingual curios with six traits
- first-match rule engine, five deterministic difficulty bands, and 12-item shifts
- drag-to-destination and button sorting, one Hold slot, combo/coins/hearts, revive or double-coins rewarded placement
- tutorial, daily seed, collection, six desk charms, language/privacy settings, and resilient local JSON saves
- generated URP 2D project assets, Noto Sans KR, Android icon, EditMode/PlayMode tests, and AAB automation

## Human-run Unity commands

The current project workflow does not enable Unity MCP or other Authorized Agentic Access. The human developer runs Unity tests and builds; Codex prepares repository changes and reviews the supplied output.

```powershell
.\scripts\test-unity.ps1
.\scripts\build-android.ps1
```

Version 1 uses AdMob/UMP only for opt-in rewarded ads and ships no Firebase or remote gameplay/crash telemetry transport. Gameplay remains fully available when ads are declined or unavailable.

## Release route

The v1 release route is Samsung Galaxy Store in South Korea through a commercial Seller Portal account. See the [Samsung Seller setup record](Docs/Store/SamsungSellerSetup.md) and [release checklist](Docs/ReleaseChecklist.md). Google Play and Steam are deferred; their historical notes are retained only as references and are not v1 release gates.

See [project context](Docs/UnityProjectContext.md), [service setup](Docs/ServiceSetup.md), [Unity automation boundary](Docs/MCPSetup.md), [AI provenance policy](Docs/AIAssetProvenance.md), and [third-party notices](Docs/ThirdPartyNotices.md).

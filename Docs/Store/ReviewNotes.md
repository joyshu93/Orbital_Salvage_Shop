# Galaxy Store review notes — 1.0.0

Package: `com.joyshu93.curioclerknightshift`  
Version: `1.0.0`  
Login credentials: not applicable; the game has no account or login.

The certification build must use its validated release configuration. Ad availability is not guaranteed, and lack of an ad never blocks base progression.

## Reviewer path

1. Launch without an account.
2. Complete the tutorial.
3. Finish or fail a shift.
4. Confirm that base progression works without an ad and that the ordinary result or reward remains available.
5. If a rewarded ad is available, choose revive or double coins once. Verify that the earned benefit is granted once and the other option is locked for that shift. If no ad is available because of no-fill, is declined, or fails to load, verify that base progression remains usable and no base reward is removed.
6. Open Settings, change language between English and Korean, and open privacy options when required. The advertising privacy entry point appears only when UMP reports that it is required.
7. Force-stop and relaunch the app to confirm that local progress and settings recover.

## Service notes

- Only opt-in rewarded ads are included. There are no banner, interstitial, app-open, or rewarded-interstitial placements.
- UMP consent information is refreshed on launch. The app requests ads only when UMP reports that ads may be requested.
- The game remains playable offline without an ad. Network access is used only for the configured AdMob/UMP service path.
- Version 1 has no developer gameplay account, gameplay backend, cloud save, Firebase, remote gameplay analytics, or remote crash reporting.

## Official references

- Google, UMP Unity setup: https://developers.google.com/admob/unity/privacy — Accessed 2026-08-21.
- Samsung, Seller Portal app registration and review notes: https://developer.samsung.com/galaxy-store/launch.html — Accessed 2026-08-21.

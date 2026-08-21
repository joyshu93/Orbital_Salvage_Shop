# AdMob/UMP data-safety reconciliation — 1.0.0

Reconciliation status: PENDING_FINAL_CONFIGURATION_CONFIRMATION

This is an internal worksheet for reconciling the signed build, `Docs/PrivacyPolicy.md`, and Samsung Seller Portal. Google Play's disclosure guidance is a reconciliation source, not a claim that Samsung uses the same form verbatim. The developer must inspect the final Seller Portal questions and signed release configuration before submitting answers.

## Actual v1 service boundary

The runtime performs a UMP consent information update on every Android launch. When UMP reports that ads may be requested, the Google Mobile Ads service initializes and performs a Consent-authorized rewarded ad preload so the optional placement can be ready before the player presses it. The player does not need to watch or accept the ad, and an unavailable ad does not affect base progression.

The developer does not collect gameplay or crash payloads remotely. Google AdMob/UMP may process and share the categories below for advertising delivery and personalization when allowed, measurement and analytics for the advertising SDK, fraud prevention, security, compliance, and SDK operation. UMP manages consent and privacy choices and determines whether ads may be requested.

| Developer-operated feature | v1 state |
| --- | --- |
| Developer gameplay account | Absent |
| Gameplay backend / cloud save | Absent |
| Firebase | Absent |
| Remote gameplay analytics | Absent |
| Remote crash reporting | Absent |

## AdMob/UMP data categories to reconcile

| Category | Conservative v1 declaration basis |
| --- | --- |
| IP address / network-derived approximate location | Google says IP address is collected and may estimate general location. This corresponds to the Privacy Policy phrase “approximate location derived from network information.” |
| User product interactions / advertising interaction data | Google identifies app launch, taps, and video views for advertising, SDK analytics, and fraud prevention. This is advertising data, not developer gameplay telemetry. |
| Diagnostics and performance data | Google identifies app and SDK performance data such as launch time, hang rate, and energy usage. This is diagnostics processed by the advertising SDK, not remote crash reporting to the developer. |
| Device and account identifiers, including advertising ID and app set ID | Google identifies device identifiers and, where applicable, account-related identifiers. Advertising-ID transmission can vary with manifest, user controls, and Limited Ads behavior. |
| Consent and privacy choices | UMP updates consent status, presents required privacy messages, and conditionally exposes privacy options; the final message configuration and regional result require developer confirmation. |

The Privacy Policy and this worksheet both disclose device identifiers, advertising data, diagnostics, consent choices, and approximate location derived from network information. The signed build must be rechecked if the SDK, manifest, AdMob message configuration, mediation setup, or advertising-ID handling changes.

## Collection, sharing, and purpose cautions

- Samsung's Data Safety tab asks which user data the app collects or shares and the reason for each choice. Treat Google third-party processing conservatively during the final Seller Portal reconciliation.
- Do not mark data as ephemeral, not shared, sold, deletable on request, or retained for a specific period without verifying the final Samsung definitions and Google configuration.
- Do not claim a universal personalized-ad result. UMP region, consent choice, user controls, and Limited Ads behavior can change the eligible request.
- Do not add Firebase analytics, developer gameplay-event collection, or developer crash-report collection to the submission unless the shipped architecture and all privacy documents are redesigned and approved.

## Human submission checks

- [ ] Confirm the resolved Google Mobile Ads/UMP versions in the exact signed RC.
- [ ] Confirm advertising-ID manifest behavior and any Limited Ads configuration.
- [ ] Confirm no mediation or optional reporting feature adds another data category or recipient.
- [ ] Reconcile every Samsung Data Safety selection and purpose with the live Seller Portal wording.
- [ ] Confirm the hosted privacy policy has the same categories and service boundary.

## Official sources

- Google, Google Mobile Ads Unity data disclosure: https://developers.google.com/admob/unity/privacy/play-data-disclosure — Accessed 2026-08-21. The page states that the plugin automatically collects and shares IP address, user product interactions, diagnostic information, and device/account identifiers, and makes the developer responsible for the final disclosure.
- Google, UMP Unity setup: https://developers.google.com/admob/unity/privacy — Accessed 2026-08-21. The page requires consent-information updates on every launch, conditional privacy options, and `CanRequestAds()` before ad requests.
- Samsung, Register Your App in Seller Portal: https://developer.samsung.com/galaxy-store/launch.html — Accessed 2026-08-21. The Data Safety tab records collected/shared types and purposes shown on the app detail page.

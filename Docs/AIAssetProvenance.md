# AI asset provenance and release policy

Last reviewed: 2026-08-21 (KST)

This document is the release evidence ledger for AI-assisted work in **Curio Clerk: Night Shift**. It is an internal risk-control record, not legal advice. Update it whenever an AI tool creates or edits source code, art, audio, text, video, localization, or store material.

## Current product classification

- The shipped game does not accept prompts or generate content at runtime. Under the current Google Play guidance, it is not treated as a generative-AI app merely because AI tools assisted development.
- Google Play separately asks for asset-by-asset declarations when visual assets are submitted through Store listing, Promotional content, or YouTube content flows. An AI-generated or AI-edited store asset must be evaluated and declared on its own merits.
- OpenAI's terms assign Output to the user as between OpenAI and the user, to the extent permitted by law, but also state that output may not be unique. That assignment does not clear third-party copyright, trademark, publicity, or other rights.
- Korean Copyright Commission guidance distinguishes AI output from identifiable human creative expression. This project therefore records the tool, prompt, role of the output, and the human-authored contribution separately as its evidence policy.

Re-evaluate this classification before every production submission and whenever the game gains runtime generation, user-generated content, camera/microphone input, or AI-mediated chat.

## Release gates

An asset is not release-approved until all applicable items are complete:

- [ ] It has a stable asset ID and repository path.
- [ ] Its source, author/tool, creation date, prompt summary, and input references are recorded.
- [ ] Every input and reference image is owned, public domain, or licensed for this use and for submission to the selected AI tool.
- [ ] Human creative decisions and edits are described; before/after evidence is retained when meaningful edits are claimed.
- [ ] Similarity, trademark, character, logo, likeness, and misleading-content checks are complete.
- [ ] Third-party license and attribution obligations are copied to `Docs/ThirdPartyNotices.md`.
- [ ] Store assets have an individual Play Console AI-declaration decision.
- [ ] A human has marked the asset `Approved for release` below.

Unity import settings, compression, automatic resizing, format conversion, or selecting an output alone must not be recorded as substantial human creative editing.

## Prompt and reference rules

Do not request or accept:

- the style of a named living or deceased artist, illustrator, studio, game, film, or franchise;
- recognizable copyrighted characters, brand mascots, logos, product trade dress, or confusingly similar app icons;
- a real person's face, voice, signature, or identity without documented permission;
- an Asset Store, stock, commissioned, or purchased asset as AI input unless its license explicitly permits that use;
- private data, credentials, unpublished partner material, or tester information in a prompt;
- output that contains unexplained signatures, watermarks, brand marks, or recognizable protected elements.

Describe visual characteristics in generic production terms instead: palette, materials, line weight, lighting, camera, composition, mood, and readability target.

## Provenance ledger

### ART-BRAND-001 — current application icon concept

| Field | Record |
| --- | --- |
| Repository file | `Assets/Art/Brand/AppIcon.png` |
| SHA-256 | `45AF5AB7914D4750E1BC41BFC209B2E9892FADEC4A93AC46B05E547BE410710E` |
| Dimensions | 1254 × 1254 PNG |
| Tool | OpenAI ImageGen invoked through Codex |
| Creation evidence | File created 2026-08-20; first committed in `74754efb7354ca3bba629744ca8660c6c3ac835d` on 2026-08-20 KST |
| Prompt record | Exact prompt was not retained. Reconstructed summary: a square, warm occult lost-and-found game icon using burgundy, amber, and brass; a large antique key, a sleeping moth-like curio, a glowing desk lamp, crystals, bottles, and feathers; no text. This reconstruction is a known provenance gap and must not be represented as the exact prompt. |
| Reference inputs | None supplied |
| Input rights basis | No reference files were uploaded. The reconstructed prompt summary describes the developer's original product direction, but the missing exact prompt prevents a complete token-by-token input audit. |
| Provider terms basis | OpenAI's Rest-of-World Terms of Use effective 2026-01-01 were checked on 2026-08-21. They assign Output to the user as between OpenAI and the user, to the extent permitted by law. The generation record did not preserve which account agreement governed this session, so the developer must confirm the applicable account terms before release. https://openai.com/policies/row-terms-of-use/ |
| Rights caveat | Provider assignment is not third-party rights clearance. Output may not be unique, and copyright, trademark, character, likeness, and confusing-similarity review remains the developer's responsibility. |
| Human direction | The developer selected the warm-occult product direction and approved the concept subject and mood. |
| Human edits | No direct repaint, redraw, compositing, or documented shape/color correction. Unity import/resizing is automated processing only. |
| Third-party elements | None intentionally requested or identified; final similarity and trademark review remains open. |
| Intended uses | Prototype launcher icon. It may become a Store listing icon only after release approval. |
| Play Console decision | **Declare as AI-generated** if this file or an AI-edited derivative is submitted. Re-evaluate only if replaced with a separately documented human-created asset. |
| Release status | **Prototype only — not approved for RC.** Replace it or complete a documented human creative pass, similarity review, and explicit human approval. |

### DEV-CODE-001 — initial project implementation

| Field | Record |
| --- | --- |
| Scope | Initial Unity project structure, gameplay code, tests, build automation, and documentation committed from 2026-08-20 onward |
| Tool | OpenAI Codex with developer-authored product requirements and approvals |
| Human contribution | Product goals, genre selection, monetization constraints, platform decisions, acceptance decisions, and final release authority |
| Verification | Changes remain subject to Git review, automated tests, manual Unity execution, device testing, and third-party license review. AI assistance is not evidence of correctness or non-infringement. |
| Play Console decision | The current asset-declaration help page concerns submitted visual assets, not source-code authorship. No code declaration is identified, but this must be rechecked at submission time. |
| Release status | Allowed as reviewed source; each change still requires normal engineering and license gates. |

## New entry template

Copy this section before adding or materially editing an asset:

```text
ID:
Repository path(s):
SHA-256 or Git commit:
Asset type and intended use:
Creator or AI tool/model:
Creation date:
Prompt or concise prompt summary:
Negative constraints:
Reference/input files and rights basis:
AI output's role in the final asset:
Human-authored decisions and edits:
Before/after evidence path:
Third-party/license/attribution notes:
Similarity/trademark/likeness review:
Play Console declaration decision and reason:
Reviewer, review date, and status:
```

For store screenshots, also record which in-game assets are visible. A screenshot of AI-assisted game art and a separately AI-generated promotional composition are distinct declaration decisions.

## Evidence retention

- Use Git history for source and document changes.
- Keep project-owned editable source files and meaningful before/after exports. Do not commit secrets or personal data.
- Store purchase receipts and licenses outside the public repository; record a non-sensitive receipt ID or storage reference here.
- If an exact historic prompt is unavailable, label the summary as reconstructed. Never invent an exact prompt after the fact.
- Review the ledger at content freeze and again immediately before Play Console submission.

## Official references

- Google Play, AI asset declaration: https://support.google.com/googleplay/android-developer/answer/17262077?hl=en
- Google Play, AI-Generated Content policy scope: https://support.google.com/googleplay/android-developer/answer/14094294?hl=en
- Korea Copyright Commission, AI/copyright guide collection: https://www.copyright.or.kr/notify/notice/view.do?brdctsno=55402
- Korea Copyright Commission, AI-assisted work registration guide: https://www.copyright.or.kr/information-materials/publication/research-report/view.do?brdctsno=54253
- OpenAI Terms of Use: https://openai.com/policies/row-terms-of-use/

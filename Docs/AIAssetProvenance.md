# AI asset provenance and release policy

Last reviewed: 2026-09-08 (KST)

This document is the release evidence ledger for AI-assisted work in **Curio Clerk: Night Shift**. It is an internal risk-control record, not legal advice. Update it whenever an AI tool creates or edits source code, art, audio, text, video, localization, or store material.

## Current product classification

- The shipped game does not accept prompts or generate content at runtime. Under the historic Google Play guidance retained below as a reference, it is not treated as a generative-AI app merely because AI tools assisted development.
- Each store may require an asset-by-asset declaration when visual assets are submitted through listing, promotional, or video-content flows. An AI-generated or AI-edited store asset must be evaluated and handled according to the selected store's current submission requirements.
- OpenAI's terms assign Output to the user as between OpenAI and the user, to the extent permitted by law, but also state that output may not be unique. That assignment does not clear third-party copyright, trademark, publicity, or other rights.
- Korean Copyright Commission guidance distinguishes AI output from identifiable human creative expression. This project therefore records the tool, prompt, role of the output, and the human-authored contribution separately as its evidence policy.

Re-evaluate this classification against the selected store's current requirements before every production submission and whenever the game gains runtime generation, user-generated content, camera/microphone input, or AI-mediated chat.

## Release gates

An asset is not release-approved until all applicable items are complete:

- [ ] It has a stable asset ID and repository path.
- [ ] Its source, author/tool, creation date, prompt summary, and input references are recorded.
- [ ] Every input and reference image is owned, public domain, or licensed for this use and for submission to the selected AI tool.
- [ ] Human creative decisions and edits are described; before/after evidence is retained when meaningful edits are claimed.
- [ ] Similarity, trademark, character, logo, likeness, and misleading-content checks are complete.
- [ ] Third-party license and attribution obligations are copied to `Docs/ThirdPartyNotices.md`.
- [ ] Store assets have an individual store submission decision and supporting rationale.
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

### QA-ANDROID-20260908 — emulator validation and callback regressions

- Tool/date: Codex, 2026-09-08 KST.
- Human instruction: validate the existing QA APK on the existing Android emulator, preserve saves and unrelated work, fix evidenced defects with failing tests, and record actual results and limitations.
- Scope: QA report, local ADB screenshots/logs, and narrowly scoped regression tests and fixes. Rules-panel sizing and request callback isolation are code changes. The existing English/Korean fallback label is corrected to format the rule's actual destination (Otherwise → {0} / 그 외 → {0}); no narrative, art, audio, or third-party assets are added. Screenshots are unaltered captures of the project, not generated art or store media.
- Evidence is retained locally under ignored `Logs/EmulatorQa-20260908`; existing app data and 22 dirty files were backed up before installation. This work does not grant release approval.
- Final-scene correction: later cases reuse their existing localized case title, CASE RESOLVED / 사건 해결 label, and authored final-stage lead artwork. The ice/umbrella ending remains specific to Unmelting Ice; no new story copy or visual asset is generated.

### ART-NARRATIVE-001 — senior night clerk portrait set

| Field | Record |
| --- | --- |
| Repository files | `Assets/Resources/Art/Characters/senior-clerk-neutral.png`; `senior-clerk-concerned.png`; `senior-clerk-alert.png`; `senior-clerk-relieved.png` |
| SHA-256 | Neutral `09259FCDB4EEA196F6689D8FACDC7F36C410D72DB21AC7B85FE5F4565951EE99`; concerned `8553D09FF755B579E75AF9BCFFB711971C35AC7A361D3EA3246ED9A1E15EB8BF`; alert `09A8193D755CFF26E3E86685DDDA4FEC490B93DFA99D08B16372FB3FC56C7411`; relieved `AB7541374EE39EBC17D4AAFA822C0D9AB97F2D1EBBD05E5A13041CF6A7BCD9DC` |
| Dimensions | Four 768 × 1024 transparent PNGs |
| Asset type and intended use | Four waist-up visual-novel portraits for the senior night clerk in the first incident; large Android portrait-screen silhouette and emotional state changes |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation date | 2026-08-31 KST |
| Prompt record | Exact normalized generation and identity-preserving edit prompts are retained in `Docs/NarrativeSlicePrompts.md`. |
| Selected source outputs and reference inputs | Neutral source output `exec-6c165bd5-02a1-4811-9cd1-6711e3659eb2`; concerned `exec-a9024307-47c2-4743-bbc4-a76a4591be05`; alert `exec-7f60217b-816b-4e43-86cf-04f5d6ca54c5`; relieved `exec-5bd33717-6175-4ccf-a0f2-73e764fd777f`. Neutral used text only. Each expression edit used only that project-owned neutral output. No third-party image was supplied. |
| Negative constraints | No named artist, studio, game, film, franchise, protected character, real-person likeness, modern clothing, text, logo, signature, watermark, gore, or opaque checkerboard. |
| Human direction | The developer approved a bold but warm visual-novel presentation, a single recurring senior clerk, large readable expressions, and the plum/parchment/brass Curio Clerk palette. |
| Technical normalization | The three expression edits arrived with a baked neutral checkerboard. After explicit developer approval, Codex removed only the connected/high-luminance neutral checker field and its edge matte, preserved the character pixels, and resized all four portraits from 1086 × 1448 to 768 × 1024 with high-quality bicubic filtering. No repaint or redraw is claimed. |
| Human selection | The developer reviewed the four labeled expressions plus the overlay and approved this set for prototype integration on 2026-08-31 KST. Alternate opaque-background and inconsistent-identity generations were rejected. |
| Similarity and trademark review | No visible text, logo, signature, watermark, famous likeness, or recognizable protected character was identified during prototype review. Final release similarity/trademark review remains pending. |
| Third-party elements | None intentionally requested or supplied; no attribution currently identified. |
| Store submission decision | Treat selected portraits and prominent screenshots containing them as AI-assisted/generated media wherever the selected store asks. Recheck current wording at submission. |
| Release status | **Approved for first-incident prototype integration. Not yet approved as final release art.** |

### ART-EFFECT-001 — incident frost-edge overlay

| Field | Record |
| --- | --- |
| Repository file | `Assets/Resources/Art/Effects/frost-overlay.png` |
| SHA-256 | `A583F60946BA77C2F357ED3CF6B80FBD867AFC5FCC5A56A162E276F98A9D25A2` |
| Dimensions | 1024 × 1536 transparent PNG; sampled center alpha is 0 |
| Asset type and intended use | Transparent 9:16 frost-edge overlay with a clear center for artifact and narrative emphasis |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation date | 2026-08-31 KST |
| Prompt record | Exact normalized generation prompt is retained in `Docs/NarrativeSlicePrompts.md`. |
| Reference inputs | Text only; no third-party input or reference image. |
| Negative constraints | No text, letters, symbols, logos, signatures, watermarks, opaque checkerboard, central obstruction, gore, or franchise imagery. |
| Human direction | The developer approved atmosphere-preserving but visibly stronger frost/amber feedback around a readable portrait gameplay center. |
| Human selection and technical review | The developer approved the labeled overlay preview on 2026-08-31 KST. Codex confirmed true alpha at the center and retained the original 1024 × 1536 output without repaint or redraw. |
| Similarity and trademark review | No visible text, symbol, logo, signature, watermark, or recognizable protected imagery was identified during prototype review. Final release review remains pending. |
| Third-party elements | None intentionally requested or supplied; no attribution currently identified. |
| Store submission decision | Treat the selected overlay and prominent screenshots containing it as AI-assisted/generated media wherever the selected store asks. |
| Release status | **Approved for first-incident prototype integration. Not yet approved as final release art.** |


### DESIGN-NARRATIVE-001 — narrative occupational puzzle specification and visual board

| Field | Record |
| --- | --- |
| Repository files | `Docs/superpowers/specs/2026-08-28-narrative-occupational-puzzle-design.md`; `Docs/Design/NarrativeOccupationalPuzzle/curio-narrative-ux-board.html`; rendered PNG when present beside the HTML source |
| Asset type and intended use | Internal product specification and editable visual reference for the approved narrative occupational puzzle redesign; not an in-game or store asset |
| Tool | OpenAI Codex authored the text, layout, and CSS from the developer-approved design dialogue |
| Creation date | 2026-08-28 KST |
| Reference inputs | Existing Curio Clerk product constraints, repository artifact catalog, Three-Seal Docket design, and developer feedback; no third-party prose, image, or style reference supplied |
| Human direction | The developer selected the caretaker fantasy, case-shift plus Free Shift structure, visual-novel framing, first-person clerk, one senior clerk, bold presentation, warm restrained effects, linear story with reactive outcomes, 12-incident arc, first-slice boundary, and playtest gates. |
| Human edits | The developer repeatedly rejected or redirected shallow collection, short-campaign, weak-hook, small-screen, and restrained-feedback concepts, and explicitly approved each final design section. No claim is made that Codex output alone constitutes final shipped creative authorship. |
| Third-party elements | None. The editable board uses repository-authored text and CSS shapes only; it does not bundle fonts, images, or copied UI assets. |
| Intended uses | Internal reference, implementation planning, before/after product review, and future design iteration. |
| Store submission decision | Not submitted to a store and not included in the player build. If reused in promotional material, create a separate provenance and store-media decision. |
| Release status | **Approved as internal design documentation; game implementation and final creative assets remain subject to separate review.** |

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
| Store submission decision | For any selected store, determine its current AI-asset disclosure requirement before submission. Historic Google Play reference: **Declare as AI-generated** if this file or an AI-edited derivative is submitted. Re-evaluate only if replaced with a separately documented human-created asset. |
| Release status | **Prototype only — not approved for RC.** Replace it or complete a documented human creative pass, similarity review, and explicit human approval. |

### ART-VSLICE-001 — warm occult desk and tutorial curios

| Field | Record |
| --- | --- |
| Repository files | `Assets/Resources/Art/Desk/occult-desk-background.png`; `Assets/Resources/Art/Artifacts/sleeping-teacup.png`; `mirror-seed.png`; `thimble-storm.png`; `whispering-key.png` |
| SHA-256 | Desk `5F1F207A22038A6EA43C931AC5433A8A7A2E69F5A49239A7F0042B6EAFD2A0F1`; teacup `D0DD8ED8D46CF26BE5B070A7FD653C09C1E82A7B60CB263163C2B23361F3B8BF`; seed `B33C74D1879EAD0C90EF60DB6CC9515B9915E63B810E37D0283C6A43F5A3211F`; thimble `BDC792E798AFAFD471F63D69235F3875B6B3374AE45A256C701232605F8C5DC8`; key `6962D1C5733BD1FC914736CE2A295AD45867207F2DC0CBAD3D161DDE9CA2CAB4` |
| Dimensions | Desk 1024 × 1536 PNG; each artifact 1254 × 1254 PNG |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation evidence | Generated and selected 2026-08-24 KST. Exact normalized prompts and transparency-correction records are retained in `Docs/VisualSlicePrompts.md`. |
| Reference inputs | No third-party inputs. Later generations used only the project-owned desk and earlier artifact outputs from this same prompt set as palette/brushwork references. |
| Input rights basis | The user approved the original warm-occult product direction. No named artist, studio, franchise, brand, person, private file, or third-party image was supplied. |
| Human direction | The developer approved the warm-occult commercial vertical-slice scope, four tutorial subjects, bilingual mobile use, and provenance workflow. Codex specified the composition, palette, readability constraints, subject details, and negative constraints within that approved direction. |
| Human edits | No manual repaint or redraw is claimed. Codex selected the displayed outputs, rejected opaque checkerboard results for two sprites, directed background-only alpha corrections, verified PNG alpha, named files, and integrated them into the UI. Unity resizing/compression is technical processing only. |
| Similarity and trademark review | Prompts excluded protected characters, brands, logos, named styles, likenesses, signatures, and watermarks. No intentional protected element or visible mark was identified in the selected outputs. Broader visual-similarity search and final human release review remain open. |
| Third-party elements | None intentionally requested or identified. No attribution is currently required for these ImageGen outputs; provider terms and third-party-rights caveats still apply. |
| Intended uses | In-game prototype/vertical-slice background and tutorial artifact illustrations. They may appear incidentally in store screenshots only after the release gate below is completed. |
| Store submission decision | Treat the images and any store screenshot prominently containing them as AI-assisted/generated media wherever the selected store asks. Recheck current Samsung submission wording at upload time. |
| Release status | **Vertical-slice prototype — not approved for RC.** Requires developer visual review in Unity, similarity/trademark review, any desired direct human repaint/composition adjustment, and explicit human release approval. |

### ART-VSLICE-002 — standard-shift curio set

| Field | Record |
| --- | --- |
| Repository files | `Assets/Resources/Art/Artifacts/clockwork-moth.png`; `rain-jar.png`; `moon-umbrella.png`; `silent-bell.png` |
| SHA-256 | Moth `893B752DA3D8BD5BB5A0D4DD9A044BAEC35DBE5D9B5FD90C8BA428A7AA77BAC2`; jar `E485852C8611CC442AAEC371B0E333E6536C887D0345999E7C0621BF8180DE0C`; umbrella `3FFD4A6B696EDA32124AB9193ED6E286AA555B112F3CCA4921FBE07BEB44F454`; bell `273CADF80AE3C961B5C9030A9002A37CD6BBB4EC0F02EE1E14733E203A5813BB` |
| Dimensions | Each artifact 1254 × 1254, 32-bit ARGB PNG with transparent corners |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation evidence | Generated, background-extracted, technically checked, and selected 2026-08-24 KST. Exact normalized prompts and shared correction constraints are retained in `Docs/VisualSlicePrompts.md`. |
| Reference inputs | Only the project-owned `sleeping-teacup.png` and `mirror-seed.png` from `ART-VSLICE-001`, used as palette, brushwork, outline, lighting, and mobile-readability references. |
| Input rights basis | No third-party image, named artist, studio, franchise, brand, person, or private material was supplied. The user approved the Phase 2 subject list and recommended integration scope. |
| Human direction | Codex translated the approved catalog subjects and traits into composition, material, readability, palette, mood, and negative constraints. The developer approved the product direction and implementation scope. |
| Human edits | No manual repaint or redraw is claimed. Codex selected the displayed outputs, rejected the opaque checkerboard backgrounds, directed background-only alpha extraction, verified pixel format and corner alpha, named files, and integrated them into the existing resource path. |
| Similarity and trademark review | Prompts prohibited protected characters, brands, logos, named styles, likenesses, signatures, and watermarks. No intentional protected element or visible mark was identified in the selected outputs. Broader similarity and final human release review remain open. |
| Intended uses | Standard-shift current, next, and held artifact illustrations in the in-game vertical slice. Store screenshots require the release gate below. |
| Store submission decision | Treat the images and prominent screenshots containing them as AI-assisted/generated media wherever the selected store asks. Recheck the selected store's current wording at upload time. |
| Release status | **Vertical-slice prototype — not approved for RC.** Requires in-Unity composition review, similarity/trademark review, any desired human repaint or adjustment, and explicit developer release approval. |

### ART-CATALOG-001 — remaining curio catalog illustrations

| Field | Record |
| --- | --- |
| Repository files | Sixteen transparent sprites under `Assets/Resources/Art/Artifacts`: `borrowed-shadow`, `mossy-watch`, `paper-fish`, `backward-candle`, `porcelain-tooth`, `humming-scarf`, `sundial-egg`, `rusty-comet`, `ink-snowglobe`, `patient-compass`, `yesterday-ticket`, `tea-crown`, `lantern-snail`, `tide-locket`, `murmur-box`, and `unmelting-ice` |
| SHA-256 | Shadow `DAEF84B290CEC82569A8B7D7C8952C4019F91AE7F56C678077373443C48C79DF`; watch `816819806FE16A6BBF3DB21C4A5ED36C97CF0B18BD6332ED076E48E0A9575A50`; fish `29CD1292E2F02E54033350DB3C9A840A63E920B2E70B3A07941F68FC5A033D29`; candle `32CF4C685EA214605932089DCF7C694334A5260F5CF9A295705BFA2A9B208B7A`; tooth `3BBCFD843D88C98093C009A1D3BB1527E1E1A98F50AD4B6A3E0789581C91B6FB`; scarf `CC876492AEE2AE32E206CD6035C9D2697975B29EBAA9E9BE145A48E5CC1C718F`; egg `9BF25E0D5FB0BE16703BF8CBB6295238B9455BAA139A951061762F7D004156A5`; comet `2ADEF7045C5CFF101C179C1DE402D118114F331C7013BFA53719D80CB347B464`; snowglobe `A3412F9E525C36CB35ADB426D7F2BD31743C295434F9F7D8EE37B4DEB157D284`; compass `D0333C84E3FEB9632381A052033BA14758D37DEE3704E98F1D776C3F81F982A0`; ticket `A47E34E3E35AF9011830505A58FD514EDFE6AC01EFFF46D03E5B8C690AC90996`; crown `A15611F3EB4CE91638EC01BA6C56E36C3B4AD4F669D39FEED83BA48131F039FD`; snail `9D86A547011DF1081CEA79EDF166E9B91DDAD838BB86A8FECE7D5BEC665DFC96`; locket `A1B8291262F8A3003D3B747A3D3EFF338B2D1F94B7AAC4F3C642BB20B652E82C`; box `F781BF0CB3B6EB415F7ECA4C497AE1D4C7046BF9DE593EBD5575C3C3454FC003`; ice `7C9FA4CEA21C5B0C6750355136E822F3A468D428BBF27C7CD6DFAA870DE648DD` |
| Dimensions | Each selected sprite is 1254 × 1254 PNG with transparent corners. |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation evidence | Generated, selected, alpha-corrected where necessary, and technically checked 2026-08-26 KST. Normalized subject prompts and correction history are retained in `Docs/VisualSlicePrompts.md`. |
| Prompt summary | Complete the authoritative 24-curio catalog with one readable, isolated, transparent warm-occult inventory illustration per remaining object. Use a tactile hand-painted gouache finish, dark-plum outline, brass/parchment/amber palette, and a silhouette readable at 128 px. |
| Reference inputs | The first eight generations used only project-owned `sleeping-teacup.png` and `mirror-seed.png` as palette, brushwork, outline, lighting, and mobile-readability references. The final eight were generated from text only. No third-party reference was supplied. |
| Input rights basis | No third-party image, named artist, studio, franchise, brand, person, or private material was supplied. |
| Negative constraints | No text, letters, numbers, brands, logos, signatures, watermarks, named styles, protected characters, likenesses, scenery, frames, opaque checkerboards, or extra unrelated props. |
| Human direction | The developer approved completing the remaining catalog and the existing warm-occult visual direction. Codex specified each subject, palette, composition, material, readability target, and negative constraints, then rejected or corrected outputs that violated transparency or no-text requirements. |
| Human edits | No manual repaint or redraw is claimed. Codex selected outputs, directed background-only alpha extraction where checkerboards were rendered as pixels, removed unintended Roman numerals from `yesterday-ticket`, verified file dimensions and corner alpha, named files, and integrated them. Unity import processing is not a creative edit. |
| Similarity and trademark review | Prompts prohibited brands, logos, named styles, protected characters, likenesses, signatures, and watermarks. No intentional protected element or visible brand mark was identified during selection. Broader visual-similarity review and final human release review remain open. |
| Intended uses | In-game current, next, held, and casebook artifact illustrations. Store screenshots require the release gate below. |
| Store submission decision | Treat selected images and prominent screenshots containing them as AI-assisted/generated media wherever Samsung asks. Recheck the live submission wording at upload time. |
| Release status | **Catalog prototype — not approved for RC.** Requires in-Unity scale/composition review, similarity/trademark review, any desired direct human repaint or adjustment, and explicit developer release approval. |

### ART-COSMETICS-001 — desk charm illustration set

| Field | Record |
| --- | --- |
| Repository files | Six transparent sprites under `Assets/Resources/Art/Cosmetics`: `brass-lamp`, `moth-mobile`, `plum-runner`, `moon-mug`, `fern-familiar`, and `amber-window` |
| SHA-256 | Lamp `2A7957A690681F41A59D14B06DFB347067B618764F6DF7F4FD5AC1BCB65A75D8`; mobile `A11CFB98192885490EEB5344B0C2DFBC27B7EF071ED2312C5A89EEDCD72F954E`; runner `0FA33B56E2669B7755FFB20A06B7FCFA716D23725A5A26C15975BA618BFD17D1`; mug `82163ABC4C0354CBA56EF92002F450F31ED230385E7D6902408398EFCFCEB1E1`; fern `00EB501E74A228B16AE0E26B80B2F5331FC71A0E65A53F827C361112EB8D94E4`; window `2D4E29EA72FE28951C1643F1B18A6105F111FCE2A37F855E3412DDDFFD46D315` |
| Dimensions | Each selected sprite is 1254 × 1254 PNG with effectively transparent corners. |
| Asset type and intended use | Illustrated previews for the six unlockable desk charms, plus the currently equipped charm shown on the menu and shift desk |
| Tool | Built-in OpenAI ImageGen invoked through Codex |
| Creation evidence | Generated, selected, copied into the repository, and technically checked 2026-08-26 KST. Normalized prompts are retained in `Docs/VisualSlicePrompts.md`. |
| Prompt summary | Create six isolated, readable warm-occult desk accessories using the established deep-plum, parchment, antique-brass, sage, and amber palette, tactile hand-painted gouache texture, and a strong silhouette at 128 px. |
| Negative constraints | No text, letters, numbers, brands, logos, signatures, watermarks, named styles, protected characters, real-person likenesses, scenery, frames, opaque checkerboards, or unrelated props. |
| Reference inputs | None. The prompts describe the repository-owned product palette and generic production characteristics without uploading third-party or project images. |
| Human direction | The developer approved an illustrated cosmetics tab and equipped-desk previews as the recommended product-quality pass. Codex defines each subject, composition, material, readability target, and negative constraint within that scope. |
| Human edits | No human repaint or redraw is claimed. Codex selected the displayed outputs, verified transparent corners and dimensions, named the files, and integrated them. Unity import is technical processing only; direct human creative edits and before/after evidence remain open. |
| Similarity and trademark review | Prompt constraints prohibit brands, logos, protected characters, likenesses, and named styles. Final selected files still require human visual and similarity review. |
| Third-party elements | None intentionally requested or identified. Update this field immediately if any external input or recognizable third-party element is introduced. |
| Store submission decision | Treat the selected images and prominent store screenshots containing them as AI-assisted/generated media wherever the selected store asks. Recheck live submission wording before upload. |
| Release status | **Cosmetics prototype — not approved for RC.** In-Unity scale/composition review, similarity/trademark review, any desired direct human repaint or adjustment, and explicit developer release approval remain required. |

### TEXT-GAMEPLAY-003 — Three-Seal Docket rules and curio resolutions

| Field | Record |
| --- | --- |
| Repository files | `Assets/Scripts/Runtime/Localization/Localizer.cs`; `Assets/Scripts/Runtime/Content/ContentCatalog.cs`; generated Unity localization tables and generated Artifact assets after `ProjectBuilder.BuildAll` |
| Asset type and intended use | English and Korean Three-Seal Docket rules and artifact-specific resolution copy shown during gameplay and in the Casebook |
| Tool | OpenAI Codex |
| Creation date | 2026-08-27 KST |
| Reference inputs | Existing project-authored artifact names and descriptions, plus the developer-approved Three-Seal Docket design; no third-party prose supplied or copied |
| Human direction | The developer approved the core-loop redesign, its rules, and the requirement that each artifact's fiction connect to its gameplay result. |
| Human edits | No independent human rewrite claimed at intake. Korean naturalness, English clarity, line wrapping, tone, and contextual fit require developer bilingual review in Unity. |
| Third-party elements | None identified; no third-party prose or reference text was used. |
| Store submission decision | Ordinary in-game gameplay copy; no separate AI-media declaration currently identified. Recheck selected-store requirements at submission time. |
| Release status | **Prototype copy — developer bilingual review required before RC.** |

### TEXT-NARRATIVE-004 — Unmelting Ice five-stage prototype script

| Field | Record |
| --- | --- |
| Repository files | `Assets/Scripts/Runtime/Content/Incidents/FirstIncidentCatalog.cs`; generated runtime presentation derived from this source after later integration |
| Asset type and intended use | Five-stage English/Korean opening beats, closing hooks, and Stable/Precise/Resonant artifact reactions for the first playable incident prototype; includes the revised stage-two frost trail, stage-three temporal-priority reveal, stage-four protective Hold crisis, and stage-five mastery finale |
| Tool | OpenAI Codex drafted and revised the bilingual narrative copy from the developer-approved incident specification and fixed implementation plan |
| Creation date | 2026-08-31 KST; stage-two through stage-five pacing revisions 2026-09-03 KST |
| Reference inputs | The approved Curio Clerk narrative occupational puzzle specification, fixed five-stage incident beats, existing artifact catalog fiction, and Three-Seal Docket terminology; no third-party prose supplied or copied |
| Human direction | The developer approved the five authored shift matrix and caretaker tone, then requested stronger, larger dialogue, judgment, and reactions without breaking the warm occult atmosphere. On 2026-09-01, the first shift was revised to state the fantasy, immediate threat, three-seal/Hold job, and next-night hook more clearly. On 2026-09-03, the developer approved moving the ice reveal later in stage two, repeating visible frost-versus-time priority judgments in stage three, combining that judgment with a protective Hold crisis in stage four, and ending with a balanced Hold/priority/narrative mastery shift without adding a new system. |
| Human edits | The opening, closing, and quality reactions remain prototype drafts. Korean naturalness, English clarity, tone, line wrapping, and contextual fit require developer bilingual review in Unity. |
| Third-party elements | None identified. No external prose, named style, franchise text, quotation, or third-party reference material was used. |
| Store submission decision | Ordinary in-game narrative text; no separate AI-media declaration currently identified. Recheck the selected store's current requirements at submission time. |
| Release status | **Prototype copy — developer bilingual and in-context review required before RC.** |

### DESIGN-NARRATIVE-002 — multi-incident board and Remembering Rain vertical-slice design

| Field | Record |
| --- | --- |
| Repository files | `Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md`; `Docs/superpowers/plans/2026-09-03-multi-incident-remembering-rain.md` |
| Asset type and intended use | Internal product and technical specification for a reusable multi-incident progression model, the portrait incident-board menu, and the first playable shift of the second incident; not an in-game or store asset |
| Tool | OpenAI Codex authored the specification from the developer-approved design dialogue |
| Creation date | 2026-09-03 KST |
| Reference inputs | Existing Curio Clerk product constraints, the approved narrative occupational puzzle specification, the Three-Seal Docket rules, current repository architecture, artifact catalog fiction, and developer playtest feedback; no third-party prose, image, audio, or style reference supplied |
| Human direction | The developer selected the current-incident hero layout, required a genuine second incident instead of a placeholder, approved the `Remembering Rain / 기억하는 비` mystery, chose an unresolved `next shift in preparation` boundary, requested stronger visual-novel presentation, and required reusable use of appropriate Unity systems. |
| Human edits | The developer approved the design section by section and retains authority over implementation, bilingual wording, pacing, and release. No claim is made that Codex output alone constitutes final shipped creative authorship. |
| Third-party elements | None. The document contains project-authored prose and references only repository concepts and Unity APIs already governed by the project's existing notices. |
| Intended uses | Internal implementation planning, save-migration review, UI review, and future incident expansion. |
| Store submission decision | Not submitted to a store and not included in the player build. Promotional reuse requires a separate provenance and store-media decision. |
| Release status | **Approved as internal design documentation; implementation and final creative assets require separate review.** |

### DESIGN-NARRATIVE-003 — full-campaign story master blueprint

| Field | Record |
| --- | --- |
| Repository files | `Docs/superpowers/specs/2026-09-04-curio-clerk-story-master-design.md` |
| Asset type and intended use | Internal long-form story bible and production storyboard covering the three-act, twelve-incident, approximately sixty-shift campaign; not an in-game or store asset |
| Tool | OpenAI Codex authored the structure and prose from the developer-approved story direction |
| Creation date | 2026-09-04 KST |
| Reference inputs | Existing Curio Clerk product constraints, repository artifact catalog and fiction, implemented Unmelting Ice and Remembering Rain content, the approved narrative occupational puzzle specification, and developer playtest feedback; no third-party prose, image, audio, film, game, or style reference supplied |
| Human direction | The developer requested a reusable campaign-scale storyboard before further scene-by-scene implementation, selected the senior clerk's broken promise as the emotional spine, approved the promise to reveal the repository's truth to the next clerk, requested exciting and absorbing pacing with fair and credible reversals, and authorized Codex to resolve remaining story choices consistently without repeated questions. |
| Human edits | The document is a review draft. Exact dialogue, incident titles, bilingual copy, shift rules, pacing, and final canon remain subject to developer approval and later in-context revision. |
| Third-party elements | None. The document uses only project-owned concepts, artifact names, traits, mechanics, and original prose. |
| Intended uses | Campaign continuity source, incident planning, dialogue briefs, reveal tracking, gameplay-story alignment, production scoping, and later playtest review. |
| Store submission decision | Internal documentation only and not included in the player build. Any direct reuse as store copy or promotional media requires a separate review and provenance decision. |
| Release status | **Internal review draft — developer story and continuity approval required before it becomes campaign canon.** |

### DESIGN-NARRATIVE-004 — Remembering Rain full-incident implementation plan

| Field | Record |
| --- | --- |
| Repository files | `Docs/superpowers/plans/2026-09-04-remembering-rain-full-incident.md` |
| Asset type and intended use | Internal implementation plan that converts the approved campaign blueprint into the five-stage Remembering Rain incident, reusable three-docket narrative interludes, and a read-only third-incident teaser; not an in-game or store asset |
| Tool | OpenAI Codex authored the technical and narrative implementation plan from the approved master story blueprint and the existing Unity architecture |
| Creation date | 2026-09-04 KST |
| Reference inputs | Existing repository code and tests, the approved full-campaign story master blueprint, implemented first-shift Remembering Rain content, Three-Seal Docket mechanics, and repository-owned artifact fiction only; no third-party prose, image, audio, game, film, or style reference supplied |
| Human direction | The developer approved the campaign-scale story direction, asked Codex to make remaining detailed decisions without repeated questions, required exciting and fair reversals, and requested reusable use of Unity systems without unnecessary feature growth. |
| Human edits | The plan fixes implementation boundaries, test checkpoints, rule matrices, and draft bilingual beats. Runtime wording and pacing remain subject to developer review in Unity before release. |
| Third-party elements | None. The plan uses project-owned mechanics, artifact names, story concepts, and original prose. |
| Intended uses | TDD implementation, code review, continuity checks, manual playtest instructions, and future incident reuse. |
| Store submission decision | Internal documentation only and not included in the player build. Any direct reuse as store copy or promotional material requires separate review. |
| Release status | **Approved planning scope only — runtime implementation and bilingual in-context review remain pending.** |

### TEXT-NARRATIVE-005 — Remembering Rain first-shift prototype copy

| Field | Record |
| --- | --- |
| Repository files | Initial approved draft in `Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md`; future runtime source under `Assets/Scripts/Runtime/Content/Incidents` after implementation approval |
| Asset type and intended use | English/Korean incident title, opening dialogue, first clue, completion hook, and menu status copy for the first playable shift of the second incident prototype |
| Tool | OpenAI Codex drafted the bilingual narrative copy from the developer-approved mystery and gameplay constraints |
| Creation date | 2026-09-03 KST |
| Reference inputs | The approved Curio Clerk caretaker fantasy, existing artifact names and traits, Three-Seal Docket terminology, first-incident ending hook, and developer direction for bold visual-novel presentation; no third-party prose supplied or copied |
| Human direction | The developer approved `Remembering Rain / 기억하는 비`, the indoor rain that repeats owners' memories, the senior clerk connection, the Wet-over-Fragile priority judgment, and the unresolved first-clue ending. |
| Human edits | The text remains a prototype draft. Korean naturalness, English clarity, tone, line wrapping, and contextual fit require developer bilingual review in Unity before RC. |
| Third-party elements | None identified. No external prose, quotation, named style, franchise text, likeness, or third-party reference material was used. |
| Store submission decision | Ordinary in-game narrative text; no separate AI-media declaration currently identified. Recheck the selected store's current requirements at submission time. |
| Release status | **Prototype copy — developer bilingual and in-context review required before RC.** |

### TEXT-NARRATIVE-006 — Remembering Rain five-stage prototype script

| Field | Record |
| --- | --- |
| Repository files | `Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs`; generated runtime presentation derived from this source after catalog generation |
| Asset type and intended use | English/Korean five-stage incident dialogue, three-docket interludes, judgment reactions, completion hook, and read-only next-incident clue for the complete Remembering Rain prototype |
| Tool | OpenAI Codex drafted the bilingual narrative copy from the developer-approved campaign blueprint and implementation plan |
| Creation date | 2026-09-07 KST |
| Reference inputs | The approved Curio Clerk story master blueprint and Remembering Rain implementation plan, existing repository artifact fiction and mechanics, and the previously approved first-shift draft only; no third-party prose, quotation, image, audio, game, film, or style reference was supplied or copied |
| Human direction | The developer approved direct implementation of all five shifts, their connected three-docket reveals, the fixed judgment matrices, and the third-incident teaser, and authorized AI-driven Unity validation and Korean play acceptance. |
| Human edits | All English and Korean prose remains prototype copy. Naturalness, clarity, emotional pacing, line wrapping, and in-context presentation require review in the running Korean build before RC. |
| Third-party elements | None identified. No external prose, quotation, named style, franchise text, likeness, or third-party reference material was used. |
| Store submission decision | Ordinary in-game narrative text; no separate AI-media declaration currently identified. Recheck the selected store's current requirements at submission time. |
| Release status | **Prototype copy — developer bilingual and in-context review required before RC.** |

### FONT-UI-001 — Gowun Batang display typography

| Field | Record |
| --- | --- |
| Repository files | `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf`; `Assets/Fonts/GowunBatang/OFL.txt`; generated `Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset` after `ProjectBuilder.BuildAll` |
| Asset type and intended use | Third-party open-source display font for curio names, titles, and short resolution copy in the approved Curio-First UX overhaul |
| Source | Official Google Fonts repository: https://github.com/google/fonts/tree/main/ofl/gowunbatang |
| Creator | Copyright 2021 The Gowun Batang Project Authors; no AI generation or modification is claimed |
| Acquisition date | 2026-08-27 KST |
| Reference inputs | None; the original font binary and its OFL text are copied without modification from the official source directory |
| Human direction | The developer explicitly approved the F1 pairing: Gowun Batang for short display copy and Noto Sans KR for functional interface copy. |
| Human edits | No glyph editing, renaming, or derivative font work is claimed. Unity TMP atlas generation is technical processing only. |
| Third-party elements | SIL Open Font License 1.1; full text retained at `Assets/Fonts/GowunBatang/OFL.txt` and notice recorded in `Docs/ThirdPartyNotices.md` |
| Store submission decision | Ordinary bundled open-source font; retain the copyright and OFL notice with the distributed product and re-audit the final bundle before RC. |
| Release status | **Approved for prototype integration under OFL 1.1; final in-game readability and bundled-notice review remain required before RC.** |

### TEXT-UI-002 — casebook and cosmetics interface copy

| Field | Record |
| --- | --- |
| Repository files | `Assets/Scripts/Runtime/Localization/Localizer.cs`; generated Unity localization tables after `ProjectBuilder.BuildAll` |
| Asset type and intended use | English and Korean labels for casebook progress, locked records, collection tabs, cosmetic prices, ownership, and equip feedback |
| Tool | OpenAI Codex drafts concise bilingual functional interface copy from the developer-approved collection redesign |
| Creation date | 2026-08-26 KST |
| Reference inputs | Existing project terminology and bilingual catalog names only; no third-party prose supplied or copied |
| Human direction | The developer approved the recommended illustrated casebook and cosmetics workflow and retains final wording and release authority. |
| Human edits | No independent human rewrite claimed at intake. Korean naturalness, English clarity, line wrapping, and accessibility require developer review in Unity. |
| Third-party elements | None identified; short functional labels require no third-party attribution. |
| Store submission decision | Ordinary in-game interface copy; no separate AI-media declaration currently identified. Recheck selected-store requirements at submission time. |
| Release status | **Prototype copy — bilingual visual review required before RC.** |

### AUDIO-SYNTH-001 — procedural interaction feedback tones

| Field | Record |
| --- | --- |
| Repository files | Runtime synthesis code under `Assets/Scripts/Runtime/Infrastructure/Feedback`; no audio binary is stored or imported |
| Asset type and intended use | Very short interface tones for hold, correct sort, wrong sort, and shift completion |
| Tool | OpenAI Codex authored the deterministic waveform code from the developer-approved Phase 3 game-feel design |
| Creation date | 2026-08-26 KST |
| Prompt summary | Add restrained, warm-occult interaction feedback without third-party audio files or packages; keep the game fully playable when sound and haptics are disabled. |
| Negative constraints | No sampled audio, synthesized voice, music, named composer or franchise imitation, external model output, trademark sound, or third-party reference recording. |
| Reference inputs | None |
| Human direction | The developer approved the recommended Phase 3 scope. Codex selected short procedural tones, optional Android haptics, and independent persisted toggles to minimize cost and external dependencies. |
| Human edits | No human audio edit is claimed. Final gain, pitch, duration, comfort, and device-speaker suitability require developer listening review. |
| Third-party elements | None. Waveforms are computed at runtime from repository-owned source code and use Unity's built-in audio APIs. |
| Store submission decision | Runtime-generated interface tones are not submitted AI-generated audio files. Recheck the selected store's current disclosure wording if these tones are later rendered into promotional media. |
| Release status | **Prototype only — listening and device-volume review required before RC.** |

### AUDIO-SYNTH-002 — incident reaction and resolution tones

| Field | Record |
| --- | --- |
| Repository files | Runtime synthesis code under `Assets/Scripts/Runtime/Infrastructure/Feedback`; no audio binary is stored or imported |
| Asset type and intended use | Short procedural cues for authored key reactions and the completion of an incident |
| Tool | OpenAI Codex authored the deterministic waveform code from the developer-approved first-incident vertical-slice plan |
| Creation date | 2026-08-31 KST |
| Prompt summary | Strengthen decisive story moments with warm-occult chimes that remain subordinate to dialogue and readable on mobile speakers. |
| Negative constraints | No sampled audio, voice, music, named composer or franchise imitation, external model output, trademark sound, third-party recording, jump-scare impact, or harsh alarm. |
| Reference inputs | Existing repository-owned procedural feedback implementation only; no audio reference file was supplied. |
| Human direction | The developer approved larger, bolder dialogue, decisions, and reactions while preserving the warm occult atmosphere. Codex limited the scope to two short procedural cues and retained independent sound disablement. |
| Human edits | No human audio edit is claimed. Final gain, pitch, duration, comfort, dialogue balance, and device-speaker suitability require developer listening review. |
| Third-party elements | None. Waveforms are computed at runtime from repository-owned source code and Unity's built-in audio APIs. |
| Store submission decision | Runtime-generated feedback tones are not submitted AI-generated audio files. Recheck the selected store's current disclosure wording if rendered into promotional media. |
| Release status | **Prototype only — listening and device-volume review required before RC.** |

### TEXT-UI-001 — Phase 3 feedback settings copy

| Field | Record |
| --- | --- |
| Repository files | `Assets/Scripts/Runtime/Localization/Localizer.cs`; generated Unity localization tables after `ProjectBuilder.BuildAll` |
| Asset type and intended use | English and Korean labels for Feedback, Sound, Haptics, On, and Off in the in-game Settings screen |
| Tool | OpenAI Codex drafted the bilingual interface copy from the developer-approved Phase 3 feature scope |
| Creation date | 2026-08-26 KST |
| Reference inputs | Existing project terminology and bilingual UI conventions only; no third-party prose was supplied or copied |
| Human direction | The developer approved the recommended implementation and retains final wording and release authority. |
| Human edits | No independent human rewrite is claimed yet. Korean naturalness, English clarity, truncation, and accessibility require developer in-Unity review. |
| Third-party elements | None identified; short functional labels do not require attribution. |
| Store submission decision | Ordinary in-game settings copy; no separate AI-media declaration is currently identified. Recheck selected-store requirements at submission time. |
| Release status | **Prototype copy — bilingual visual review required before RC.** |

### DEV-CODE-001 — initial project implementation

| Field | Record |
| --- | --- |
| Scope | Initial Unity project structure, gameplay code, tests, build automation, and documentation committed from 2026-08-20 onward |
| Tool | OpenAI Codex with developer-authored product requirements and approvals |
| Human contribution | Product goals, genre selection, monetization constraints, platform decisions, acceptance decisions, and final release authority |
| Verification | Changes remain subject to Git review, automated tests, manual Unity execution, device testing, and third-party license review. AI assistance is not evidence of correctness or non-infringement. |
| Store submission decision | The historic Google Play asset-declaration help page concerns submitted visual assets, not source-code authorship. No code declaration is identified there; recheck the selected store's current requirements at submission time. |
| Release status | Allowed as reviewed source; each change still requires normal engineering and license gates. |

### SDK-INTEGRATION-001 — Android QA and resolved Google packages

| Field | Record |
| --- | --- |
| Date and scope | 2026-09-08 KST; completion of developer-restored Android QA/GMA integration |
| Repository files | `Assets/GoogleMobileAds/link.xml`, generated Android Gradle templates under `Assets/Plugins/Android`, `ProjectSettings/AndroidResolverDependencies.xml`, `ProjectSettings/GvhProjectSettings.xml`; build scripts, tests and integration documentation |
| Source and versions | Google-authored GMA Unity 11.3.0 via OpenUPM; official EDM4U archive 1.2.188 already pinned in the repository. Resolved package metadata and dependency XML were inspected locally. |
| AI role | Codex reviewed restored code, added regression tests and scoped build-state fixes, and drafted documentation. No new art, audio, player-facing prose or external AI media was generated. |
| Generated output | Linker configuration comes from the GMA plugin; Android dependency blocks/settings come from EDM4U and Unity 6000.3.21f1 templates. These are build support files, not a second SDK installation. |
| Rights and notices | Preserve the package Apache 2.0 licenses in `Docs/Licenses`; native Android GMA/UMP terms and AndroidX licenses require separate distribution review. See `Docs/ThirdPartyNotices.md`. |
| Preservation | Initial dirty assets and settings were copied with SHA-256 to ignored local validation evidence before Unity execution. Art metadata whitespace and the existing generated Korean font atlas are excluded from the integration commit. Remembering Rain source and approved narrative remain preserved. |
| Release decision | Development APK validation only, using Google sample app/rewarded IDs and debug signing. No release AAB, real identifiers, signing secrets or store deployment are authorized by this entry. |

## TEXT-WORKBENCH-20260911 — Hands-on case rewrite

Recorded before authoring: original AI-assisted English/Korean scene objectives, observations, tool labels, NPC dialogue and menu copy for the two existing cases, authored by Codex on 2026-09-11 under the developer's direction to make play understandable and engaging. Inputs are this repository's existing Ice/Rain story catalogs and the developer's first-play feedback. Reuses existing project artwork, portraits, fonts and audio; no new third-party source, image, voice, likeness or audio is introduced. Procedural uGUI highlights/interaction feedback visualize object changes. Human review of fun and final prose remains pending. See the dated hands-on investigations spec and QA evidence for implementation and screenshots. Not a store submission.

Native readability follow-up recorded before implementation on 2026-09-11: clarify the bilingual case/stage counter as `CASE {case} · {stage}/{count}` / `사건 {case} · {stage}/{count}`, using the authored stage count. Replace generic paper symbols for physical tools with original procedural cloth, putty-container, pad, wedge, flat spatula, lid, dropper, ribbon and envelope silhouettes; paper materials may share a paper symbol. Match cream-panel dialogue, observations and action results to the existing objective's dark-ink stroke weight. Show original uGUI reply-card/envelope shapes when Rain's letter compartment opens, then depict writing and packing through lines and folded shapes. Remove temporary intervention marks when another artifact is revealed or the workbench completes. These changes reuse the existing font and uGUI geometry only; no new image generation, bitmap editing, audio or external source is introduced. The native screenshots identify usability issues; this record makes no new human-fun or release approval claim.

## ART-WORKBENCH-20260911 — physical object state atlas

Recorded before image generation on 2026-09-11: the developer's hands-on investigation direction authorizes original AI-assisted object-state artwork so that performed actions have visible consequences. Codex will use the built-in ImageGen tool, without an API/CLI fallback, to generate one six-panel atlas for internal prototype play. No external reference, likeness, logo, additional package, voice, or audio is supplied. Inputs are these existing project-owned generated sprites; their original provenance and release limitations continue to apply:

| Reference | SHA-256 |
| --- | --- |
| `Assets/Resources/Art/Artifacts/unmelting-ice.png` | `7C9FA4CEA21C5B0C6750355136E822F3A468D428BBF27C7CD6DFAA870DE648DD` |
| `Assets/Resources/Art/Artifacts/mossy-watch.png` | `816819806FE16A6BBF3DB21C4A5ED36C97CF0B18BD6332ED076E48E0A9575A50` |
| `Assets/Resources/Art/Artifacts/paper-fish.png` | `29CD1292E2F02E54033350DB3C9A840A63E920B2E70B3A07941F68FC5A033D29` |
| `Assets/Resources/Art/Artifacts/moon-umbrella.png` | `3FFD4A6B696EDA32124AB9193ED6E286AA555B112F3CCA4921FBE07BEB44F454` |

Intended output: `Assets/Resources/Art/Workbench/workbench-states.png`, exact three-column/two-row grid with six equal square cells, alpha preserved, no captions or readable text. Intended states, in top-row then bottom-row order: repaired ice crack with leaf retained; leafless ice with open empty brass base; open watch glass after leaf extraction; opened watch back revealing crescent key; paper fish unfolded into a letter; repaired umbrella with a dry reply pocket. The atlas will be sliced in Unity at runtime without resampling or external image processing. Generation result, dimensions, hash, exact prompt and inspection outcome will be recorded below after selection. This is internal prototype art, not a store asset approval or a human fun assessment.

Exact built-in ImageGen prompt:

```text
Use case: precise-object-edit.
Asset type: ONE production game sprite atlas for Curio Clerk, a night lost-property repair game. Use the four supplied project-owned sprites as edit targets and identity/style references: image 1 = unmelting ice, image 2 = mossy pocket watch, image 3 = paper fish, image 4 = crescent-patched purple umbrella. Preserve their distinctive shapes, rich hand-painted faceted surfaces, dark plum outlines, warm brass, jewel blue ice, parchment and plum fabric. No outside style reference.

Canvas/layout: landscape 3:2, preferably 3072 x 2048 pixels. An EXACT uniform 3 columns by 2 rows atlas. Six SQUARE cells, each exactly one third of the width and one half of the height. Read order is top-left, top-middle, top-right, bottom-left, bottom-middle, bottom-right. Every cell contains one centered isolated complete object with a small transparent safety margin. All cells meet directly: NO drawn grid, NO panel borders, NO captions, NO numbering, NO gutters, NO extra page margin. Real transparent background (alpha) behind each object, NOT a black/white rectangle or a checkerboard drawn into the image. Nothing crosses cell boundaries.

Panel top-left: edit image 1. Same upright jewel-blue ice on ornate brass base with the red-purple maple leaf still clearly trapped inside. The leaking crack along the left front edge is now sealed by one restrained visible ivory repair seam. No leaking frost. Keep the ice identity and framing.
Panel top-middle: edit image 1. Same upright blue ice and brass base, but the ice interior is EMPTY and transparent blue: absolutely NO leaf, NO plant shape, NO leaf silhouette or residue anywhere. The brass base's small front lid is open and reveals an EMPTY dark compartment. Keep the ice silhouette.
Panel top-right: edit image 2. The same golden mossy pocket watch facing us at the reference angle, winding crown upper left. Its round glass cover is hinged visibly OPEN to the side, exposing the dial and free hands. Most moss has been brushed away from the hinge, but a little remains at the outer edge. There is NO leaf caught inside.
Panel bottom-left: edit image 2. The same golden mossy pocket watch is turned to show its BACK, with the round back cover visibly hinged OPEN. Inside the back compartment, a small brass key with a clear crescent-shaped bow is visibly revealed beside the gears. The key is resting inside the open watch, not floating outside. Recognizably the same pocket watch.
Panel bottom-middle: edit image 3. The paper fish is now COMPLETELY UNFOLDED into a single broad warm parchment letter, with clear old diagonal fish-fold crease lines still visible. Flat open letter, not a fish silhouette, no fins, no fish eye. Only a few non-readable ink strokes suggesting handwriting; absolutely no legible letters, words or numbers. Preserve the reference's textured parchment and plum edge shading.
Panel bottom-right: edit image 4. Same closed purple crescent-patched umbrella at its diagonal reference angle, brass curved handle upper-right and point lower-left. Neat clearly visible pale repair stitches now close the loose inner pocket seam. A small DRY CREAM ENVELOPE peeks out of this repaired fabric pocket, with a soft ribbon securing the umbrella. Retain the recognizable crescent patches and the whole umbrella silhouette. No dripping water.

Art direction: match the four source sprites, polish for in-game use at small size, clear silhouettes and visibly different physical states. Maintain full objects without clipping. No UI, no people, no tools outside objects, no added room or scenery, no text or logo or watermark. Generate exactly this single six-cell atlas.
```

First generation inspection: 1536 x 1024 pixels, PNG RGB24. The six subjects and style were usable, but the background was a baked checkerboard rather than alpha and the top-middle base ornament crossed the intended row boundary. This candidate is not selected for integration. The integration owner authorized a flat workbench-color background and a two-candidate limit, so the actual second generation uses the solid-background prompt recorded below.

Actual second-generation prompt (built-in ImageGen edit, first candidate as the sole input):

```text
Use case: precise-object-edit.
Edit the supplied six-panel sprite atlas. Keep the same six subjects, painting style, state details, row order and column order. Make ONLY these technical corrections:
1. REPLACE every grey/white checkerboard pixel with a perfectly flat solid dark plum background color #1F141F (RGB 31,20,31). One uniform background across the whole atlas; no texture, grain, glow, lighting gradient, checkerboard, grid lines, shadows, borders or panel rectangles behind the objects. This background is intentionally opaque; DO NOT use transparency or a checkerboard.
2. Exact output canvas 1536 x 1024 pixels, with 3 columns × 2 rows of exactly 512 × 512 square cells. Fit each complete subject inside its own cell with at least 20 pixels of that flat dark plum background clear on all four edges. The TOP MIDDLE ice base ornament must end above y=492, never crossing the row boundary at y=512. No object, open lid, ornament or shadow crosses x=512, x=1024 or y=512. Keep all six objects complete, shrinking each minimally inside its own square if required.
State order: top-left sealed blue ice with trapped red-purple leaf; top-middle EMPTY leafless blue ice and OPEN EMPTY brass base; top-right golden mossy watch with glass cover OPEN, NO leaf; bottom-left golden watch BACK OPEN and crescent-bow key INSIDE; bottom-middle fully unfolded parchment letter, no fish shape, only non-readable scribbles; bottom-right repaired purple crescent umbrella with dry envelope pocket and soft ribbon.
No labels, readable writing, numbers, panel outlines or gutters. Keep the ornate warm brass, faceted hand-painted jewel surfaces, dark plum object outlines and existing object identities. Deliver exactly one six-cell atlas.
```

Selected result: built-in ImageGen second candidate `exec-04b2002c-d8ec-4952-805d-157d71bbe522.png`, copied byte-for-byte to `Assets/Resources/Art/Workbench/workbench-states.png`. Size: 1536 x 1024, RGB24, 2,169,406 bytes. SHA-256: `8F9C39B6DDF45D55BBA74CF7B809B5327916EF098CA914208BF167A45D5F918F`. The first and second tool outputs remain in the local generated-images directory; no CLI, manual pixel editing, resampling, or other image-transformation tool was used. Metadata and sample pixels were read with System.Drawing only.

Inspection: all six requested states are recognizable, with no readable writing, labels, branding or depicted people. The second candidate has an opaque dark plum background, not alpha; sampled background pixels are approximately RGB(33,20,30), with slight variation rather than the requested exact flat color. The 3x2 subject order is correct, but the top-middle base ornament extends to source y=521 and violates the intended uniform row boundary at y=512. Preserve the PNG and use these runtime source rectangles to retain complete objects and exclude the ornament from the letter sprite. Coordinates use Unity's lower-left origin; runtime slicing requires in-Unity visual validation:

| State | Column / row from top | Unity source rectangle `(x,y,width,height)` |
| --- | --- | --- |
| Sealed ice, leaf retained | 1 / 1 | `(0,512,512,512)` |
| Leafless ice, open empty base | 2 / 1 | `(512,492,512,532)` |
| Open watch face, leaf removed | 3 / 1 | `(1024,512,512,512)` |
| Open watch back, crescent key | 1 / 2 | `(0,0,512,512)` |
| Unfolded letter | 2 / 2 | `(512,0,512,488)` |
| Repaired umbrella and reply | 3 / 2 | `(1024,0,512,512)` |

This documented crop exception and approximate background supersede the initial alpha/uniform-cell intent. No third-party attribution has been identified. Internal prototype selection only; physical-state comprehension, visual fit on device and final similarity/trademark review remain separate checks.

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
Store submission decision and reason:
Reviewer, review date, and status:
```

For store screenshots, also record which in-game assets are visible. A screenshot of AI-assisted game art and a separately AI-generated promotional composition are distinct declaration decisions.

## Evidence retention

- Use Git history for source and document changes.
- Keep project-owned editable source files and meaningful before/after exports. Do not commit secrets or personal data.
- Store purchase receipts and licenses outside the public repository; record a non-sensitive receipt ID or storage reference here.
- If an exact historic prompt is unavailable, label the summary as reconstructed. Never invent an exact prompt after the fact.
- Review the ledger at content freeze and again immediately before store submission.

## Official references

- Google Play, AI asset declaration: https://support.google.com/googleplay/android-developer/answer/17262077?hl=en
- Google Play, AI-Generated Content policy scope: https://support.google.com/googleplay/android-developer/answer/14094294?hl=en
- Korea Copyright Commission, AI/copyright guide collection: https://www.copyright.or.kr/notify/notice/view.do?brdctsno=55402
- Korea Copyright Commission, AI-assisted work registration guide: https://www.copyright.or.kr/information-materials/publication/research-report/view.do?brdctsno=54253
- OpenAI Terms of Use: https://openai.com/policies/row-terms-of-use/

## TEXT-QA-20260909 — Native SDK validation

Original Codex-authored functional English/Korean labels for the QA-only advertising/consent panel and technical validation report. No new art, audio, story prose, or external assets. Reuses repository TMP fonts. QA labels describe in-memory rewards and SDK state; they are excluded from release players.

## TEXT-PLAYTEST-20260909 — Internal playtest integration

Recorded before authoring: Codex will supply original English/Korean functional Hold-availability guidance, the internal integration QA report, and a bilingual human playtest guide with an anonymous response form. The request is to make the existing two-incident build ready for internal human testing without changing story identity or adding incidents. Existing project art and licensed fonts are reused; no new artwork, audio, external asset, or story prose is introduced. The board layout separates existing artwork from existing titles. Technical agent validation is not human playtest evidence. Human assessment of comprehension, enjoyment, and release suitability remains pending; these materials are for internal testing, not store submission.

## TEXT-WORKBENCH-20260911 — Candidate 2 copy corrections

Recorded before editing on 2026-09-11: Codex will make three bilingual copy corrections from the Candidate 1 Android screenshot review. The frozen-watch objective will describe protecting the object inside; the paper-fish objective will describe avoiding damage to the letter; and the unfolded letter's result and discovery will replace a reference to a nonexistent box drawing with written instructions for leaving a reply in the voice box. Inputs are existing project-authored English/Korean content and local game screenshots. No new image, external text, likeness, audio, package, puzzle action, or story outcome is introduced. The review checks consistency and wording; it is not human enjoyment evidence.

## TEXT-WORKBENCH-HINT-20260911 — Optional contextual help

Recorded before player-facing copy changes on 2026-09-11: following the user's report that the revised puzzles are more enjoyable but the second ice scene leaves them stuck without a hint, Codex will add original functional English/Korean labels and contextual guidance for a player-requested `Hint / 힌트` button. The first request reuses the existing bilingual hint for the next available unfinished action. A further request names any required observation still missing, or formats an instruction using the current tool and target labels. Guidance is limited to the current action and does not reveal later story events. It uses existing project-owned text, fonts and UI shapes; no new artwork, audio, outside prose, likeness, package or dependency is introduced. Hints require no currency, ad or timer and do not change saved progression. Runtime wording, highlight behavior and bilingual readability require separate validation; this record does not claim the help has already resolved the user's difficulty or establish enjoyment beyond their stated feedback.

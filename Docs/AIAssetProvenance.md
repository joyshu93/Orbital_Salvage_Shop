# AI asset provenance and release policy

Last reviewed: 2026-08-28 (KST)

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
| Asset type and intended use | Five-stage English/Korean opening beats, closing hooks, and Stable/Precise/Resonant artifact reactions for the first playable incident prototype |
| Tool | OpenAI Codex drafted the bilingual narrative copy from the developer-approved incident specification and fixed implementation plan |
| Creation date | 2026-08-31 KST |
| Reference inputs | The approved Curio Clerk narrative occupational puzzle specification, fixed five-stage incident beats, existing artifact catalog fiction, and Three-Seal Docket terminology; no third-party prose supplied or copied |
| Human direction | The developer approved the exact opening and closing beats, the five authored shift matrix, the caretaker tone, and the semantic limits for Stable, Precise, and Resonant reactions. |
| Human edits | The approved plan fixes the opening and closing copy. The short quality reactions are prototype drafts; Korean naturalness, English clarity, tone, line wrapping, and contextual fit require developer bilingual review in Unity. |
| Third-party elements | None identified. No external prose, named style, franchise text, quotation, or third-party reference material was used. |
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

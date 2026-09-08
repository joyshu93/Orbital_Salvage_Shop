# First incident narrative image prompts

Date: 2026-08-31 KST
Tool path: built-in OpenAI ImageGen
Status: developer-approved prototype selections integrated after preview review.

Developer approval was received on 2026-08-31 KST. The selected outputs, hashes,
technical alpha normalization, and prototype-only release status are recorded under
`ART-NARRATIVE-001` and `ART-EFFECT-001` in `Docs/AIAssetProvenance.md`.

## Shared portrait invariants

- Original, non-famous adult senior night clerk; no real-person likeness.
- Same face, age, hairstyle, body proportions, clothing, brooch, and palette across all four moods.
- Warm occult archive clothing: layered plum waistcoat and coat, parchment shirt, antique-brass fasteners, practical sleeves, small moon-and-key brooch.
- Waist-up, front three-quarter portrait, face and hands readable at mobile size, broad uncluttered silhouette.
- Hand-painted gouache-and-ink game illustration with tactile paper grain; deep plum, parchment, antique brass, muted sage, restrained amber glow.
- Genuine transparent background with clean hair and clothing edges.
- No text, letters, logos, signature, watermark, named artist/studio/franchise style, protected character, horror gore, modern clothing, tiny facial features, extra people, desk, scenery, frame, or checkerboard pixels.

## Neutral anchor — normalized generation prompt

```text
Use case: stylized-concept
Asset type: transparent visual-novel character portrait for an Android portrait game
Primary request: create one original, non-famous adult senior night clerk for a warm occult lost-property archive; calm neutral expression that feels observant, trustworthy, slightly mysterious, and ready to teach without condescension
Scene/backdrop: genuinely transparent background; isolated character only
Subject: adult senior clerk, waist-up, front three-quarter view, direct but gentle gaze; distinctive face with mature lines; tidy dark hair with one warm gray streak; layered deep-plum waistcoat and archive coat over a parchment shirt; antique-brass fasteners; practical rolled sleeves; small original moon-and-key brooch; one hand lightly raised as if about to explain a rule
Style/medium: polished hand-painted gouache-and-ink game illustration, tactile paper grain, confident dark-plum contours, warm restrained occult fantasy, commercially readable mobile-game character art
Composition/framing: 3:4 portrait canvas, large head-and-torso silhouette occupying most of the frame, face and hand readable at small mobile size, generous clean margin around hair and sleeves
Lighting/mood: warm amber archive lamplight from one side with a very subtle cool nocturnal rim; inviting, composed, quietly uncanny rather than frightening
Color palette: deep plum, parchment cream, antique brass, muted sage, restrained amber highlights
Constraints: original person and costume; transparent alpha; no text, letters, symbols beyond the original simple brooch, logo, signature, watermark, famous likeness, named artist/studio/game/film/franchise style, protected character, modern clothing, horror gore, tiny facial features, extra people, furniture, scenery, frame, or checkerboard pixels
```

## Concerned expression — normalized identity-preserving edit prompt

```text
Use case: identity-preserve
Asset type: transparent visual-novel character portrait variant
Input images: Image 1 is the project-owned neutral senior clerk portrait and the sole edit target/reference
Primary request: change only the facial expression and small hand gesture to concerned attentiveness; brows gently knit, eyes following an unexpected frost trace, mouth slightly tense, raised hand closer to the chest
Constraints: preserve exactly the same identity, face structure, age, hairstyle and gray streak, body proportions, pose direction, clothing, brooch, palette, gouache-and-ink finish, linework, lighting, crop, scale, and transparent background; no redesign, no extra objects, no text, logo, signature, watermark, gore, or checkerboard pixels
```

## Alert expression — normalized identity-preserving edit prompt

```text
Use case: identity-preserve
Asset type: transparent visual-novel character portrait variant
Input images: Image 1 is the project-owned neutral senior clerk portrait and the sole edit target/reference
Primary request: change only the facial expression and small hand gesture to sharply alert but controlled; eyes widened and focused, brows lifted then drawn inward, mouth open just enough for an urgent warning, raised hand extended in a clear protective stop gesture
Constraints: preserve exactly the same identity, face structure, age, hairstyle and gray streak, body proportions, pose direction, clothing, brooch, palette, gouache-and-ink finish, linework, lighting, crop, scale, and transparent background; dramatic mobile-readable emotion without panic or horror; no redesign, no extra objects, no text, logo, signature, watermark, gore, or checkerboard pixels
```

## Relieved expression — normalized identity-preserving edit prompt

```text
Use case: identity-preserve
Asset type: transparent visual-novel character portrait variant
Input images: Image 1 is the project-owned neutral senior clerk portrait and the sole edit target/reference
Primary request: change only the facial expression and small hand gesture to visible restrained relief; shoulders soften slightly, eyes warm, a genuine small smile, raised hand opens palm-up as if acknowledging the player's care
Constraints: preserve exactly the same identity, face structure, age, hairstyle and gray streak, body proportions, pose direction, clothing, brooch, palette, gouache-and-ink finish, linework, lighting, crop, scale, and transparent background; warm emotional payoff without comedy; no redesign, no extra objects, no text, logo, signature, watermark, gore, or checkerboard pixels
```

## Frost-edge overlay — normalized generation prompt

```text
Use case: stylized-concept
Asset type: transparent 9:16 mobile game screen effect overlay
Primary request: create a decorative frost-edge overlay for a warm occult archive game; pale blue-white crystalline veins gather boldly around all four outer edges while leaving the center broadly empty and readable; a few restrained antique-amber reflections connect the cold effect to lamplight
Scene/backdrop: genuinely transparent background and transparent open center
Subject: irregular organic frost feathers, hairline cracks, small ice blooms, and a few larger corner crystals; strongest at corners and lower edge, thinning rapidly toward the center
Style/medium: hand-painted gouache-and-ink effect texture matching tactile warm-occult game illustration; elegant, dramatic, readable on a 9:16 artifact card; not photorealistic and not horror
Composition/framing: 2:3 portrait canvas; continuous edge treatment; at least the central 60 percent remains visually empty; no enclosed frame line
Lighting/mood: cold pale-blue frost with subtle warm amber reflection, magical tension that remains cozy and legible
Constraints: genuine alpha transparency; empty center; no text, letters, rune, symbol, logo, signature, watermark, famous style, protected imagery, blood, gore, opaque background, scenery, character, object, border frame, or checkerboard pixels
```

## Selection and rejection record

- Selected the first neutral portrait and its concerned, alert, and relieved identity-preserving variants because the face, glasses, hair streak, costume, books, palette, crop, and lighting remained consistent while the expressions and hand gestures changed clearly.
- The three selected expression outputs contained a baked checkerboard. After the developer explicitly approved local technical cleanup, Codex removed the neutral checker field and edge matte and resized the four portraits to 768 × 1024. No facial, costume, pose, or prop repaint was performed.
- Rejected an ImageGen background-cleanup attempt because it replaced transparency with an opaque amber/black backdrop.
- Rejected later text-only transparent portrait generations because their age, face, costume, and pose no longer matched the selected neutral identity.
- Selected the original 1024 × 1536 frost overlay after confirming a transparent center and restrained amber reflections. It required no resize or repaint.

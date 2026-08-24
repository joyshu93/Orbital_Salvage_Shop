# Visual slice ImageGen prompt record

Generated through the built-in OpenAI ImageGen tool on 2026-08-24 KST. No third-party images, named artists, studios, games, films, franchises, brands, logos, people, or private materials were supplied. Later artifact prompts used only earlier outputs from this same project as palette and brushwork references.

## ART-VSLICE-BG-001 — occult desk background

```text
Use case: stylized-concept
Asset type: portrait mobile game background texture
Primary request: an original warm occult night-shift lost-and-found clerk desk viewed from directly overhead, designed as a subdued full-screen UI backdrop
Scene/backdrop: dark plum-stained wooden desktop with faint paper grain, a soft amber lamplight vignette near the upper edge, restrained brass corner inlays and barely visible filing marks around the outer margins
Style/medium: hand-painted gouache with clean modern mobile-game finish, softly textured rather than photorealistic
Composition/framing: 2:3 portrait; the central 75 percent must remain visually quiet and dark so cream UI cards and bilingual text stay readable; decoration only near edges
Lighting/mood: cozy midnight office, mysterious but welcoming
Color palette: deep plum #351B2B, wine #5B2944, parchment #F2E5C4, amber #E0A24B, muted sage accents
Materials/textures: dark wood, paper grain, restrained brass
Constraints: no text, no letters, no symbols, no characters, no branded objects, no watermark, no signatures, no existing franchise or artist imitation; original composition; seamless enough to crop across tall Android aspect ratios
Avoid: busy props, high contrast in the center, horror gore, neon colors, photorealism
```

## ART-VSLICE-ARTIFACT-001 — sleeping teacup

```text
Use case: stylized-concept
Asset type: transparent 2D mobile game artifact sprite
Input images: the project-owned desk background is a palette and brushwork reference only; do not copy its objects or composition
Primary request: an original "Sleeping Teacup" curio for a warm occult lost-and-found sorting game
Subject: one small cream porcelain teacup resting on its saucer, subtly alive, with a peaceful closed-eye motif formed by the glaze and one delicate curl of amber steam
Style/medium: hand-painted gouache mobile-game inventory illustration; crisp silhouette; restrained dark-plum outline; same warm rendering language as the project background
Composition/framing: centered three-quarter view, entire object visible, generous transparent padding, square sprite
Lighting/mood: cozy lamplight, whimsical and gentle rather than childish
Color palette: parchment cream, deep plum, muted rose, small amber highlights
Materials/textures: softly worn porcelain, tiny hairline glaze detail, painted paper texture
Constraints: genuinely transparent background; exactly one teacup and saucer; no text, letters, numbers, logos, signature, watermark, hands, characters, scenery, shadow box, frame, or existing franchise/artist imitation; strong silhouette readable at 128 px
Avoid: photorealism, horror, face-like human features, glossy 3D render, extra props
```

## ART-VSLICE-ARTIFACT-002 — mirror seed

```text
Use case: stylized-concept
Asset type: transparent 2D mobile game artifact sprite
Input images: the project-owned desk background is the palette reference; the project-owned Sleeping Teacup is the brushwork, outline weight, lighting, and mobile readability reference only
Primary request: an original "Mirror Seed" curio for the same warm occult lost-and-found sorting game
Subject: exactly one small almond-shaped seed, its outer shell deep plum and antique brass, split just enough to reveal a smooth mirror-silver inner face with a subtle impossible reflection and one tiny root curl
Style/medium: hand-painted gouache mobile-game inventory illustration; crisp silhouette; restrained dark-plum outline; match the project reference finish
Composition/framing: centered three-quarter view, entire object visible, generous transparent padding, square sprite
Lighting/mood: cozy lamplight with a slightly uncanny but friendly glint
Color palette: deep plum, tarnished brass, parchment highlights, mirror silver, tiny amber glint
Materials/textures: organic seed shell, brushed antique metal, softly reflective inner surface
Constraints: genuinely transparent background; exactly one seed; no text, letters, numbers, logos, signature, watermark, hands, characters, scenery, frame, extra props, or existing franchise/artist imitation; strong silhouette readable at 128 px
Avoid: photorealism, horror, eyeballs, human face, glossy 3D render, gemstones, flowers
```

## ART-VSLICE-ARTIFACT-003 — thimble storm

```text
Use case: stylized-concept
Asset type: transparent 2D mobile game artifact sprite
Input images: the project-owned Sleeping Teacup and Mirror Seed are brushwork, outline, lighting, palette, and mobile readability references only
Primary request: an original "Thimble Storm" curio for the same warm occult lost-and-found sorting game
Subject: exactly one antique brass sewing thimble standing upright, containing a tiny soft plum thundercloud with one curling silver-blue raindrop stream and a miniature amber lightning spark
Style/medium: hand-painted gouache mobile-game inventory illustration; crisp silhouette; restrained dark-plum outline; match the project reference finish
Composition/framing: centered three-quarter view, entire thimble and contained storm visible, generous transparent padding, square sprite
Lighting/mood: cozy lamplight, lively and whimsical rather than dangerous
Color palette: tarnished brass, deep plum cloud, muted blue-gray rain, parchment highlights, tiny amber spark
Materials/textures: dimpled antique thimble metal, soft painted cloud, delicate wet glints
Constraints: genuinely transparent background; exactly one thimble and one contained cloud; no text, letters, numbers, logos, signature, watermark, hands, characters, scenery, frame, extra props, or existing franchise/artist imitation; strong silhouette readable at 128 px
Avoid: photorealism, horror, tornadoes, large violent lightning, glossy 3D render, extra sewing tools
```

ImageGen initially rendered the checkerboard as opaque pixels. A second background-extraction edit removed only that checkerboard and required genuine transparent alpha while preserving the object, composition, colors, texture, lighting, scale, and square canvas.

## ART-VSLICE-ARTIFACT-004 — whispering key

```text
Use case: stylized-concept
Asset type: transparent 2D mobile game artifact sprite
Input images: the project-owned Sleeping Teacup and Mirror Seed are brushwork, outline, lighting, palette, and mobile readability references only
Primary request: an original "Whispering Key" curio for the same warm occult lost-and-found sorting game
Subject: exactly one elegant antique brass key with a deep-plum enamel bow, a small keyhole-shaped negative space, and two very subtle parchment-colored whisper wisps curling near the teeth
Style/medium: hand-painted gouache mobile-game inventory illustration; crisp silhouette; restrained dark-plum outline; match the project reference finish
Composition/framing: centered diagonal three-quarter view, entire key visible, generous transparent padding, square sprite
Lighting/mood: cozy lamplight, mysterious and welcoming
Color palette: tarnished brass, deep plum enamel, parchment wisps, small amber highlights
Materials/textures: engraved antique metal with modest wear, matte enamel, softly painted wisps
Constraints: genuinely transparent background; exactly one key; no text, letters, numbers, logos, signature, watermark, hands, characters, scenery, doors, frame, extra props, or existing franchise/artist imitation; strong silhouette readable at 128 px
Avoid: photorealism, horror, skulls, eyeballs, glossy 3D render, ornate clutter, multiple keys
```

ImageGen initially rendered the checkerboard as opaque pixels. A second background-extraction edit used the same preservation constraints as the Thimble Storm correction.

## Technical validation record

- The four selected sprites were checked as 32-bit ARGB PNGs with a corner alpha value of zero.
- The desk background intentionally remains opaque.
- Unity import compression and resizing are technical processing, not claimed human creative edits.

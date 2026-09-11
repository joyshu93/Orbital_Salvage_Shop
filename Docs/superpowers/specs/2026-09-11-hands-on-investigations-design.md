# Hands-on investigations

The developer's first play session found no reason to play, unclear meaning of actions, repetitive classification, and unnatural Korean NPC/menu text. The developer explicitly delegates design decisions and implementation without repeated questions. This supersedes prior assumptions that twelve-item classification and three stamps must remain the main story activity.

## Player contract

You work at a night lost-property office and help strange objects that are causing concrete problems. Each scene begins with a visible problem and one plain-language request. Inspect parts of a large object; choose a tool and drag it onto the relevant part (tap tool then target is also supported). The object and room visibly change after interventions. Findings explain the next scene; a completed case resolves its immediate problem and supplies a reason to open the next case.

Replace both existing incidents' main gameplay with ten authored workbench scenes, retaining their stable incident/stage IDs, chronology, collectibles and completed-stage save checkpoints. Ice follows leaking cold, a missing leaf, a watch, a frozen seal and an umbrella. Rain follows the umbrella's remembered voice, old names, an unsent letter, a promise and a safe ending. Do not require three destination stamps or twelve unrelated items to reveal each next scene. Regular Free Shift remains the separate sorting practice mode.

Tools have concrete names, targets are visible and inspectable, and observations persist on the workbench. Actions have prerequisites grounded in clues and physical effects. Wrong experiments explain the observed result, do not consume lives, and never create an unrecoverable state. No timer, repeated tapping quota, ad or paywall. Completion fires once. Retry restarts the current workbench; pause/menu preserves completed-stage progress. Legacy progress must remain readable.

## Architecture and interfaces

- Core/Workbench: immutable WorkbenchPuzzle and WorkbenchStep definitions; WorkbenchSession owns observations, completed steps and one-way completion. Pure C#, no UnityEngine.
- Content/Workbench: WorkbenchSceneDefinition, targets/tools/step copy and WorkbenchCatalog.Find(stageId). All text English and Korean using existing LocalizedCopy.
- Presentation: GameApp partial Workbench page reuses existing canvas, art, fonts, audio feedback and IncidentRunner/progression/save boundary. WorkbenchToolDrag handles EventSystem pointer drag/drop and tap alternatives, no new input package.
- Old sorting tests stay applicable to sorting practice; story integration tests change to test the newly intended experience, with explicit coverage of entry, observations, tools, completion, next case, replay, save and both languages.

## Acceptance

First screen establishes job and immediate problem in plain words. First meaningful physical action occurs without a terminology tutorial. Every stage offers observable object interaction and causally distinct feedback, not a relabelled three-button classification. Ten scenes can complete in order offline; replay does not erase completion. Menu/NPC copy uses conversational Korean and equivalent English. Both portrait 16:9 and tall layouts remain readable. Validate real drag/drop and tap input on the APK, preserve existing app/save/artifacts, and report remaining human fun uncertainty honestly.

Unity 6000.3.21f1. Android portrait, API 29 minimum/API 36 target, ARM64 IL2CPP debug APK, Google official sample ad IDs only. No AAB, merge, release or credentials. No new third-party assets or packages. Generated assets only through ProjectBuilder.BuildAll.

# Orbital Salvage Shop Design

## Goal

Build a small PC/Steam-focused indie management simulation that can reach a playable 7-day demo before expanding toward a full release.

## Working Title

Orbital Salvage Shop

## Target Platform

The primary target is PC via Steam. The project should be designed for keyboard and mouse first. Web or mobile versions are not the first release target. A web prototype or trailer capture build can be considered only after the 7-day PC demo proves the core loop.

## Engine Direction

Use Godot as the preferred engine. The game is 2D, UI-heavy, and small in scope, which fits Godot's scene system and data-driven workflow. The design should avoid engine-specific complexity where possible, but implementation planning can assume Godot.

## Concept

The player runs a small salvage shop and repair bench in a quiet corner of an outer-orbit space station. Each day, the player buys scrap lots, breaks them down into useful parts, chooses which parts to keep or sell, fulfills customer orders, and pays modest shop costs at the end of the day.

The tone is warm lo-fi science fiction: old metal, soft lights, a small neon sign, shelves of parts, a round station window, and a shop that feels worn but cared for.

## Core Loop

1. Start the day with money, reputation, inventory, and available upgrades.
2. Review three scrap lots in the market.
3. Buy one or two scrap lots within the available budget.
4. Disassemble scrap lots into parts.
5. Keep, sell, or use parts in recipes.
6. Complete customer orders when the inventory satisfies their requirements.
7. End the day and pay rent or maintenance.
8. Unlock better orders, recipes, or upgrades as the week progresses.

## First Playable Slice Scope

The first playable slice covers 7 in-game days.

Initial content:

- 6 scrap lot types
- 12 part types
- 8 customer order types
- 6 crafting or repair recipes
- 4 shop upgrades
- 4 customer portraits
- 1 main shop background
- 5 main UI panels

This scope is intentionally small. The demo should prove whether the loop is understandable, repeatable, and satisfying before expanding content.

## Player Decisions

The game should create small but frequent decisions:

- Buy a cheap scrap lot with uncertain output or save money for a better lot.
- Sell a part immediately or keep it for a future order.
- Complete a low-value order now or wait for a better use of rare parts.
- Upgrade storage, scanner accuracy, disassembly yield, or assembly capability.
- Accept that some orders will expire instead of trying to complete everything.

The game should not punish the player harshly in the first slice. Pressure should come from opportunity cost and modest daily expenses, not from sudden failure.

## Economy

The economy has four main resources:

- Money: used to buy scrap lots and upgrades; earned from selling parts and completing orders.
- Reputation: earned from orders; increases market and order quality over time.
- Parts: items with tags, rarity, and value.
- Day count: advances after each end-of-day settlement.

Daily cost should be light. A bad day slows progress but should not usually end the run.

## Data Model

Scrap lots have:

- id
- display name
- purchase price range
- possible part outputs
- output count range
- quality hint
- risk or uncertainty value

Parts have:

- id
- display name
- tags such as circuit, power, hull, sensor, rare, unstable
- rarity
- base sale value

Orders have:

- id
- customer name or portrait id
- display title
- required parts or required tags
- reward money
- reputation reward
- expiry behavior

Recipes have:

- id
- display name
- required part ids or tags
- output item or completed order type

Upgrades have:

- id
- display name
- cost
- effect
- unlock day or reputation requirement

Content should live in data files or Godot resources rather than being hard-coded into UI scripts.

## Main Screens

The main presentation is a 2D side-view shop scene with UI panels.

Required panels:

- MarketPanel: shows daily scrap lots and lets the player buy them.
- WorkbenchPanel: disassembles scrap and performs recipe assembly.
- InventoryPanel: shows owned parts and allows selling.
- OrdersPanel: shows available customer orders and completion status.
- DayEndPanel: summarizes income, expenses, reputation, and next-day unlocks.

The player should always be able to see current money, day, and reputation.

## Visual Direction

The first version uses a limited asset set with strong consistency:

- One warm lo-fi space shop background.
- Simple but polished part and scrap icons.
- Soft lighting, small animated details, and restrained UI motion.
- Color palette built around warm amber lights, desaturated metal, muted teal/blue space tones, and a few neon accent colors.

Animation should be modest:

- slow star movement outside the window
- subtle neon flicker
- workbench light changes
- small particles or sound when disassembling scrap
- a short order-complete transition

The first slice should prefer readable, cohesive visuals over large asset volume.

## Audio Direction

Audio should support the warm lo-fi science fiction tone:

- soft ambient shop loop
- low mechanical hum
- small clicks, clunks, and scanner beeps
- satisfying but restrained order completion sound

Music and sound are not required for the earliest logic prototype, but the playable demo should include temporary audio cues for the core actions.

## Technical Architecture

Primary Godot scenes and scripts:

- ShopScene: main scene and panel container.
- MarketPanel: scrap purchasing.
- WorkbenchPanel: disassembly and assembly.
- InventoryPanel: part browsing and selling.
- OrdersPanel: order display and completion.
- DayEndPanel: day settlement.
- GameState: money, day, reputation, inventory, upgrades, and current offers.
- DataLoader: loads scrap, part, order, recipe, and upgrade definitions.
- EconomyService: calculates prices, rewards, expenses, and upgrade effects.
- SalvageService: resolves scrap disassembly into parts.
- OrderService: checks whether orders can be fulfilled.
- SaveService: saves and loads current run state.

Business logic should be separated from UI where practical so core calculations can be tested without clicking through the whole game.

## Save System

The first save system is simple:

- auto-save at the start of each day
- store day, money, reputation, inventory, upgrades, and progression unlocks
- one save slot for the first slice

Manual save slots, cloud saves, and profile management are excluded from the first slice.

## Testing And Verification

The first implementation should verify these core behaviors:

- disassembling a scrap lot produces valid parts
- order completion consumes the right parts
- recipes match the right tags or part ids
- end-of-day settlement applies rewards and expenses correctly
- upgrades change the correct system values
- a 7-day run can be completed without blocking progress

Manual playtesting success criteria:

- The player understands the basic loop within 5 minutes.
- Buying scrap lots feels meaningfully different from day to day.
- Completing or skipping orders requires a real choice.
- At the end of day 7, the player wants to see more upgrades or content.
- The screen feels cohesive even with temporary prototype assets.

## Risks

The largest risk is weak economy tuning. Mitigation: keep all values data-driven and make the 7-day run easy to rebalance.

The second risk is UI overload. Mitigation: limit the first slice to five panels and keep the permanent HUD minimal.

The third risk is insufficient visual appeal. Mitigation: invest early in one cohesive shop background, UI style, and icon language before expanding content.

## Out Of Scope For The First Slice

These features are not part of the 7-day demo:

- online multiplayer
- mobile release
- controller-first UI
- complex character animation
- large shop layout editing
- procedural story generation
- 21-day campaign
- Steam achievements
- multiple save slots
- localization beyond the first working language

## Expansion Path

If the 7-day demo is fun, expand toward:

- 21-day campaign
- more scrap and part types
- customer personalities
- rare part events
- shop expansion
- more recipes and order chains
- Steam demo page
- trailer and capsule art planning

## Approved Direction

The approved direction is a PC/Steam-focused, Godot-built, 2D side-view, warm lo-fi space salvage shop management game centered on buying scrap, disassembling it into parts, fulfilling orders, and surviving light daily costs.

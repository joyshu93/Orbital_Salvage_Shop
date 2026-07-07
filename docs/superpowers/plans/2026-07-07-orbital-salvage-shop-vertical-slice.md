# Orbital Salvage Shop Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first Godot-based 7-day playable slice for Orbital Salvage Shop: buy scrap, disassemble it into parts, fulfill orders, settle days, save progress, and operate the loop through a simple 2D shop UI.

**Architecture:** Use Godot 4.x with GDScript. Keep the economy, salvage, order, recipe, day-flow, and save logic in small service scripts that do not depend on UI nodes. Load all playable content from JSON so values can be tuned without editing gameplay scripts.

**Tech Stack:** Godot 4.x, GDScript, JSON data files, Git, built-in headless Godot script execution for lightweight tests.

## Global Constraints

- Primary repository root: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop`.
- Primary target is PC via Steam.
- Project should be designed for keyboard and mouse first.
- Web or mobile versions are not the first release target.
- Use Godot as the preferred engine.
- The game is 2D, UI-heavy, and small in scope.
- First playable slice covers 7 in-game days.
- First slice content target: 6 scrap lot types, 12 part types, 8 customer order types, 6 crafting or repair recipes, 4 shop upgrades, 4 customer portraits, 1 main shop background, 5 main UI panels.
- Content should live in data files or Godot resources rather than being hard-coded into UI scripts.
- Business logic should be separated from UI where practical so core calculations can be tested without clicking through the whole game.
- Manual save slots, cloud saves, and profile management are excluded from the first slice.
- Out of scope for the first slice: online multiplayer, mobile release, controller-first UI, complex character animation, large shop layout editing, procedural story generation, 21-day campaign, Steam achievements, multiple save slots, localization beyond the first working language.
- Verification commands assume `godot` resolves to a Godot 4.x executable on PATH. If `where godot` fails, install Godot 4.x and reopen the terminal before running project tests.

---

## File Structure

Create this project layout under `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop`:

- `project.godot`: Godot project configuration and main scene pointer.
- `data/parts.json`: 12 part definitions.
- `data/scrap_lots.json`: 6 scrap lot definitions.
- `data/orders.json`: 8 customer order definitions.
- `data/recipes.json`: 6 recipe definitions.
- `data/upgrades.json`: 4 upgrade definitions.
- `scripts/data/data_loader.gd`: Loads and indexes JSON data.
- `scripts/state/game_state.gd`: Holds current run state.
- `scripts/services/economy_service.gd`: Prices, rewards, expenses, and upgrade modifiers.
- `scripts/services/salvage_service.gd`: Turns scrap lots into part rewards.
- `scripts/services/recipe_service.gd`: Checks and consumes recipe ingredients.
- `scripts/services/order_service.gd`: Checks and completes customer orders.
- `scripts/services/day_flow_service.gd`: Generates daily offers/orders and advances days.
- `scripts/services/save_service.gd`: Writes and reads one save slot.
- `scripts/ui/shop_scene.gd`: Main shop scene controller.
- `scripts/ui/hud.gd`: Money, day, and reputation display.
- `scripts/ui/market_panel.gd`: Scrap purchase panel.
- `scripts/ui/workbench_panel.gd`: Disassembly and recipe assembly panel.
- `scripts/ui/inventory_panel.gd`: Inventory and selling panel.
- `scripts/ui/orders_panel.gd`: Order completion panel.
- `scripts/ui/day_end_panel.gd`: Day settlement panel.
- `scenes/shop_scene.tscn`: Main scene.
- `scenes/ui/*.tscn`: Panel scenes.
- `assets/generated/*.svg`: Temporary vector art for the shop and icons.
- `tests/test_helpers.gd`: Test assertion helpers.
- `tests/test_runner.gd`: Lightweight Godot test runner.
- `tests/test_data_loader.gd`: Data loading tests.
- `tests/test_game_state.gd`: Game state tests.
- `tests/test_services.gd`: Economy, salvage, recipe, and order tests.
- `tests/test_day_flow.gd`: 7-day loop tests.
- `tests/test_save_service.gd`: Save/load tests.
- `docs/playtest-checklist.md`: Manual playtest checklist for the 7-day demo.

---

### Task 1: Bootstrap Godot Project And Test Harness

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/project.godot`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_helpers.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_runner.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_bootstrap.gd`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/.gitignore`

**Interfaces:**
- Consumes: no project code.
- Produces: `tests/test_runner.gd` can run every `res://tests/test_*.gd` suite, where each test method returns an `Array[String]` of failure messages.

- [ ] **Step 1: Confirm Godot is available**

Run:

```powershell
where godot
```

Expected: prints a path to a Godot 4.x executable. If it does not, install Godot 4.x and reopen the terminal.

- [ ] **Step 2: Create Godot project configuration**

Create `project.godot`:

```ini
; Engine configuration file.
; It is best edited using the editor UI and not directly,
; since the parameters that go here are not all obvious.

config_version=5

[application]

config/name="Orbital Salvage Shop"
run/main_scene="res://scenes/shop_scene.tscn"
config/features=PackedStringArray("4.0")
config/icon="res://assets/generated/icon.svg"

[display]

window/size/viewport_width=1280
window/size/viewport_height=720
window/stretch/mode="canvas_items"
window/stretch/aspect="expand"

[rendering]

renderer/rendering_method="forward_plus"
```

- [ ] **Step 3: Create the test helper**

Create `tests/test_helpers.gd`:

```gdscript
extends RefCounted
class_name TestHelpers

static func expect_equal(failures: Array[String], actual: Variant, expected: Variant, message: String) -> void:
	if actual != expected:
		failures.append("%s | expected=%s actual=%s" % [message, str(expected), str(actual)])

static func expect_true(failures: Array[String], condition: bool, message: String) -> void:
	if not condition:
		failures.append(message)

static func expect_false(failures: Array[String], condition: bool, message: String) -> void:
	if condition:
		failures.append(message)
```

- [ ] **Step 4: Create the test runner**

Create `tests/test_runner.gd`:

```gdscript
extends SceneTree

const TEST_PATHS: Array[String] = [
	"res://tests/test_bootstrap.gd",
	"res://tests/test_data_loader.gd",
	"res://tests/test_game_state.gd",
	"res://tests/test_services.gd",
	"res://tests/test_day_flow.gd",
	"res://tests/test_save_service.gd"
]

func _initialize() -> void:
	var failed_count := 0
	var passed_count := 0

	for path in TEST_PATHS:
		if not ResourceLoader.exists(path):
			continue

		var script := load(path)
		var suite = script.new()
		for method in suite.get_method_list():
			var method_name := String(method.name)
			if not method_name.begins_with("test_"):
				continue

			var failures: Array[String] = suite.call(method_name)
			if failures.is_empty():
				passed_count += 1
				print("PASS %s::%s" % [path, method_name])
			else:
				failed_count += 1
				push_error("FAIL %s::%s" % [path, method_name])
				for failure in failures:
					push_error("  - %s" % failure)

	print("Test result: %d passed, %d failed" % [passed_count, failed_count])
	quit(failed_count)
```

- [ ] **Step 5: Create a bootstrap test**

Create `tests/test_bootstrap.gd`:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")

func test_runner_executes_a_passing_test() -> Array[String]:
	var failures: Array[String] = []
	TestHelpers.expect_equal(failures, 2 + 2, 4, "basic arithmetic should work")
	return failures
```

- [ ] **Step 6: Add Godot ignores**

Modify `.gitignore` so it contains:

```gitignore
.agents/
.codex/
.superpowers/
outputs/
work/
.godot/
*.tmp
*.import
export_presets.cfg
```

- [ ] **Step 7: Run the bootstrap test**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: output includes `PASS res://tests/test_bootstrap.gd::test_runner_executes_a_passing_test` and exits with code 0.

- [ ] **Step 8: Commit**

Run:

```powershell
git add .gitignore project.godot tests/test_helpers.gd tests/test_runner.gd tests/test_bootstrap.gd
git commit -m "chore: bootstrap godot project"
```

---

### Task 2: Add Data Files For The 7-Day Slice

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/parts.json`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/scrap_lots.json`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/orders.json`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/recipes.json`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/upgrades.json`

**Interfaces:**
- Consumes: no project code.
- Produces: JSON content with ids used by all gameplay services.

- [ ] **Step 1: Create part definitions**

Create `data/parts.json`:

```json
[
  {"id":"wire_bundle","name":"Wire Bundle","tags":["circuit"],"rarity":"common","base_value":8},
  {"id":"cracked_panel","name":"Cracked Panel","tags":["hull"],"rarity":"common","base_value":7},
  {"id":"spark_cell","name":"Spark Cell","tags":["power"],"rarity":"common","base_value":10},
  {"id":"sensor_eye","name":"Sensor Eye","tags":["sensor"],"rarity":"common","base_value":9},
  {"id":"servo_joint","name":"Servo Joint","tags":["motion"],"rarity":"common","base_value":11},
  {"id":"coolant_tube","name":"Coolant Tube","tags":["thermal"],"rarity":"common","base_value":8},
  {"id":"stable_core","name":"Stable Core","tags":["power","rare"],"rarity":"uncommon","base_value":22},
  {"id":"nav_chip","name":"Nav Chip","tags":["circuit","sensor"],"rarity":"uncommon","base_value":20},
  {"id":"hull_plate","name":"Hull Plate","tags":["hull"],"rarity":"uncommon","base_value":18},
  {"id":"phase_coil","name":"Phase Coil","tags":["power","unstable"],"rarity":"uncommon","base_value":24},
  {"id":"quantum_lens","name":"Quantum Lens","tags":["sensor","rare"],"rarity":"rare","base_value":36},
  {"id":"mini_thruster","name":"Mini Thruster","tags":["motion","power","rare"],"rarity":"rare","base_value":40}
]
```

- [ ] **Step 2: Create scrap lot definitions**

Create `data/scrap_lots.json`:

```json
[
  {"id":"dockside_crate","name":"Dockside Crate","base_price":28,"quality_hint":"Reliable common parts","risk":1,"outputs":[{"part_id":"wire_bundle","min":1,"max":2},{"part_id":"cracked_panel","min":1,"max":2},{"part_id":"spark_cell","min":0,"max":1}]},
  {"id":"drone_husk","name":"Drone Husk","base_price":36,"quality_hint":"Motion and sensor parts","risk":2,"outputs":[{"part_id":"servo_joint","min":1,"max":2},{"part_id":"sensor_eye","min":1,"max":2},{"part_id":"wire_bundle","min":0,"max":1}]},
  {"id":"reactor_scrap","name":"Reactor Scrap","base_price":48,"quality_hint":"Power parts with risk","risk":3,"outputs":[{"part_id":"spark_cell","min":1,"max":2},{"part_id":"coolant_tube","min":1,"max":2},{"part_id":"phase_coil","min":0,"max":1}]},
  {"id":"nav_console","name":"Nav Console","base_price":52,"quality_hint":"Circuit and sensor parts","risk":2,"outputs":[{"part_id":"nav_chip","min":0,"max":1},{"part_id":"sensor_eye","min":1,"max":2},{"part_id":"wire_bundle","min":1,"max":2}]},
  {"id":"patched_lifeboat","name":"Patched Lifeboat","base_price":58,"quality_hint":"Hull parts and a chance at propulsion","risk":2,"outputs":[{"part_id":"hull_plate","min":1,"max":2},{"part_id":"coolant_tube","min":0,"max":1},{"part_id":"mini_thruster","min":0,"max":1}]},
  {"id":"sealed_black_box","name":"Sealed Black Box","base_price":72,"quality_hint":"Uncertain rare electronics","risk":4,"outputs":[{"part_id":"quantum_lens","min":0,"max":1},{"part_id":"nav_chip","min":0,"max":1},{"part_id":"stable_core","min":0,"max":1},{"part_id":"wire_bundle","min":1,"max":2}]}
]
```

- [ ] **Step 3: Create order definitions**

Create `data/orders.json`:

```json
[
  {"id":"patch_delivery_drone","customer_id":"courier","title":"Patch a Delivery Drone","requirements":[{"tag":"motion","count":1},{"tag":"circuit","count":1}],"reward_money":52,"reward_reputation":1,"expires_after_days":1},
  {"id":"backup_lamp_grid","customer_id":"vendor","title":"Restore a Lamp Grid","requirements":[{"tag":"power","count":1},{"part_id":"wire_bundle","count":1}],"reward_money":44,"reward_reputation":1,"expires_after_days":1},
  {"id":"seal_window_panel","customer_id":"dockhand","title":"Seal a Window Panel","requirements":[{"tag":"hull","count":2}],"reward_money":48,"reward_reputation":1,"expires_after_days":1},
  {"id":"calibrate_scanner","customer_id":"pilot","title":"Calibrate a Scanner","requirements":[{"tag":"sensor","count":2}],"reward_money":62,"reward_reputation":2,"expires_after_days":1},
  {"id":"build_stabilizer","customer_id":"mechanic","title":"Build a Power Stabilizer","requirements":[{"part_id":"stable_core","count":1},{"tag":"thermal","count":1}],"reward_money":88,"reward_reputation":2,"expires_after_days":2},
  {"id":"repair_shuttle_fins","customer_id":"pilot","title":"Repair Shuttle Fins","requirements":[{"part_id":"hull_plate","count":1},{"tag":"motion","count":1}],"reward_money":92,"reward_reputation":2,"expires_after_days":2},
  {"id":"tune_nav_beacon","customer_id":"cartographer","title":"Tune a Nav Beacon","requirements":[{"part_id":"nav_chip","count":1},{"tag":"power","count":1}],"reward_money":96,"reward_reputation":2,"expires_after_days":2},
  {"id":"assemble_micro_tug","customer_id":"dockhand","title":"Assemble a Micro Tug","requirements":[{"part_id":"mini_thruster","count":1},{"tag":"sensor","count":1},{"tag":"hull","count":1}],"reward_money":140,"reward_reputation":3,"expires_after_days":2}
]
```

- [ ] **Step 4: Create recipe definitions**

Create `data/recipes.json`:

```json
[
  {"id":"patched_panel","name":"Patched Panel","requirements":[{"tag":"hull","count":1},{"part_id":"wire_bundle","count":1}],"output_value":34},
  {"id":"lamp_grid","name":"Lamp Grid","requirements":[{"tag":"power","count":1},{"tag":"circuit","count":1}],"output_value":38},
  {"id":"sensor_cluster","name":"Sensor Cluster","requirements":[{"tag":"sensor","count":2}],"output_value":44},
  {"id":"stabilizer_pack","name":"Stabilizer Pack","requirements":[{"tag":"power","count":1},{"tag":"thermal","count":1}],"output_value":58},
  {"id":"drone_frame","name":"Drone Frame","requirements":[{"tag":"motion","count":1},{"tag":"hull","count":1},{"tag":"circuit","count":1}],"output_value":72},
  {"id":"micro_tug_core","name":"Micro Tug Core","requirements":[{"part_id":"mini_thruster","count":1},{"tag":"sensor","count":1},{"tag":"power","count":1}],"output_value":110}
]
```

- [ ] **Step 5: Create upgrade definitions**

Create `data/upgrades.json`:

```json
[
  {"id":"storage_shelf","name":"Storage Shelf","cost":90,"effect":{"storage_bonus":8},"unlock_day":1,"unlock_reputation":0},
  {"id":"scanner_array","name":"Scanner Array","cost":120,"effect":{"scanner_bonus":1},"unlock_day":2,"unlock_reputation":2},
  {"id":"precision_disassembler","name":"Precision Disassembler","cost":150,"effect":{"salvage_bonus":1},"unlock_day":3,"unlock_reputation":4},
  {"id":"assembly_bench","name":"Assembly Bench","cost":180,"effect":{"order_bonus":1},"unlock_day":4,"unlock_reputation":6}
]
```

- [ ] **Step 6: Confirm files are valid JSON**

Run:

```powershell
node -e "for (const f of ['data/parts.json','data/scrap_lots.json','data/orders.json','data/recipes.json','data/upgrades.json']) { JSON.parse(require('fs').readFileSync(f,'utf8')); console.log('valid', f); }"
```

Expected: prints `valid` for all five files.

- [ ] **Step 7: Commit**

Run:

```powershell
git add data/parts.json data/scrap_lots.json data/orders.json data/recipes.json data/upgrades.json
git commit -m "feat: add vertical slice data"
```

---

### Task 3: Load And Validate Data

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_data_loader.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/data/data_loader.gd`

**Interfaces:**
- Consumes: JSON files from Task 2.
- Produces: `DataLoader.load_all() -> bool`, dictionaries `parts`, `scrap_lots`, `orders`, `recipes`, `upgrades`.

- [ ] **Step 1: Write failing data loader tests**

Create `tests/test_data_loader.gd`:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")
const DataLoader = preload("res://scripts/data/data_loader.gd")

func test_loads_expected_slice_counts() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	TestHelpers.expect_true(failures, loader.load_all(), "data loader should load all data files")
	TestHelpers.expect_equal(failures, loader.parts.size(), 12, "part count")
	TestHelpers.expect_equal(failures, loader.scrap_lots.size(), 6, "scrap lot count")
	TestHelpers.expect_equal(failures, loader.orders.size(), 8, "order count")
	TestHelpers.expect_equal(failures, loader.recipes.size(), 6, "recipe count")
	TestHelpers.expect_equal(failures, loader.upgrades.size(), 4, "upgrade count")
	return failures

func test_indexes_records_by_id() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	TestHelpers.expect_true(failures, loader.parts.has("wire_bundle"), "wire bundle should be indexed")
	TestHelpers.expect_equal(failures, loader.parts["wire_bundle"]["base_value"], 8, "wire bundle value")
	TestHelpers.expect_true(failures, loader.scrap_lots.has("dockside_crate"), "dockside crate should be indexed")
	TestHelpers.expect_true(failures, loader.orders.has("assemble_micro_tug"), "micro tug order should be indexed")
	return failures
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because `res://scripts/data/data_loader.gd` does not exist.

- [ ] **Step 3: Implement DataLoader**

Create `scripts/data/data_loader.gd`:

```gdscript
extends RefCounted
class_name DataLoader

var parts: Dictionary = {}
var scrap_lots: Dictionary = {}
var orders: Dictionary = {}
var recipes: Dictionary = {}
var upgrades: Dictionary = {}

func load_all() -> bool:
	parts = _load_indexed_array("res://data/parts.json")
	scrap_lots = _load_indexed_array("res://data/scrap_lots.json")
	orders = _load_indexed_array("res://data/orders.json")
	recipes = _load_indexed_array("res://data/recipes.json")
	upgrades = _load_indexed_array("res://data/upgrades.json")
	return not parts.is_empty() and not scrap_lots.is_empty() and not orders.is_empty() and not recipes.is_empty() and not upgrades.is_empty()

func _load_indexed_array(path: String) -> Dictionary:
	var records := _load_array(path)
	var indexed: Dictionary = {}
	for record in records:
		if typeof(record) != TYPE_DICTIONARY:
			push_error("Record in %s is not an object" % path)
			continue
		if not record.has("id"):
			push_error("Record in %s is missing id" % path)
			continue
		indexed[String(record["id"])] = record
	return indexed

func _load_array(path: String) -> Array:
	if not FileAccess.file_exists(path):
		push_error("Data file does not exist: %s" % path)
		return []
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Could not open data file: %s" % path)
		return []
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_ARRAY:
		push_error("Data file must contain an array: %s" % path)
		return []
	return parsed
```

- [ ] **Step 4: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: data loader tests pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add scripts/data/data_loader.gd tests/test_data_loader.gd
git commit -m "feat: load game data"
```

---

### Task 4: Implement GameState Inventory And Money

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_game_state.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/state/game_state.gd`

**Interfaces:**
- Consumes: no service scripts.
- Produces: `GameState.new_run()`, `add_part(part_id, count)`, `remove_part(part_id, count)`, `get_part_count(part_id)`, `spend(amount)`, `earn(amount)`, `add_reputation(amount)`, `to_save_dict()`, `load_save_dict(data)`.

- [ ] **Step 1: Write failing GameState tests**

Create `tests/test_game_state.gd`:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")
const GameState = preload("res://scripts/state/game_state.gd")

func test_new_run_defaults() -> Array[String]:
	var failures: Array[String] = []
	var state := GameState.new_run()
	TestHelpers.expect_equal(failures, state.day, 1, "new run day")
	TestHelpers.expect_equal(failures, state.money, 120, "new run money")
	TestHelpers.expect_equal(failures, state.reputation, 0, "new run reputation")
	TestHelpers.expect_equal(failures, state.inventory.size(), 0, "new run inventory")
	return failures

func test_inventory_add_and_remove() -> Array[String]:
	var failures: Array[String] = []
	var state := GameState.new_run()
	state.add_part("wire_bundle", 3)
	TestHelpers.expect_equal(failures, state.get_part_count("wire_bundle"), 3, "added count")
	TestHelpers.expect_true(failures, state.remove_part("wire_bundle", 2), "remove should succeed")
	TestHelpers.expect_equal(failures, state.get_part_count("wire_bundle"), 1, "remaining count")
	TestHelpers.expect_false(failures, state.remove_part("wire_bundle", 2), "remove should fail when count is short")
	return failures

func test_save_round_trip() -> Array[String]:
	var failures: Array[String] = []
	var state := GameState.new_run()
	state.day = 3
	state.money = 75
	state.reputation = 4
	state.add_part("spark_cell", 2)
	state.upgrades["scanner_array"] = true
	var restored := GameState.new()
	restored.load_save_dict(state.to_save_dict())
	TestHelpers.expect_equal(failures, restored.day, 3, "restored day")
	TestHelpers.expect_equal(failures, restored.money, 75, "restored money")
	TestHelpers.expect_equal(failures, restored.reputation, 4, "restored reputation")
	TestHelpers.expect_equal(failures, restored.get_part_count("spark_cell"), 2, "restored inventory")
	TestHelpers.expect_true(failures, restored.upgrades.has("scanner_array"), "restored upgrade")
	return failures
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because `res://scripts/state/game_state.gd` does not exist.

- [ ] **Step 3: Implement GameState**

Create `scripts/state/game_state.gd`:

```gdscript
extends RefCounted
class_name GameState

var day: int = 1
var money: int = 120
var reputation: int = 0
var inventory: Dictionary = {}
var upgrades: Dictionary = {}
var current_scrap_offers: Array[String] = []
var current_order_ids: Array[String] = []
var completed_order_ids: Array[String] = []

static func new_run() -> GameState:
	var state := GameState.new()
	state.day = 1
	state.money = 120
	state.reputation = 0
	state.inventory = {}
	state.upgrades = {}
	state.current_scrap_offers = []
	state.current_order_ids = []
	state.completed_order_ids = []
	return state

func can_afford(amount: int) -> bool:
	return money >= amount

func spend(amount: int) -> bool:
	if amount < 0:
		return false
	if not can_afford(amount):
		return false
	money -= amount
	return true

func earn(amount: int) -> void:
	if amount > 0:
		money += amount

func add_reputation(amount: int) -> void:
	if amount > 0:
		reputation += amount

func add_part(part_id: String, count: int = 1) -> void:
	if count <= 0:
		return
	inventory[part_id] = get_part_count(part_id) + count

func remove_part(part_id: String, count: int = 1) -> bool:
	if count <= 0:
		return false
	var current := get_part_count(part_id)
	if current < count:
		return false
	var next_count := current - count
	if next_count == 0:
		inventory.erase(part_id)
	else:
		inventory[part_id] = next_count
	return true

func get_part_count(part_id: String) -> int:
	return int(inventory.get(part_id, 0))

func to_save_dict() -> Dictionary:
	return {
		"day": day,
		"money": money,
		"reputation": reputation,
		"inventory": inventory.duplicate(true),
		"upgrades": upgrades.duplicate(true),
		"current_scrap_offers": current_scrap_offers.duplicate(),
		"current_order_ids": current_order_ids.duplicate(),
		"completed_order_ids": completed_order_ids.duplicate()
	}

func load_save_dict(data: Dictionary) -> void:
	day = int(data.get("day", 1))
	money = int(data.get("money", 120))
	reputation = int(data.get("reputation", 0))
	inventory = Dictionary(data.get("inventory", {})).duplicate(true)
	upgrades = Dictionary(data.get("upgrades", {})).duplicate(true)
	current_scrap_offers = Array(data.get("current_scrap_offers", []))
	current_order_ids = Array(data.get("current_order_ids", []))
	completed_order_ids = Array(data.get("completed_order_ids", []))
```

- [ ] **Step 4: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: GameState tests pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add scripts/state/game_state.gd tests/test_game_state.gd
git commit -m "feat: add game state"
```

---

### Task 5: Implement Economy And Salvage Services

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_services.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/economy_service.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/salvage_service.gd`

**Interfaces:**
- Consumes: `GameState`, `DataLoader` dictionaries.
- Produces: `EconomyService.daily_expense(day)`, `EconomyService.scrap_price(scrap_def, state)`, `EconomyService.sell_part_value(part_def)`, `SalvageService.disassemble(scrap_def, rng, salvage_bonus)`.

- [ ] **Step 1: Write failing economy and salvage tests**

Create `tests/test_services.gd` with these initial tests:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")
const DataLoader = preload("res://scripts/data/data_loader.gd")
const GameState = preload("res://scripts/state/game_state.gd")
const EconomyService = preload("res://scripts/services/economy_service.gd")
const SalvageService = preload("res://scripts/services/salvage_service.gd")

func test_daily_expense_scales_lightly() -> Array[String]:
	var failures: Array[String] = []
	TestHelpers.expect_equal(failures, EconomyService.daily_expense(1), 18, "day 1 expense")
	TestHelpers.expect_equal(failures, EconomyService.daily_expense(7), 30, "day 7 expense")
	return failures

func test_scrap_price_uses_base_price() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	var price := EconomyService.scrap_price(loader.scrap_lots["dockside_crate"], state)
	TestHelpers.expect_equal(failures, price, 28, "dockside crate day 1 price")
	return failures

func test_disassemble_produces_valid_part_counts() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var rng := RandomNumberGenerator.new()
	rng.seed = 1
	var result := SalvageService.disassemble(loader.scrap_lots["dockside_crate"], rng, 0)
	TestHelpers.expect_true(failures, result.has("wire_bundle"), "dockside crate should produce wire")
	TestHelpers.expect_true(failures, result.has("cracked_panel"), "dockside crate should produce panel")
	TestHelpers.expect_true(failures, int(result["wire_bundle"]) >= 1, "wire count minimum")
	return failures
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because economy and salvage service scripts do not exist.

- [ ] **Step 3: Implement EconomyService**

Create `scripts/services/economy_service.gd`:

```gdscript
extends RefCounted
class_name EconomyService

static func daily_expense(day: int) -> int:
	return 18 + max(day - 1, 0) * 2

static func scrap_price(scrap_def: Dictionary, state: GameState) -> int:
	var base_price := int(scrap_def.get("base_price", 0))
	var scanner_discount := 0
	if state.upgrades.has("scanner_array"):
		scanner_discount = 2
	return max(base_price - scanner_discount, 1)

static func sell_part_value(part_def: Dictionary) -> int:
	return int(part_def.get("base_value", 0))

static func order_reward_money(order_def: Dictionary, state: GameState) -> int:
	var reward := int(order_def.get("reward_money", 0))
	if state.upgrades.has("assembly_bench"):
		reward += 5
	return reward
```

- [ ] **Step 4: Implement SalvageService**

Create `scripts/services/salvage_service.gd`:

```gdscript
extends RefCounted
class_name SalvageService

static func disassemble(scrap_def: Dictionary, rng: RandomNumberGenerator, salvage_bonus: int = 0) -> Dictionary:
	var result: Dictionary = {}
	for output in Array(scrap_def.get("outputs", [])):
		var part_id := String(output.get("part_id", ""))
		var min_count := int(output.get("min", 0))
		var max_count := int(output.get("max", min_count))
		var count := rng.randi_range(min_count, max_count)
		if count <= 0 or part_id.is_empty():
			continue
		result[part_id] = int(result.get(part_id, 0)) + count

	if salvage_bonus > 0 and not result.is_empty():
		var keys := result.keys()
		keys.sort()
		result[String(keys[0])] = int(result[keys[0]]) + salvage_bonus

	return result
```

- [ ] **Step 5: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: service tests pass.

- [ ] **Step 6: Commit**

Run:

```powershell
git add scripts/services/economy_service.gd scripts/services/salvage_service.gd tests/test_services.gd
git commit -m "feat: add economy and salvage services"
```

---

### Task 6: Implement Recipe And Order Completion

**Files:**
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_services.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/recipe_service.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/order_service.gd`

**Interfaces:**
- Consumes: `GameState`, `DataLoader.parts`, recipe and order dictionaries.
- Produces: `RecipeService.can_satisfy(requirements, inventory, parts_by_id)`, `RecipeService.consume_requirements(requirements, state, parts_by_id)`, `OrderService.can_complete(order_def, state, parts_by_id)`, `OrderService.complete_order(order_def, state, parts_by_id)`.

- [ ] **Step 1: Extend service tests for requirements**

Modify the preload block at the top of `tests/test_services.gd` so it includes these two constants:

```gdscript
const RecipeService = preload("res://scripts/services/recipe_service.gd")
const OrderService = preload("res://scripts/services/order_service.gd")
```

Append these test functions below the existing test functions:

```gdscript
func test_recipe_service_matches_part_id_and_tag_requirements() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	state.add_part("wire_bundle", 1)
	state.add_part("spark_cell", 1)
	var requirements: Array = [{"part_id":"wire_bundle","count":1},{"tag":"power","count":1}]
	TestHelpers.expect_true(failures, RecipeService.can_satisfy(requirements, state.inventory, loader.parts), "requirements should be satisfiable")
	TestHelpers.expect_true(failures, RecipeService.consume_requirements(requirements, state, loader.parts), "requirements should consume")
	TestHelpers.expect_equal(failures, state.get_part_count("wire_bundle"), 0, "wire consumed")
	TestHelpers.expect_equal(failures, state.get_part_count("spark_cell"), 0, "power tag consumed")
	return failures

func test_order_completion_rewards_and_consumes() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	state.add_part("wire_bundle", 1)
	state.add_part("servo_joint", 1)
	var order := loader.orders["patch_delivery_drone"]
	TestHelpers.expect_true(failures, OrderService.can_complete(order, state, loader.parts), "order should be completable")
	TestHelpers.expect_true(failures, OrderService.complete_order(order, state, loader.parts), "order completion should succeed")
	TestHelpers.expect_equal(failures, state.money, 172, "money reward applied")
	TestHelpers.expect_equal(failures, state.reputation, 1, "reputation reward applied")
	TestHelpers.expect_true(failures, state.completed_order_ids.has("patch_delivery_drone"), "completed order tracked")
	return failures
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because recipe and order service scripts do not exist.

- [ ] **Step 3: Implement RecipeService**

Create `scripts/services/recipe_service.gd`:

```gdscript
extends RefCounted
class_name RecipeService

static func can_satisfy(requirements: Array, inventory: Dictionary, parts_by_id: Dictionary) -> bool:
	var planned_consumption := _plan_consumption(requirements, inventory, parts_by_id)
	return bool(planned_consumption.get("ok", false))

static func consume_requirements(requirements: Array, state: GameState, parts_by_id: Dictionary) -> bool:
	var planned_consumption := _plan_consumption(requirements, state.inventory, parts_by_id)
	if not bool(planned_consumption.get("ok", false)):
		return false
	for part_id in Dictionary(planned_consumption.get("consume", {})).keys():
		if not state.remove_part(String(part_id), int(planned_consumption["consume"][part_id])):
			return false
	return true

static func _plan_consumption(requirements: Array, inventory: Dictionary, parts_by_id: Dictionary) -> Dictionary:
	var simulated := inventory.duplicate(true)
	var consume: Dictionary = {}

	for requirement in requirements:
		var count := int(requirement.get("count", 1))
		if requirement.has("part_id"):
			var part_id := String(requirement["part_id"])
			if int(simulated.get(part_id, 0)) < count:
				return {"ok": false, "consume": {}}
			simulated[part_id] = int(simulated[part_id]) - count
			consume[part_id] = int(consume.get(part_id, 0)) + count
		elif requirement.has("tag"):
			var tag := String(requirement["tag"])
			var selected := _select_parts_for_tag(simulated, parts_by_id, tag, count)
			if selected.size() < count:
				return {"ok": false, "consume": {}}
			for selected_part_id in selected:
				simulated[selected_part_id] = int(simulated[selected_part_id]) - 1
				consume[selected_part_id] = int(consume.get(selected_part_id, 0)) + 1

	return {"ok": true, "consume": consume}

static func _select_parts_for_tag(inventory: Dictionary, parts_by_id: Dictionary, tag: String, count: int) -> Array[String]:
	var candidates: Array = []
	for part_id in inventory.keys():
		var available := int(inventory[part_id])
		if available <= 0 or not parts_by_id.has(part_id):
			continue
		var part_def: Dictionary = parts_by_id[part_id]
		if not Array(part_def.get("tags", [])).has(tag):
			continue
		candidates.append({"id": String(part_id), "value": int(part_def.get("base_value", 0))})

	candidates.sort_custom(func(a, b): return a["value"] < b["value"] or (a["value"] == b["value"] and a["id"] < b["id"]))

	var selected: Array[String] = []
	for candidate in candidates:
		var available_count := int(inventory[candidate["id"]])
		for i in range(available_count):
			if selected.size() >= count:
				return selected
			selected.append(String(candidate["id"]))
	return selected
```

- [ ] **Step 4: Implement OrderService**

Create `scripts/services/order_service.gd`:

```gdscript
extends RefCounted
class_name OrderService

const RecipeService = preload("res://scripts/services/recipe_service.gd")
const EconomyService = preload("res://scripts/services/economy_service.gd")

static func can_complete(order_def: Dictionary, state: GameState, parts_by_id: Dictionary) -> bool:
	if state.completed_order_ids.has(String(order_def.get("id", ""))):
		return false
	return RecipeService.can_satisfy(Array(order_def.get("requirements", [])), state.inventory, parts_by_id)

static func complete_order(order_def: Dictionary, state: GameState, parts_by_id: Dictionary) -> bool:
	if not can_complete(order_def, state, parts_by_id):
		return false
	var requirements := Array(order_def.get("requirements", []))
	if not RecipeService.consume_requirements(requirements, state, parts_by_id):
		return false
	state.earn(EconomyService.order_reward_money(order_def, state))
	state.add_reputation(int(order_def.get("reward_reputation", 0)))
	state.completed_order_ids.append(String(order_def.get("id", "")))
	return true
```

- [ ] **Step 5: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: all service tests pass.

- [ ] **Step 6: Commit**

Run:

```powershell
git add scripts/services/recipe_service.gd scripts/services/order_service.gd tests/test_services.gd
git commit -m "feat: add recipe and order services"
```

---

### Task 7: Implement Day Flow And 7-Day Simulation

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_day_flow.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/day_flow_service.gd`

**Interfaces:**
- Consumes: `GameState`, `DataLoader`, `EconomyService`.
- Produces: `DayFlowService.start_day(state, loader, seed)`, `DayFlowService.end_day(state)`, `DayFlowService.purchase_scrap(state, scrap_def)`.

- [ ] **Step 1: Write failing day-flow tests**

Create `tests/test_day_flow.gd`:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")
const DataLoader = preload("res://scripts/data/data_loader.gd")
const GameState = preload("res://scripts/state/game_state.gd")
const DayFlowService = preload("res://scripts/services/day_flow_service.gd")

func test_start_day_generates_three_scrap_offers_and_three_orders() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	DayFlowService.start_day(state, loader, 100)
	TestHelpers.expect_equal(failures, state.current_scrap_offers.size(), 3, "scrap offer count")
	TestHelpers.expect_equal(failures, state.current_order_ids.size(), 3, "order count")
	return failures

func test_purchase_scrap_spends_money_and_records_owned_scrap() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	DayFlowService.start_day(state, loader, 100)
	var scrap_id := state.current_scrap_offers[0]
	var price := int(loader.scrap_lots[scrap_id]["base_price"])
	TestHelpers.expect_true(failures, DayFlowService.purchase_scrap(state, loader.scrap_lots[scrap_id]), "purchase should succeed")
	TestHelpers.expect_equal(failures, state.money, 120 - price, "money after purchase")
	TestHelpers.expect_true(failures, state.owned_scrap_lots.has(scrap_id), "owned scrap tracked")
	return failures

func test_can_advance_seven_days_without_negative_day() -> Array[String]:
	var failures: Array[String] = []
	var loader := DataLoader.new()
	loader.load_all()
	var state := GameState.new_run()
	for i in range(7):
		DayFlowService.start_day(state, loader, 200 + i)
		DayFlowService.end_day(state)
	TestHelpers.expect_equal(failures, state.day, 8, "day after seven settlements")
	TestHelpers.expect_true(failures, state.money >= 0, "money should not go negative in empty-loop simulation")
	return failures
```

- [ ] **Step 2: Add owned scrap lots to GameState**

Modify `scripts/state/game_state.gd`:

```gdscript
var owned_scrap_lots: Array[String] = []
```

In `new_run()` add:

```gdscript
state.owned_scrap_lots = []
```

In `to_save_dict()` add:

```gdscript
"owned_scrap_lots": owned_scrap_lots.duplicate(),
```

In `load_save_dict(data)` add:

```gdscript
owned_scrap_lots = Array(data.get("owned_scrap_lots", []))
```

- [ ] **Step 3: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because `DayFlowService` does not exist.

- [ ] **Step 4: Implement DayFlowService**

Create `scripts/services/day_flow_service.gd`:

```gdscript
extends RefCounted
class_name DayFlowService

const EconomyService = preload("res://scripts/services/economy_service.gd")

static func start_day(state: GameState, loader: DataLoader, seed: int) -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = seed + state.day * 97
	state.current_scrap_offers = _pick_ids(loader.scrap_lots.keys(), 3, rng)
	state.current_order_ids = _pick_ids(loader.orders.keys(), 3, rng)

static func purchase_scrap(state: GameState, scrap_def: Dictionary) -> bool:
	var price := EconomyService.scrap_price(scrap_def, state)
	if not state.spend(price):
		return false
	state.owned_scrap_lots.append(String(scrap_def.get("id", "")))
	return true

static func end_day(state: GameState) -> Dictionary:
	var expense := EconomyService.daily_expense(state.day)
	state.money = max(state.money - expense, 0)
	state.day += 1
	state.current_scrap_offers = []
	state.current_order_ids = []
	state.owned_scrap_lots = []
	return {"expense": expense, "next_day": state.day, "money": state.money}

static func _pick_ids(ids: Array, count: int, rng: RandomNumberGenerator) -> Array[String]:
	var pool: Array[String] = []
	for id in ids:
		pool.append(String(id))
	pool.sort()

	var picked: Array[String] = []
	while picked.size() < count and not pool.is_empty():
		var index := rng.randi_range(0, pool.size() - 1)
		picked.append(pool[index])
		pool.remove_at(index)
	return picked
```

- [ ] **Step 5: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: day-flow tests pass.

- [ ] **Step 6: Commit**

Run:

```powershell
git add scripts/state/game_state.gd scripts/services/day_flow_service.gd tests/test_day_flow.gd
git commit -m "feat: add day flow"
```

---

### Task 8: Implement Save And Load

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/tests/test_save_service.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/services/save_service.gd`

**Interfaces:**
- Consumes: `GameState.to_save_dict()` and `GameState.load_save_dict(data)`.
- Produces: `SaveService.save_state(state, path) -> bool`, `SaveService.load_state(path) -> GameState`.

- [ ] **Step 1: Write failing save service tests**

Create `tests/test_save_service.gd`:

```gdscript
extends RefCounted

const TestHelpers = preload("res://tests/test_helpers.gd")
const GameState = preload("res://scripts/state/game_state.gd")
const SaveService = preload("res://scripts/services/save_service.gd")

func test_save_and_load_state() -> Array[String]:
	var failures: Array[String] = []
	var path := "user://test_orbital_save.json"
	var state := GameState.new_run()
	state.day = 5
	state.money = 211
	state.reputation = 9
	state.add_part("nav_chip", 2)
	TestHelpers.expect_true(failures, SaveService.save_state(state, path), "save should succeed")
	var loaded := SaveService.load_state(path)
	TestHelpers.expect_equal(failures, loaded.day, 5, "loaded day")
	TestHelpers.expect_equal(failures, loaded.money, 211, "loaded money")
	TestHelpers.expect_equal(failures, loaded.reputation, 9, "loaded reputation")
	TestHelpers.expect_equal(failures, loaded.get_part_count("nav_chip"), 2, "loaded inventory")
	return failures
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: fails because `SaveService` does not exist.

- [ ] **Step 3: Implement SaveService**

Create `scripts/services/save_service.gd`:

```gdscript
extends RefCounted
class_name SaveService

const GameState = preload("res://scripts/state/game_state.gd")

static func save_state(state: GameState, path: String = "user://save_slot_1.json") -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error("Could not open save file for writing: %s" % path)
		return false
	file.store_string(JSON.stringify(state.to_save_dict(), "\t"))
	return true

static func load_state(path: String = "user://save_slot_1.json") -> GameState:
	if not FileAccess.file_exists(path):
		return GameState.new_run()
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Could not open save file for reading: %s" % path)
		return GameState.new_run()
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		push_error("Save file did not contain an object: %s" % path)
		return GameState.new_run()
	var state := GameState.new()
	state.load_save_dict(parsed)
	return state
```

- [ ] **Step 4: Run tests and confirm pass**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: save tests pass.

- [ ] **Step 5: Commit**

Run:

```powershell
git add scripts/services/save_service.gd tests/test_save_service.gd
git commit -m "feat: add save service"
```

---

### Task 9: Create Main Shop Scene And HUD

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/shop_scene.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/hud.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/shop_scene.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/hud.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/assets/generated/icon.svg`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/assets/generated/shop_background.svg`

**Interfaces:**
- Consumes: `DataLoader`, `GameState`, `DayFlowService`.
- Produces: a runnable main scene with visible day, money, reputation, and panel buttons.

- [ ] **Step 1: Create temporary SVG icon**

Create `assets/generated/icon.svg`:

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="0 0 128 128">
  <rect width="128" height="128" rx="18" fill="#18242e"/>
  <circle cx="64" cy="64" r="42" fill="#f2b84b"/>
  <path d="M34 72h60v14H34zM44 42h40v18H44z" fill="#263844"/>
  <path d="M48 66l16-20 16 20z" fill="#75d7c6"/>
</svg>
```

- [ ] **Step 2: Create temporary shop background SVG**

Create `assets/generated/shop_background.svg`:

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="720" viewBox="0 0 1280 720">
  <rect width="1280" height="720" fill="#111922"/>
  <rect y="500" width="1280" height="220" fill="#1f2b33"/>
  <rect x="0" y="0" width="1280" height="500" fill="#17232d"/>
  <circle cx="980" cy="170" r="86" fill="#0c141b" stroke="#6b8794" stroke-width="10"/>
  <circle cx="1008" cy="140" r="4" fill="#d9f5ff"/>
  <circle cx="944" cy="188" r="3" fill="#d9f5ff"/>
  <circle cx="1018" cy="204" r="2" fill="#d9f5ff"/>
  <rect x="96" y="168" width="380" height="250" rx="12" fill="#243541"/>
  <rect x="126" y="198" width="320" height="26" fill="#f2b84b"/>
  <rect x="130" y="258" width="78" height="52" fill="#5c7482"/>
  <rect x="230" y="250" width="76" height="68" fill="#6a4f3f"/>
  <rect x="332" y="244" width="72" height="72" fill="#425765"/>
  <rect x="540" y="390" width="320" height="92" rx="10" fill="#2a3b45"/>
  <rect x="578" y="340" width="240" height="56" rx="12" fill="#415762"/>
  <text x="112" y="148" font-family="Arial" font-size="34" fill="#75d7c6">ORBITAL SALVAGE</text>
</svg>
```

- [ ] **Step 3: Create HUD script**

Create `scripts/ui/hud.gd`:

```gdscript
extends Control
class_name Hud

@onready var day_label: Label = %DayLabel
@onready var money_label: Label = %MoneyLabel
@onready var reputation_label: Label = %ReputationLabel

func update_state(state: GameState) -> void:
	day_label.text = "Day %d" % state.day
	money_label.text = "%d credits" % state.money
	reputation_label.text = "Rep %d" % state.reputation
```

- [ ] **Step 4: Create HUD scene in Godot editor**

Create `scenes/ui/hud.tscn` with this node tree:

```text
Hud (Control, script=res://scripts/ui/hud.gd)
  PanelContainer
    HBoxContainer
      DayLabel (Label, unique_name_in_owner=true)
      MoneyLabel (Label, unique_name_in_owner=true)
      ReputationLabel (Label, unique_name_in_owner=true)
```

Set initial label texts to `Day 1`, `120 credits`, and `Rep 0`.

- [ ] **Step 5: Create ShopScene script**

Create `scripts/ui/shop_scene.gd`:

```gdscript
extends Control
class_name ShopScene

const DataLoader = preload("res://scripts/data/data_loader.gd")
const GameState = preload("res://scripts/state/game_state.gd")
const DayFlowService = preload("res://scripts/services/day_flow_service.gd")

@onready var hud: Hud = %Hud
@onready var status_label: Label = %StatusLabel

var loader := DataLoader.new()
var state := GameState.new_run()

func _ready() -> void:
	if not loader.load_all():
		status_label.text = "Data failed to load."
		return
	DayFlowService.start_day(state, loader, 7001)
	_refresh()

func _refresh() -> void:
	hud.update_state(state)
	status_label.text = "Choose a panel to run the shop day."
```

- [ ] **Step 6: Create main shop scene in Godot editor**

Create `scenes/shop_scene.tscn` with this node tree:

```text
ShopScene (Control, script=res://scripts/ui/shop_scene.gd)
  Background (TextureRect, texture=res://assets/generated/shop_background.svg, stretch_mode=keep_aspect_covered)
  Hud (instance=res://scenes/ui/hud.tscn, unique_name_in_owner=true)
  CenterContainer
    VBoxContainer
      TitleLabel (Label, text="Orbital Salvage Shop")
      StatusLabel (Label, unique_name_in_owner=true)
      HBoxContainer
        MarketButton (Button, text="Market")
        WorkbenchButton (Button, text="Workbench")
        InventoryButton (Button, text="Inventory")
        OrdersButton (Button, text="Orders")
        EndDayButton (Button, text="End Day")
```

- [ ] **Step 7: Run the main scene headlessly**

Run:

```powershell
godot --headless --path . --quit-after 1
```

Expected: exits with code 0 without data-load errors.

- [ ] **Step 8: Commit**

Run:

```powershell
git add assets/generated/icon.svg assets/generated/shop_background.svg scenes/shop_scene.tscn scenes/ui/hud.tscn scripts/ui/shop_scene.gd scripts/ui/hud.gd
git commit -m "feat: add shop scene shell"
```

---

### Task 10: Add Market, Workbench, Inventory, Orders, And Day-End Panels

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/market_panel.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/workbench_panel.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/inventory_panel.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/orders_panel.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/ui/day_end_panel.tscn`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/market_panel.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/workbench_panel.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/inventory_panel.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/orders_panel.gd`
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/day_end_panel.gd`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scenes/shop_scene.tscn`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/scripts/ui/shop_scene.gd`

**Interfaces:**
- Consumes: services from previous tasks.
- Produces: clickable panels that complete the full day loop.

- [ ] **Step 1: Create panel scene node pattern**

For each panel scene, use this node pattern:

```text
PanelRoot (PanelContainer, script=res://scripts/ui/<panel_script>.gd)
  VBoxContainer
    HeaderLabel (Label, unique_name_in_owner=true)
    ContentList (VBoxContainer, unique_name_in_owner=true)
    CloseButton (Button, text="Close", unique_name_in_owner=true)
```

Use these root names and scripts:

```text
MarketPanel -> res://scripts/ui/market_panel.gd
WorkbenchPanel -> res://scripts/ui/workbench_panel.gd
InventoryPanel -> res://scripts/ui/inventory_panel.gd
OrdersPanel -> res://scripts/ui/orders_panel.gd
DayEndPanel -> res://scripts/ui/day_end_panel.gd
```

- [ ] **Step 2: Implement MarketPanel**

Create `scripts/ui/market_panel.gd`:

```gdscript
extends PanelContainer
class_name MarketPanel

signal changed
signal closed

const DayFlowService = preload("res://scripts/services/day_flow_service.gd")
const EconomyService = preload("res://scripts/services/economy_service.gd")

@onready var content_list: VBoxContainer = %ContentList
@onready var close_button: Button = %CloseButton

var state: GameState
var loader: DataLoader

func _ready() -> void:
	close_button.pressed.connect(func(): closed.emit())

func bind(input_state: GameState, input_loader: DataLoader) -> void:
	state = input_state
	loader = input_loader
	_refresh()

func _refresh() -> void:
	for child in content_list.get_children():
		child.queue_free()
	for scrap_id in state.current_scrap_offers:
		var scrap: Dictionary = loader.scrap_lots[scrap_id]
		var price := EconomyService.scrap_price(scrap, state)
		var button := Button.new()
		button.text = "%s - %d credits - %s" % [scrap["name"], price, scrap["quality_hint"]]
		button.disabled = not state.can_afford(price) or state.owned_scrap_lots.has(scrap_id)
		button.pressed.connect(func(): _buy_scrap(scrap))
		content_list.add_child(button)

func _buy_scrap(scrap: Dictionary) -> void:
	DayFlowService.purchase_scrap(state, scrap)
	_refresh()
	changed.emit()
```

- [ ] **Step 3: Implement WorkbenchPanel**

Create `scripts/ui/workbench_panel.gd`:

```gdscript
extends PanelContainer
class_name WorkbenchPanel

signal changed
signal closed

const SalvageService = preload("res://scripts/services/salvage_service.gd")

@onready var content_list: VBoxContainer = %ContentList
@onready var close_button: Button = %CloseButton

var state: GameState
var loader: DataLoader
var rng := RandomNumberGenerator.new()

func _ready() -> void:
	rng.seed = 9001
	close_button.pressed.connect(func(): closed.emit())

func bind(input_state: GameState, input_loader: DataLoader) -> void:
	state = input_state
	loader = input_loader
	_refresh()

func _refresh() -> void:
	for child in content_list.get_children():
		child.queue_free()
	for scrap_id in state.owned_scrap_lots:
		var button := Button.new()
		button.text = "Disassemble %s" % loader.scrap_lots[scrap_id]["name"]
		button.pressed.connect(func(): _disassemble(scrap_id))
		content_list.add_child(button)
	if state.owned_scrap_lots.is_empty():
		var label := Label.new()
		label.text = "No scrap lots ready for disassembly."
		content_list.add_child(label)

func _disassemble(scrap_id: String) -> void:
	var bonus := 1 if state.upgrades.has("precision_disassembler") else 0
	var parts := SalvageService.disassemble(loader.scrap_lots[scrap_id], rng, bonus)
	for part_id in parts.keys():
		state.add_part(String(part_id), int(parts[part_id]))
	state.owned_scrap_lots.erase(scrap_id)
	_refresh()
	changed.emit()
```

- [ ] **Step 4: Implement InventoryPanel**

Create `scripts/ui/inventory_panel.gd`:

```gdscript
extends PanelContainer
class_name InventoryPanel

signal changed
signal closed

const EconomyService = preload("res://scripts/services/economy_service.gd")

@onready var content_list: VBoxContainer = %ContentList
@onready var close_button: Button = %CloseButton

var state: GameState
var loader: DataLoader

func _ready() -> void:
	close_button.pressed.connect(func(): closed.emit())

func bind(input_state: GameState, input_loader: DataLoader) -> void:
	state = input_state
	loader = input_loader
	_refresh()

func _refresh() -> void:
	for child in content_list.get_children():
		child.queue_free()
	var ids := state.inventory.keys()
	ids.sort()
	for part_id in ids:
		var part: Dictionary = loader.parts[part_id]
		var count := state.get_part_count(part_id)
		var value := EconomyService.sell_part_value(part)
		var button := Button.new()
		button.text = "%s x%d - sell one for %d" % [part["name"], count, value]
		button.pressed.connect(func(): _sell_one(String(part_id), value))
		content_list.add_child(button)
	if ids.is_empty():
		var label := Label.new()
		label.text = "No parts in storage."
		content_list.add_child(label)

func _sell_one(part_id: String, value: int) -> void:
	if state.remove_part(part_id, 1):
		state.earn(value)
	_refresh()
	changed.emit()
```

- [ ] **Step 5: Implement OrdersPanel**

Create `scripts/ui/orders_panel.gd`:

```gdscript
extends PanelContainer
class_name OrdersPanel

signal changed
signal closed

const OrderService = preload("res://scripts/services/order_service.gd")

@onready var content_list: VBoxContainer = %ContentList
@onready var close_button: Button = %CloseButton

var state: GameState
var loader: DataLoader

func _ready() -> void:
	close_button.pressed.connect(func(): closed.emit())

func bind(input_state: GameState, input_loader: DataLoader) -> void:
	state = input_state
	loader = input_loader
	_refresh()

func _refresh() -> void:
	for child in content_list.get_children():
		child.queue_free()
	for order_id in state.current_order_ids:
		var order: Dictionary = loader.orders[order_id]
		var button := Button.new()
		button.text = "%s - %d credits / %d rep" % [order["title"], order["reward_money"], order["reward_reputation"]]
		button.disabled = not OrderService.can_complete(order, state, loader.parts)
		button.pressed.connect(func(): _complete_order(order))
		content_list.add_child(button)

func _complete_order(order: Dictionary) -> void:
	OrderService.complete_order(order, state, loader.parts)
	_refresh()
	changed.emit()
```

- [ ] **Step 6: Implement DayEndPanel**

Create `scripts/ui/day_end_panel.gd`:

```gdscript
extends PanelContainer
class_name DayEndPanel

signal changed
signal closed

const DayFlowService = preload("res://scripts/services/day_flow_service.gd")

@onready var content_list: VBoxContainer = %ContentList
@onready var close_button: Button = %CloseButton

var state: GameState
var loader: DataLoader
var seed_base: int = 12000

func _ready() -> void:
	close_button.pressed.connect(func(): closed.emit())

func bind(input_state: GameState, input_loader: DataLoader) -> void:
	state = input_state
	loader = input_loader
	_refresh()

func _refresh() -> void:
	for child in content_list.get_children():
		child.queue_free()
	var button := Button.new()
	button.text = "End day %d" % state.day
	button.pressed.connect(_end_day)
	content_list.add_child(button)

func _end_day() -> void:
	DayFlowService.end_day(state)
	if state.day <= 7:
		DayFlowService.start_day(state, loader, seed_base + state.day)
	_refresh()
	changed.emit()
```

- [ ] **Step 7: Modify ShopScene to show and bind panels**

Modify `scripts/ui/shop_scene.gd`:

```gdscript
extends Control
class_name ShopScene

const DataLoader = preload("res://scripts/data/data_loader.gd")
const GameState = preload("res://scripts/state/game_state.gd")
const DayFlowService = preload("res://scripts/services/day_flow_service.gd")

@onready var hud: Hud = %Hud
@onready var status_label: Label = %StatusLabel
@onready var market_panel: MarketPanel = %MarketPanel
@onready var workbench_panel: WorkbenchPanel = %WorkbenchPanel
@onready var inventory_panel: InventoryPanel = %InventoryPanel
@onready var orders_panel: OrdersPanel = %OrdersPanel
@onready var day_end_panel: DayEndPanel = %DayEndPanel
@onready var market_button: Button = %MarketButton
@onready var workbench_button: Button = %WorkbenchButton
@onready var inventory_button: Button = %InventoryButton
@onready var orders_button: Button = %OrdersButton
@onready var end_day_button: Button = %EndDayButton

var loader := DataLoader.new()
var state := GameState.new_run()

func _ready() -> void:
	if not loader.load_all():
		status_label.text = "Data failed to load."
		return
	DayFlowService.start_day(state, loader, 7001)
	_bind_panels()
	_connect_buttons()
	_show_panel(null)
	_refresh()

func _bind_panels() -> void:
	for panel in [market_panel, workbench_panel, inventory_panel, orders_panel, day_end_panel]:
		panel.bind(state, loader)
		panel.changed.connect(_refresh)
		panel.closed.connect(func(): _show_panel(null))

func _connect_buttons() -> void:
	market_button.pressed.connect(func(): _show_panel(market_panel))
	workbench_button.pressed.connect(func(): _show_panel(workbench_panel))
	inventory_button.pressed.connect(func(): _show_panel(inventory_panel))
	orders_button.pressed.connect(func(): _show_panel(orders_panel))
	end_day_button.pressed.connect(func(): _show_panel(day_end_panel))

func _show_panel(target: Control) -> void:
	for panel in [market_panel, workbench_panel, inventory_panel, orders_panel, day_end_panel]:
		panel.visible = panel == target

func _refresh() -> void:
	hud.update_state(state)
	status_label.text = "Run the shop through 7 days." if state.day <= 7 else "Demo complete. Review the run."
	for panel in [market_panel, workbench_panel, inventory_panel, orders_panel, day_end_panel]:
		panel.bind(state, loader)
```

- [ ] **Step 8: Add panel instances to shop scene**

Modify `scenes/shop_scene.tscn` in Godot editor by adding these children under `ShopScene`:

```text
MarketPanel (instance=res://scenes/ui/market_panel.tscn, unique_name_in_owner=true, visible=false)
WorkbenchPanel (instance=res://scenes/ui/workbench_panel.tscn, unique_name_in_owner=true, visible=false)
InventoryPanel (instance=res://scenes/ui/inventory_panel.tscn, unique_name_in_owner=true, visible=false)
OrdersPanel (instance=res://scenes/ui/orders_panel.tscn, unique_name_in_owner=true, visible=false)
DayEndPanel (instance=res://scenes/ui/day_end_panel.tscn, unique_name_in_owner=true, visible=false)
```

Set each panel anchors to center with a minimum size of `720 x 460`.

- [ ] **Step 9: Run tests and main scene smoke check**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
godot --headless --path . --quit-after 1
```

Expected: tests pass; main scene exits with code 0.

- [ ] **Step 10: Commit**

Run:

```powershell
git add scenes scripts/ui
git commit -m "feat: add playable shop panels"
```

---

### Task 11: Add Manual Playtest Checklist And Tune The Demo

**Files:**
- Create: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/docs/playtest-checklist.md`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/scrap_lots.json`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/orders.json`
- Modify: `C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/data/upgrades.json`

**Interfaces:**
- Consumes: playable panels and service tests.
- Produces: a documented 7-day manual playtest and first balance pass.

- [ ] **Step 1: Create manual playtest checklist**

Create `docs/playtest-checklist.md`:

```markdown
# Orbital Salvage Shop 7-Day Playtest Checklist

## Setup

- Run the game from `scenes/shop_scene.tscn`.
- Start a new run.
- Use keyboard and mouse only.

## Core Loop Checks

- Day 1 shows money, day, and reputation.
- Market shows exactly three scrap offers.
- Buying one scrap lot reduces money by the shown price.
- Workbench can disassemble owned scrap into parts.
- Inventory shows the gained parts.
- Selling one part increases money by the shown value.
- Orders panel disables orders that cannot be completed.
- Completing an order consumes parts and rewards money and reputation.
- End Day advances the day and charges daily expense.
- The run reaches day 8 after seven settlements.

## Feel Checks

- The basic loop is understandable within 5 minutes.
- At least one scrap purchase choice feels meaningful.
- At least one order choice feels meaningful.
- Money pressure slows progress without ending the run suddenly.
- The screen reads as a warm lo-fi space salvage shop.

## Notes

- Record the final day, money, reputation, and any confusing UI moment.
- Record one thing that felt satisfying.
- Record one thing that felt repetitive.
```

- [ ] **Step 2: Run automated tests**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: all tests pass.

- [ ] **Step 3: Run one manual 7-day playtest**

Run:

```powershell
godot --path .
```

Expected: the main shop opens. Complete the checklist in `docs/playtest-checklist.md`.

- [ ] **Step 4: Apply first balance changes**

If the player cannot buy any useful scrap after day 2, reduce day expenses in `scripts/services/economy_service.gd` from:

```gdscript
return 18 + max(day - 1, 0) * 2
```

to:

```gdscript
return 14 + max(day - 1, 0) * 2
```

If orders are too hard to complete on days 1 and 2, reduce the first four order rewards and requirements by editing `data/orders.json` to this content for the first four records:

```json
[
  {"id":"patch_delivery_drone","customer_id":"courier","title":"Patch a Delivery Drone","requirements":[{"tag":"motion","count":1}],"reward_money":42,"reward_reputation":1,"expires_after_days":1},
  {"id":"backup_lamp_grid","customer_id":"vendor","title":"Restore a Lamp Grid","requirements":[{"tag":"power","count":1}],"reward_money":38,"reward_reputation":1,"expires_after_days":1},
  {"id":"seal_window_panel","customer_id":"dockhand","title":"Seal a Window Panel","requirements":[{"tag":"hull","count":1}],"reward_money":36,"reward_reputation":1,"expires_after_days":1},
  {"id":"calibrate_scanner","customer_id":"pilot","title":"Calibrate a Scanner","requirements":[{"tag":"sensor","count":1}],"reward_money":44,"reward_reputation":1,"expires_after_days":1}
]
```

Keep records 5 through 8 unchanged.

- [ ] **Step 5: Re-run automated tests after tuning**

Run:

```powershell
godot --headless --path . -s res://tests/test_runner.gd
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

Run:

```powershell
git add docs/playtest-checklist.md data scripts/services/economy_service.gd
git commit -m "docs: add playtest checklist"
```

---

## Self-Review

Spec coverage:

- PC/Steam target is preserved in global constraints.
- Godot 2D UI-heavy direction is preserved.
- Data-driven scrap, parts, orders, recipes, and upgrades are covered in Tasks 2 and 3.
- Business logic separation is covered in Tasks 3 through 8.
- Five required UI panels are covered in Task 10.
- 7-day demo loop is covered in Task 7 and Task 11.
- Save system with one slot is covered in Task 8.
- Manual playtest success criteria are covered in Task 11.
- Out-of-scope features are not included in implementation tasks.

Placeholder scan:

- The plan uses exact file paths, exact commands, exact data seeds, and concrete code snippets.
- No task depends on unspecified assets; all first-slice art uses temporary SVG files.

Type consistency:

- `GameState` method names match all service and UI calls.
- `DataLoader` dictionary names match all service and UI calls.
- `RecipeService` and `OrderService` signatures match all tests.
- `DayFlowService` signatures match all tests and panel code.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-07-orbital-salvage-shop-vertical-slice.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration.

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints.

Which approach?

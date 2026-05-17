# Prime Force — Claude Code Context

This file is read automatically by Claude Code on every machine (Linux dev, Surface, etc.).
It is the single source of truth for session continuity.

---

## Project overview

Prime Force is a Godot 4.6.2 (.NET/C#) 3D action platformer with Soulslike mechanics and math puzzles, built for educational use in EU schools (GDPR — local saves only, AES-256 encrypted).

**Core rule:** All business logic is pure C# (no Godot types). Godot Nodes are thin adapters only. Strict SOLID — especially DIP and ISP.

---

## Environment

| Machine | OS | Godot | .NET |
|---|---|---|---|
| Surface (primary) | Windows 11 | 4.6.2-stable.mono | 8 (built-in) |
| Linux dev | Ubuntu | 4.6.2-stable.mono | 8 |

Git remote: `https://github.com/Matiako/Prime_Force` (main branch)

Always `git pull` before starting work. Commit + push after **every file change** — user is on two machines.

---

## Project settings

- Viewport: **1280×800** (landscape tablet / Horizontal+)
- Main scene: `Scenes/World/Main.tscn`
- Autoload: `GameServices` → `res://Scripts/Core/Services/GameServices.cs`
- GodotSharp NuGet: **4.6.2** (must match Godot.NET.Sdk/4.6.2 — avoids NU1605)
- Stretch mode: canvas_items / expand

---

## Scene graph — Main.tscn

```
Main (Node3D)
├── DirectionalLight3D
├── Camera3D              side-view (0,4,12), -15° tilt
├── Floor (StaticBody3D)  40×0.5×10 m BoxShape3D
├── Ninja (CharacterBody3D)  NinjaController.cs, spawns at (0,2,0)
│   ├── CollisionShape3D  CapsuleShape3D r=0.35 h=1.7
│   └── MeshInstance3D    CapsuleMesh
└── GameUI  instance of Scenes/UI/GameUI.tscn (CanvasLayer layer=10)

Signal connections (all in Main.tscn [connection] blocks):
  GameUI.MovementChanged  → Ninja.OnMovementChanged(Vector2)
  GameUI.JumpPressed      → Ninja.OnJumpPressed()
  GameUI.AttackPressed    → Ninja.OnAttackButtonPressed()
  GameUI.BlockPressed     → Ninja.OnBlockPressed()
```

---

## GameUI layout (Scenes/UI/GameUI.tscn)

**All buttons are `Button`, NOT `TextureButton`** — TextureButton without a texture is invisible.

```
Top:  StatsBar → GoldLabel, GeldLabel, MetallLabel, KristalleLabel, LevelLabel, EnergyBar
      Hotbar → 7× WeaponSlot (Panel + TextureRect)
BotL: DPadArea (ColorRect bg + 4× Button absolutely positioned)
        DPadUp "▲"  DPadDown "▼"  DPadLeft "◄"  DPadRight "►"
BotR: ActionArea → HBoxContainer
        JumpButton "SKOK"  (80×80, Fill+Expand vertical)
        AttackBlockVBox (VBoxContainer, separation=10)
          BlockButton "BLOK"  (80×80)
          AttackButton "ATAK" (80×80)
```

`GameUiController.cs` uses `GetNode<T>(path)` in `_Ready()` — no Inspector [Export] wiring.
Subscribes to `PlayerLevelUpEvent` → updates LevelLabel + EnergyBar.MaxValue.
Initial Level + MaxHealth read from `PlayerProgressionManager.Data` on startup.
Gold/Geld/Metall/Kristalle show 0 — resource system not yet built.

---

## Scripts map

```
Scripts/
  Core/
    Events/         ChallengeEvents, CombatEvents, ProgressionEvents
    Interfaces/     IEventBus
    Services/       GameServices (Autoload), PlayerProgressionManager, SimpleEventBus
  Combat/
    Interfaces/     ICombatCalculator, ICombatEntity, IChallengeAnswerReceiver
    Systems/        BasicCombatCalculator (async TCS, NO RunContinuationsAsynchronously)
  Entities/
    Player/         NinjaController, NinjaCombatEntity
    Enemies/        EnemyController, BossController, EnemyCombatEntity, BossCombatEntity
  MathEngine/
    Interfaces/     IMathChallenge, IMathChallengeGenerator
    Generators/     AdditionSubtractionChallenge/Generator (levels 1-5)
                    MultiplicationDivisionChallenge/Generator (levels 6-10)
                    LevelAwareChallengeGenerator (Func<int> closure, threshold=5)
                    PrimeNumberChallenge/Generator (Boss only)
  SaveSystem/
    Interfaces/     ISaveSystem
    Serializers/    JsonSaveSystem (AES-256, Task.Run)
    PlayerSaveData  (Level, MaxHealth, TotalChallengesAnswered/Correct, LastSaved)
  Localization/
    Interfaces/     ILocalizationProvider
    Providers/      GodotLocalizationProvider (wraps TranslationServer)
  UI/
    GameUiController  (CanvasLayer, GetNode wiring, EventBus subscriber)
    MathChallengeUI   (reacts to ChallengeStartedEvent, calls IChallengeAnswerReceiver)
  World/
    CheckpointController (Area3D, "player" group, calls progression.SaveAsync())
Tests/
  MathEngine/MultiplicationDivisionChallengeGeneratorTests.cs  (xUnit, 12 tests)
```

---

## NinjaController current capabilities

```csharp
_PhysicsProcess  // gravity -20, XZ movement from _moveInput, MoveAndSlide()
OnMovementChanged(Vector2 direction)  // sets _moveInput (from D-Pad signal)
OnJumpPressed()                       // if IsOnFloor() → Velocity.Y = 7
OnBlockPressed()                      // toggles _isBlocking bool
OnAttackButtonPressed()               // async, ICombatCalculator.CalculateDamageAsync()
```

---

## Key architecture decisions (do not change without discussion)

- `BasicCombatCalculator`: **no `RunContinuationsAsynchronously`** — continuation runs on Godot main thread when `SubmitAnswer()` is called
- `LevelAwareChallengeGenerator`: **`Func<int>` closure**, not direct reference to PlayerProgressionManager (DIP)
- **Checkpoint-only saves** (Soulslike): auto-save after enemy defeat removed — only `CheckpointController` triggers save
- `BossController` **bypasses** GameServices' `IMathChallengeGenerator` — creates its own `BasicCombatCalculator(PrimeNumberChallengeGenerator)` instance
- `ServiceLocator.cs` **deleted** — `GameServices` supersedes it

---

## Coding conventions

- No comments unless WHY is non-obvious
- No `[Export]` for internal node wiring — use `GetNode<T>(path)` in `_Ready()`
- `async void` for Godot signal callbacks (not `async Task`)
- Commit after every file change (user pulls on Surface immediately)
- Ask before modifying existing code that wasn't part of the request

---

## Phase 7 — next tasks (pending user approval)

1. `NinjaController`: add `[Export] BossController? TargetBoss`, switch to BossCombatCalculator when target is Boss
2. Localized feedback in `MathChallengeUI` (Richtig!/Correct!/Poprawnie!) via `ILocalizationProvider`
3. `MathChallengeUI`: show correct answer when player answers wrong
4. `PrimeNumberChallengeGeneratorTests`
5. Enemy + platform scene for actual combat testing

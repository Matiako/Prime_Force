# AI_SYNC — Prime Force

> **Purpose:** Context bridge between AI sessions (Claude, Gemini, etc.).  
> Update after every major implementation step. Always request human feedback before moving to the next phase.

---

## Project Snapshot

| Field | Value |
|---|---|
| Engine | Godot 4 (.NET / C# 12) |
| Genre | Action 3D TPP / Educational (Soulslike for kids) |
| Player character | Ninja |
| Core mechanic | Math puzzles gate and amplify combat actions |
| Locales | `de` · `en` · `pl` |
| Save policy | Local only — GDPR / checkpoint-driven (Soulslike bonfires) |
| Last updated | 2026-05-14 |

---

## Current Architecture State

### Phase: 6 — Boss, Prime Numbers, Tests (code complete — test project setup pending)

```
Scripts/
  Core/
    Interfaces/   IEventBus
    Events/       ChallengeEvents · CombatEvents · ProgressionEvents
    Services/     GameServices (Autoload) · SimpleEventBus · PlayerProgressionManager
  Entities/
    Player/       NinjaCombatEntity (POCO) · NinjaController (CharacterBody3D)
    Enemies/      EnemyCombatEntity (POCO) · EnemyController (CharacterBody3D)
                  BossCombatEntity (POCO, phases) · BossController (CharacterBody3D, owns prime calculator)
  MathEngine/
    Interfaces/   IMathChallenge · IMathChallengeGenerator
    Generators/   AdditionSubtractionChallenge · AdditionSubtractionChallengeGenerator
                  MultiplicationDivisionChallenge · MultiplicationDivisionChallengeGenerator
                  LevelAwareChallengeGenerator  ← composite, registered as IMathChallengeGenerator
                  PrimeNumberChallenge · PrimeNumberChallengeGenerator  ← Boss only
  Combat/
    Interfaces/   ICombatEntity · ICombatCalculator · IChallengeAnswerReceiver
    Systems/      BasicCombatCalculator
  Localization/
    Interfaces/   ILocalizationProvider
    Providers/    GodotLocalizationProvider
  SaveSystem/
    PlayerSaveData (POCO)
    Interfaces/   ISaveSystem  (Save · SaveAsync · Load · SlotExists · DeleteSlot · ListSlots)
    Serializers/  JsonSaveSystem (AES-256 encrypted, async-capable)
  UI/
    MathChallengeUI (Control)
  World/
    CheckpointController (Area3D — Soulslike bonfire)
  Tests/
    MathEngine/  MultiplicationDivisionChallengeGeneratorTests (xUnit — test project setup pending)
```

---

## Dependency Diagram

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  GODOT LAYER (Nodes)              DOMAIN LAYER (Pure C#)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  GameServices : Node (Autoload — Composition Root)
  │
  ├─ creates ──► SimpleEventBus : IEventBus
  │
  ├─ creates ──► AdditionSubtractionChallengeGenerator
  ├─ creates ──► MultiplicationDivisionChallengeGenerator
  ├─ creates ──► LevelAwareChallengeGenerator : IMathChallengeGenerator
  │               ├─ lowLevel  = AdditionSubtractionChallengeGenerator
  │               ├─ highLevel = MultiplicationDivisionChallengeGenerator
  │               └─ Func<int> = () => progression.Data.Level  (closure, no direct ref)
  │
  ├─ creates ──► BasicCombatCalculator : ICombatCalculator, IChallengeAnswerReceiver
  │               ├─ IMathChallengeGenerator (LevelAwareChallengeGenerator)
  │               └─ IEventBus
  │
  ├─ creates ──► GodotLocalizationProvider : ILocalizationProvider
  │               └─ wraps TranslationServer (only Godot API in domain boundary)
  │
  ├─ creates ──► JsonSaveSystem : ISaveSystem
  │               └─ FileAccess.OpenEncryptedWithPass (AES-256, user://saves/)
  │
  └─ creates ──► PlayerProgressionManager
                  ├─ ISaveSystem  (Load on start; Save via SaveAsync() on checkpoint)
                  ├─ IEventBus    (Subscribe: ChallengeAnsweredEvent → XP + LevelUp)
                  └─ publishes:   PlayerLevelUpEvent


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  GODOT NODE DEPENDENCIES  (all resolved via GameServices.Get<T>)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  NinjaController : CharacterBody3D
  │  owns ──► NinjaCombatEntity (POCO, init from PlayerProgressionManager.Data)
  │  uses ──► ICombatCalculator
  │  uses ──► ILocalizationProvider
  │  uses ──► IEventBus          (subscribes PlayerLevelUpEvent → ApplyLevelUp)
  └─ uses ──► PlayerProgressionManager  (reads Level + MaxHealth on _Ready)

  EnemyController : CharacterBody3D
  │  owns ──► EnemyCombatEntity (POCO)
  │  uses ──► IEventBus         (subscribes DamageDealtEvent → HP display)
  └─ publishes: EnemyDefeatedEvent · (no current domain subscriber — ready for audio/UI)

  MathChallengeUI : Control
  │  uses ──► IEventBus             (subscribes ChallengeStartedEvent → show puzzle)
  └─ uses ──► IChallengeAnswerReceiver  (calls SubmitAnswer → unblocks BasicCombatCalculator)

  CheckpointController : Area3D
  └─ uses ──► PlayerProgressionManager  (calls SaveAsync on player body entry)


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  EVENT BUS — signal flow
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  ChallengeStartedEvent    ──► MathChallengeUI          (show puzzle UI)
  ChallengeAnsweredEvent   ──► PlayerProgressionManager  (XP, level-up check)
  DamageDealtEvent         ──► EnemyController           (refresh HP display)
  EnemyDefeatedEvent       ──► (published, no subscriber — extensibility hook)
  PlayerLevelUpEvent       ──► NinjaController           (ApplyLevelUp on entity)


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  CORE LOOP — full sequence
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  [Player presses Attack]
       │
  NinjaController.OnAttackButtonPressed()   [async void, main thread]
       │ await
  BasicCombatCalculator.CalculateDamageAsync()
       ├─ LevelAwareChallengeGenerator.Generate()  →  puzzle question
       ├─ Publish(ChallengeStartedEvent)  →  MathChallengeUI shows puzzle
       └─ await TaskCompletionSource  ···  [main thread free]

  [Player submits answer]
       │
  MathChallengeUI.OnSubmitPressed()
       └─ IChallengeAnswerReceiver.SubmitAnswer()
            └─ TCS.TrySetResult()  →  resumes on main thread (no RunContinuationsAsynchronously)
                 ├─ defender.TakeDamage(finalDamage)
                 ├─ Publish(ChallengeAnsweredEvent)  →  PlayerProgressionManager (XP)
                 └─ Publish(DamageDealtEvent)        →  EnemyController (HP display)

  [If enemy HP = 0]
       EnemyController publishes EnemyDefeatedEvent

  [Player enters Checkpoint area]
       CheckpointController.OnBodyEntered()
            └─ await PlayerProgressionManager.SaveAsync()
                      └─ Task.Run(() => JsonSaveSystem.Save())  [thread pool, non-blocking]
```

---

## Architecture Decision Records (ADR)

### ADR-001 — Pure C# logic decoupled from Godot nodes
- **Decision:** All business logic (MathEngine, Combat, Save, Localization) lives in plain C# classes. Godot Nodes are thin adapters only.
- **Reason:** Unit testing without a running Godot instance; respects DIP.
- **Status:** Accepted

### ADR-002 — Event-driven communication via IEventBus
- **Decision:** Systems communicate through typed pub/sub, not direct calls or Godot signals across domains.
- **Reason:** Decouples MathEngine ↔ Combat ↔ UI; each system independently testable.
- **Status:** Accepted

### ADR-003 — Local-only saves (GDPR)
- **Decision:** ISaveSystem writes to `user://saves/` only. AES-256 encryption prevents student tampering. No network I/O.
- **Reason:** GDPR compliance for EU school deployments.
- **Status:** Accepted

### ADR-004 — IMathChallenge.GetCombatModifier() as the integration seam
- **Decision:** The bridge between MathEngine and Combat is `GetCombatModifier() → float`. ICombatCalculator applies it.
- **Reason:** Both systems evolve independently; formula changes don't touch entity code.
- **Status:** Accepted

### ADR-005 — GameServices as single Autoload (replaces ServiceLocator pattern)
- **Decision:** One `GameServices : Node` is both registry and composition root. ServiceLocator.cs deleted.
- **Reason:** Single point of instantiation; lifecycle tied to Godot scene tree.
- **Status:** Accepted

### ADR-006 — Vertical Slice as first milestone
- **Decision:** One room + one enemy + one challenge type = Core Loop validation.
- **Status:** Completed ✅

### ADR-007 — Math challenge priority: Add/Sub → Mul/Div → Primes (Boss) → Fractions
- **Status:** Accepted. Add/Sub and Mul/Div implemented. Primes pending.

### ADR-008 — Checkpoint-driven saves (Soulslike)
- **Decision:** Auto-save after enemy defeat removed. Save is triggered exclusively by CheckpointController (Area3D bonfire). PlayerProgressionManager exposes `SaveAsync()` called externally.
- **Reason:** Matches Soulslike design contract; player controls when progress is committed.
- **Status:** Accepted

### ADR-009 — LevelAwareChallengeGenerator via Func<int> closure
- **Decision:** Composite generator receives `Func<int> getCurrentLevel` instead of a direct reference to `PlayerProgressionManager`. Threshold: level ≤ 5 → Add/Sub, level > 5 → Mul/Div.
- **Reason:** Keeps MathEngine domain free of any dependency on the progression layer (DIP). Closure in GameServices.Bootstrap() is the only coupling point.
- **Status:** Accepted

---

## To-Do / Roadmap

### Phases 1–5 ✅ COMPLETE

### Phase 6 — Boss, Prime Numbers, Tests ✅ CODE COMPLETE
- [x] `PrimeNumberChallenge` + `PrimeNumberChallengeGenerator` (identification + sequence types)
- [x] `BossCombatEntity` — phase system (phases 1/2/3 at 66%/33% HP), EffectiveDifficulty scales per phase
- [x] `BossController` — owns private `BasicCombatCalculator(PrimeNumberChallengeGenerator)`, bypasses GameServices
- [x] `MultiplicationDivisionChallengeGeneratorTests` — xUnit exemplary test class (12 tests)
- [ ] Test project setup — awaiting `dotnet` installation + `ls *.csproj` confirmation
- [ ] `NinjaController` update for Boss target — awaiting approval (Phase 7)

### Phase 7 — Next steps (pending your decision)
- [ ] NinjaController: add `[Export] BossController? TargetBoss` + use `BossCombatCalculator`
- [ ] Localized feedback strings (Richtig! / Correct! / Poprawnie!) via ILocalizationProvider
- [ ] MathChallengeUI: show correct answer on failure
- [ ] PrimeNumberChallengeGeneratorTests (test coverage expansion)

---

## Open Questions

_No open questions. Awaiting Phase 6 direction._

---

## Feedback Log

| Date | Topic | Decision |
|---|---|---|
| 2026-05-14 | Initial structure | Reviewed — approved |
| 2026-05-14 | DI strategy | GameServices Autoload (ServiceLocator deleted) |
| 2026-05-14 | First milestone scope | Vertical Slice — Core Loop |
| 2026-05-14 | Math challenge order | Add/Sub → Mul/Div → Primes (Boss) → Fractions |
| 2026-05-14 | Phases 1–2 | Core Loop + Godot wiring approved |
| 2026-05-14 | Phase 3 | Enemy, Localization, Save System approved |
| 2026-05-14 | Phase 4 | PlayerProgressionManager, MulDiv, async save, level-up |
| 2026-05-14 | Phase 5 | LevelAwareChallengeGenerator (Option A), checkpoint saves (ADR-008, ADR-009) |
| 2026-05-14 | Phase 6 | BossController+BossCombatEntity, PrimeNumberChallengeGenerator, xUnit test class |

---

*Update this file after every implementation phase. Do not proceed to the next phase without human confirmation.*

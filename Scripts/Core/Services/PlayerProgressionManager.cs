using PrimeForce.Core.Events;
using PrimeForce.Core.Interfaces;
using PrimeForce.SaveSystem;
using PrimeForce.SaveSystem.Interfaces;

namespace PrimeForce.Core.Services;

/// <summary>
/// Domain service (pure C#, no Godot dependency) responsible for:
///   - Loading PlayerSaveData on startup via ISaveSystem
///   - Tracking math challenge outcomes (XP) and triggering level-up
///   - Exposing SaveAsync() for external triggers (CheckpointController)
///
/// Save is NO LONGER automatic after enemy defeat — it is checkpoint-driven (Soulslike).
/// Lifecycle: created in GameServices.Bootstrap(); call Dispose() when the game exits.
/// </summary>
public sealed class PlayerProgressionManager
{
    private const string SaveSlot          = "player";
    private const int    ChallengesPerLevel = 5;

    private readonly ISaveSystem _saveSystem;
    private readonly IEventBus   _eventBus;

    public PlayerSaveData Data { get; private set; }

    public PlayerProgressionManager(ISaveSystem saveSystem, IEventBus eventBus)
    {
        _saveSystem = saveSystem;
        _eventBus   = eventBus;

        Data = _saveSystem.Load<PlayerSaveData>(SaveSlot) ?? new PlayerSaveData();

        _eventBus.Subscribe<ChallengeAnsweredEvent>(OnChallengeAnswered);
    }

    /// <summary>
    /// Checkpoint-triggered save. Called by CheckpointController, never automatically.
    /// </summary>
    public Task SaveAsync()
    {
        Data.LastSaved = DateTime.UtcNow;
        return _saveSystem.SaveAsync(SaveSlot, Data);
    }

    public void Dispose()
    {
        _eventBus.Unsubscribe<ChallengeAnsweredEvent>(OnChallengeAnswered);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnChallengeAnswered(ChallengeAnsweredEvent e)
    {
        Data.TotalChallengesAnswered++;
        if (!e.WasCorrect) return;

        Data.TotalChallengesCorrect++;

        int newLevel = 1 + Data.TotalChallengesCorrect / ChallengesPerLevel;
        if (newLevel <= Data.Level) return;

        Data.Level     = newLevel;
        Data.MaxHealth = BaseMaxHealth(newLevel);
        _eventBus.Publish(new PlayerLevelUpEvent(newLevel, Data.MaxHealth));
    }

    private static int BaseMaxHealth(int level) => 100 + (level - 1) * 10;
}

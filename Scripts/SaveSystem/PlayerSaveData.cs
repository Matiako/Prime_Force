namespace PrimeForce.SaveSystem;

/// <summary>
/// POCO — the only data class that crosses the save boundary.
/// Serialised to JSON by JsonSaveSystem; never contains Godot types.
/// </summary>
public sealed class PlayerSaveData
{
    public int      Level                    { get; set; } = 1;
    public int      MaxHealth                { get; set; } = 100;
    public int      TotalChallengesAnswered  { get; set; }
    public int      TotalChallengesCorrect   { get; set; }
    public DateTime LastSaved                { get; set; } = DateTime.UtcNow;

    /// <summary>Computed — not persisted, derived on load.</summary>
    public float MathAccuracy =>
        TotalChallengesAnswered == 0
            ? 0f
            : (float)TotalChallengesCorrect / TotalChallengesAnswered;
}

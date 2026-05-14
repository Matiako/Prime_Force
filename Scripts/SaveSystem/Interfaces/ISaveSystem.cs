namespace PrimeForce.SaveSystem.Interfaces;

/// <summary>
/// Local-only save system (GDPR — no cloud sync, no external transmission).
/// </summary>
public interface ISaveSystem
{
    /// <summary>Persists the data object under the given slot name (blocking).</summary>
    void Save<T>(string slotName, T data) where T : notnull;

    /// <summary>Non-blocking save — offloads file I/O to a thread-pool thread.</summary>
    Task SaveAsync<T>(string slotName, T data) where T : notnull;

    /// <summary>Returns null if the slot does not exist.</summary>
    T? Load<T>(string slotName) where T : class;

    bool SlotExists(string slotName);
    void DeleteSlot(string slotName);
    IReadOnlyList<string> ListSlots();
}

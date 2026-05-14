using System.Text.Json;
using Godot;
using PrimeForce.SaveSystem.Interfaces;

namespace PrimeForce.SaveSystem.Serializers;

/// <summary>
/// Writes save slots as AES-256 encrypted JSON files under user://saves/.
/// Encryption prevents casual editing in a text editor — sufficient for school deployments.
/// The password is intentionally embedded; this is not a security boundary, only tamper-proofing.
/// </summary>
public sealed class JsonSaveSystem : ISaveSystem
{
    private const string SaveDir        = "user://saves";
    private const string EncryptionPass = "pf_2026_school";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
    };

    public void Save<T>(string slotName, T data) where T : notnull
    {
        EnsureSaveDirectoryExists();

        string json = JsonSerializer.Serialize(data, SerializerOptions);
        string path = SlotPath(slotName);

        using var file = FileAccess.OpenEncryptedWithPass(path, FileAccess.ModeFlags.Write, EncryptionPass);
        if (file is null)
            throw new IOException(
                $"[JsonSaveSystem] Cannot write '{path}': {FileAccess.GetOpenError()}");

        file.StoreString(json);
    }

    public T? Load<T>(string slotName) where T : class
    {
        string path = SlotPath(slotName);
        if (!FileAccess.FileExists(path)) return null;

        using var file = FileAccess.OpenEncryptedWithPass(path, FileAccess.ModeFlags.Read, EncryptionPass);
        if (file is null) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(file.GetAsText(), SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Offloads file I/O to a thread-pool thread so the Godot main thread is not blocked.
    /// FileAccess is safe to call from non-main threads (no scene-tree operations).
    /// </summary>
    public Task SaveAsync<T>(string slotName, T data) where T : notnull
        => Task.Run(() => Save(slotName, data));

    public bool SlotExists(string slotName) => FileAccess.FileExists(SlotPath(slotName));

    public void DeleteSlot(string slotName)
    {
        using var dir = DirAccess.Open(SaveDir);
        dir?.Remove(slotName + ".json");
    }

    public IReadOnlyList<string> ListSlots()
    {
        using var dir = DirAccess.Open(SaveDir);
        if (dir is null) return Array.Empty<string>();

        var slots = new List<string>();
        dir.ListDirBegin();
        string entry;
        while ((entry = dir.GetNext()) != string.Empty)
        {
            if (!dir.CurrentIsDir() && entry.EndsWith(".json"))
                slots.Add(entry[..^5]);
        }
        dir.ListDirEnd();
        return slots;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private static string SlotPath(string slotName) => $"{SaveDir}/{slotName}.json";

    private static void EnsureSaveDirectoryExists()
    {
        using var dir = DirAccess.Open("user://");
        if (dir is not null && !dir.DirExists("saves"))
            dir.MakeDir("saves");
    }
}

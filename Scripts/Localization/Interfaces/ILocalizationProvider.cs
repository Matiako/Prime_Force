namespace PrimeForce.Localization.Interfaces;

/// <summary>
/// Abstracts Godot's TranslationServer so pure C# classes (e.g. MathEngine)
/// never depend directly on a Godot node (DIP).
/// </summary>
public interface ILocalizationProvider
{
    /// <param name="key">Translation key defined in .po/.csv resource.</param>
    string GetTranslation(string key);

    /// <summary>ISO 639-1 code of the currently active locale.</summary>
    string CurrentLocale { get; }

    /// <summary>Supported locales: ["de", "en", "pl"]</summary>
    IReadOnlyList<string> SupportedLocales { get; }
}

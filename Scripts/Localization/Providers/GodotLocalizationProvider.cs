using Godot;
using PrimeForce.Localization.Interfaces;

namespace PrimeForce.Localization.Providers;

/// <summary>
/// Bridges Godot's TranslationServer to the domain-layer ILocalizationProvider contract.
/// This is the only class in the project that may reference TranslationServer directly.
/// </summary>
public sealed class GodotLocalizationProvider : ILocalizationProvider
{
    public IReadOnlyList<string> SupportedLocales { get; } = new[] { "de", "en", "pl" };

    /// <summary>
    /// Returns only the ISO 639-1 language code (e.g. "en", not "en_US"),
    /// so consumers never need to parse locale strings themselves.
    /// </summary>
    public string CurrentLocale => TranslationServer.GetLocale().Split('_')[0];

    public string GetTranslation(string key) => TranslationServer.Translate(key);
}

namespace backend.Services;

public interface ILocalizationService
{
    /// <summary>
    /// Gets all translations for a specific language
    /// </summary>
    /// <param name="language">Language code (az, en, tr)</param>
    /// <returns>Dictionary of translation keys and values</returns>
    Task<Dictionary<string, object>> GetTranslationsAsync(string language);
    
    /// <summary>
    /// Gets a specific translation value by key for a language
    /// </summary>
    /// <param name="language">Language code (az, en, tr)</param>
    /// <param name="key">Translation key (e.g., "common.welcome")</param>
    /// <returns>Translation value or null if not found</returns>
    Task<string?> GetTranslationAsync(string language, string key);
    
    /// <summary>
    /// Gets the language from Accept-Language header
    /// </summary>
    /// <param name="acceptLanguageHeader">Accept-Language header value</param>
    /// <returns>Supported language code or default (en)</returns>
    string GetLanguageFromHeader(string? acceptLanguageHeader);
    
    /// <summary>
    /// Gets list of supported languages
    /// </summary>
    /// <returns>List of supported language codes</returns>
    List<string> GetSupportedLanguages();
}

using System.Text.Json;

namespace backend.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalizationService> _logger;
    private readonly Dictionary<string, Dictionary<string, object>> _translationsCache;
    private readonly List<string> _supportedLanguages = new() { "az", "en", "tr" };
    private const string DefaultLanguage = "en";
    private const string TranslationsPath = "Resources/Translations";

    public LocalizationService(IWebHostEnvironment environment, ILogger<LocalizationService> logger)
    {
        _environment = environment;
        _logger = logger;
        _translationsCache = new Dictionary<string, Dictionary<string, object>>();
        
        // Load all translations at startup
        LoadAllTranslations();
    }

    private void LoadAllTranslations()
    {
        foreach (var language in _supportedLanguages)
        {
            try
            {
                var filePath = Path.Combine(_environment.ContentRootPath, TranslationsPath, $"{language}.json");
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"Translation file not found: {filePath}");
                    continue;
                }

                var jsonContent = File.ReadAllText(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

                if (translations != null)
                {
                    _translationsCache[language] = translations;
                    _logger.LogInformation($"Loaded translations for language: {language}");
                    _logger.LogInformation($"  Translation keys: {string.Join(", ", translations.Keys)}");

                    // Debug: Check if 'account' key exists
                    if (translations.ContainsKey("account"))
                    {
                        _logger.LogInformation($"  ✓ 'account' key found in {language} translations");
                    }
                    else
                    {
                        _logger.LogWarning($"  ✗ 'account' key NOT found in {language} translations");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading translations for language: {language}");
            }
        }
    }

    public async Task<Dictionary<string, object>> GetTranslationsAsync(string language)
    {
        // Normalize language code
        language = language.ToLower();
        
        // Check if language is supported
        if (!_supportedLanguages.Contains(language))
        {
            _logger.LogWarning($"Unsupported language requested: {language}. Falling back to {DefaultLanguage}");
            language = DefaultLanguage;
        }

        // Return from cache if available
        if (_translationsCache.ContainsKey(language))
        {
            return await Task.FromResult(_translationsCache[language]);
        }

        // If not in cache, try to load it
        var filePath = Path.Combine(_environment.ContentRootPath, TranslationsPath, $"{language}.json");
        
        if (!File.Exists(filePath))
        {
            _logger.LogError($"Translation file not found: {filePath}");
            return new Dictionary<string, object>();
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var translations = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
            
            if (translations != null)
            {
                _translationsCache[language] = translations;
                return translations;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading translation file: {filePath}");
        }

        return new Dictionary<string, object>();
    }

    public async Task<string?> GetTranslationAsync(string language, string key)
    {
        var translations = await GetTranslationsAsync(language);
        
        // Support nested keys like "common.welcome"
        var keys = key.Split('.');
        object? current = translations;
        
        foreach (var k in keys)
        {
            if (current is Dictionary<string, object> dict && dict.ContainsKey(k))
            {
                current = dict[k];
            }
            else if (current is JsonElement element)
            {
                if (element.TryGetProperty(k, out var property))
                {
                    current = property;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        
        return current?.ToString();
    }

    public string GetLanguageFromHeader(string? acceptLanguageHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            return DefaultLanguage;
        }

        // Parse Accept-Language header
        // Example: "az-AZ,az;q=0.9,en-US;q=0.8,en;q=0.7"
        var languages = acceptLanguageHeader
            .Split(',')
            .Select(lang =>
            {
                var parts = lang.Split(';');
                var code = parts[0].Trim();
                var quality = 1.0;
                
                if (parts.Length > 1 && parts[1].Trim().StartsWith("q="))
                {
                    double.TryParse(parts[1].Trim().Substring(2), out quality);
                }
                
                return new { Code = code, Quality = quality };
            })
            .OrderByDescending(x => x.Quality)
            .ToList();

        // Find the first supported language
        foreach (var lang in languages)
        {
            var langCode = lang.Code.Split('-')[0].ToLower();
            
            if (_supportedLanguages.Contains(langCode))
            {
                return langCode;
            }
        }

        return DefaultLanguage;
    }

    public List<string> GetSupportedLanguages()
    {
        return _supportedLanguages;
    }
}

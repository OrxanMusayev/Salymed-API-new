using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<LocalizationController> _logger;

    public LocalizationController(
        ILocalizationService localizationService,
        ILogger<LocalizationController> logger)
    {
        _localizationService = localizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all translations for the requested language
    /// Language is determined from Accept-Language header or query parameter
    /// </summary>
    /// <param name="lang">Optional language code (az, en, tr). If not provided, uses Accept-Language header</param>
    /// <returns>Dictionary of all translations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTranslations([FromQuery] string? lang = null)
    {
        try
        {
            // Determine language from query parameter or Accept-Language header
            var language = !string.IsNullOrWhiteSpace(lang)
                ? lang
                : _localizationService.GetLanguageFromHeader(Request.Headers["Accept-Language"]);

            _logger.LogInformation($"Fetching translations for language: {language}");

            var translations = await _localizationService.GetTranslationsAsync(language);

            return Ok(new
            {
                language,
                translations,
                supportedLanguages = _localizationService.GetSupportedLanguages()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching translations");
            return StatusCode(500, new { error = "Error fetching translations" });
        }
    }

    /// <summary>
    /// Get translations for a specific language
    /// </summary>
    /// <param name="language">Language code (az, en, tr)</param>
    /// <returns>Dictionary of all translations for the specified language</returns>
    [HttpGet("{language}")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTranslationsByLanguage(string language)
    {
        try
        {
            _logger.LogInformation($"Fetching translations for language: {language}");

            var translations = await _localizationService.GetTranslationsAsync(language);

            if (translations == null || translations.Count == 0)
            {
                return NotFound(new { error = $"Translations not found for language: {language}" });
            }

            return Ok(new
            {
                language,
                translations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching translations for language: {language}");
            return StatusCode(500, new { error = "Error fetching translations" });
        }
    }

    /// <summary>
    /// Get a specific translation by key
    /// </summary>
    /// <param name="language">Language code (az, en, tr)</param>
    /// <param name="key">Translation key (e.g., "common.welcome")</param>
    /// <returns>Translation value</returns>
    [HttpGet("{language}/{key}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTranslationByKey(string language, string key)
    {
        try
        {
            // Replace dashes with dots to support URL-friendly keys
            // e.g., "common-welcome" becomes "common.welcome"
            key = key.Replace("-", ".");

            _logger.LogInformation($"Fetching translation for language: {language}, key: {key}");

            var translation = await _localizationService.GetTranslationAsync(language, key);

            if (translation == null)
            {
                return NotFound(new { error = $"Translation not found for key: {key}" });
            }

            return Ok(new
            {
                language,
                key,
                value = translation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching translation for key: {key}");
            return StatusCode(500, new { error = "Error fetching translation" });
        }
    }

    /// <summary>
    /// Get list of supported languages
    /// </summary>
    /// <returns>List of supported language codes</returns>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedLanguages()
    {
        try
        {
            var languages = _localizationService.GetSupportedLanguages();
            
            return Ok(new
            {
                supportedLanguages = languages,
                defaultLanguage = "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching supported languages");
            return StatusCode(500, new { error = "Error fetching supported languages" });
        }
    }
}

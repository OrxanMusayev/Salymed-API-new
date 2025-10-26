# 🌐 Backend Localization System

A comprehensive localization system for the .NET backend that stores translations in JSON files and provides REST API endpoints to fetch translations based on the `Accept-Language` header.

## 📁 File Structure

```
backend/
├── Resources/
│   └── Translations/
│       ├── az.json          # Azerbaijani translations
│       ├── en.json          # English translations
│       └── tr.json          # Turkish translations
├── Services/
│   ├── ILocalizationService.cs
│   └── LocalizationService.cs
└── Controllers/
    └── LocalizationController.cs
```

## 🚀 Features

- ✅ Multi-language support (Azerbaijani, English, Turkish)
- ✅ Automatic language detection from `Accept-Language` header
- ✅ REST API endpoints for fetching translations
- ✅ In-memory caching for performance
- ✅ Nested translation keys support (e.g., `common.welcome`)
- ✅ Fallback to default language (English)
- ✅ Quality-based language preference parsing

## 📝 Translation JSON Structure

Each language file follows this structure:

```json
{
  "common": {
    "welcome": "Welcome",
    "save": "Save",
    "cancel": "Cancel"
  },
  "auth": {
    "login": "Login",
    "logout": "Logout"
  },
  "subscription": {
    "title": "Subscription",
    "plans": "Plans"
  }
}
```

## 🔌 API Endpoints

### 1. Get All Translations (Auto-detect Language)

**Endpoint:** `GET /api/localization`

**Description:** Fetches all translations based on the `Accept-Language` header.

**Request:**
```http
GET /api/localization
Accept-Language: az-AZ,az;q=0.9,en;q=0.8
```

**Response:**
```json
{
  "language": "az",
  "translations": {
    "common": {
      "welcome": "Xoş gəlmisiniz",
      "save": "Yadda saxla"
    },
    "auth": {
      "login": "Daxil ol"
    }
  },
  "supportedLanguages": ["az", "en", "tr"]
}
```

### 2. Get Translations by Language Query Parameter

**Endpoint:** `GET /api/localization?lang={languageCode}`

**Description:** Fetches all translations for a specific language.

**Request:**
```http
GET /api/localization?lang=tr
```

**Response:**
```json
{
  "language": "tr",
  "translations": {
    "common": {
      "welcome": "Hoş geldiniz",
      "save": "Kaydet"
    }
  },
  "supportedLanguages": ["az", "en", "tr"]
}
```

### 3. Get Translations by Language (Path Parameter)

**Endpoint:** `GET /api/localization/{language}`

**Description:** Fetches all translations for a specific language.

**Request:**
```http
GET /api/localization/en
```

**Response:**
```json
{
  "language": "en",
  "translations": {
    "common": {
      "welcome": "Welcome",
      "save": "Save"
    }
  }
}
```

### 4. Get Specific Translation by Key

**Endpoint:** `GET /api/localization/{language}/{key}`

**Description:** Fetches a specific translation value by key. Keys can use dots or dashes.

**Request:**
```http
GET /api/localization/az/common.welcome
# or
GET /api/localization/az/common-welcome
```

**Response:**
```json
{
  "language": "az",
  "key": "common.welcome",
  "value": "Xoş gəlmisiniz"
}
```

### 5. Get Supported Languages

**Endpoint:** `GET /api/localization/languages`

**Description:** Returns list of supported languages.

**Response:**
```json
{
  "supportedLanguages": ["az", "en", "tr"],
  "defaultLanguage": "en"
}
```

## 🛠️ Usage Examples

### From Angular/TypeScript Frontend

```typescript
import { HttpClient, HttpHeaders } from '@angular/common/http';

export class TranslationService {
  private apiUrl = 'http://localhost:5000/api/localization';

  constructor(private http: HttpClient) {}

  // Auto-detect language from browser
  getTranslations() {
    const headers = new HttpHeaders({
      'Accept-Language': navigator.language
    });
    return this.http.get(`${this.apiUrl}`, { headers });
  }

  // Get specific language
  getTranslationsByLanguage(lang: string) {
    return this.http.get(`${this.apiUrl}/${lang}`);
  }

  // Get specific translation
  getTranslation(lang: string, key: string) {
    return this.http.get(`${this.apiUrl}/${lang}/${key}`);
  }
}
```

### Using cURL

```bash
# Get translations with Accept-Language header
curl -H "Accept-Language: az-AZ,az;q=0.9" http://localhost:5000/api/localization

# Get English translations
curl http://localhost:5000/api/localization/en

# Get specific translation
curl http://localhost:5000/api/localization/az/common.welcome

# Get supported languages
curl http://localhost:5000/api/localization/languages
```

## 🔧 Configuration

The localization service is registered as a **Singleton** in `Program.cs`:

```csharp
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
```

This ensures translations are loaded once at startup and cached in memory for optimal performance.

## 📌 Accept-Language Header Parsing

The system supports standard `Accept-Language` header format with quality values:

```
Accept-Language: az-AZ,az;q=0.9,en-US;q=0.8,en;q=0.7,tr;q=0.6
```

The service will:
1. Parse all languages and their quality values
2. Sort by quality (highest first)
3. Return the first supported language
4. Fallback to English if no supported language is found

## 🎯 Adding New Languages

To add a new language:

1. Create a new JSON file in `Resources/Translations/` (e.g., `de.json`)
2. Add the language code to `_supportedLanguages` list in `LocalizationService.cs`:
   ```csharp
   private readonly List<string> _supportedLanguages = new() { "az", "en", "tr", "de" };
   ```
3. Restart the application

## 🔍 Translation Key Conventions

- Use dot notation for nested keys: `common.welcome`
- Group related translations: `auth.login`, `auth.logout`
- Keep keys descriptive and consistent across languages
- Use camelCase for consistency: `forgotPassword`, `rememberMe`

## 🚨 Error Handling

- If a language is not supported, it falls back to English
- If a translation file is missing, an empty object is returned
- If a translation key is not found, `null` is returned
- All errors are logged for debugging

## 📊 Performance

- **In-memory caching**: All translations are loaded at startup
- **Singleton service**: Single instance across the application
- **Fast lookups**: Dictionary-based key-value retrieval
- **Minimal I/O**: Files are read once at startup

## 🧪 Testing

You can test the localization endpoints using the provided `.http` files or Swagger UI at:

```
http://localhost:5000/swagger
```

## 📚 Related Documentation

- [Frontend Localization Guide](../../frontend/LOCALIZATION_GUIDE.md)
- [API Documentation](./README.md)

---

**Created:** October 23, 2025
**Last Updated:** October 23, 2025

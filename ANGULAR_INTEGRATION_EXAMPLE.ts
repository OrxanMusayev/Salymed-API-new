// Example Angular Service for Backend Localization Integration
// File: src/app/services/translation.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface TranslationResponse {
  language: string;
  translations: Record<string, any>;
  supportedLanguages?: string[];
}

export interface SupportedLanguagesResponse {
  supportedLanguages: string[];
  defaultLanguage: string;
}

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private apiUrl = 'http://localhost:5000/api/localization';
  private currentLanguage$ = new BehaviorSubject<string>('en');
  private translations$ = new BehaviorSubject<Record<string, any>>({});

  constructor(private http: HttpClient) {
    this.initializeLanguage();
  }

  /**
   * Initialize language from localStorage or browser settings
   */
  private initializeLanguage(): void {
    const savedLanguage = localStorage.getItem('language');
    const browserLanguage = navigator.language.split('-')[0];
    const language = savedLanguage || browserLanguage || 'en';
    
    this.loadTranslations(language);
  }

  /**
   * Load all translations for a specific language
   */
  loadTranslations(language: string): Observable<TranslationResponse> {
    return this.http.get<TranslationResponse>(`${this.apiUrl}/${language}`)
      .pipe(
        tap(response => {
          this.currentLanguage$.next(response.language);
          this.translations$.next(response.translations);
          localStorage.setItem('language', response.language);
        })
      );
  }

  /**
   * Get translations using Accept-Language header
   */
  getTranslationsWithHeader(): Observable<TranslationResponse> {
    const headers = new HttpHeaders({
      'Accept-Language': navigator.language
    });
    
    return this.http.get<TranslationResponse>(this.apiUrl, { headers })
      .pipe(
        tap(response => {
          this.currentLanguage$.next(response.language);
          this.translations$.next(response.translations);
        })
      );
  }

  /**
   * Get a specific translation by key
   */
  getTranslation(key: string, language?: string): Observable<any> {
    const lang = language || this.currentLanguage$.value;
    return this.http.get(`${this.apiUrl}/${lang}/${key.replace('.', '-')}`);
  }

  /**
   * Get supported languages
   */
  getSupportedLanguages(): Observable<SupportedLanguagesResponse> {
    return this.http.get<SupportedLanguagesResponse>(`${this.apiUrl}/languages`);
  }

  /**
   * Change current language
   */
  changeLanguage(language: string): void {
    this.loadTranslations(language).subscribe();
  }

  /**
   * Get current language as observable
   */
  getCurrentLanguage(): Observable<string> {
    return this.currentLanguage$.asObservable();
  }

  /**
   * Get current language value
   */
  getCurrentLanguageValue(): string {
    return this.currentLanguage$.value;
  }

  /**
   * Get translations as observable
   */
  getTranslations(): Observable<Record<string, any>> {
    return this.translations$.asObservable();
  }

  /**
   * Get translations value
   */
  getTranslationsValue(): Record<string, any> {
    return this.translations$.value;
  }

  /**
   * Translate a key using current translations
   */
  translate(key: string): string {
    const keys = key.split('.');
    let value: any = this.translations$.value;

    for (const k of keys) {
      if (value && typeof value === 'object' && k in value) {
        value = value[k];
      } else {
        return key; // Return key if translation not found
      }
    }

    return typeof value === 'string' ? value : key;
  }

  /**
   * Translate with parameters
   */
  translateWithParams(key: string, params: Record<string, any>): string {
    let translation = this.translate(key);
    
    Object.keys(params).forEach(param => {
      translation = translation.replace(`{${param}}`, params[param]);
    });
    
    return translation;
  }
}

// ============================================
// Example Component Usage
// ============================================

/*
import { Component, OnInit } from '@angular/core';
import { TranslationService } from './services/translation.service';

@Component({
  selector: 'app-language-switcher',
  template: `
    <div class="language-switcher">
      <button *ngFor="let lang of supportedLanguages" 
              (click)="changeLanguage(lang)"
              [class.active]="currentLanguage === lang">
        {{ lang.toUpperCase() }}
      </button>
    </div>
    
    <div class="content">
      <h1>{{ translate('common.welcome') }}</h1>
      <button>{{ translate('common.save') }}</button>
      <button>{{ translate('common.cancel') }}</button>
    </div>
  `,
  styles: [`
    .language-switcher button {
      margin: 0 5px;
      padding: 10px 20px;
      cursor: pointer;
    }
    .language-switcher button.active {
      background-color: #007bff;
      color: white;
    }
  `]
})
export class LanguageSwitcherComponent implements OnInit {
  supportedLanguages: string[] = [];
  currentLanguage: string = 'en';

  constructor(private translationService: TranslationService) {}

  ngOnInit(): void {
    // Load supported languages
    this.translationService.getSupportedLanguages().subscribe(response => {
      this.supportedLanguages = response.supportedLanguages;
    });

    // Subscribe to current language
    this.translationService.getCurrentLanguage().subscribe(lang => {
      this.currentLanguage = lang;
    });

    // Load initial translations
    this.translationService.getTranslationsWithHeader().subscribe();
  }

  changeLanguage(language: string): void {
    this.translationService.changeLanguage(language);
  }

  translate(key: string): string {
    return this.translationService.translate(key);
  }
}
*/

// ============================================
// Example Pipe for Translation
// ============================================

/*
import { Pipe, PipeTransform } from '@angular/core';
import { TranslationService } from './services/translation.service';

@Pipe({
  name: 'translate',
  pure: false // Re-evaluate when language changes
})
export class TranslatePipe implements PipeTransform {
  constructor(private translationService: TranslationService) {}

  transform(key: string, params?: Record<string, any>): string {
    if (!params) {
      return this.translationService.translate(key);
    }
    return this.translationService.translateWithParams(key, params);
  }
}

// Usage in template:
// <h1>{{ 'common.welcome' | translate }}</h1>
// <p>{{ 'messages.greeting' | translate: {name: 'John'} }}</p>
*/

// ============================================
// App Module Registration
// ============================================

/*
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { TranslationService } from './services/translation.service';
import { TranslatePipe } from './pipes/translate.pipe';

@NgModule({
  declarations: [
    // ... your components
    TranslatePipe
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [
    TranslationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
*/

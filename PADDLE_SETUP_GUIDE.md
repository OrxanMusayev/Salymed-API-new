# Paddle Entegrasyonu Kurulum Rehberi

Bu rehber, Salymed projesi için Paddle recurring subscription entegrasyonunun nasıl kurulacağını açıklar.

## 1. Paddle Dashboard Kurulumu

### 1.1 Product Oluşturma

1. Paddle Dashboard'a giriş yapın: https://sandbox-vendors.paddle.com/ (test için)
2. **Catalog > Products** bölümüne gidin
3. **+ New Product** butonuna tıklayın
4. Her abonelik planı için bir product oluşturun:
   - **Basic Plan**
     - Name: Salymed Basic
     - Description: Klinikler için temel özellikler
   - **Professional Plan**
     - Name: Salymed Professional
     - Description: Orta ölçekli klinikler için gelişmiş özellikler
   - **Enterprise Plan**
     - Name: Salymed Enterprise
     - Description: Büyük klinikler için tam özellikler

### 1.2 Price (Fiyat) Oluşturma

Her product için recurring price oluşturun:

1. Product sayfasında **+ Add Price** butonuna tıklayın
2. Aşağıdaki bilgileri girin:
   - **Type**: Recurring (önemli!)
   - **Billing Cycle**: Monthly veya Annually
   - **Price**: Planın fiyatı (örn: $29, $99, $299)
   - **Currency**: USD
   - **Trial Period**: 1 month (isteğe bağlı - ilk ay ücretsiz için)
3. **Create Price** butonuna tıklayın
4. Oluşturulan price'ın ID'sini kopyalayın (örn: `pri_01hxxx...`)

### 1.3 Price ID'lerini Veritabanına Ekleyin

Price ID'leri aldıktan sonra, aşağıdaki SQL komutlarını çalıştırın:

```sql
-- Basic Plan için (ID = 1)
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hxxx...' -- Paddle'dan aldığınız gerçek Price ID
WHERE Id = 1;

-- Professional Plan için (ID = 2)
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hyyy...' -- Paddle'dan aldığınız gerçek Price ID
WHERE Id = 2;

-- Enterprise Plan için (ID = 3)
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hzzz...' -- Paddle'dan aldığınız gerçek Price ID
WHERE Id = 3;

-- Kontrol
SELECT Id, Name, Price, Currency, PaddlePriceId FROM SubscriptionPlans;
```

### 1.4 Webhook Kurulumu

1. **Developer Tools > Notifications** bölümüne gidin
2. **+ New Notification Destination** butonuna tıklayın
3. Aşağıdaki bilgileri girin:
   - **Destination URL**: `https://your-backend-url.com/api/PaddleWebhook`
     - Localhost için: ngrok veya benzer bir tunneling servisi kullanın
     - Örn: `https://abc123.ngrok.io/api/PaddleWebhook`
   - **Subscribe to Events**: Aşağıdaki event'leri seçin:
     - ✅ `transaction.completed`
     - ✅ `transaction.paid`
     - ✅ `transaction.payment_failed`
     - ✅ `subscription.created`
     - ✅ `subscription.updated`
     - ✅ `subscription.canceled`
4. **Create Destination** butonuna tıklayın

### 1.5 API Key Alma

1. **Developer Tools > Authentication** bölümüne gidin
2. Mevcut API key'inizi görün veya yeni bir tane oluşturun
3. API key'i kopyalayın ve güvenli bir yere kaydedin
4. `appsettings.json` dosyasına ekleyin:

```json
{
  "Paddle": {
    "ApiKey": "pdl_sdbx_apikey_xxxxxxx...",
    "Environment": "sandbox"
  }
}
```

## 2. Backend Konfigürasyonu

### 2.1 appsettings.json

```json
{
  "Paddle": {
    "ApiKey": "pdl_sdbx_apikey_xxxxxxx...",  // Paddle API key'iniz
    "Environment": "sandbox"  // Production için "production" yapın
  },
  "Frontend": {
    "Url": "http://localhost:4200"  // Frontend URL'iniz
  }
}
```

### 2.2 Program.cs

PaddleService'in doğru şekilde register edildiğinden emin olun:

```csharp
// Paddle settings
builder.Services.Configure<PaddleSettings>(builder.Configuration.GetSection("Paddle"));

// Paddle service
builder.Services.AddHttpClient<IPaddleService, PaddleService>();
```

## 3. Frontend Konfigürasyonu

### 3.1 environment.ts

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5208/api'
};
```

## 4. Test Etme

### 4.1 Test Kartları

Paddle sandbox'ta test yapmak için aşağıdaki test kartlarını kullanın:

**Başarılı Ödeme:**
- Card Number: 4242 4242 4242 4242
- Expiry: Gelecekte herhangi bir tarih
- CVC: Herhangi bir 3 haneli sayı

**Başarısız Ödeme:**
- Card Number: 4000 0000 0000 0002
- Expiry: Gelecekte herhangi bir tarih
- CVC: Herhangi bir 3 haneli sayı

### 4.2 Test Adımları

1. Backend'i çalıştırın: `dotnet run`
2. Frontend'i çalıştırın: `npm start`
3. Klinik kaydı yapın
4. Subscription selection sayfasında bir plan seçin
5. Paddle Overlay açılmalı
6. Test kartıyla ödeme yapın
7. Webhook'ların geldiğini backend loglarından kontrol edin
8. Dashboard'da subscription'ın aktif olduğunu kontrol edin

## 5. Production'a Geçiş

### 5.1 Paddle Production Hesabı

1. Paddle'da production hesabınızı aktif edin
2. Production product ve price'lar oluşturun
3. Production API key'i alın

### 5.2 Konfigürasyon Değişiklikleri

```json
{
  "Paddle": {
    "ApiKey": "pdl_live_apikey_xxxxxxx...",
    "Environment": "production"
  },
  "Frontend": {
    "Url": "https://your-production-domain.com"
  }
}
```

### 5.3 Webhook URL Güncelleme

Production webhook URL'ini Paddle dashboard'dan güncelleyin:
- `https://your-backend-domain.com/api/PaddleWebhook`

## 6. Önemli Notlar

### 6.1 Paddle Overlay

- Paddle Overlay otomatik olarak açılır ve kullanıcıyı ödeme sayfasına yönlendirir
- Frontend'de ekstra bir modal veya iframe eklemenize gerek yok
- Ödeme tamamlandığında kullanıcı otomatik olarak success_url'e yönlendirilir

### 6.2 Recurring Subscriptions

- Paddle otomatik olarak aylık/yıllık ödemeleri yönetir
- Her ödeme sonrası webhook gönderilir
- Subscription durumu webhook'lar ile güncellenir

### 6.3 Webhook Security

- Production'da webhook signature verification'ı mutlaka implement edin
- Şu anda sandbox modda signature validation skip ediliyor

### 6.4 Error Handling

- Backend loglarını düzenli olarak kontrol edin
- Paddle API hataları detaylı olarak loglanır
- Frontend'de kullanıcı dostu hata mesajları gösterilir

## 7. Troubleshooting

### Problem: Checkout URL boş geliyor

**Çözüm:**
- Price ID'lerin doğru olduğunu kontrol edin
- Price'ların "Recurring" tipinde olduğunu kontrol edin
- Backend loglarından Paddle API yanıtını kontrol edin

### Problem: Webhook'lar gelmiyor

**Çözüm:**
- Webhook URL'inin erişilebilir olduğunu kontrol edin (ngrok kullanıyorsanız)
- Paddle dashboard'da webhook event'lerinin seçildiğini kontrol edin
- Backend'in çalıştığını ve `/api/PaddleWebhook` endpoint'inin erişilebilir olduğunu kontrol edin

### Problem: Ödeme yapıldı ama subscription aktif olmadı

**Çözüm:**
- Webhook'ların gelip gelmediğini backend loglarından kontrol edin
- `transaction.paid` veya `transaction.completed` event'lerinin handle edildiğini kontrol edin
- Veritabanında subscription status'ünü manuel olarak kontrol edin

## 8. Kaynaklar

- [Paddle API Documentation](https://developer.paddle.com/api-reference/overview)
- [Paddle Checkout Guide](https://developer.paddle.com/build/checkout/build-overlay-checkout)
- [Paddle Webhooks Guide](https://developer.paddle.com/webhooks/overview)

# Paddle Entegrasyonu - Yapılan Değişiklikler ve Düzeltmeler

## 📋 Özet

Paddle recurring subscription entegrasyonunda tespit edilen sorunlar düzeltildi ve doğru implementasyon yapıldı.

## 🔧 Yapılan Değişiklikler

### 1. PaddleService.cs - Kritik Düzeltmeler ✅

**Önceki Sorunlar:**
- ❌ Yanlış endpoint kullanılıyordu: `/subscriptions`
- ❌ Request formatı hatalıydı
- ❌ Error handling eksikti

**Düzeltmeler:**
- ✅ Doğru endpoint kullanıldı: `/transactions` (Paddle Checkout için)
- ✅ Doğru request formatı uygulandı:
  ```json
  {
    "items": [{"price_id": "pri_xxx", "quantity": 1}],
    "customer_email": "user@example.com",
    "custom_data": {"clinic_id": 1, "plan_id": 1, "user_id": 1},
    "checkout": {
      "url": "success_url",
      "settings": {"success_url": "..."}
    }
  }
  ```
- ✅ Detaylı error handling eklendi
- ✅ Paddle error response parsing eklendi
- ✅ Checkout URL validation eklendi

### 2. Price ID vs Product ID - Kritik Düzeltme ✅

**Önceki Sorun:**
- ❌ `pro_01k7akgks94hd1pnz42f76z0a6` (Product ID) kullanılıyordu

**Düzeltme:**
- ✅ `pri_01hxxx...` (Price ID) kullanılmalı
- ✅ Yeni SQL dosyası oluşturuldu: `update_paddle_price_ids.sql`
- ✅ Price ID formatı validation eklendi

### 3. Dokümantasyon ✅

**Yeni Dosyalar:**
- ✅ `PADDLE_SETUP_GUIDE.md` - Detaylı kurulum rehberi
- ✅ `update_paddle_price_ids.sql` - Price ID güncelleme script'i
- ✅ `PADDLE_CHANGES_SUMMARY.md` - Bu dosya

## 🎯 Akış Diagramı

```
1. Kullanıcı Plan Seçer (Frontend)
   ↓
2. Frontend → Backend: POST /api/payment/create-checkout
   {
     planId: 1,
     customerEmail: "user@example.com",
     clinicId: 123,
     userId: 456
   }
   ↓
3. Backend → Paddle API: POST /transactions
   {
     items: [{price_id: "pri_xxx", quantity: 1}],
     customer_email: "user@example.com",
     custom_data: {...},
     checkout: {settings: {success_url: "..."}}
   }
   ↓
4. Paddle API → Backend: Response
   {
     data: {
       id: "txn_xxx",
       checkout: {url: "https://sandbox-buy.paddle.com/..."}
     }
   }
   ↓
5. Backend → Frontend: Response
   {
     success: true,
     checkoutUrl: "https://sandbox-buy.paddle.com/...",
     transactionId: "txn_xxx"
   }
   ↓
6. Frontend: window.location.href = checkoutUrl
   ↓
7. Paddle Overlay Açılır (Kullanıcı ödeme yapar)
   ↓
8. Paddle → Backend: Webhook (transaction.paid)
   ↓
9. Backend: Subscription Status = Active
   ↓
10. Kullanıcı success_url'e yönlendirilir
```

## 📝 Paddle Dashboard'da Yapılması Gerekenler

### 1. Product Oluşturma
- Catalog > Products > + New Product
- Her plan için ayrı product oluştur (Basic, Professional, Enterprise)

### 2. Price Oluşturma (ÖNEMLİ!)
- Her product için **Recurring** price oluştur
- Billing Cycle: Monthly veya Annually
- Trial Period: 1 month (opsiyonel)
- Price ID'yi kopyala (örn: `pri_01hxxx...`)

### 3. Price ID'leri Veritabanına Ekle
```sql
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01hxxx...' WHERE Id = 1;
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01hyyy...' WHERE Id = 2;
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01hzzz...' WHERE Id = 3;
```

### 4. Webhook Kurulumu
- Developer Tools > Notifications
- Destination URL: `https://your-backend.com/api/PaddleWebhook`
- Events:
  - transaction.completed
  - transaction.paid
  - transaction.payment_failed
  - subscription.created
  - subscription.updated
  - subscription.canceled

### 5. API Key
- Developer Tools > Authentication
- API Key'i `appsettings.json`'a ekle

## 🧪 Test Kartları (Sandbox)

**Başarılı Ödeme:**
```
Card: 4242 4242 4242 4242
Expiry: 12/25
CVC: 123
```

**Başarısız Ödeme:**
```
Card: 4000 0000 0000 0002
Expiry: 12/25
CVC: 123
```

## ✅ Kontrol Listesi

Backend:
- [x] PaddleService doğru endpoint kullanıyor (`/transactions`)
- [x] Request formatı doğru (items array with price_id)
- [x] Error handling eksiksiz
- [x] appsettings.json'da Paddle ayarları mevcut
- [x] Program.cs'de PaddleSettings register edilmiş
- [x] PaymentController checkout endpoint'i doğru çalışıyor
- [x] Webhook controller event'leri handle ediyor

Frontend:
- [x] SubscriptionService createCheckout metodu doğru
- [x] Component checkout URL'e yönlendiriyor (window.location.href)
- [x] environment.ts'de API URL doğru

Paddle Dashboard:
- [ ] Products oluşturuldu (Basic, Professional, Enterprise)
- [ ] Her product için Recurring price oluşturuldu
- [ ] Price ID'ler veritabanına eklendi (`pri_` ile başlayan)
- [ ] Webhook destination eklendi
- [ ] Gerekli event'ler subscribe edildi

## 🚀 Test Adımları

1. Backend'i çalıştır: `cd backend && dotnet run`
2. Frontend'i çalıştır: `cd frontend && npm start`
3. Klinik kaydı yap
4. Subscription selection sayfasında plan seç
5. Paddle Overlay açılmalı
6. Test kartıyla ödeme yap
7. Success sayfasına yönlendirilmeli
8. Backend loglarından webhook'ları kontrol et
9. Veritabanından subscription status'ü kontrol et

## 📚 Detaylı Dokümantasyon

Detaylı kurulum talimatları için `PADDLE_SETUP_GUIDE.md` dosyasına bakın.

## ⚠️ Önemli Notlar

1. **Price ID vs Product ID**: Mutlaka `pri_` ile başlayan Price ID kullanın, `pro_` ile başlayan Product ID değil!

2. **Recurring Subscription**: Price oluştururken mutlaka **Recurring** seçeneğini seçin, One-time değil!

3. **Webhook URL**: Localhost test için ngrok veya benzeri tunnel servisi kullanın.

4. **Trial Period**: İlk ay ücretsiz için price oluştururken trial period ekleyin.

5. **Sandbox vs Production**: Test için sandbox kullanın, production'a geçerken API key ve environment'ı güncelleyin.

## 🐛 Sorun Giderme

Sorunlarla karşılaşırsanız `PADDLE_SETUP_GUIDE.md` dosyasındaki **Troubleshooting** bölümüne bakın.

---

**Son Güncelleme:** 13 Ekim 2025
**Durum:** ✅ Düzeltmeler tamamlandı

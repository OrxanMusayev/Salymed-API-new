# ⚠️ PADDLE DOMAIN APPROVAL HATASI - HIZLI ÇÖZÜM

## 📋 Hata
```
transaction_checkout_url_domain_is_not_approved
```

## 🚀 Hızlı Çözüm (3 Seçenek)

### ✅ Seçenek 1: Paddle Dashboard'da Domain Onaylama (ÖNERİLEN)

1. https://sandbox-vendors.paddle.com/ 
2. **Settings** > **Checkout Settings** > **Checkout domains**
3. `http://localhost:4200` ekleyin
4. 2-3 dakika bekleyin
5. Tekrar test edin

### ✅ Seçenek 2: success_url'i Kaldırma (GEÇİCİ)

`PaddleService.cs` dosyasında (satır ~74):

**Şu anki kod:**
```csharp
checkout = new
{
    settings = new
    {
        success_url = request.SuccessUrl,
    }
}
```

**Şuna değiştirin:**
```csharp
// checkout bölümünü tamamen kaldırın
```

Yani `checkoutData` nesnesinden `checkout` property'sini çıkarın.

### ✅ Seçenek 3: Boş checkout (TEST İÇİN)

```csharp
checkout = new { }
```

## 🎯 Hangi Seçeneği Kullanmalıyım?

| Seçenek | Ne Zaman | Avantaj | Dezavantaj |
|---------|----------|---------|------------|
| **1. Domain Onaylama** | Production'a yakın | En güvenli, production için gerekli | 2-3 dakika bekleme |
| **2. success_url kaldırma** | Hızlı test | Anında çalışır | Kullanıcı nereye gider bilmiyoruz |
| **3. Boş checkout** | Hızlı test | Anında çalışır | Paddle default kullanır |

## 📝 Düzeltme Adımları (Seçenek 2)

```bash
# Backend klasörüne git
cd "/Users/orxanmusayev/Projects - Important/Salymed/backend"

# Dosyayı aç
nano Services/PaddleService.cs
# veya
code Services/PaddleService.cs
```

**73-80 satırlar arası şöyle olmalı:**
```csharp
custom_data = new
{
    clinic_id = request.ClinicId,
    plan_id = request.PlanId,
    user_id = request.UserId
}
// checkout kısmını tamamen sil
```

**Backend'i yeniden başlat:**
```bash
dotnet run
```

## ✅ Test

Düzeltme sonrası Paddle'dan şu response'u almalısınız:

```json
{
  "data": {
    "id": "txn_xxx",
    "checkout": {
      "url": "https://sandbox-buy.paddle.com/checkout/..."
    }
  }
}
```

## 📚 Detaylı Dokümantasyon

- `PADDLE_API_TEST.md` - Debug ve test rehberi
- `PADDLE_DOMAIN_APPROVAL_FIX.md` - Domain onaylama detayları
- `PADDLE_SETUP_GUIDE.md` - Tam kurulum rehberi

## 🆘 Hala Çalışmıyor?

1. Backend loglarını kontrol edin
2. Paddle API response'unu kontrol edin
3. Price ID'lerin doğru olduğunu kontrol edin (`pri_` ile başlamalı)
4. Paddle API key'in geçerli olduğunu kontrol edin

---
**Son Güncelleme:** 13 Ekim 2025  
**Durum:** ✅ Çözüm hazır

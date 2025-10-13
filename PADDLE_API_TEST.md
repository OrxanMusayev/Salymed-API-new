# Paddle API Hata Çözümü - Domain Onayı

## 🔴 Aldığınız Hata

```json
{
  "error": {
    "type": "request_error",
    "code": "transaction_checkout_url_domain_is_not_approved",
    "detail": "The value you passed for `checkout.url` does not contain a domain that has been approved by Paddle"
  }
}
```

## ✅ Kod Düzeltmesi YAPILDI

`checkout.url` parametresi zaten koddan kaldırılmış durumda. Şu anki kod yapısı:

```csharp
checkout = new
{
    settings = new
    {
        success_url = request.SuccessUrl,
    }
}
```

## 🔧 Şimdi Yapmanız Gerekenler

### Seçenek 1: Domain Onaylama (En İyisi)

1. **Paddle Sandbox Dashboard**'a gidin: https://sandbox-vendors.paddle.com/
2. Sol menüden **Settings** veya **Checkout** seçeneğine tıklayın
3. **Checkout domains** veya **Approved domains** bölümünü bulun
4. **Add domain** butonuna tıklayın
5. Şu domain'i ekleyin: `http://localhost:4200`
6. Save/Submit yapın
7. 2-3 dakika bekleyin (değişikliğin aktif olması için)
8. Tekrar test edin

### Seçenek 2: Alternatif - success_url'i de kaldırma (ÖNERİLMEZ)

Eğer yukarıdaki çözüm işe yaramazsa, geçici olarak success_url'i de kaldırabilirsiniz:

```csharp
checkout = new
{
    // settings bölümünü tamamen kaldır
}
```

Ancak bu durumda kullanıcı ödeme sonrası nereye yönlendirilecek bilemeyiz.

## 🎯 Hızlı Test

Backend'i yeniden başlatın ve tekrar deneyin:

```bash
cd backend
dotnet run
```

## 📋 Paddle Dashboard Adımları (Detaylı)

### Sandbox için:

1. https://sandbox-vendors.paddle.com/ → Login
2. Sol menüden **Checkout** → **Checkout Settings**
3. Veya **Developer Tools** → **Checkout**
4. "Checkout domains" veya "Approved checkout URLs" başlığını arayın
5. Eğer bulamazsanız, **Support**'a ticket açın ve şunu sorun:
   - "How do I approve localhost domain for checkout URLs in sandbox?"

### Alternatif: Paddle Support

Eğer dashboard'da bu ayarı bulamazsanız:
- support@paddle.com'a mail atın
- Sandbox hesabınız için `http://localhost:4200` domain'ini onaylamalarını isteyin

## ⚠️ Önemli Not

Paddle'ın yeni API versiyonunda (Paddle Billing), bazı ayarlar farklı yerlerde olabilir:
- **Classic Paddle**: Checkout settings altında
- **Paddle Billing**: Settings > Checkout > Domains

Hangi versiyonu kullandığınızı kontrol edin.

## 🔍 Debug için

Backend'i çalıştırdığınızda loglarda şunu göreceksiniz:

```
Sending transaction request to Paddle: {
  "items": [...],
  "checkout": {
    "settings": {
      "success_url": "http://localhost:4200/payment-success"
    }
  }
}
```

Eğer hala aynı hatayı alıyorsanız, bu domain (`http://localhost:4200`) Paddle'da onaylanmamış demektir.

## ✅ Başarı Senaryosu

Domain onaylandıktan sonra Paddle'dan şöyle bir response almalısınız:

```json
{
  "data": {
    "id": "txn_01hxxx...",
    "status": "ready",
    "checkout": {
      "url": "https://sandbox-buy.paddle.com/checkout/..."
    }
  }
}
```

Bu URL'i frontend'e gönderip kullanıcıyı yönlendiriyorsunuz.

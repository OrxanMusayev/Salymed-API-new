# Paddle Domain Onay Hatası Çözümü

## ❌ Hata Mesajı

```json
{
  "error": {
    "type": "request_error",
    "code": "transaction_checkout_url_domain_is_not_approved",
    "detail": "The value you passed for `checkout.url` does not contain a domain that has been approved by Paddle"
  }
}
```

## 🔍 Sorun

Paddle API'ye gönderilen `checkout.url` (success_url) domain'i Paddle dashboard'da onaylanmamış.

## ✅ Çözüm 1: Domain Onaylama (Önerilen)

### Adım 1: Paddle Dashboard'a Giriş

1. https://sandbox-vendors.paddle.com/ adresine gidin
2. Hesabınıza giriş yapın

### Adım 2: Domain Onaylama

1. **Settings** > **Checkout Settings** bölümüne gidin
2. **Checkout domains** veya **Approved domains** seçeneğini bulun
3. **+ Add Domain** butonuna tıklayın
4. Aşağıdaki domain'leri ekleyin:
   - `http://localhost:4200` (Development için)
   - `https://yourdomain.com` (Production için, varsa)
5. **Save** butonuna tıklayın

### Adım 3: Test

Domain onaylandıktan sonra (birkaç dakika sürebilir) tekrar deneyin.

## ✅ Çözüm 2: Geçici - Checkout URL Kaldırma

Eğer domain onaylama mümkün değilse, geçici olarak `checkout.url` parametresini kaldırabilirsiniz.

### Backend Değişikliği

`PaddleService.cs` dosyasında `checkout.url` parametresini kaldırın:

**Önceki:**
```csharp
checkout = new
{
    url = request.SuccessUrl,  // <-- Bu satırı kaldırın
    settings = new
    {
        success_url = request.SuccessUrl,
    }
}
```

**Yeni:**
```csharp
checkout = new
{
    settings = new
    {
        success_url = request.SuccessUrl,
    }
}
```

Not: `checkout.url` ve `checkout.settings.success_url` farklı parametrelerdir:
- `checkout.url`: Checkout başladığında kullanıcının yönlendirileceği URL (opsiyonel)
- `checkout.settings.success_url`: Ödeme tamamlandıktan sonra yönlendirileceği URL

## 🎯 Önerilen Çözüm

**Çözüm 1**'i kullanmanızı öneririz çünkü:
- Production'da mutlaka domain onayı gerekecek
- Daha güvenli
- Paddle tarafından önerilen yöntem

## 📝 Hızlı Düzeltme Komutu

Backend'de hızlı düzeltme için aşağıdaki değişikliği yapabilirsiniz:

```bash
# PaddleService.cs'i düzenleyin ve checkout.url satırını kaldırın
```

## ✅ Kontrol Listesi

- [ ] Paddle Dashboard > Settings > Checkout Settings
- [ ] Approved Domains listesine `http://localhost:4200` eklendi
- [ ] Değişiklik kaydedildi
- [ ] 2-3 dakika beklendi (propagation için)
- [ ] Tekrar test edildi

## 🔗 Kaynaklar

- [Paddle Domain Approval Documentation](https://developer.paddle.com/v1/errors/transactions/transaction_checkout_url_domain_is_not_approved)
- [Paddle Checkout Settings](https://developer.paddle.com/build/checkout/build-overlay-checkout)

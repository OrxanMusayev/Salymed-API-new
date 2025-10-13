# Salymed Backend API

.NET 9 Web API projesi - Salymed uygulamasının backend kısmı.

## 🚀 Kurulum

```bash
# Projeyi klonla
git clone <repository-url>
cd salymed-backend

# Paketleri yükle
dotnet restore

# Veritabanı migration'larını çalıştır
dotnet ef database update

# Uygulamayı başlat
dotnet run
```

## 🔧 Teknolojiler

- .NET 9
- Entity Framework Core
- SQL Server
- ASP.NET Core Web API
- Swagger/OpenAPI

## 📝 API Endpoints

### Authentication
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/logout` - Kullanıcı çıkışı

### Clinics
- `GET /api/clinics` - Tüm klinikleri listele
- `GET /api/clinics/{id}` - Klinik detayı
- `POST /api/clinics` - Yeni klinik oluştur
- `PUT /api/clinics/{id}` - Klinik güncelle
- `DELETE /api/clinics/{id}` - Klinik sil

## 🗄️ Veritabanı

SQL Server kullanılmaktadır. Connection string `appsettings.json` dosyasında yapılandırılabilir.

## 🔧 Yapılandırma

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SalymedDB;User Id=sa;Password=MyStrongPassword123;TrustServerCertificate=True;"
  }
}
```
## 💳 Paddle Payment Integration

Salymed projesi Paddle recurring subscription entegrasyonu kullanmaktadır.

### 🆘 Domain Approval Hatası Aldınız mı?

Eğer şu hatayı alıyorsanız:
```
transaction_checkout_url_domain_is_not_approved
```

**Hızlı Çözüm:** `README_DOMAIN_ERROR_FIX.md` dosyasına bakın.

### 📚 Paddle Dokümantasyonu

- **`README_DOMAIN_ERROR_FIX.md`** - Domain onay hatası çözümü (başlangıç için)
- **`PADDLE_SETUP_GUIDE.md`** - Detaylı Paddle kurulum rehberi
- **`PADDLE_CHANGES_SUMMARY.md`** - Yapılan değişikliklerin özeti
- **`PADDLE_API_TEST.md`** - API test ve debug rehberi
- **`PADDLE_DOMAIN_APPROVAL_FIX.md`** - Domain onaylama detayları

### ⚡ Hızlı Başlangıç

1. Paddle Dashboard'da product ve price oluşturun
2. Price ID'leri `update_paddle_price_ids.sql` ile veritabanına ekleyin
3. `appsettings.json`'da Paddle API key'inizi ekleyin
4. Domain onay hatası için `README_DOMAIN_ERROR_FIX.md`'e bakın
5. Backend'i başlatın: `dotnet run`

### 🔑 Paddle Ayarları

`appsettings.json`:
```json
{
  "Paddle": {
    "ApiKey": "pdl_sdbx_apikey_xxx...",
    "Environment": "sandbox"
  },
  "Frontend": {
    "Url": "http://localhost:4200"
  }
}
```

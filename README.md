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
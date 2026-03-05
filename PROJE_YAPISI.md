# Sertifika Olusturma ve Yonetim Sistemi

## Proje Yapisi

```
Sertifika/
├── Sertifika.slnx                    # Solution dosyasi
├── CLAUDE.md                         # AI asistan kurallari
├── PROJE_YAPISI.md                   # Bu dosya
├── PdfService/                       # Python PDF servisi
│   ├── main.py                       # FastAPI uygulama
│   ├── pdf_generator.py              # PDF uretim motoru
│   ├── requirements.txt              # Python bagimliliklari
│   └── fonts/                        # Ozel TTF fontlar
└── Sertifika/                        # .NET Web API projesi
    ├── Context/
    │   └── AppDbContext.cs            # EF Core DbContext
    ├── Controllers/
    │   ├── AuthController.cs          # JWT giris/kayit
    │   ├── CategoriesController.cs    # Kategori CRUD
    │   ├── CertificatesController.cs  # Sertifika CRUD
    │   ├── CompaniesController.cs     # Firma CRUD
    │   ├── HoldersController.cs       # Sahip CRUD
    │   ├── ParticipantsController.cs  # Katilimci CRUD
    │   ├── SignaturesController.cs    # Imza CRUD + dosya yukleme
    │   ├── TemplatesController.cs     # Sablon CRUD + arka plan
    │   ├── TrainingsController.cs     # Egitim + uretim + dagitim
    │   └── VerifyController.cs        # Public sertifika dogrulama
    ├── DependencyInjection/
    │   └── ServiceRegistration.cs     # DI kayitlari
    ├── Entities/
    │   ├── BaseEntity.cs              # Id, CreatedAt, UpdatedAt, IsActive
    │   ├── Category.cs
    │   ├── Certificate.cs
    │   ├── CertificateTemplate.cs     # Sablon + LayoutJson
    │   ├── Company.cs
    │   ├── Holder.cs
    │   ├── Participant.cs
    │   ├── Signature.cs
    │   ├── TemplateField.cs           # Layout JSON alanları (serialize)
    │   ├── Training.cs
    │   ├── TrainingSignature.cs
    │   └── User.cs                    # Admin, CertificateCreator, Viewer
    ├── EntityServices/                # Veritabani islemleri
    │   ├── I*EntityService.cs         # Interfaceler
    │   └── *EntityService.cs          # Implementasyonlar
    ├── Factories/                     # Is mantigi katmani
    │   ├── Auth/                      # Login/Register
    │   ├── Categories/
    │   ├── CertificateGeneration/     # PDF uretim orchestration
    │   ├── Certificates/
    │   ├── Companies/
    │   ├── Distribution/              # E-posta dagitim
    │   ├── Holders/
    │   ├── Participants/
    │   ├── Signatures/
    │   ├── Templates/
    │   └── Trainings/
    ├── Infrastructure/
    │   ├── IUnitOfWork.cs
    │   └── UnitOfWork.cs
    ├── Services/
    │   ├── EmailService.cs            # SMTP e-posta
    │   ├── JwtService.cs              # JWT token uretimi
    │   ├── OneDriveService.cs         # OneDrive arsivleme
    │   └── PdfService.cs              # Python PDF servisi istemcisi
    ├── wwwroot/
    │   ├── index.html                 # Ana sayfa (SPA)
    │   ├── verify.html                # Public dogrulama sayfasi
    │   ├── css/style.css
    │   ├── js/api.js                  # API istemci
    │   ├── js/app.js                  # Uygulama mantigi
    │   ├── js/editor.js               # Sablon editoru (drag & drop)
    │   └── uploads/                   # Yuklenen dosyalar
    │       ├── backgrounds/
    │       ├── certificates/
    │       └── signatures/
    ├── appsettings.json               # Yapilandirma
    └── Program.cs                     # Uygulama giris noktasi
```

## Mimari

```
Controller → Factory → EntityService → DbContext
                ↓
           UnitOfWork (SaveChanges)
```

**Kural:** Controller ASLA DbContext'e erisemez. Sadece Factory inject edilir.

## Teknolojiler

| Katman | Teknoloji |
|--------|-----------|
| Backend | .NET 9.0 Web API |
| Veritabani | PostgreSQL + EF Core 9.0 |
| Kimlik Dogrulama | JWT Bearer (8 saat) |
| Sifre Hash | BCrypt |
| PDF Uretim | Python FastAPI + ReportLab |
| Frontend | Vanilla HTML/CSS/JS |
| Bulut Arsiv | OneDrive (Microsoft Graph API) |
| E-posta | SMTP |

## Roller

| Rol | Yetki |
|-----|-------|
| Admin | Tum islemler |
| CertificateCreator | Sablon, egitim, sertifika yonetimi |
| Viewer | Sadece goruntuleme |

## API Endpointleri

### Auth
- `POST /api/auth/login` - Giris
- `POST /api/auth/register` - Kayit (Admin)
- `GET /api/auth/me` - Mevcut kullanici

### CRUD Endpointleri
- `/api/categories` - Kategori CRUD
- `/api/certificates` - Sertifika CRUD
- `/api/companies` - Firma CRUD
- `/api/holders` - Sahip CRUD
- `/api/templates` - Sablon CRUD
- `/api/signatures` - Imza CRUD (dosya yukleme)
- `/api/trainings` - Egitim CRUD

### Katilimci
- `GET /api/trainings/{id}/participants` - Katilimci listesi
- `POST /api/trainings/{id}/participants` - Katilimci ekle
- `POST /api/trainings/{id}/participants/import-excel` - Excel import

### Sertifika Uretim
- `POST /api/trainings/{id}/generate` - Toplu sertifika uretimi
- `GET /api/trainings/{id}/preview` - Sertifika onizleme
- `GET /api/trainings/{id}/download-zip` - ZIP indirme

### Dagitim
- `POST /api/trainings/{id}/send-certificates` - Katilimcilara e-posta
- `POST /api/trainings/{id}/send-to-contact` - Yetkiliye gonderim
- `POST /api/trainings/{id}/archive-onedrive` - OneDrive arsivleme

### Public
- `GET /api/certificates/verify/{certificateNumber}` - Sertifika dogrulama

## Kurulum

### Gereksinimler
- .NET 9.0 SDK
- PostgreSQL
- Python 3.10+

### 1. Veritabani
```bash
# appsettings.json'da ConnectionString duzenleyin
# Uygulama baslatildiginda auto-migration calisir
```

### 2. .NET API
```bash
cd Sertifika
dotnet restore
dotnet run
# http://localhost:5000 adresinde calisir
```

### 3. Python PDF Servisi
```bash
cd PdfService
pip install -r requirements.txt
python main.py
# http://127.0.0.1:5050 adresinde calisir
```

### 4. Varsayilan Admin
- E-posta: admin@sertifika.com
- Sifre: admin123

## Windows Server Deployment

### IIS ile .NET API
1. .NET 9.0 Hosting Bundle yukleyin
2. `dotnet publish -c Release -o C:\inetpub\sertifika`
3. IIS'te yeni site ekleyin, fiziksel yol: C:\inetpub\sertifika
4. Application Pool: No Managed Code

### Python PDF Servisi (Windows Service)
1. Python 3.10+ yukleyin
2. `pip install -r requirements.txt`
3. NSSM ile servis olarak kaydedin:
```cmd
nssm install SertifikaPdfService python.exe C:\sertifika\PdfService\main.py
nssm set SertifikaPdfService AppDirectory C:\sertifika\PdfService
nssm start SertifikaPdfService
```

### appsettings.json Yapilandirmasi
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SertifikaDB;Username=postgres;Password=GUCLU_SIFRE"
  },
  "Jwt": {
    "Key": "EN_AZ_32_KARAKTER_UZUNLUGUNDA_GUCLU_ANAHTAR",
    "Issuer": "SertifikaApp",
    "Audience": "SertifikaApp"
  },
  "Smtp": {
    "Host": "smtp.sirketiniz.com",
    "Port": "587",
    "Username": "noreply@sirketiniz.com",
    "Password": "SMTP_SIFRESI",
    "EnableSsl": "true",
    "From": "noreply@sirketiniz.com",
    "FromName": "Sertifika Sistemi"
  },
  "OneDrive": {
    "TenantId": "AZURE_TENANT_ID",
    "ClientId": "AZURE_CLIENT_ID",
    "ClientSecret": "AZURE_CLIENT_SECRET",
    "DriveUserId": "ONEDRIVE_KULLANICI_ID"
  },
  "PdfService": {
    "BaseUrl": "http://127.0.0.1:5050"
  }
}
```

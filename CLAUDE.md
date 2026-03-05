# Sertifika Projesi

## Zorunlu: Her Session Basinda

**Her session basinda asagidaki API'yi MUTLAKA oku ve uygula:**

```bash
curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"
curl -s "http://127.0.0.1:41847/api/projects/62/patterns"
```

Bu API'den gelen pattern'leri ve kurallari oku, her zaman uygula. Yeni ogrenilenler oldugunda pattern olarak kaydet.

## Proje Yapisi

Tek .NET 9.0 Web API projesi (ayri sunucu yok):

```
Sertifika/
├── Sertifika.slnx
└── Sertifika/
    ├── Entities/          # Domain modelleri (BaseEntity, Certificate, Holder, Category)
    ├── Context/           # AppDbContext (EF Core + PostgreSQL)
    ├── EntityServices/    # Veri erisim katmani (DbContext burada)
    ├── Factories/         # Is mantigi katmani (EntityService + UnitOfWork kullanir)
    ├── Infrastructure/    # IUnitOfWork, UnitOfWork
    ├── DependencyInjection/ # DI registrations
    ├── Controllers/       # API Controller'lari (DbContext KULLANMAZ, Factory inject eder)
    └── Program.cs
```

## Mimari Kurallar

- **Controller'da DbContext OLMAZ** — Controller sadece Factory inject eder
- **Factory** is mantigi katmani — EntityService + UnitOfWork inject eder
- **EntityService** veri erisim katmani — DbContext sadece burada kullanilir
- **UnitOfWork** SaveChanges islemlerini yonetir
- Silme islemleri soft delete (IsActive = false)
- Tum entity'ler BaseEntity'den turetilir

## Teknoloji

| Katman | Teknoloji |
|--------|-----------|
| Framework | .NET 9.0 |
| API | ASP.NET Core Web API |
| Veritabani | PostgreSQL |
| ORM | Entity Framework Core 9.0 |
| PDF Servisi | Python (ayri servis, Docker yok, direkt calisir) |

## API Endpoint'leri

- GET/POST /api/certificates
- GET/PUT/DELETE /api/certificates/{id}
- GET /api/certificates/holder/{holderId}
- GET /api/certificates/category/{categoryId}
- GET/POST /api/holders
- GET/PUT/DELETE /api/holders/{id}
- GET/POST /api/categories
- GET/PUT/DELETE /api/categories/{id}

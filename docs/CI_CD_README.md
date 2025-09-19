# CafeMenu CI/CD Pipeline

Bu belge CafeMenu uygulaması için Dev-Test-Prod ortamlarını yöneten CI/CD pipeline'ının kurulum ve kullanımını açıklar.

## Ortam Yapısı

1. **Development (Dev)** - `dev` branch
   - Geliştirme ortamı
   - Otomatik build ve test
   - Debug modda çalışır

2. **Test** - `test` branch
   - Test ortamı
   - Otomatik deployment
   - Release modda çalışır

3. **Production (Prod)** - `main` branch
   - Canlı ortam
   - Manuel onay gerektirir
   - Release modda çalışır

## CI/CD Pipeline Yapısı

### GitHub Actions Workflows

- `.github/workflows/dev.yml` - Development pipeline
- `.github/workflows/test.yml` - Test pipeline
- `.github/workflows/prod.yml` - Production pipeline

### Ortam Yapılandırmaları

Her ortam için ayrı config dosyaları:
- `App.Dev.config` - Development ayarları
- `App.Test.config` - Test ayarları
- `App.Prod.config` - Production ayarları

### Deployment Scriptleri

- `deploy/deploy-dev.ps1` - Development deployment
- `deploy/deploy-test.ps1` - Test deployment
- `deploy/deploy-prod.ps1` - Production deployment

## Kurulum Talimatları

### 1. GitHub Repository Ayarları

1. Repository'de aşağıdaki branch'leri oluşturun:
   - `dev` (development)
   - `test` (testing)
   - `main` (production)

2. Branch protection kurallarını ayarlayın:
   - `main`: Require pull request reviews
   - `test`: Require pull request reviews
   - `dev`: Direct push izni (geliştiriciler için)

### 2. Ortam Değişkenleri

Her ortam için aşağıdaki ayarlamaları yapın:

#### Development
- Connection string: LocalDB kullanır
- Debug mode: Açık
- Detailed errors: Açık

#### Test
- Connection string: Test veritabanı
- Debug mode: Kapalı
- Detailed errors: Sadece remote

#### Production
- Connection string: Production veritabanı
- Debug mode: Kapalı
- Detailed errors: Kapalı

## Kullanım Talimatları

### 1. Development Workflow

1. `dev` branch'inde yeni özellik geliştirin
2. Commit ve push yapın
3. GitHub Actions otomatik olarak build ve test yapar
4. Uygulama development sunucusuna deploy edilir

### 2. Test'e Yükseltme

1. `dev` branch'inden `test` branch'ine pull request oluşturun
2. Code review ve testler tamamlandıktan sonra merge yapın
3. GitHub Actions otomatik olarak test ortamına deploy eder

### 3. Production'a Yükseltme

1. `test` branch'inden `main` branch'ine pull request oluşturun
2. Code review ve testler tamamlandıktan sonra merge yapın
3. GitHub Actions production deployment için hazırlık yapar
4. Manuel onay sonrası production'a deploy edilir

## Manuel Deployment

### Development
```powershell
cd deploy
.\deploy-dev.ps1
```

### Test
```powershell
cd deploy
.\deploy-test.ps1
```

### Production
```powershell
cd deploy
.\deploy-prod.ps1
```

## Versiyonlama

Semantic versioning kullanılır:
- Major versiyon: Breaking changes
- Minor versiyon: Yeni özellikler
- Patch versiyon: Bug fixler

Production release'ler için tag kullanılır:
```bash
git tag v1.0.0
git push origin v1.0.0
```

## İzleme ve Loglama

Her ortamda farklı log seviyeleri:
- Dev: Debug
- Test: Info
- Prod: Error

## Güvenlik

Production ortamında:
- Detailed errors kapalı
- Custom errors açık
- Güvenlik kontrolleri aktif

## Sorun Giderme

### Build Hataları
- NuGet paketlerin doğru restore edildiğinden emin olun
- .NET Framework 4.8 yüklü olmalı

### Deployment Hataları
- IIS permissions kontrol edin
- Connection string'lerin doğru olduğundan emin olun

### Ortam Değişkeni Hataları
- Config dosyalarının doğru kopyalandığından emin olun
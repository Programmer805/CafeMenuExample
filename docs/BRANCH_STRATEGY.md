# CafeMenu Branch Stratejisi

Bu döküman CafeMenu projesi için kullanılan Git branch stratejisini açıklar.

## Branch Yapısı

### Ana Branch'ler

1. **main** (Production)
   - Canlı ortam kodu
   - Sadece testten gelen merge'ler
   - Tag'ler ile versiyonlanır

2. **test**
   - Test ortamı kodu
   - Development branch'inden gelen merge'ler
   - Otomatik test ve deployment

3. **dev** (Development)
   - Ana geliştirme branch'i
   - Feature branch'lerin merge'lendiği yer
   - Otomatik build ve test

### Yardımcı Branch'ler

1. **feature/* 
   - Yeni özellik geliştirme
   - Örnek: feature/user-authentication

2. **bugfix/*
   - Bug düzeltmeleri
   - Örnek: bugfix/login-error

3. **hotfix/*
   - Production acil düzeltmeleri
   - main branch'ten ayrılır
   - main ve dev'e merge edilir

## Workflow

### Yeni Özellik Geliştirme

1. `dev` branch'inden feature branch oluştur
   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/yeni-ozellik
   ```

2. Geliştirme yap
   ```bash
   git add .
   git commit -m "Yeni özellik eklendi"
   git push origin feature/yeni-ozellik
   ```

3. Pull Request oluştur
   - Source: feature/yeni-ozellik
   - Target: dev

4. Code review ve merge
   - Review tamamlandıktan sonra dev'e merge

### Test Ortamına Yükseltme

1. `dev` branch'inden `test` branch'ine pull request oluştur
2. Otomatik testlerin geçmesini bekle
3. Manuel testlerin tamamlanması
4. Merge işlemi

### Production'a Yükseltme

1. `test` branch'inden `main` branch'ine pull request oluştur
2. Final testlerin tamamlanması
3. Versiyon tag'i oluştur
4. Merge işlemi

### Acil Bug Fix (Hotfix)

1. `main` branch'ten hotfix branch oluştur
   ```bash
   git checkout main
   git pull origin main
   git checkout -b hotfix/acil-duzeltme
   ```

2. Düzeltmeyi yap ve test et

3. `main` ve `dev` branch'lerine merge
   ```bash
   git checkout main
   git merge hotfix/acil-duzeltme
   git push origin main
   
   git checkout dev
   git merge hotfix/acil-duzeltme
   git push origin dev
   ```

4. Tag oluştur
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

## Commit Mesajları

### Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Türler
- **feat**: Yeni özellik
- **fix**: Bug düzeltmesi
- **docs**: Dokümantasyon değişikliği
- **style**: Kod formatı değişikliği
- **refactor**: Kod yeniden yapılandırması
- **test**: Test ekleme/değişiklik
- **chore**: Diğer değişiklikler

### Örnekler
```
feat(auth): Kullanıcı girişi eklendi

Kullanıcı adı ve şifre ile giriş yapılabilir hale getirildi.
JWT token desteği eklendi.
```

```
fix(menu): Fiyat hesaplama hatası düzeltildi

KDV dahil fiyat hesaplamasında virgülden sonra 2 basamak 
gösterimi düzeltildi.
```

## Pull Request Kuralları

### Gerekli Kontroller
1. CI pipeline geçmeli
2. En az 1 code review onayı
3. Branch ismi standartlara uygun olmalı
4. Commit mesajları açıklayıcı olmalı

### PR Açıklaması
```markdown
## Açıklama
Bu PR ile [açıklama]

## Değişiklikler
- [ ] Değişiklik 1
- [ ] Değişiklik 2

## Testler
- [ ] Unit test
- [ ] Integration test
- [ ] Manuel test

## Ekran Görüntüleri (varsa)
[Ekran görüntüleri]
```

## Merge Stratejisi

### Feature → Dev
- Squash merge (tüm commit'leri tek commit'e birleştir)

### Dev → Test
- Regular merge (tüm commit'ler korunur)

### Test → Main
- Regular merge (tüm commit'ler korunur)

### Hotfix → Main/Dev
- Regular merge

## Tag'leme

### Format
- v1.0.0 (Major.Minor.Patch)

### Zamanlama
- Her production release sonrasında
- Hotfix sonrasında

### Komutlar
```bash
git tag v1.0.0
git push origin v1.0.0
```

## Branch Koruma Kuralları

### main
- Require pull request reviews
- Require status checks to pass
- Include administrators

### test
- Require pull request reviews
- Require status checks to pass

### dev
- Require linear history
- Allow force pushes (geliştiricilere)

## Temizlik

### Silinecek Branch'ler
- Mergelenmiş feature branch'ler
- Tamamlanmış bugfix branch'ler

### Otomatik Temizlik
- 30 gün sonra mergelenmiş branch'ler silinir
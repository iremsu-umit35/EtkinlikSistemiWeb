# EtkinlikSistemiWeb
ASP.NET Core ile geliştirilen, kullanıcı ve yönetici rolleriyle etkinlik yönetimi, bilet alma ve hava durumu entegrasyonuna sahip bir web API projesi.
Bu proje, bir **Etkinlik Yönetim Sistemi**’dir. Kullanıcılar etkinlikleri görüntüleyebilir, bilet alabilir ve yönetici onayı ile sisteme giriş yapabilir. Hava durumu verisi ve kullanıcı ilgi alanları gibi dinamik özelliklerle zenginleştirilmiştir.

## 👥 Kullanıcı Rolleri

### 👤 Kullanıcı
- Mail ve şifre ile kayıt (yönetici onayı gerekir)
- İlk girişte şifre değiştirme zorunluluğu
- Etkinlik ve duyuruları listeleme
- İlgi alanlarına göre öneri alma
- Hava durumuna göre etkinlik planlanabilirliğini görüntüleme
- Bilet satın alma ve sepet işlemleri
   kontenjan azaltma

### 🛠️ Yönetici
- Yeni kullanıcıları onaylama
- Etkinlik ekleme, düzenleme veya silme
- Duyuru oluşturma ve yayından kaldırma
- Etkinlik listesini yönetme

## 🌐 Entegrasyonlar
- **OpenWeatherMap API:** Şehir ve tarihe göre hava durumu verisi

- **Entity Framework Core** ile veritabanı yönetimi

## 💻 Teknolojiler
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- HTML / CSS / JavaScript (Frontend)
- RESTful API entegrasyonları

## 🧪 Özellikler
- Rol bazlı giriş ve yetkilendirme
- Dinamik bilet fiyat hesaplama
- Gerçek zamanlı kontenjan güncelleme
- Sepet ve ödeme işlemleri
- Hava durumuna göre etkinlik planlaması

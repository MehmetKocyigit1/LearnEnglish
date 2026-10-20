# 🌍 LearnEnglish — Bütünleşik İngilizce Öğrenme Platformu (Monorepo)

Bu proje; öncelikle yazılımcı ve bilgisayar mühendislerinin teknik İngilizcelerini ve genel kelime dağarcıklarını geliştirmesi amacıyla tasarlanmış, **Backend API**, **.NET MAUI Mobil Uygulaması** ve **React Web İstemcisi** içeren tam kapsamlı (full-stack) modern bir monorepo projesidir.

---

## 🏗️ Proje Yapısı

Repository tüm bileşenleri tek bir çatı altında toplar:

```text
LearnEnglish/
├── LearnEnglish.sln              # 🚀 Tüm projeleri bağlayan ana Visual Studio Solution dosyası
├── LearnEnglishAPI-clone/        # ⚙️ Backend Servisleri ve Web İstemcisi
│   ├── Domain/                   # Varlıklar ve temel kurallar (Entities)
│   ├── Application/              # İş mantığı, DTO ve servis arayüzleri
│   ├── Infrastructure/           # EF Core, SQL Server, Harici API ve Gemini Entegrasyonları
│   ├── IngilizceProjeAPI/        # ASP.NET Core Web API (REST endpoints & Swagger)
│   ├── web-client/               # React + Vite web ön yüzü
│   └── docker-compose.yml        # Docker üzerinden API + Web + SQL Server ayağa kaldırma
└── LearnEnglishMaui-clone/       # 📱 Çapraz Platform Mobil Uygulama (.NET MAUI)
    ├── Models/                   # Mobil veri modelleri
    ├── ViewModels/               # MVVM mimarisi ve reaktif UI mantığı
    ├── Views/                    # XAML sayfaları (Flashcard, Quiz, Sözlük, Eşleştirme)
    ├── Services/                 # API İstemcisi, Gemini AI ve Bildirim servisleri
    └── Resources/                # Renk paletleri, stiller ve özel fontlar
```

---

## 📱 .NET MAUI Mobil Uygulama Özellikleri
- **Akıllı Eşleştirme Quizi (Matching Quiz):** Kelimeler ve Türkçe anlamları eşleştikçe dinamik olarak aynı renge bürünen ve numaralandırılan ikili kart sistemi.
- **Teknik ve Genel Kelime Sözlüğü:** Yazılım, algoritma ve bilgisayar mühendisliğine özel kelime havuzu.
- **Flashcard (3D Kartlar):** Öğrenme durumuna göre akıllı kart tekrar sistemi.
- **Sesli Telaffuz (TTS):** Eklediğiniz İngilizce kelimeleri kelime listesi, kelime detayı ve her iki kelime kartı ekranındaki **🔊 Telaffuzu dinle** düğmesinden dinleyin. Cihazın İngilizce konuşma motorunu kullanır; ayrı bir yapay zekâ API anahtarı veya servis aboneliği gerektirmez.
- **Yapay Zeka Destekli Sözlük & Asistan:** Gemini AI destekli kelime kontrolü, bağlam içi kullanım ve telaffuz rehberi.
- **Modern Renk & Tipografi:** Plus Jakarta Sans & Poppins tipografisi ile göze hitap eden modern arayüz.

---

## 🚀 Çalıştırma Rehberi

### 🔊 Kelime Telaffuzunu Dinleme

- **MAUI:** Kelime ekledikten sonra **Kelimelerim** listesindeki, kelime detayındaki veya kelime kartındaki **🔊 Telaffuzu dinle** düğmesine basın. Kartı çevirmeden de telaffuzu dinleyebilirsiniz.
- **Ses seçimi:** Cihazda varsa Amerikan İngilizcesi, yoksa yüklü başka bir İngilizce ses kullanılır. İngilizce ses bulunamazsa uygulama ses paketi yüklemeniz için açıklama gösterir; kelimeyi Türkçe sesle okumaya çalışmaz.
- **MAUI okuma kontrolü:** Başka bir telaffuz başlatıldığında önceki okuma iptal edilir. Sayfadan ayrılınca, alt sekme veya kelime kartı değişince de okuma durur.
- **Cihaz ayarları:** Android'de metin okuma motorunun ve İngilizce ses verisinin; Windows, iOS ve macOS'ta İngilizce konuşma sesinin kullanılabilir olması gerekir. Ayar menülerinin adı cihaza göre değişebilir. Çevrimdışı kullanım seçilen motorun ve indirilmiş ses paketinin desteğine bağlıdır.
- **Web:** Kelime listesindeki hoparlör simgesi ve kartlardaki **Telaffuz Dinle** düğmesi tarayıcının Web Speech API desteğini kullanır. Ses kalitesi ve kullanılabilir sesler tarayıcıya ve cihaza bağlıdır.

MAUI seslendirmesi ortak `LearnEnglishMaui/Services/PronunciationService.cs` servisi üzerinden yürütülür. Android 11 ve sonrası için gereken TTS servis sorgusu `Platforms/Android/AndroidManifest.xml` içinde tanımlıdır. Bu özellik için backend veya veritabanı değişikliği gerekmez.

### 1. Visual Studio ile Geliştirme (Önerilen)
Kök dizindeki **`LearnEnglish.sln`** dosyasını Visual Studio 2022 ile açın:
- **API'yi başlatmak için:** `IngilizceProjeAPI` projesini başlangıç projesi olarak seçip çalıştırın.
- **Mobil uygulamayı başlatmak için:** `IngilizceProjeMAUI` projesini seçip Windows Machine veya Android Emulator üzerinde F5 ile çalıştırın.
- İsterseniz çözüm ayarlarından "Birden Çok Başlangıç Projesi" (Multiple Startup Projects) seçerek hem API'yi hem MAUI'yi aynı anda tek tıkla çalıştırabilirsiniz.

### 2. Docker ile Backend ve Web İstemcisini Başlatma
`LearnEnglishAPI/` dizininde:
```bash
docker compose up -d
```
- **Web Arayüzü:** [http://localhost:3000](http://localhost:3000)
- **Swagger Dokümantasyonu:** [http://localhost:5030/swagger](http://localhost:5030/swagger)


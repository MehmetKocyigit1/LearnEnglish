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
- **Sesli Telaffuz (TTS):** Eklediğiniz İngilizce kelimeleri kelime listesi, kelime detayı ve kelime kartı ekranlarındaki **🔊 Telaffuzu dinle** düğmesinden dinleyin. Teknik sözlüğün kart çevirme ekranında da kullanılabilir. Windows uygulaması doğal ses için yerel **Kokoro-82M / af_heart** modelini; mobil uygulama cihazın İngilizce konuşma motorunu kullanır. Harici API anahtarı veya servis aboneliği gerekmez.
- **Yapay Zeka Destekli Sözlük & Asistan:** Gemini AI destekli kelime kontrolü, bağlam içi kullanım ve telaffuz rehberi.
- **Modern Renk & Tipografi:** Plus Jakarta Sans & Poppins tipografisi ile göze hitap eden modern arayüz.

---

## 🚀 Çalıştırma Rehberi

### 🔊 Kelime Telaffuzunu Dinleme

- **MAUI:** Kelime ekledikten sonra **Kelimelerim** listesindeki, kelime detayındaki veya kelime kartındaki **🔊 Telaffuzu dinle** düğmesine basın. Kartı çevirmeden de telaffuzu dinleyebilirsiniz.
- **Teknik sözlük kartları:** Teknik sözlükte bir kategori seçip **Kartlar** sekmesini açın. Kartın dışındaki **🔊 Telaffuzu dinle** düğmesi hem ön hem arka yüz açıkken kullanılabilir ve kartı çevirmez.
- **Windows sesi:** Kokoro-82M modelinin **af_heart** Amerikan İngilizcesi sesi, daha anlaşılır okuma için **0.85 hızla** kullanılır. Telaffuz düğmesine ilk basışta yerel servis görünür bir terminal açmadan otomatik başlatılır. Model belleğe alınırken ilk okuma birkaç saniye sürebilir; aynı kelimenin sesi servis açıkken önbellekten çalınır. Kokoro kullanılamıyorsa kurulum açıklaması gösterilir; Zira sesine sessizce geri dönülmez.
- **Mobil ses seçimi:** Cihazda varsa Amerikan İngilizcesi, yoksa yüklü başka bir İngilizce ses kullanılır. İngilizce ses bulunamazsa uygulama ses paketi yüklemeniz için açıklama gösterir; kelimeyi Türkçe sesle okumaya çalışmaz.
- **MAUI okuma kontrolü:** Başka bir telaffuz başlatıldığında önceki okuma iptal edilir. Sayfadan ayrılınca, alt sekme veya kelime kartı değişince de okuma durur.
- **Mobil cihaz ayarları:** Android'de metin okuma motorunun ve İngilizce ses verisinin; iOS ve macOS'ta İngilizce konuşma sesinin kullanılabilir olması gerekir. Çevrimdışı kullanım seçilen motorun ve indirilmiş ses paketinin desteğine bağlıdır. Windows Kokoro kurulumu bu ses paketlerine ihtiyaç duymaz.
- **Web:** Kelime listesindeki hoparlör simgesi ve kartlardaki **Telaffuz Dinle** düğmesi tarayıcının Web Speech API desteğini kullanır. Ses kalitesi ve kullanılabilir sesler tarayıcıya ve cihaza bağlıdır.

MAUI seslendirmesi ortak `LearnEnglishMaui/Services/PronunciationService.cs` servisi üzerinden yürütülür. Android 11 ve sonrası için gereken TTS servis sorgusu `Platforms/Android/AndroidManifest.xml` içinde tanımlıdır. Bu özellik için backend veya veritabanı değişikliği gerekmez.

### Windows için Kokoro Kurulumu

Node.js 20 veya sonrasını kurduktan sonra proje kökünde PowerShell ile bir kez çalıştırın:

```powershell
.\tools\kokoro-tts\Setup-Kokoro.ps1
```

Bu komut resmî `kokoro-js` paketini ve `onnx-community/Kokoro-82M-v1.0-ONNX` modelinin CPU üzerinde çalışan `q8` sürümünü hazırlar. Yalnızca Kokoro kaynak kodunu GitHub'dan indirmek yeterli değildir; çalışma bağımlılıkları ve model ağırlıkları da gerekir. İlk kurulum internet gerektirir; hazırlık tamamlandığında seslendirme yerel çalışır. Örnek ses `tools/kokoro-tts/.cache/sample.wav` dosyasına yazılır.

Ardından MAUI Windows uygulamasını yeniden başlatın. Geliştirme sırasında uygulama `tools/kokoro-tts/server.mjs` dosyasını proje dizininden bulur ve servisi otomatik çalıştırır. Servis sadece bu bilgisayardaki `127.0.0.1:17787` adresini dinler; kelimeleri bir bulut API'sine göndermez. Uygulamanın başlattığı servis uygulama kapandıktan sonra kendiliğinden kapanır. Farklı bir dizine yayımlanan uygulama için `LEARNENGLISH_KOKORO_DIR` ortam değişkenini bu servis klasörünün tam yoluna ayarlayın. Servisi elle çalıştırmak için `tools/kokoro-tts` dizininde `npm start` kullanılabilir.

Model önbelleği ve `node_modules` Git'e eklenmez. Kokoro Windows istemcisinde kullanılır; React web istemcisi ve mobil cihazlar mevcut seslendirme yollarını kullanmayı sürdürür.

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


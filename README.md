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
- **Yapay Zeka Destekli Sözlük & Asistan:** Gemini AI destekli kelime kontrolü, bağlam içi kullanım ve telaffuz rehberi.
- **Modern Renk & Tipografi:** Plus Jakarta Sans & Poppins tipografisi ile göze hitap eden modern arayüz.

---

## 🚀 Çalıştırma Rehberi

### 1. Visual Studio ile Geliştirme (Önerilen)
Kök dizindeki **`LearnEnglish.sln`** dosyasını Visual Studio 2022 ile açın:
- **API'yi başlatmak için:** `IngilizceProjeAPI` projesini başlangıç projesi olarak seçip çalıştırın.
- **Mobil uygulamayı başlatmak için:** `IngilizceProjeMAUI` projesini seçip Windows Machine veya Android Emulator üzerinde F5 ile çalıştırın.
- İsterseniz çözüm ayarlarından "Birden Çok Başlangıç Projesi" (Multiple Startup Projects) seçerek hem API'yi hem MAUI'yi aynı anda tek tıkla çalıştırabilirsiniz.

### 2. Docker ile Backend ve Web İstemcisini Başlatma
`LearnEnglishAPI-clone/` dizininde:
```bash
docker compose up -d
```
- **Web Arayüzü:** [http://localhost:3000](http://localhost:3000)
- **Swagger Dokümantasyonu:** [http://localhost:5030/swagger](http://localhost:5030/swagger)


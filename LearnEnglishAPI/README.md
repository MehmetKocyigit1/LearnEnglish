# 📖 VocabBook — İngilizce Kelime Defteri & Pratik Uygulaması

İngilizce kitap veya makale okurken bilmediğiniz kelimeleri, Türkçe anlamlarını ve örnek cümlelerini anında not alabileceğiniz; sonrasında **3D Kart Çevirme (Flashcard)** ve **Çoktan Seçmeli Test (Quiz)** ile pratik yapabileceğiniz full-stack uygulama.

---

## 🚀 Mimari & Teknolojiler

- **Frontend:** React (Vite) + Modern CSS + Lucide Icons + Nginx
- **Backend:** .NET 9 Web API (Onion Architecture: Domain, Application, Infrastructure, API)
- **Veritabanı:** Microsoft SQL Server 2022 (Docker üzerinde izole)
- **ORM & Göç:** Entity Framework Core (Uygulama açılışında tablolar ve migration'lar otomatik oluşturulur)
- **Containerization:** Docker & Docker Compose (Tek komutla tüm stack ayağa kalkar)

---

## 🐳 Docker ile Tek Komutla Çalıştırma

Proje klasöründe terminali açıp şu komutu vermeniz yeterlidir:

```bash
docker compose up -d
```

### Erişim Noktaları:
| Servis | Adres | Açıklama |
|---|---|---|
| **Web Arayüzü (React)** | [http://localhost:3000](http://localhost:3000) | Kitap kelime defteri, kart çevirme ve quiz |
| **Backend API (Swagger)** | [http://localhost:5030/swagger](http://localhost:5030/swagger) | RESTful API dokümantasyonu ve test paneli |
| **MS SQL Server** | `localhost,1433` | Kullanıcı: `sa`, Şifre: `KelimeOgren123!` |

> **Not:** İlk açılışta sistem otomatik olarak `demo` kullanıcısını ve örnek kelimeleri veritabanına yükler. Web arayüzünü açtığınızda doğrudan kullanmaya başlayabilirsiniz!

---

## ✨ Özellikler

1. **Kitap Okuma Modu (Hızlı Ekleme):**
   - Kitaptan kelime, Türkçe anlamı, kelime türü (isim, fiil, sıfat vb.) ve örnek cümle girin.
   - <kbd>Enter</kbd> tuşuna basarak formu anında kaydedin ve bir sonraki kelime için odaklanın.
2. **Kelime Havuzu & Arama:**
   - Kelimelerinizi anlık arayın, öğrenme durumuna (Yeni, Tekrar Gereken, Öğrenildi, Zor) göre filtreleyin.
   - Favorilere ekleyin veya telaffuzlarını dinleyin.
3. **Akıllı Kart Çevirme (Flashcard):**
   - 3D kart çevirme efektiyle İngilizce kelimeyi görün, tıklayarak Türkçe anlamını ve cümleyi açığa çıkarın.
   - "Doğru Bildim" / "Yanlış Bildim" butonları algoritma üzerinden kelimenin öğrenilme durumunu günceller.
4. **Çoktan Seçmeli Test (Quiz):**
   - Rastgele üretilen 4 seçenekli testler ile kendinizi sınayın. Anlık yeşil/kırmızı geri bildirim alın.
5. **İlerleme & Günlük Hedef:**
   - Günlük kelime ekleme hedefinizi belirleyin ve ilerleme çubuğundan takip edin.
   - Toplam başarı yüzdenizi ve seviyelerinizi görün.

---

## 🛑 Sistemi Durdurma

```bash
docker compose down
```

Veritabanını ve verileri tamamen sıfırlamak isterseniz:
```bash
docker compose down -v
```

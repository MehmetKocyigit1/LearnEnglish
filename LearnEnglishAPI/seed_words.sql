-- İngilizce Kelime Öğrenme Projesi - Hazır Kelime Veritabanı Seed Betiği (100 Kelime)
-- Bu betik, AspNetUsers tablosundaki ilk kullanıcıyı bulur ve tüm kelimeleri o kullanıcıya atayarak Words tablosuna ekler.
-- SQL Server'da Türkçe (Unicode) karakterlerin bozulmasını önlemek için N'...' öneki kullanılmıştır.

DECLARE @TargetUserId NVARCHAR(450);

-- Aktif ilk kullanıcıyı seç
SELECT TOP 1 @TargetUserId = Id FROM AspNetUsers;

IF @TargetUserId IS NULL
BEGIN
    PRINT 'HATA: Sistemde kayıtlı hiçbir kullanıcı bulunamadı! Lütfen önce uygulamadan bir kullanıcı kaydı oluşturun.';
END
ELSE
BEGIN
    PRINT 'Kelimeler şu kullanıcıya atanıyor: ' + @TargetUserId;

    -- Zaten ekli olan kelimelerin çiftlenmesini önlemek için kontrol ederek ekle
    INSERT INTO Words (
        EnglishWord, 
        TurkishMeaning, 
        WordType, 
        ExampleSentence, 
        ExampleSentenceTranslation, 
        LearningStatus, 
        IsFavorite, 
        ReviewCount, 
        CorrectAnswerCount, 
        WrongAnswerCount, 
        CreatedDate, 
        CreatedBy, 
        UserId
    )
    SELECT EnglishWord, TurkishMeaning, WordType, ExampleSentence, ExampleSentenceTranslation, LearningStatus, IsFavorite, ReviewCount, CorrectAnswerCount, WrongAnswerCount, CreatedDate, CreatedBy, UserId
    FROM (
        VALUES
        ('ability', N'yetenek', 1, 'She has a great ability to learn languages.', N'Dil öğrenme konusunda harika bir yeteneği var.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('accept', N'kabul etmek', 2, 'He accepted the job offer.', N'İş teklifini kabul etti.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('achieve', N'ulaşmak / başarmak', 2, 'You can achieve your goals with hard work.', N'Zor çalışarak hedeflerine ulaşabilirsin.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('adventure', N'macera', 1, 'Climbing the mountain was a great adventure.', N'Dağa tırmanmak büyük bir maceraydı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('afford', N'maddi gücü yetmek', 2, 'I cannot afford to buy a new car right now.', N'Şu anda yeni bir araba satın almaya gücüm yetmiyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('agreement', N'anlaşma / sözleşme', 1, 'They signed a business agreement.', N'Bir iş anlaşması imzaladılar.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('almost', N'neredeyse', 4, 'The dinner is almost ready.', N'Akşam yemeği neredeyse hazır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('already', N'zaten / çoktan', 4, 'I have already finished my homework.', N'Ödevimi zaten bitirdim.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('amazing', N'harika / şaşırtıcı', 3, 'The view from the top of the hill was amazing.', N'Tepenin zirvesinden manzara harikaydı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('angry', N'kızgın / öfkeli', 3, 'He was angry because he lost his keys.', N'Anahtarlarını kaybettiği için kızgındı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('answer', N'cevap / yanıt', 1, 'Please write your answer on the paper.', N'Lütfen cevabınızı kağıda yazın.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('anxious', N'endişeli / kaygılı', 3, 'She felt anxious before the interview.', N'Mülakattan önce endişeli hissetti.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('apologize', N'özür dilemek', 2, 'You should apologize for your mistake.', N'Hatan için özür dilemelisin.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('argue', N'tartışmak', 2, 'They argue about politics all the time.', N'Her zaman siyaset hakkında tartışırlar.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('arrival', N'varış / geliş', 1, 'The arrival of the train was delayed.', N'Trenin varışı ertelendi.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('attention', N'dikkat', 1, 'Pay attention to the teacher.', N'Öğretmene dikkat edin.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('avoid', N'kaçınmak / uzak durmak', 2, 'Try to avoid sweet foods.', N'Tatlı yiyeceklerden kaçınmaya çalışın.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('beautiful', N'güzel', 3, 'She wore a beautiful dress.', N'Güzel bir elbise giydi.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('behavior', N'davranış', 1, 'His behavior in school was excellent.', N'Okuldaki davranışı mükemmeldi.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('behind', N'arkasında', 9, 'The cat is hiding behind the sofa.', N'Kedi kanepenin arkasında saklanıyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('believe', N'inanmak', 2, 'I believe that you can pass the test.', N'Sınavı geçebileceğine inanıyorum.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('benefit', N'fayda / yarar', 1, 'Regular exercise has many benefits.', N'Düzenli egzersizin birçok faydası vardır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('borrow', N'ödünç almak', 2, 'Can I borrow your pen for a minute?', N'Kalemini bir dakikalığına ödünç alabilir miyim?', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('brave', N'cesur', 3, 'The brave firefighter saved the child.', N'Cesur itfaiyeci çocuğu kurtardı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('bright', N'parlak', 3, 'The sun is very bright today.', N'Güneş bugün çok parlak.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('business', N'iş / ticaret', 1, 'She runs a successful clothing business.', N'Başarılı bir giyim işletmesi yönetiyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('cancel', N'iptal etmek', 2, 'They had to cancel the meeting.', N'Toplantıyı iptal etmek zorunda kaldılar.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('carefully', N'dikkatlice', 4, 'Please drive carefully in the rain.', N'Lütfen yağmurda dikkatlice sürün.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('celebrated', N'ünlü / meşhur', 3, 'He is a celebrated scientist.', N'O, ünlü bir bilim insanıdır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('challenge', N'zorluk / mücadele', 1, 'Learning English is a good challenge.', N'İngilizce öğrenmek iyi bir mücadeledir.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('chance', N'şans / fırsat', 1, 'Give him a second chance.', N'Ona ikinci bir şans ver.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('change', N'değiştirmek', 2, 'I need to change my clothes.', N'Elbiselerimi değiştirmem gerekiyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('choose', N'seçmek', 2, 'Choose the best answer from the list.', N'Listeden en iyi cevabı seçin.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('clean', 'temiz', 3, 'Keep your room clean and tidy.', N'Odanızı temiz ve düzenli tutun.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('clever', N'zeki / akıllı', 3, 'The clever dog found the hidden toy.', N'Zeki köpek gizli oyuncağı buldu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('compare', N'karşılaştırmak', 2, 'Do not compare yourself to others.', N'Kendini başkalarıyla karşılaştırma.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('complain', N'şikayet etmek', 2, 'Customers complain about the service.', N'Müşteriler hizmet hakkında şikayet ediyorlar.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('connect', N'bağlamak', 2, 'You can connect the computer to the internet.', N'Bilgisayarı internete bağlayabilirsiniz.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('create', N'yaratmak / oluşturmak', 2, 'Artists create beautiful paintings.', N'Sanatçılar güzel tablolar yaratır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('crowd', N'kalabalık', 1, 'A large crowd gathered in the square.', N'Meydanda büyük bir kalabalık toplandı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('danger', N'tehlike', 1, 'The signs warn of danger ahead.', N'Tabelalar ilerideki tehlike konusunda uyarıyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('decide', N'karar vermek', 2, 'I cannot decide which shoes to buy.', N'Hangi ayakkabıyı alacağıma karar veremiyorum.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('delicious', N'lezzetli', 3, 'This chocolate cake is delicious.', N'Bu çikolatalı kek lezzetli.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('demand', N'talep / istek', 1, 'The demand for electric cars is rising.', N'Elektrikli arabalara olan talep artıyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('describe', N'tarif etmek / açıklamak', 2, 'Can you describe the lost bag?', N'Kayıp çantayı tarif edebilir misiniz?', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('difference', N'fark', 1, 'There is a big difference between the two.', N'İkisi arasında büyük bir fark var.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('difficult', N'zor', 3, 'The exam was very difficult.', N'Sınav çok zordu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('disappear', N'gözden kaybolmak', 2, 'The sun disappeared behind the clouds.', N'Güneş bulutların arkasında kayboldu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('discover', N'keşfetmek', 2, 'Scientists discovered a new planet.', N'Bilim insanları yeni bir gezegen keşfetti.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('discuss', N'tartışmak / görüşmek', 2, 'We need to discuss the project plan.', N'Proje planını tartışmamız gerekiyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('distance', N'mesafe', 1, 'The distance between the cities is short.', N'Şehirler arasındaki mesafe kısa.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('dream', N'rüya / hayal', 1, 'My dream is to travel the world.', N'Hayalim dünyayı gezmektir.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('early', N'erken', 4, 'He woke up early this morning.', N'Bu sabah erken uyandı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('easily', N'kolayca', 4, 'She passed the driving test easily.', N'Direksiyon sınavını kolayca geçti.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('education', N'eğitim', 1, 'Education is important for a good job.', N'İyi bir iş için eğitim önemlidir.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('effort', N'çaba / gayret', 1, 'She put a lot of effort into her work.', N'İşine çok çaba sarf etti.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('encourage', N'teşvik etmek', 2, 'Parents encourage their kids to read.', N'Ebeveynler çocuklarını okumaya teşvik eder.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('enemy', N'düşman', 1, 'The two countries were once enemies.', N'İki ülke bir zamanlar düşmandı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('enormous', N'kocaman / devasa', 3, 'They live in an enormous house.', N'Kocaman bir evde yaşıyorlar.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('environment', N'çevre', 1, 'We must protect the environment.', N'Çevreyi korumalıyız.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('escape', N'kaçmak', 2, 'The prisoner tried to escape from jail.', N'Mahkum hapishaneden kaçmaya çalıştı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('excellent', N'mükemmel', 3, 'Your English pronunciation is excellent.', N'İngilizce telaffuzunuz mükemmel.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('exciting', N'heyecan verici', 3, 'The movie had an exciting ending.', N'Filmin heyecan verici bir sonu vardı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('experience', N'deneyim / tecrübe', 1, 'He has five years of work experience.', N'Beş yıllık iş deneyimi var.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('explain', N'açıklamak', 2, 'Can you explain this rule again?', N'Bu kuralı tekrar açıklayabilir misiniz?', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('failure', N'başarısızlık', 1, 'Failure is a step toward success.', N'Başarısızlık başarıya giden bir adımdır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('famous', N'ünlü / meşhur', 3, 'He wants to be a famous singer.', N'Ünlü bir şarkıcı olmak istiyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('finally', N'sonunda / nihayet', 4, 'We finally arrived at the hotel.', N'Sonunda otele vardık.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('foreign', N'yabancı', 3, 'I like learning foreign languages.', N'Yabancı diller öğrenmeyi severim.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('forget', N'unutmak', 2, 'Do not forget to lock the door.', N'Kapıyı kilitlemeyi unutma.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('freedom', N'özgürlük', 1, 'Freedom of speech is a basic right.', N'İfade özgürlüğü temel bir haktır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('friendly', N'arkadaş canlısı', 3, 'The neighbors are very friendly.', N'Komşular çok arkadaş canlısı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('future', N'gelecek', 1, 'She is planning for her future career.', N'Gelecekteki kariyeri için plan yapıyor.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('generous', N'cömert', 3, 'He is generous with his money.', N'Parası konusunda cömerttir.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('gentle', N'nazik / kibar', 3, 'He gave the baby a gentle touch.', N'Bebeğe nazik bir dokunuş verdi.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('glad', N'memnun', 3, 'I am glad to hear that you are well.', N'İyi olduğunuzu duyduğuma sevindim.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('grow', N'büyümek / yetişmek', 2, 'Plants need water and sun to grow.', N'Bitkilerin büyümek için suya ve güneşe ihtiyacı vardır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('healthy', N'sağlıklı', 3, 'Eating vegetables is a healthy habit.', N'Sebze yemek sağlıklı bir alışkanlıktır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('hope', N'umut etmek', 2, 'I hope it will not rain tomorrow.', N'Umarım yarın yağmur yağmaz.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('huge', N'kocaman / devasa', 3, 'An elephant is a huge animal.', N'Fil devasa bir hayvandır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('immediately', N'hemen / acilen', 4, 'Please call me immediately.', N'Lütfen beni hemen arayın.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('improve', N'geliştirmek / iyileştirmek', 2, 'You can improve your reading skills.', N'Okuma becerilerini geliştirebilirsin.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('innocent', N'suçsuz / masum', 3, 'The court found him innocent.', N'Mahkeme onu suçsuz buldu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('intelligent', N'zeki / akıllı', 3, 'Dolphins are very intelligent creatures.', N'Yunuslar çok zeki yaratıklardır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('intention', 'niyet / kasıt', 1, 'I have no intention of leaving.', N'Ayrılmaya hiç niyetim yok.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('jealous', N'kıskanç', 3, 'He felt jealous of his brother.', N'Kardeşini kıskandı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('journey', N'yolculuk / seyahat', 1, 'It was a long journey by train.', N'Trenle uzun bir yolculuktu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('knowledge', N'bilgi', 1, 'She has a deep knowledge of history.', N'Derin bir tarih bilgisine sahip.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('lazy', N'tembel', 3, 'The lazy boy slept all day.', N'Tembel çocuk bütün gün uyudu.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('management', N'yönetim / idare', 1, 'The company is under new management.', N'Şirket yeni yönetim altında.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('memory', N'hafıza / bellek', 1, 'He has a good memory for names.', N'İsimler konusunda iyi bir hafızası var.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('narrow', N'dar', 3, 'The car drove down a narrow street.', N'Araba dar bir sokaktan aşağı sürdü.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('nervous', N'gergin / heyecanlı', 3, 'He was nervous before the exam.', N'Sınavdan önce gergindi.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('opportunity', N'fırsat', 1, 'Studying abroad is a great opportunity.', N'Yurt dışında okumak büyük bir fırsattır.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('patient', N'sabırlı', 3, 'You need to be patient with children.', N'Çocuklara karşı sabırlı olman gerekir.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('quietly', N'sessizce', 4, 'Please speak quietly in the library.', N'Lütfen kütüphanede sessizce konuşun.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('realize', N'farkına varmak', 2, 'I did not realize you were here.', N'Burada olduğunun farkına varmadım.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('recommend', N'tavsiye etmek', 2, 'Which book do you recommend?', N'Hangi kitabı tavsiye edersiniz?', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('search', N'aramak / araştırmak', 2, 'He searched the house for his keys.', N'Anahtarlarını bulmak için evi aradı.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId),
        ('suddenly', N'aniden / birdenbire', 4, 'The lights went out suddenly.', N'Işıklar aniden söndü.', 4, 0, 0, 0, 0, GETUTCDATE(), @TargetUserId, @TargetUserId)
    ) AS source (EnglishWord, TurkishMeaning, WordType, ExampleSentence, ExampleSentenceTranslation, LearningStatus, IsFavorite, ReviewCount, CorrectAnswerCount, WrongAnswerCount, CreatedDate, CreatedBy, UserId)
    WHERE NOT EXISTS (
        SELECT 1 FROM Words 
        WHERE Words.UserId = @TargetUserId 
          AND Words.EnglishWord = source.EnglishWord
    );

    PRINT 'Kelime aktarımı başarıyla tamamlandı!';
END

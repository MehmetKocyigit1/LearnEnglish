import React, { useState, useEffect, useRef } from 'react';
import { api } from './api';
import './App.css';
import { 
  BookOpen, 
  Plus, 
  Search, 
  RotateCw, 
  Brain, 
  Award, 
  Heart, 
  Trash2, 
  CheckCircle2, 
  XCircle, 
  Sparkles, 
  BarChart3, 
  ArrowRight,
  Bookmark,
  Volume2
} from 'lucide-react';

const WORD_TYPES = [
  { id: 1, name: 'İsim (Noun)' },
  { id: 2, name: 'Fiil (Verb)' },
  { id: 3, name: 'Sıfat (Adjective)' },
  { id: 4, name: 'Zarf (Adverb)' },
  { id: 5, name: 'Zamir (Pronoun)' },
  { id: 6, name: 'Bağlaç (Conjunction)' },
  { id: 7, name: 'Edat (Preposition)' },
  { id: 8, name: 'Ünlem (Interjection)' },
  { id: 9, name: 'Diğer / Kalıp (Other)' }
];

export default function App() {
  const [activeTab, setActiveTab] = useState('words'); // 'words' | 'flashcard' | 'quiz' | 'stats'
  const [words, setWords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  
  // Quick Add state
  const [showAddForm, setShowAddForm] = useState(true);
  const [newEnglish, setNewEnglish] = useState('');
  const [newTurkish, setNewTurkish] = useState('');
  const [newType, setNewType] = useState(1);
  const [newSentence, setNewSentence] = useState('');
  const [newSentenceTr, setNewSentenceTr] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [addFeedback, setAddFeedback] = useState(null);
  
  // Search & Filter
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [typeFilter, setTypeFilter] = useState('all');
  const [favOnly, setFavOnly] = useState(false);

  // Flashcard state
  const [flashcardWord, setFlashcardWord] = useState(null);
  const [isFlipped, setIsFlipped] = useState(false);
  const [flashcardLoading, setFlashcardLoading] = useState(false);

  // Quiz state
  const [currentQuiz, setCurrentQuiz] = useState(null);
  const [quizLoading, setQuizLoading] = useState(false);
  const [selectedOption, setSelectedOption] = useState(null);
  const [quizFeedback, setQuizFeedback] = useState(null);

  // Stats state
  const [stats, setStats] = useState(null);
  const [dailyTarget, setDailyTarget] = useState(10);

  const englishInputRef = useRef(null);

  // 1. Initial Load & Auto-login
  useEffect(() => {
    async function init() {
      try {
        if (!api.auth.isAuthenticated()) {
          // Auto login as demo
          await api.auth.demoLogin();
        }
        setUser(api.auth.getCurrentUser());
        await loadWords();
      } catch (err) {
        console.error('Başlangıç hatası:', err);
      } finally {
        setLoading(false);
      }
    }
    init();
  }, []);

  // 2. Fetch Words
  async function loadWords() {
    try {
      const data = await api.words.getAll();
      setWords(data || []);
    } catch (err) {
      console.error('Kelimeler yüklenemedi:', err);
    }
  }

  // 3. Quick Add Word
  async function handleAddWord(e) {
    if (e) e.preventDefault();
    if (!newEnglish.trim() || !newTurkish.trim()) return;

    setIsSubmitting(true);
    setAddFeedback(null);
    try {
      const newWord = await api.words.create({
        englishWord: newEnglish.trim(),
        turkishMeaning: newTurkish.trim(),
        wordType: parseInt(newType, 10),
        exampleSentence: newSentence.trim(),
        exampleSentenceTranslation: newSentenceTr.trim()
      });

      setWords(prev => [newWord, ...prev]);
      setAddFeedback(`"${newEnglish}" başarıyla kaydedildi! 🚀`);
      
      // Clear inputs and keep focus on English input for immediate next entry
      setNewEnglish('');
      setNewTurkish('');
      setNewSentence('');
      setNewSentenceTr('');
      if (englishInputRef.current) {
        englishInputRef.current.focus();
      }
      setTimeout(() => setAddFeedback(null), 3500);
    } catch (err) {
      setAddFeedback(`Hata: ${err.message}`);
    } finally {
      setIsSubmitting(false);
    }
  }

  // Toggle Favorite
  async function handleToggleFavorite(id) {
    try {
      await api.words.toggleFavorite(id);
      setWords(prev => prev.map(w => w.id === id ? { ...w, isFavorite: !w.isFavorite } : w));
    } catch (err) {
      console.error(err);
    }
  }

  // Delete Word
  async function handleDeleteWord(id, wordText) {
    if (!window.confirm(`"${wordText}" kelimesini silmek istediğine emin misin?`)) return;
    try {
      await api.words.delete(id);
      setWords(prev => prev.filter(w => w.id !== id));
    } catch (err) {
      alert(`Silinemedi: ${err.message}`);
    }
  }

  // 4. Flashcard Functions
  async function loadNextFlashcard() {
    setFlashcardLoading(true);
    setIsFlipped(false);
    try {
      const word = await api.words.getRandomForReview();
      setFlashcardWord(word);
    } catch (err) {
      console.error(err);
      setFlashcardWord(null);
    } finally {
      setFlashcardLoading(false);
    }
  }

  async function handleReviewAnswer(isCorrect) {
    if (!flashcardWord) return;
    try {
      await api.words.submitReview(flashcardWord.id, isCorrect);
      await loadWords(); // refresh counts
      await loadNextFlashcard();
    } catch (err) {
      console.error(err);
    }
  }

  // 5. Quiz Functions
  async function loadNextQuiz() {
    setQuizLoading(true);
    setSelectedOption(null);
    setQuizFeedback(null);
    try {
      const quiz = await api.quizzes.getMultipleChoice();
      setCurrentQuiz(quiz);
    } catch (err) {
      console.error(err);
      setCurrentQuiz(null);
    } finally {
      setQuizLoading(false);
    }
  }

  async function handleSelectQuizOption(option) {
    if (selectedOption !== null || !currentQuiz) return;
    setSelectedOption(option);
    
    // Check answer against word details
    try {
      const word = await api.words.getById(currentQuiz.wordId);
      const isCorrect = word.turkishMeaning.trim().toLowerCase() === option.trim().toLowerCase();
      
      setQuizFeedback({
        isCorrect,
        correctAnswer: word.turkishMeaning
      });

      await api.quizzes.submitResult(currentQuiz.wordId, isCorrect);
      await loadWords();
    } catch (err) {
      console.error(err);
    }
  }

  // 6. Stats Functions
  async function loadStats() {
    try {
      const data = await api.stats.getSummary();
      setStats(data);
      if (data && data.dailyGoalTarget) {
        setDailyTarget(data.dailyGoalTarget);
      }
    } catch (err) {
      console.error(err);
    }
  }

  async function handleUpdateDailyTarget(e) {
    e.preventDefault();
    try {
      await api.stats.setDailyGoal(parseInt(dailyTarget, 10));
      await loadStats();
      alert('Günlük hedef güncellendi! 🎯');
    } catch (err) {
      alert(`Hedef güncellenemedi: ${err.message}`);
    }
  }

  // Tab change handlers
  useEffect(() => {
    if (activeTab === 'flashcard') {
      loadNextFlashcard();
    } else if (activeTab === 'quiz') {
      loadNextQuiz();
    } else if (activeTab === 'stats') {
      loadStats();
    }
  }, [activeTab]);

  // Audio pronunciation helper
  function speakWord(text) {
    if ('speechSynthesis' in window) {
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = 'en-US';
      window.speechSynthesis.speak(utterance);
    }
  }

  // Filtering
  const filteredWords = words.filter(w => {
    const matchesSearch = 
      w.englishWord.toLowerCase().includes(searchQuery.toLowerCase()) ||
      w.turkishMeaning.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = 
      statusFilter === 'all' || w.learningStatus.toString() === statusFilter;
      
    const matchesType = 
      typeFilter === 'all' || w.wordType.toString() === typeFilter;
      
    const matchesFav = 
      !favOnly || w.isFavorite;

    return matchesSearch && matchesStatus && matchesType && matchesFav;
  });

  return (
    <div className="app-container">
      {/* NAVBAR */}
      <header className="navbar">
        <div className="logo-area">
          <div className="logo-icon">
            <BookOpen size={22} />
          </div>
          <div className="logo-text">
            <h1>VocabBook</h1>
            <p>Kitap Kelime Notları & Pratik</p>
          </div>
        </div>

        <nav className="nav-links">
          <button 
            className={`nav-btn ${activeTab === 'words' ? 'active' : ''}`}
            onClick={() => setActiveTab('words')}
          >
            <Bookmark size={16} /> Kelimelerim ({words.length})
          </button>
          <button 
            className={`nav-btn ${activeTab === 'flashcard' ? 'active' : ''}`}
            onClick={() => setActiveTab('flashcard')}
          >
            <RotateCw size={16} /> Kart Çevirme
          </button>
          <button 
            className={`nav-btn ${activeTab === 'quiz' ? 'active' : ''}`}
            onClick={() => setActiveTab('quiz')}
          >
            <Brain size={16} /> Quiz Test
          </button>
          <button 
            className={`nav-btn ${activeTab === 'stats' ? 'active' : ''}`}
            onClick={() => setActiveTab('stats')}
          >
            <BarChart3 size={16} /> İstatistik
          </button>
        </nav>

        <div className="user-area">
          <button 
            className="quick-add-btn"
            onClick={() => {
              setActiveTab('words');
              setShowAddForm(true);
              setTimeout(() => englishInputRef.current?.focus(), 50);
            }}
          >
            <Plus size={16} /> Hızlı Kelime Ekle
          </button>
        </div>
      </header>

      {/* READING MODE HERO BANNER */}
      <section className="reading-banner">
        <div className="reading-info">
          <h2>
            <span>📖</span> Kitap Okuma Modu
          </h2>
          <p>
            İngilizce kitabında takıldığın kelimeleri anında not al, örnek cümleleriyle birlikte kaydet ve daha sonra kart çevirerek tekrar et.
          </p>
        </div>
        <div className="reading-stats">
          <div className="stat-item">
            <div className="val">{words.length}</div>
            <div className="lbl">Toplam Kelime</div>
          </div>
          <div className="stat-item">
            <div className="val" style={{ color: 'var(--accent-success)' }}>
              {words.filter(w => w.learningStatus === 3).length}
            </div>
            <div className="lbl">Öğrenilen</div>
          </div>
        </div>
      </section>

      {/* TAB 1: WORDS LIST & QUICK ADD FORM */}
      {activeTab === 'words' && (
        <main>
          {/* QUICK ADD INLINE FORM */}
          {showAddForm && (
            <div className="quick-add-card">
              <div className="quick-add-header">
                <h3>
                  <Sparkles size={18} color="var(--accent-primary)" />
                  Kitaptan Yeni Kelime Kaydet
                </h3>
                <span className="shortcut-tip">
                  Kaydetmek için <kbd>Enter</kbd> tuşuna basabilirsin
                </span>
              </div>

              {addFeedback && (
                <div style={{
                  padding: '10px 14px',
                  borderRadius: 'var(--radius-sm)',
                  marginBottom: '14px',
                  backgroundColor: addFeedback.startsWith('Hata') ? 'rgba(239, 68, 68, 0.2)' : 'rgba(16, 185, 129, 0.2)',
                  color: addFeedback.startsWith('Hata') ? '#f87171' : '#34d399',
                  fontSize: '13px',
                  fontWeight: '600'
                }}>
                  {addFeedback}
                </div>
              )}

              <form onSubmit={handleAddWord}>
                <div className="form-grid">
                  <div className="form-group">
                    <label>İngilizce Kelime *</label>
                    <input 
                      ref={englishInputRef}
                      type="text" 
                      className="input-field" 
                      placeholder="Örn: serendipity, reluctant..."
                      value={newEnglish}
                      onChange={e => setNewEnglish(e.target.value)}
                      required
                      autoFocus
                    />
                  </div>

                  <div className="form-group">
                    <label>Türkçe Anlamı *</label>
                    <input 
                      type="text" 
                      className="input-field" 
                      placeholder="Örn: tesadüfi güzellik, isteksiz..."
                      value={newTurkish}
                      onChange={e => setNewTurkish(e.target.value)}
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label>Kelime Türü</label>
                    <select 
                      className="select-field"
                      value={newType}
                      onChange={e => setNewType(e.target.value)}
                    >
                      {WORD_TYPES.map(t => (
                        <option key={t.id} value={t.id}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px' }}>
                  <div className="form-group">
                    <label>Kitaptaki Örnek Cümle (İsteğe bağlı)</label>
                    <input 
                      type="text" 
                      className="input-field" 
                      placeholder="Örn: She was reluctant to leave the room."
                      value={newSentence}
                      onChange={e => setNewSentence(e.target.value)}
                    />
                  </div>

                  <div className="form-group">
                    <label>Cümlenin Türkçe Çevirisi (İsteğe bağlı)</label>
                    <input 
                      type="text" 
                      className="input-field" 
                      placeholder="Örn: Odadan ayrılma konusunda isteksizdi."
                      value={newSentenceTr}
                      onChange={e => setNewSentenceTr(e.target.value)}
                    />
                  </div>
                </div>

                <div className="form-actions">
                  <span className="shortcut-tip">
                    * Zorunlu alanlar. Kelime eklendikten sonra form bir sonraki kelime için otomatik temizlenir.
                  </span>
                  <button type="submit" className="btn-primary" disabled={isSubmitting}>
                    <Plus size={16} /> {isSubmitting ? 'Kaydediliyor...' : 'Deftere Ekle'}
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* SEARCH & FILTER CONTROLS */}
          <div className="controls-bar">
            <div className="search-wrapper">
              <Search className="search-icon" size={18} />
              <input 
                type="text" 
                className="search-input" 
                placeholder="Listede kelime veya Türkçe anlam ara..."
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
              />
            </div>

            <select 
              className="filter-select"
              value={statusFilter}
              onChange={e => setStatusFilter(e.target.value)}
            >
              <option value="all">Tüm Durumlar</option>
              <option value="1">🔴 Çok Önemli (Hatalı)</option>
              <option value="2">🟡 Tekrar Gereken</option>
              <option value="3">🟢 Öğrenildi</option>
              <option value="4">⚪ Yeni / İncelenmedi</option>
            </select>

            <select 
              className="filter-select"
              value={typeFilter}
              onChange={e => setTypeFilter(e.target.value)}
            >
              <option value="all">Tüm Türler</option>
              {WORD_TYPES.map(t => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>

            <button 
              className={`filter-select ${favOnly ? 'btn-primary' : ''}`}
              style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
              onClick={() => setFavOnly(!favOnly)}
            >
              <Heart size={16} fill={favOnly ? 'white' : 'none'} /> Favoriler
            </button>
          </div>

          {/* WORD CARDS GRID */}
          {loading ? (
            <div className="loading-spinner">Kelimeler yükleniyor...</div>
          ) : filteredWords.length === 0 ? (
            <div className="empty-box">
              <h3>Kayıtlı kelime bulunamadı</h3>
              <p>Arama kriterlerini temizleyebilir veya yukarıdaki formdan hemen yeni bir kelime ekleyebilirsin.</p>
            </div>
          ) : (
            <div className="words-grid">
              {filteredWords.map(word => {
                const typeObj = WORD_TYPES.find(t => t.id === word.wordType);
                return (
                  <div key={word.id} className="word-card">
                    <div>
                      <div className="card-top">
                        <div className="word-title-area">
                          <span className={`status-dot status-${word.learningStatus}`} title={`Durum: ${word.learningStatus}`} />
                          <h4 className="word-english">{word.englishWord}</h4>
                          <button 
                            onClick={() => speakWord(word.englishWord)}
                            title="Telaffuzu Dinle"
                            style={{ color: 'var(--text-muted)', padding: '2px' }}
                          >
                            <Volume2 size={16} />
                          </button>
                        </div>
                        <button 
                          className={`fav-btn ${word.isFavorite ? 'active' : ''}`}
                          onClick={() => handleToggleFavorite(word.id)}
                          title="Favorilere ekle"
                        >
                          <Heart size={18} fill={word.isFavorite ? '#ef4444' : 'none'} />
                        </button>
                      </div>

                      <div className="word-turkish">{word.turkishMeaning}</div>

                      {typeObj && (
                        <div style={{ marginBottom: '10px' }}>
                          <span className="word-badge">{typeObj.name}</span>
                        </div>
                      )}

                      {word.exampleSentence && (
                        <div className="word-sentence">
                          "{word.exampleSentence}"
                          {word.exampleSentenceTranslation && (
                            <span className="word-translation">— {word.exampleSentenceTranslation}</span>
                          )}
                        </div>
                      )}
                    </div>

                    <div className="card-footer">
                      <div className="stats-pill">
                        <span>Tekrar: {word.reviewCount}</span>
                        <span style={{ color: 'var(--accent-success)' }}>✓ {word.correctAnswerCount}</span>
                        <span style={{ color: 'var(--accent-danger)' }}>✗ {word.wrongAnswerCount}</span>
                      </div>
                      <div className="card-actions">
                        <button 
                          className="action-btn delete"
                          onClick={() => handleDeleteWord(word.id, word.englishWord)}
                          title="Kelimeyi sil"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </main>
      )}

      {/* TAB 2: FLASHCARD REVIEW */}
      {activeTab === 'flashcard' && (
        <main className="study-container">
          <div style={{ marginBottom: '20px' }}>
            <h2>⚡ Akıllı Kelime Kartı</h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '14px', marginTop: '4px' }}>
              Kartın üzerine tıklayarak Türkçe anlamını ve örnek cümleyi açığa çıkar.
            </p>
          </div>

          {flashcardLoading ? (
            <div className="loading-spinner">Sıradaki kelime getiriliyor...</div>
          ) : !flashcardWord ? (
            <div className="empty-box">
              <h3>Tekrar edilecek kelime bulunamadı! 🎉</h3>
              <p>Listenize henüz kelime eklemediyseniz kelime ekleyebilir veya listeyi yenileyebilirsiniz.</p>
              <button className="btn-primary" style={{ margin: '16px auto 0' }} onClick={loadNextFlashcard}>
                Yeniden Dene
              </button>
            </div>
          ) : (
            <div>
              <div className="flashcard-wrapper" onClick={() => setIsFlipped(!isFlipped)}>
                <div className={`flashcard ${isFlipped ? 'flipped' : ''}`}>
                  {/* FRONT */}
                  <div className="card-face card-front">
                    <span className="card-tag">İNGİLİZCE</span>
                    <h3 className="card-main-word">{flashcardWord.englishWord}</h3>
                    <button 
                      onClick={(e) => { e.stopPropagation(); speakWord(flashcardWord.englishWord); }}
                      style={{ color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: '6px' }}
                    >
                      <Volume2 size={20} /> Telaffuz Dinle
                    </button>
                    <p className="card-hint">Türkçe anlamı görmek için karta dokun 👆</p>
                  </div>

                  {/* BACK */}
                  <div className="card-face card-back">
                    <span className="card-tag" style={{ color: 'var(--accent-success)' }}>TÜRKÇE ANLAMI</span>
                    <h3 className="card-meaning">{flashcardWord.turkishMeaning}</h3>

                    {flashcardWord.exampleSentence && (
                      <div className="card-example-box">
                        <div style={{ fontSize: '14px', fontStyle: 'italic', color: '#e2e8f0' }}>
                          "{flashcardWord.exampleSentence}"
                        </div>
                        {flashcardWord.exampleSentenceTranslation && (
                          <div style={{ fontSize: '13px', color: '#94a3b8', marginTop: '6px' }}>
                            {flashcardWord.exampleSentenceTranslation}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {isFlipped ? (
                <div className="flashcard-actions">
                  <button className="btn-review wrong" onClick={() => handleReviewAnswer(false)}>
                    <XCircle size={18} /> Yanlış Bildim
                  </button>
                  <button className="btn-review correct" onClick={() => handleReviewAnswer(true)}>
                    <CheckCircle2 size={18} /> Doğru Bildim!
                  </button>
                </div>
              ) : (
                <button 
                  className="btn-primary" 
                  style={{ margin: '0 auto' }}
                  onClick={() => setIsFlipped(true)}
                >
                  Anlamı Göster
                </button>
              )}
            </div>
          )}
        </main>
      )}

      {/* TAB 3: QUIZ TEST */}
      {activeTab === 'quiz' && (
        <main style={{ maxWidth: '650px', margin: '0 auto' }}>
          <div style={{ textAlign: 'center', marginBottom: '24px' }}>
            <h2>🎯 Çoktan Seçmeli Test</h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '14px', marginTop: '4px' }}>
              İngilizce kelimenin doğru Türkçe karşılığını seç.
            </p>
          </div>

          {quizLoading ? (
            <div className="loading-spinner">Test sorusu hazırlanıyor...</div>
          ) : !currentQuiz ? (
            <div className="empty-box">
              <h3>Test oluşturulamadı</h3>
              <p>Test oluşturmak için listenizde en az 4 kelime bulunmalıdır.</p>
              <button className="btn-primary" style={{ margin: '16px auto 0' }} onClick={loadNextQuiz}>
                Yeniden Dene
              </button>
            </div>
          ) : (
            <div className="quiz-card">
              <div className="quiz-header">
                <span className="card-tag">BU KELİMENİN ANLAMI NEDİR?</span>
                <h3 className="quiz-question-word">{currentQuiz.englishWord}</h3>

                {currentQuiz.exampleSentence && (
                  <div style={{ 
                    background: 'var(--bg-input)', 
                    padding: '12px 16px', 
                    borderRadius: 'var(--radius-sm)', 
                    marginTop: '16px',
                    fontSize: '14px',
                    fontStyle: 'italic',
                    color: 'var(--text-secondary)'
                  }}>
                    "{currentQuiz.exampleSentence}"
                  </div>
                )}
              </div>

              <div className="quiz-options">
                {currentQuiz.options.map((option, idx) => {
                  let btnClass = 'quiz-opt-btn';
                  if (selectedOption !== null && quizFeedback) {
                    if (option.trim().toLowerCase() === quizFeedback.correctAnswer.trim().toLowerCase()) {
                      btnClass += ' correct';
                    } else if (option === selectedOption) {
                      btnClass += ' wrong';
                    }
                  }

                  return (
                    <button 
                      key={idx}
                      className={btnClass}
                      disabled={selectedOption !== null}
                      onClick={() => handleSelectQuizOption(option)}
                    >
                      <span>{option}</span>
                      {selectedOption !== null && option.trim().toLowerCase() === quizFeedback?.correctAnswer.trim().toLowerCase() && (
                        <CheckCircle2 size={18} color="var(--accent-success)" />
                      )}
                      {selectedOption === option && !quizFeedback?.isCorrect && (
                        <XCircle size={18} color="var(--accent-danger)" />
                      )}
                    </button>
                  );
                })}
              </div>

              {quizFeedback && (
                <div style={{ marginTop: '20px', textAlign: 'center' }}>
                  <p style={{ 
                    fontSize: '16px', 
                    fontWeight: '700',
                    color: quizFeedback.isCorrect ? 'var(--accent-success)' : 'var(--accent-danger)',
                    marginBottom: '16px'
                  }}>
                    {quizFeedback.isCorrect ? '🎉 Tebrikler, Doğru Cevap!' : `❌ Yanlış. Doğru cevap: ${quizFeedback.correctAnswer}`}
                  </p>
                  <button className="btn-primary" style={{ margin: '0 auto' }} onClick={loadNextQuiz}>
                    Sıradaki Soru <ArrowRight size={16} />
                  </button>
                </div>
              )}
            </div>
          )}
        </main>
      )}

      {/* TAB 4: STATS & DAILY GOAL */}
      {activeTab === 'stats' && (
        <main style={{ maxWidth: '700px', margin: '0 auto' }}>
          <div style={{ textAlign: 'center', marginBottom: '24px' }}>
            <h2>📊 İlerleme ve Çalışma İstatistikleri</h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '14px', marginTop: '4px' }}>
              Kelime havuzunun durumu ve başarı oranı.
            </p>
          </div>

          {stats ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {/* SUMMARY GRID */}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '14px' }}>
                <div className="word-card" style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '28px', fontWeight: '800' }}>{stats.totalWords}</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Toplam Kelime</div>
                </div>
                <div className="word-card" style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '28px', fontWeight: '800', color: 'var(--accent-success)' }}>{stats.learnedCount}</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Öğrenilen</div>
                </div>
                <div className="word-card" style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '28px', fontWeight: '800', color: 'var(--accent-warning)' }}>{stats.needsReviewCount}</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Tekrar Gereken</div>
                </div>
                <div className="word-card" style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '28px', fontWeight: '800', color: 'var(--accent-danger)' }}>{stats.veryImportantCount}</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Zor / Hatalı</div>
                </div>
                <div className="word-card" style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '28px', fontWeight: '800', color: '#38bdf8' }}>%{stats.successRate}</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Başarı Oranı</div>
                </div>
              </div>

              {/* DAILY GOAL PROGRESS CARD */}
              <div className="word-card" style={{ padding: '24px' }}>
                <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '14px' }}>
                  🎯 Günlük Kelime Ekleme Hedefi
                </h3>
                
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px', fontSize: '14px' }}>
                  <span>Bugün Eklenen: <strong>{stats.dailyWordsAddedToday} kelime</strong></span>
                  <span>Hedef: <strong>{stats.dailyGoalTarget} kelime</strong></span>
                </div>

                <div style={{ 
                  background: 'var(--bg-input)', 
                  height: '10px', 
                  borderRadius: '5px', 
                  overflow: 'hidden', 
                  marginBottom: '10px' 
                }}>
                  <div style={{ 
                    width: `${Math.min(stats.dailyGoalProgress, 100)}%`, 
                    height: '100%', 
                    background: 'linear-gradient(90deg, var(--accent-primary), var(--accent-success))',
                    borderRadius: '5px',
                    transition: 'width 0.4s ease'
                  }} />
                </div>
                <p style={{ fontSize: '12px', color: 'var(--text-muted)', textAlign: 'right' }}>
                  Hedefin %{stats.dailyGoalProgress} kadarı tamamlandı!
                </p>

                <form onSubmit={handleUpdateDailyTarget} style={{ display: 'flex', gap: '10px', marginTop: '18px' }}>
                  <input 
                    type="number" 
                    className="input-field" 
                    style={{ width: '120px' }}
                    min="1" 
                    max="100" 
                    value={dailyTarget} 
                    onChange={e => setDailyTarget(e.target.value)} 
                  />
                  <button type="submit" className="btn-primary">Hedefi Güncelle</button>
                </form>
              </div>
            </div>
          ) : (
            <div className="loading-spinner">İstatistikler yükleniyor...</div>
          )}
        </main>
      )}
    </div>
  );
}

// API Client for English Vocab Learning App
const API_BASE = '/api';

function getHeaders() {
  const token = localStorage.getItem('vocab_token');
  const headers = {
    'Content-Type': 'application/json',
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
}

async function handleResponse(res) {
  if (res.status === 401) {
    // If unauthorized, clear token and return null or error
    localStorage.removeItem('vocab_token');
    localStorage.removeItem('vocab_user');
  }
  if (!res.ok) {
    let errMsg = `İstek başarısız (${res.status})`;
    try {
      const data = await res.json();
      if (data && (data.message || data.Message)) {
        errMsg = data.message || data.Message;
      }
    } catch {
      // ignore
    }
    throw new Error(errMsg);
  }
  const contentType = res.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    return await res.json();
  }
  return await res.text();
}

export const api = {
  // AUTH
  auth: {
    async demoLogin() {
      const res = await fetch(`${API_BASE}/auth/demo-login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      });
      const data = await handleResponse(res);
      if (data && data.token) {
        localStorage.setItem('vocab_token', data.token);
        localStorage.setItem('vocab_user', JSON.stringify({
          userName: data.userName || 'demo',
          email: data.email || 'demo@example.com'
        }));
      }
      return data;
    },

    async login(emailOrUserName, password) {
      const res = await fetch(`${API_BASE}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ emailOrUserName, password }),
      });
      const data = await handleResponse(res);
      if (data && data.token) {
        localStorage.setItem('vocab_token', data.token);
        localStorage.setItem('vocab_user', JSON.stringify({
          userName: data.userName,
          email: data.email
        }));
      }
      return data;
    },

    async register(userName, email, password) {
      const res = await fetch(`${API_BASE}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userName, email, password }),
      });
      return await handleResponse(res);
    },

    logout() {
      localStorage.removeItem('vocab_token');
      localStorage.removeItem('vocab_user');
    },

    getCurrentUser() {
      try {
        const u = localStorage.getItem('vocab_user');
        return u ? JSON.parse(u) : null;
      } catch {
        return null;
      }
    },

    isAuthenticated() {
      return !!localStorage.getItem('vocab_token');
    }
  },

  // WORDS
  words: {
    async getAll() {
      const res = await fetch(`${API_BASE}/words`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async getById(id) {
      const res = await fetch(`${API_BASE}/words/${id}`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async create(wordData) {
      const res = await fetch(`${API_BASE}/words`, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify(wordData),
      });
      return await handleResponse(res);
    },

    async update(id, wordData) {
      const res = await fetch(`${API_BASE}/words/${id}`, {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify({ id, ...wordData }),
      });
      return await handleResponse(res);
    },

    async delete(id) {
      const res = await fetch(`${API_BASE}/words/${id}`, {
        method: 'DELETE',
        headers: getHeaders(),
      });
      return await handleResponse(res);
    },

    async toggleFavorite(id) {
      const res = await fetch(`${API_BASE}/words/${id}/favorite`, {
        method: 'PUT',
        headers: getHeaders(),
      });
      return await handleResponse(res);
    },

    async getRandomForReview() {
      const res = await fetch(`${API_BASE}/words/random-review`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async submitReview(id, isCorrect) {
      const res = await fetch(`${API_BASE}/words/${id}/review?isCorrect=${isCorrect}`, {
        method: 'POST',
        headers: getHeaders(),
      });
      return await handleResponse(res);
    },
  },

  // QUIZZES
  quizzes: {
    async getMultipleChoice() {
      const res = await fetch(`${API_BASE}/quizzes/multiple-choice`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async getMatching() {
      const res = await fetch(`${API_BASE}/quizzes/matching`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async submitResult(wordId, isCorrect) {
      const res = await fetch(`${API_BASE}/quizzes/submit-result`, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify({ wordId, isCorrect }),
      });
      return await handleResponse(res);
    },
  },

  // STATISTICS
  stats: {
    async getSummary() {
      const res = await fetch(`${API_BASE}/statistics/summary`, { headers: getHeaders() });
      return await handleResponse(res);
    },

    async setDailyGoal(targetCount) {
      const res = await fetch(`${API_BASE}/statistics/daily-goal?targetCount=${targetCount}`, {
        method: 'POST',
        headers: getHeaders(),
      });
      return await handleResponse(res);
    },
  }
};

export const API_BASE = import.meta.env.VITE_API_BASE || window.__API_BASE || 'http://localhost:5001';

export async function apiRequest(path, options = {}) {
	const url = `${API_BASE}${path}`;
	const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
	const res = await fetch(url, { ...options, headers });
	const contentType = res.headers.get('content-type') || '';
	let data = null;
	if (contentType.includes('application/json')) {
		data = await res.json();
	}
	if (!res.ok) {
		throw new Error((data && (data.message || data.Message)) || `Request failed: ${res.status}`);
	}
	return data;
}

export function authHeaders() {
	const token = localStorage.getItem('token');
	return token ? { Authorization: `Bearer ${token}` } : {};
}


export const userAPI = {
  // Get user profile
  getProfile: async () => {
    return apiRequest("/api/User/profile", {  // Updated path
      method: "GET",
      headers: authHeaders(),
    })
  },

  // Update user profile
  updateProfile: async (data) => {
    return apiRequest("/api/User/profile", {  // Updated path
      method: "PUT",
      headers: authHeaders(),
      body: JSON.stringify(data),
    })
  },

  // Change password
  changePassword: async (data) => {
    return apiRequest("/api/User/change-password", {  // Updated path
      method: "POST",
      headers: authHeaders(),
      body: JSON.stringify(data),
    })
  },

  // Delete account
  deleteAccount: async () => {
    return apiRequest("/api/User", {  // Updated path
      method: "DELETE",
      headers: authHeaders(),
    })
  },

  // Get user transactions
  getTransactions: async () => {
    return apiRequest("/api/User/transactions", {  // Updated path
      method: "GET",
      headers: authHeaders(),
    })
  },
}

export const favoritesAPI = {
  // Get user favorites
  getFavorites: async () => {
    return apiRequest("/api/Favorites", {
      method: "GET",
      headers: authHeaders(),
    })
  },

  // Add doctor to favorites
  addFavorite: async (doctorId) => {
    return apiRequest(`/api/Favorites/${doctorId}`, {
      method: "POST",
      headers: authHeaders(),
    })
  },

  // Remove doctor from favorites
  removeFavorite: async (doctorId) => {
    return apiRequest(`/api/Favorites/${doctorId}`, {
      method: "DELETE",
      headers: authHeaders(),
    })
  },

  // Check if doctor is favorite
  checkFavorite: async (doctorId) => {
    return apiRequest(`/api/Favorites/check/${doctorId}`, {
      method: "GET",
      headers: authHeaders(),
    })
  },
}
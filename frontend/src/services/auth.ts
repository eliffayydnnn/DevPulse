import type {
  ApiResponse,
  AuthResponseDto,
  LoginRequest,
  RegisterRequest,
  SavedRepositoryDto,
  SaveRepositoryRequest,
  UserDto,
} from '../types/api';

const TOKEN_KEY = 'devpulse_auth_token';
const USER_KEY = 'devpulse_auth_user';
const API_BASE = typeof window !== 'undefined' ? '/api' : 'http://localhost:5000/api';

export function getStoredToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function getStoredUser(): UserDto | null {
  if (typeof window === 'undefined') return null;
  const userJson = localStorage.getItem(USER_KEY);
  if (!userJson) return null;
  try {
    return JSON.parse(userJson) as UserDto;
  } catch {
    return null;
  }
}

export function setSession(token: string, user: UserDto): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearSession(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

function getAuthHeaders(): HeadersInit {
  const token = getStoredToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
}

export async function loginUser(request: LoginRequest): Promise<AuthResponseDto> {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.detail || errorData.title || errorData.errorMessage || 'Invalid credentials.');
  }

  const json: ApiResponse<AuthResponseDto> = await res.json();
  setSession(json.data.token, json.data.user);
  return json.data;
}

export async function registerUser(request: RegisterRequest): Promise<AuthResponseDto> {
  const res = await fetch(`${API_BASE}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.detail || errorData.title || errorData.errorMessage || 'Registration failed.');
  }

  const json: ApiResponse<AuthResponseDto> = await res.json();
  setSession(json.data.token, json.data.user);
  return json.data;
}

export async function fetchCurrentUser(): Promise<UserDto | null> {
  const token = getStoredToken();
  if (!token) return null;

  try {
    const res = await fetch(`${API_BASE}/auth/me`, {
      headers: getAuthHeaders(),
    });

    if (!res.ok) {
      if (res.status === 401) {
        clearSession();
      }
      return null;
    }

    const json: ApiResponse<UserDto> = await res.json();
    localStorage.setItem(USER_KEY, JSON.stringify(json.data));
    return json.data;
  } catch {
    return getStoredUser();
  }
}

export async function getSavedRepositories(): Promise<SavedRepositoryDto[]> {
  const token = getStoredToken();
  if (!token) return [];

  const res = await fetch(`${API_BASE}/saved-repositories`, {
    headers: getAuthHeaders(),
  });

  if (!res.ok) {
    if (res.status === 401) {
      clearSession();
    }
    throw new Error('Failed to load bookmarked repositories.');
  }

  const json: ApiResponse<SavedRepositoryDto[]> = await res.json();
  return json.data;
}

export async function saveRepository(request: SaveRepositoryRequest): Promise<SavedRepositoryDto> {
  const token = getStoredToken();
  if (!token) {
    throw new Error('Please sign in to bookmark repositories.');
  }

  const res = await fetch(`${API_BASE}/saved-repositories`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.detail || errorData.title || errorData.errorMessage || 'Failed to bookmark repository.');
  }

  const json: ApiResponse<SavedRepositoryDto> = await res.json();
  return json.data;
}

export async function removeSavedRepository(id: string): Promise<boolean> {
  const token = getStoredToken();
  if (!token) {
    throw new Error('Please sign in to manage bookmarks.');
  }

  const res = await fetch(`${API_BASE}/saved-repositories/${id}`, {
    method: 'DELETE',
    headers: getAuthHeaders(),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData.detail || errorData.title || 'Failed to remove bookmark.');
  }

  return true;
}

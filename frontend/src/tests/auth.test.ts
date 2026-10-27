import { describe, it, expect, beforeEach } from 'vitest';
import {
  getStoredToken,
  getStoredUser,
  setSession,
  clearSession,
} from '../services/auth';

describe('Frontend Authentication Service', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('setSession stores token and user in localStorage', () => {
    const mockUser = {
      id: 'test-user-id',
      username: 'testdeveloper',
      email: 'test@devpulse.io',
      createdAt: new Date().toISOString(),
    };
    const mockToken = 'mock.jwt.token.string';

    setSession(mockToken, mockUser);

    expect(getStoredToken()).toBe(mockToken);
    expect(getStoredUser()).toEqual(mockUser);
  });

  it('clearSession removes token and user from localStorage', () => {
    const mockUser = {
      id: 'test-user-id',
      username: 'testdeveloper',
      email: 'test@devpulse.io',
      createdAt: new Date().toISOString(),
    };
    setSession('some-token', mockUser);
    expect(getStoredToken()).not.toBeNull();

    clearSession();

    expect(getStoredToken()).toBeNull();
    expect(getStoredUser()).toBeNull();
  });

  it('getStoredUser returns null when storage is corrupted or empty', () => {
    expect(getStoredUser()).toBeNull();

    localStorage.setItem('devpulse_auth_user', 'invalid-json{');
    expect(getStoredUser()).toBeNull();
  });
});

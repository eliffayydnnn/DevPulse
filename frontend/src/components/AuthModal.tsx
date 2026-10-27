import React, { useState } from 'react';
import { X, Lock, Mail, User as UserIcon, LogIn, UserPlus, AlertCircle } from 'lucide-react';
import { loginUser, registerUser } from '../services/auth';
import type { UserDto } from '../types/api';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAuthSuccess: (user: UserDto) => void;
}

export const AuthModal: React.FC<AuthModalProps> = ({ isOpen, onClose, onAuthSuccess }) => {
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      if (mode === 'login') {
        if (!username.trim() || !password) {
          throw new Error('Please fill in both username/email and password.');
        }
        const res = await loginUser({ usernameOrEmail: username.trim(), password });
        onAuthSuccess(res.user);
        onClose();
      } else {
        if (!username.trim() || !email.trim() || !password) {
          throw new Error('All fields are required.');
        }
        if (password !== confirmPassword) {
          throw new Error('Passwords do not match.');
        }
        if (password.length < 6) {
          throw new Error('Password must be at least 6 characters.');
        }
        const res = await registerUser({
          username: username.trim(),
          email: email.trim(),
          password,
        });
        onAuthSuccess(res.user);
        onClose();
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Authentication failed.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        backgroundColor: 'rgba(3, 7, 18, 0.75)',
        backdropFilter: 'blur(8px)',
        zIndex: 9999,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '1rem',
      }}
      onClick={onClose}
    >
      <div
        className="glass-card"
        style={{
          width: '100%',
          maxWidth: '420px',
          padding: '2rem',
          position: 'relative',
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.7)',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Close Button */}
        <button
          type="button"
          onClick={onClose}
          style={{
            position: 'absolute',
            top: '1.25rem',
            right: '1.25rem',
            background: 'transparent',
            border: 'none',
            color: 'var(--text-muted)',
            cursor: 'pointer',
            padding: '0.25rem',
            borderRadius: '6px',
          }}
        >
          <X size={20} />
        </button>

        {/* Modal Header */}
        <div style={{ marginBottom: '1.5rem', textAlign: 'center' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: 700, color: 'var(--text-primary)', margin: 0 }}>
            {mode === 'login' ? 'Welcome Back' : 'Create an Account'}
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', marginTop: '0.5rem' }}>
            {mode === 'login'
              ? 'Sign in to access bookmarked repositories and team insights'
              : 'Join DevPulse to save analytics snapshots and favorites'}
          </p>
        </div>

        {/* Tabs */}
        <div
          style={{
            display: 'flex',
            background: 'rgba(255, 255, 255, 0.05)',
            padding: '0.25rem',
            borderRadius: '8px',
            marginBottom: '1.5rem',
          }}
        >
          <button
            type="button"
            onClick={() => {
              setMode('login');
              setError(null);
            }}
            style={{
              flex: 1,
              padding: '0.5rem',
              border: 'none',
              borderRadius: '6px',
              fontSize: '0.875rem',
              fontWeight: 500,
              cursor: 'pointer',
              background: mode === 'login' ? 'var(--primary-color)' : 'transparent',
              color: mode === 'login' ? '#ffffff' : 'var(--text-muted)',
              transition: 'all 0.2s',
            }}
          >
            Sign In
          </button>
          <button
            type="button"
            onClick={() => {
              setMode('register');
              setError(null);
            }}
            style={{
              flex: 1,
              padding: '0.5rem',
              border: 'none',
              borderRadius: '6px',
              fontSize: '0.875rem',
              fontWeight: 500,
              cursor: 'pointer',
              background: mode === 'register' ? 'var(--primary-color)' : 'transparent',
              color: mode === 'register' ? '#ffffff' : 'var(--text-muted)',
              transition: 'all 0.2s',
            }}
          >
            Register
          </button>
        </div>

        {/* Error Banner */}
        {error && (
          <div
            style={{
              backgroundColor: 'rgba(239, 68, 68, 0.1)',
              border: '1px solid rgba(239, 68, 68, 0.3)',
              borderRadius: '8px',
              padding: '0.75rem',
              marginBottom: '1rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              color: '#f87171',
              fontSize: '0.85rem',
            }}
          >
            <AlertCircle size={16} style={{ flexShrink: 0 }} />
            <span>{error}</span>
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem' }}>
              {mode === 'login' ? 'Username or Email' : 'Username'}
            </label>
            <div style={{ position: 'relative' }}>
              <UserIcon
                size={16}
                style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }}
              />
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder={mode === 'login' ? 'eliffaydn or user@example.com' : 'johndoe'}
                required
                style={{
                  width: '100%',
                  padding: '0.625rem 0.75rem 0.625rem 2.25rem',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: '6px',
                  color: 'var(--text-primary)',
                  fontSize: '0.9rem',
                  boxSizing: 'border-box',
                }}
              />
            </div>
          </div>

          {mode === 'register' && (
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem' }}>
                Email Address
              </label>
              <div style={{ position: 'relative' }}>
                <Mail
                  size={16}
                  style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }}
                />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="name@company.com"
                  required
                  style={{
                    width: '100%',
                    padding: '0.625rem 0.75rem 0.625rem 2.25rem',
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: '6px',
                    color: 'var(--text-primary)',
                    fontSize: '0.9rem',
                    boxSizing: 'border-box',
                  }}
                />
              </div>
            </div>
          )}

          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem' }}>
              Password
            </label>
            <div style={{ position: 'relative' }}>
              <Lock
                size={16}
                style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }}
              />
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                required
                style={{
                  width: '100%',
                  padding: '0.625rem 0.75rem 0.625rem 2.25rem',
                  background: 'rgba(255, 255, 255, 0.05)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: '6px',
                  color: 'var(--text-primary)',
                  fontSize: '0.9rem',
                  boxSizing: 'border-box',
                }}
              />
            </div>
          </div>

          {mode === 'register' && (
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem' }}>
                Confirm Password
              </label>
              <div style={{ position: 'relative' }}>
                <Lock
                  size={16}
                  style={{ position: 'absolute', left: '0.75rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }}
                />
                <input
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="••••••••"
                  required
                  style={{
                    width: '100%',
                    padding: '0.625rem 0.75rem 0.625rem 2.25rem',
                    background: 'rgba(255, 255, 255, 0.05)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: '6px',
                    color: 'var(--text-primary)',
                    fontSize: '0.9rem',
                    boxSizing: 'border-box',
                  }}
                />
              </div>
            </div>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="search-btn"
            style={{
              marginTop: '0.5rem',
              width: '100%',
              justifyContent: 'center',
              padding: '0.75rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
            }}
          >
            {isLoading ? (
              <span className="spinner" style={{ width: '16px', height: '16px' }} />
            ) : mode === 'login' ? (
              <>
                <LogIn size={16} />
                <span>Sign In</span>
              </>
            ) : (
              <>
                <UserPlus size={16} />
                <span>Create Account</span>
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
};

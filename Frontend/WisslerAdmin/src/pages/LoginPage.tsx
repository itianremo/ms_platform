import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { AuthService } from '../services/authService';

const LoginPage: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const data = await AuthService.login({ email, password });
            if (data.token) {
                login(data.token);
                navigate('/');
            } else {
                setError('Invalid credentials');
            }
        } catch (err: any) {
            console.error(err);
            setError(err.response?.data?.message || 'Login failed');
        }
    };

    const handleSocialLogin = (provider: string) => {
        // Redirect to Backend for OAuth flow via Gateway
        window.location.href = `http://localhost:7032/api/externalauth/login/${provider}`;
    };

    return (
        <div style={{ display: 'flex', minHeight: '100vh', alignItems: 'center', justifyContent: 'center', backgroundColor: '#f3f4f6' }}>
            <div style={{ padding: '2rem', backgroundColor: 'white', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)', width: '100%', maxWidth: '400px' }}>
                <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '1rem' }}>
                    <img src="/logo.png" alt="Wissler Logo" style={{ height: '48px', objectFit: 'contain' }} />
                </div>
                <h2 style={{ textAlign: 'center', marginBottom: '1.5rem', fontSize: '1.5rem', fontWeight: 'bold' }}>Sign In to Wissler Admin</h2>

                {error && <div style={{ marginBottom: '1rem', padding: '0.5rem', backgroundColor: '#fee2e2', color: '#b91c1c', borderRadius: '4px' }}>{error}</div>}

                <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                    <input
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                        style={{ padding: '0.75rem', border: '1px solid #d1d5db', borderRadius: '4px' }}
                    />
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                        style={{ padding: '0.75rem', border: '1px solid #d1d5db', borderRadius: '4px' }}
                    />
                    <button type="submit" style={{ padding: '0.75rem', backgroundColor: '#2563eb', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                        Sign In
                    </button>
                    <div style={{ textAlign: 'center', marginTop: '0.5rem' }}>
                        <a href="#" style={{ color: '#2563eb', textDecoration: 'none', fontSize: '0.875rem' }} onClick={(e) => { e.preventDefault(); alert('Forgot password flow to be implemented'); }}>Forgot Password?</a>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;

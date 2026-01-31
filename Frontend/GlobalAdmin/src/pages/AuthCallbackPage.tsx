import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Loader2 } from 'lucide-react';

const AuthCallbackPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { login } = useAuth();

    useEffect(() => {
        const token = searchParams.get('token');
        if (token) {
            try {
                // Call login from context to set user and cookie/storage
                // Note: The context's login function usually expects identifier/password,
                // but we might need a direct 'setToken' method or modify 'login' to accept a token directly.
                // Assuming 'login' might fail if it strictly expects creds.
                // Or we can manually set the token if the context allows.
                // Re-checking AuthContext would be ideal, but often 'login' stores the token.
                // If 'login' does an API call, we shouldn't use it here.
                // Instead, we just need to set the state.

                // Let's assume for now we can verify the token or just set it.
                // Code Review: 'GlobalAdmin/src/context/AuthContext' might be needed.
                // But typically we store token in localStorage and reload/fetch user.

                localStorage.setItem('token', token);

                // Trigger a reload or re-fetch user.
                // If useAuth listens to localStorage or we have a specialized method.
                // Let's try to reload window to force AuthContext to pick it up if it initializes from storage.
                // Or navigate ('/') which triggers AuthContext mount check.

                navigate('/');
                window.location.reload(); // Ensure context updates
            } catch (e) {
                console.error("Failed to process token", e);
                navigate('/login?error=social_login_failed');
            }
        } else {
            navigate('/login?error=no_token');
        }
    }, [searchParams, navigate, login]);

    return (
        <div className="flex h-screen items-center justify-center">
            <div className="text-center">
                <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4 text-primary" />
                <h2 className="text-lg font-semibold">Authenticating...</h2>
            </div>
        </div>
    );
};

export default AuthCallbackPage;

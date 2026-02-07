import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const AuthCallbackPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { login } = useAuth(); // Assuming FitIT uses same context hook

    useEffect(() => {
        const token = searchParams.get('token');
        if (token) {
            try {
                login(token); // FitIT Context likely takes token directly as seen in LoginPage
                navigate('/');
            } catch (e) {
                console.error("Failed to process token", e);
                navigate('/login?error=social_login_failed');
            }
        } else {
            navigate('/login?error=no_token');
        }
    }, [searchParams, navigate, login]);

    return (
        <div style={{ display: 'flex', minHeight: '100vh', alignItems: 'center', justifyContent: 'center' }}>
            <p>Authenticating...</p>
        </div>
    );
};

export default AuthCallbackPage;

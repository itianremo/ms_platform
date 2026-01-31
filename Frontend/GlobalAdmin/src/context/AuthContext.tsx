import React, { createContext, useContext, useState, useEffect } from 'react';
import { AuthService } from '../services/authService';

interface User {
    id: string;
    email: string;
    name: string;
    roles: string[];
    phone?: string;
    avatarUrl?: string;
    bio?: string;
    isEmailVerified?: boolean;
    isPhoneVerified?: boolean;
}

interface AuthContextType {
    isAuthenticated: boolean;
    token: string | null;
    user: User | null;
    login: (identifier: string, password: string) => Promise<void>;
    register: (email: string, phone: string, password: string, verificationType?: 'None' | 'Email' | 'Phone' | 'Both') => Promise<void>;
    forgotPassword: (email: string) => Promise<void>;
    logout: () => void;
    isLoading: boolean;
    updateUser: (updates: Partial<User>) => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [token, setToken] = useState<string | null>(localStorage.getItem('admin_token'));
    const [user, setUser] = useState<User | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const parseJwt = (token: string) => {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(jsonPayload);
        } catch (e) {
            return null;
        }
    };

    const setUserFromToken = (token: string) => {
        const payload = parseJwt(token);
        if (payload) {
            // Map claims
            const roleClaim = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload.role || [];
            const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];

            setUser({
                id: payload.sub,
                email: payload.email,
                name: payload.name || payload.email.split('@')[0], // Fallback name
                roles: roles,
                phone: payload.phone,
                isEmailVerified: payload.email_verified === 'true',
                isPhoneVerified: payload.phone_verified === 'true'
            });
        }
    };

    useEffect(() => {
        const initAuth = async () => {
            // Simulate small delay or check token validity?
            if (token) {
                localStorage.setItem('admin_token', token);
                setUserFromToken(token);
            } else {
                localStorage.removeItem('admin_token');
                setUser(null);
            }
            setIsLoading(false);
        };

        initAuth();
    }, [token]);

    const login = async (identifier: string, password: string) => {
        try {
            // Pass System App ID (Global Admin)
            const appId = "00000000-0000-0000-0000-000000000001";
            const data = await AuthService.login({ email: identifier, password, appId });

            if (data && (data.token || data.accessToken)) {
                const tokenToUse = data.accessToken || data.token;
                setToken(tokenToUse);
                // Also set localStorage immediately here to ensure persistence across reloads/navigations if state update is slow
                localStorage.setItem('admin_token', tokenToUse);
                setUserFromToken(tokenToUse);
            }
        } catch (error) {
            console.error("Login failed:", error);
            throw error;
        }
    };

    const register = async (email: string, phone: string, password: string, verificationType?: 'None' | 'Email' | 'Phone' | 'Both') => {
        try {
            await AuthService.register({
                email,
                password,
                phone,
                appId: "00000000-0000-0000-0000-000000000001", // Default to System App for Global Admin registration
                verificationType
            });
        } catch (error) {
            console.error("Registration failed:", error);
            throw error;
        }
    };

    const forgotPassword = async (email: string) => {
        console.warn("Forgot Password endpoint not implemented on backend yet.");
        await new Promise(resolve => setTimeout(resolve, 500));
    };

    const logout = () => {
        AuthService.logout();
        setToken(null);
        setUser(null);
        localStorage.removeItem('admin_token');
    };

    const updateUser = (updates: Partial<User>) => {
        if (user) {
            setUser({ ...user, ...updates });
        }
    };

    return (
        <AuthContext.Provider value={{ isAuthenticated: !!token, token, user, login, register, forgotPassword, logout, isLoading, updateUser }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) throw new Error('useAuth must be used within an AuthProvider');
    return context;
};

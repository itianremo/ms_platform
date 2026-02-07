import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, ArrowRight, Eye, EyeOff, Loader2 } from 'lucide-react';

const LoginPage = () => {
    const [identifier, setIdentifier] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const navigate = useNavigate();

    const validateIdentifier = (input: string) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        // Phone: E.164-ish (starts with +, 7-15 digits)
        const phoneRegex = /^\+[1-9]\d{7,14}$/;

        if (input.includes('@')) {
            return emailRegex.test(input);
        }
        return phoneRegex.test(input);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!validateIdentifier(identifier)) {
            setError('Please enter a valid email or phone number with country code (e.g. +123456789).');
            return;
        }

        setLoading(true);
        try {
            await login(identifier, password);
            navigate('/');
        } catch (err: any) {
            console.error('Login failed object:', err);
            if (err.response) {
                console.log('Login error response status:', err.response.status);
                console.log('Login error response data:', err.response.data);
            }
            // Handle Requirements
            if (err.response?.status === 403 && err.response?.data?.error === "RequiresVerification") {
                // Pass the identifier (email/phone) to validation page
                // We should assume the user uses the same identifier to request OTP
                // Logic: Redirect to /verify with state
                const pendingStatus = err.response.data.status;
                const phoneFromBackend = err.response.data.phone; // New field
                // Ensure we pass phone if available. If login identifier was phone, use that too.
                const phoneToPass = phoneFromBackend || (validateIdentifier(identifier) && !identifier.includes('@') ? identifier : '');

                navigate('/verify', { state: { email: identifier, phone: phoneToPass, type: pendingStatus } });
                return;
            }
            if (err.response?.status === 403) {
                const errorData = err.response.data;
                const errorCode = errorData?.error || errorData?.Error;

                console.log("Checking 403 Error Code:", errorCode);

                if (errorCode === "RequiresAdminApproval") {
                    setError('Your account is pending admin approval.');
                    return;
                }

                if (errorCode === "AccountBanned") {
                    setError('Your account has been banned.');
                    return;
                }

                if (errorCode === "AccountSoftDeleted") {
                    navigate('/reactivate-init', { state: { email: identifier } });
                    return;
                }
            }

            setError('Invalid credentials. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
                <div className="rounded-md bg-destructive/15 p-3 text-sm text-destructive text-center font-medium">
                    {error}
                </div>
            )}
            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Email or Phone Number</label>
                <input
                    type="text"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={identifier}
                    onChange={(e) => setIdentifier(e.target.value)}
                    placeholder="name@example.com or +1234567890"
                    required
                />
            </div>
            <div className="space-y-2">
                <div className="flex items-center justify-between">
                    <label className="text-sm font-medium leading-none text-muted-foreground">Password</label>
                    <Link to="/forgot-password" className="text-sm font-medium text-primary hover:underline">
                        Forgot Password?
                    </Link>
                </div>
                <div className="relative">
                    <input
                        type={showPassword ? "text" : "password"}
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 pr-10 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                    <button
                        type="button"
                        onClick={() => setShowPassword(!showPassword)}
                        className="absolute right-3 top-2.5 text-muted-foreground hover:text-foreground"
                    >
                        {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                </div>
            </div>

            <button
                disabled={loading}
                className="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2 w-full gap-2"
            >
                {loading ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                    <LogIn size={20} />
                )}
                <span>{loading ? 'Signing In...' : 'Sign In'}</span>
            </button>

            <div className="text-center text-sm text-muted-foreground">
                Don't have an account?{' '}
                <Link to="/register" className="font-medium text-primary hover:underline">Create Account</Link>
            </div>

            <div className="relative">
                <div className="absolute inset-0 flex items-center">
                    <span className="w-full border-t" />
                </div>
                <div className="relative flex justify-center text-xs uppercase">
                    <span className="bg-background px-2 text-muted-foreground">Or continue with</span>
                </div>
            </div>

            <button
                type="button"
                onClick={() => window.location.href = `${process.env.REACT_APP_API_URL || 'http://localhost:7032/api'}/externalauth/login/Google`}
                className="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 border border-input bg-background hover:bg-accent hover:text-accent-foreground h-10 px-4 py-2 w-full gap-2"
            >
                <svg viewBox="0 0 24 24" className="h-5 w-5" fill="currentColor">
                    <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
                    <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
                    <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
                    <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
                </svg>
                Google
            </button>
        </form>
    );
};

export default LoginPage;

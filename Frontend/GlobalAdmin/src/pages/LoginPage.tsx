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
                    setError('Your account is deactivated. Please contact support to reactivate.');
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
        </form>
    );
};

export default LoginPage;

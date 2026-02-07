import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Mail, ArrowLeft, KeyRound, Loader2, Eye, EyeOff } from 'lucide-react';

const ForgotPasswordPage = () => {
    const [step, setStep] = useState<'request' | 'reset'>('request');
    const [email, setEmail] = useState('');
    const [code, setCode] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');

    const { forgotPassword, resetPassword } = useAuth();
    const navigate = useNavigate();

    const handleRequest = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            await forgotPassword(email);
            setStep('reset');
            setMessage(`Reset code sent to ${email}`);
        } catch (err) {
            setError('Failed to send reset code. Please try again.');
        } finally {
            setLoading(false);
        }
    };

    const handleReset = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            await resetPassword(email, code, newPassword);
            navigate('/login');
        } catch (err) {
            setError('Failed to reset password. Invalid code or generic error.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="space-y-2 text-center">
                <h2 className="text-xl font-semibold tracking-tight text-foreground">
                    {step === 'request' ? 'Forgot password?' : 'Reset Password'}
                </h2>
                <p className="text-sm text-muted-foreground">
                    {step === 'request'
                        ? "No worries, we'll send you reset instructions."
                        : "Enter the code sent to your email and your new password."}
                </p>
            </div>

            {message && <div className="p-3 text-sm text-green-600 bg-green-50 rounded text-center">{message}</div>}
            {error && <div className="p-3 text-sm text-red-600 bg-red-50 rounded text-center">{error}</div>}

            {step === 'request' ? (
                <form onSubmit={handleRequest} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium leading-none text-muted-foreground">Email</label>
                        <input
                            type="email"
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Enter your email"
                            required
                        />
                    </div>
                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full inline-flex items-center justify-center rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2"
                    >
                        {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        Send Reset Code
                    </button>
                </form>
            ) : (
                <form onSubmit={handleReset} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium leading-none text-muted-foreground">Reset Code</label>
                        <input
                            type="text"
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                            value={code}
                            onChange={(e) => setCode(e.target.value)}
                            placeholder="123456"
                            required
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-sm font-medium leading-none text-muted-foreground">New Password</label>
                        <div className="relative">
                            <input
                                type={showPassword ? "text" : "password"}
                                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 pr-10 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                                required
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="absolute right-3 top-2.5 text-muted-foreground"
                            >
                                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                            </button>
                        </div>
                    </div>
                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full inline-flex items-center justify-center rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2"
                    >
                        {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        Reset Password
                    </button>
                </form>
            )}

            <div className="text-center">
                <Link to="/login" className="inline-flex items-center justify-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors">
                    <ArrowLeft size={16} />
                    Back to Login
                </Link>
            </div>
        </div>
    );
};

export default ForgotPasswordPage;

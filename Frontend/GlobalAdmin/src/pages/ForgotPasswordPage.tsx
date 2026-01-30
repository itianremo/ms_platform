import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Mail, ArrowLeft } from 'lucide-react';

const ForgotPasswordPage = () => {
    const [email, setEmail] = useState('');
    const [sent, setSent] = useState(false);

    const { forgotPassword } = useAuth();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        await forgotPassword(email);
        setSent(true);
    };

    if (sent) {
        return (
            <div className="text-center space-y-6">
                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-500/10 text-green-600">
                    <Mail size={32} />
                </div>
                <div className="space-y-2">
                    <h2 className="text-xl font-semibold tracking-tight text-foreground">Check your email</h2>
                    <p className="text-muted-foreground">
                        We sent a password reset link to <strong className="font-medium text-foreground">{email}</strong>
                    </p>
                </div>
                <Link to="/login" className="inline-flex w-full items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
                    Back to Login
                </Link>
            </div>
        );
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2 text-center">
                <h2 className="text-xl font-semibold tracking-tight text-foreground">Forgot password?</h2>
                <p className="text-sm text-muted-foreground">
                    No worries, we'll send you reset instructions.
                </p>
            </div>

            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Email</label>
                <input
                    type="email"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Enter your email"
                    required
                />
            </div>

            <button
                type="submit"
                className="inline-flex w-full items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2"
            >
                <span>Reset Password</span>
            </button>

            <div className="text-center">
                <Link to="/login" className="inline-flex items-center justify-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors">
                    <ArrowLeft size={16} />
                    Back to Login
                </Link>
            </div>
        </form>
    );
};

export default ForgotPasswordPage;

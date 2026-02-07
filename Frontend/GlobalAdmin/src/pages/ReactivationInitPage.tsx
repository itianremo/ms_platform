import React, { useState, useEffect } from 'react';
import { useLocation, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Loader2, ArrowLeft, Mail, AlertCircle } from 'lucide-react';

const ReactivationInitPage = () => {
    const location = useLocation();
    const [oldEmail, setOldEmail] = useState('');
    const [newEmail, setNewEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const { initiateReactivation } = useAuth();

    useEffect(() => {
        if (location.state?.email) {
            setOldEmail(location.state.email);
            // Pre-fill new email if same? Or blank? User might want to keep same email.
            // Prompt says: "mask and complete his email ... type a new email".
            // Let's assume user types new email.
        }
    }, [location.state]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            await initiateReactivation(oldEmail, newEmail);
            setSuccess(true);
        } catch (err: any) {
            setError(err.response?.data?.Message || 'Failed to initiate reactivation. Please check your inputs.');
        } finally {
            setLoading(false);
        }
    };

    if (success) {
        return (
            <div className="text-center space-y-6">
                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-500/10 text-green-600">
                    <Mail size={32} />
                </div>
                <div className="space-y-2">
                    <h2 className="text-xl font-semibold tracking-tight text-foreground">Verify your email</h2>
                    <p className="text-muted-foreground">
                        We sent a verification link to <strong className="font-medium text-foreground">{newEmail}</strong>.
                        <br />Please check your inbox to reactivate your account.
                    </p>
                </div>
                <Link to="/login" className="inline-flex w-full items-center justify-center whitespace-nowrap rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
                    Back to Login
                </Link>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="space-y-2 text-center">
                <h2 className="text-xl font-semibold tracking-tight text-foreground">Reactivate Account</h2>
                <p className="text-sm text-muted-foreground">
                    Your account is currently deactivated. To reactivate, please confirm your identity and updated email.
                </p>
            </div>

            {error && <div className="p-3 text-sm text-red-600 bg-red-50 rounded text-center flex items-center justify-center gap-2"><AlertCircle size={16} /> {error}</div>}

            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                    <label className="text-sm font-medium leading-none text-muted-foreground">Current Email (for verification)</label>
                    <input
                        type="email"
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                        value={oldEmail}
                        onChange={(e) => setOldEmail(e.target.value)}
                        placeholder="Current/Old Email"
                        required
                        disabled={!!location.state?.email} // Disable if passed from login? User might need to type it to confirm identity? 
                        // Prompt says "mask and complete his email". For security, typing it validates ownership partially? 
                        // But if passed from login, we know it matches the soft deleted attempt.
                        // I'll leave it editable if not passed, but readonly if passed to simplify.
                        readOnly={!!location.state?.email}
                    />
                </div>
                <div className="space-y-2">
                    <label className="text-sm font-medium leading-none text-muted-foreground">New Email (or confirm same)</label>
                    <input
                        type="email"
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                        value={newEmail}
                        onChange={(e) => setNewEmail(e.target.value)}
                        placeholder="Enter new email address"
                        required
                    />
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="w-full inline-flex items-center justify-center rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2"
                >
                    {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                    Send Verification Link
                </button>
            </form>

            <div className="text-center">
                <Link to="/login" className="inline-flex items-center justify-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors">
                    <ArrowLeft size={16} />
                    Back to Login
                </Link>
            </div>
        </div>
    );
};

export default ReactivationInitPage;

import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';

const ReactivationVerifyPage = () => {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const email = searchParams.get('email');

    const [loading, setLoading] = useState(true);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState('');

    const { verifyReactivation } = useAuth();

    useEffect(() => {
        const verify = async () => {
            if (!token || !email) {
                setError('Invalid link parameters.');
                setLoading(false);
                return;
            }

            try {
                await verifyReactivation(email, token);
                setSuccess(true);
            } catch (err: any) {
                setError(err.response?.data?.Message || 'Verification failed. Link may be expired.');
            } finally {
                setLoading(false);
            }
        };

        verify();
    }, [token, email]);

    return (
        <div className="flex flex-col items-center justify-center space-y-6 text-center">
            {loading ? (
                <>
                    <Loader2 className="h-12 w-12 animate-spin text-primary" />
                    <h2 className="text-xl font-semibold">Verifying your account...</h2>
                    <p className="text-muted-foreground">Please wait while we restore your access.</p>
                </>
            ) : success ? (
                <>
                    <div className="h-16 w-16 rounded-full bg-green-500/10 flex items-center justify-center text-green-600">
                        <CheckCircle size={32} />
                    </div>
                    <div className="space-y-2">
                        <h2 className="text-xl font-semibold">Account Reactivated!</h2>
                        <p className="text-muted-foreground">Your account has been successfully restored.</p>
                    </div>
                    <Link to="/login" className="inline-flex w-full items-center justify-center whitespace-nowrap rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
                        Proceed to Login
                    </Link>
                </>
            ) : (
                <>
                    <div className="h-16 w-16 rounded-full bg-red-500/10 flex items-center justify-center text-red-600">
                        <XCircle size={32} />
                    </div>
                    <div className="space-y-2">
                        <h2 className="text-xl font-semibold">Reactivation Failed</h2>
                        <p className="text-muted-foreground">{error}</p>
                    </div>
                    <Link to="/reactivate-init" className="text-primary hover:underline">
                        Try Again
                    </Link>
                </>
            )}
        </div>
    );
};

export default ReactivationVerifyPage;

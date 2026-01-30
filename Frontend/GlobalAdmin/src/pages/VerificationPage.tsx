import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { AuthService } from '../services/authService';
import { SettingsService } from '../services/settingsService';
import { useToast } from '../context/ToastContext';
import { ShieldCheck, ArrowRight, Loader2, Send } from 'lucide-react';
import AuthLayout from '../layouts/AuthLayout';

type VerificationStatus = 'PendingEmailVerification' | 'PendingMobileVerification' | 'PendingAccountVerification';

export default function VerificationPage() {
    const { state } = useLocation();
    const navigate = useNavigate();
    const { showToast } = useToast();

    // State from redirection
    const email = state?.email || '';
    const phoneFromState = state?.phone || '';
    // If 'PendingAccountVerification', user might choose. For MVP, we default to Email if available or ask.
    // If PendingEmail -> Email
    // If PendingMobile -> Phone
    const initialStatus = state?.type as VerificationStatus;

    const [otp, setOtp] = useState('');
    const [loading, setLoading] = useState(false);
    const [sendingOtp, setSendingOtp] = useState(false);
    const [step, setStep] = useState<'select' | 'enter-otp'>('select'); // 'select' only if choice needed
    const [selectedMethod, setSelectedMethod] = useState<'Email' | 'Phone'>('Email');
    const [otpSent, setOtpSent] = useState(false);

    // Masking Helper
    const maskPhoneNumber = (phone: string) => {
        if (!phone) return "(Hidden Number)";
        // "Country code and first 4 digits and last 3"
        // Heuristic: Show first 8 chars (covers CC+4) and last 3.
        if (phone.length <= 11) return phone; // Too short to mask nicely
        return `${phone.slice(0, 8)}****${phone.slice(-3)}`;
    };


    useEffect(() => {
        if (!email) {
            navigate('/login');
        }

        // Determine method based on Status
        if (initialStatus === 'PendingEmailVerification') {
            setSelectedMethod('Email');
            setStep('enter-otp');
        } else if (initialStatus === 'PendingMobileVerification') {
            setSelectedMethod('Phone');
            setStep('enter-otp');
        } else {
            // PendingAccountVerification (Both possible or required)
            // Requirement: "if both needed, he can choose between them"
            setStep('select');
        }
    }, [email, initialStatus, navigate]);

    const checkMaintenanceMode = async (method: 'Email' | 'Phone') => {
        if (method === 'Email') {
            const config = await SettingsService.getSmtpConfig();
            // Check if config is returned and valid (UI check)
            // Backend might return null if not configured
            if (!config) {
                showToast("Maintenance Mode 1: Email Verification System is currently unavailable.", "error");
                return false;
            }
        } else {
            const configs = await SettingsService.getSmsConfigs();
            const hasActive = configs.some((c: any) => c.isActive);
            if (!hasActive) {
                showToast("Maintenance Mode 1: SMS Verification System is currently unavailable.", "error");
                return false;
            }
        }
        return true;
    };

    const handleRequestOtp = async () => {
        // 1. Check Maintenance Mode
        setSendingOtp(true);
        const isAvailable = await checkMaintenanceMode(selectedMethod);
        if (!isAvailable) {
            setSendingOtp(false);
            return;
        }

        try {
            await AuthService.requestOtp(email, selectedMethod);
            showToast(`OTP sent to your ${selectedMethod}`, "success");
            setOtpSent(true);
            setStep('enter-otp');
        } catch (err: any) {
            console.error(err);
            showToast(err.response?.data?.error || "Failed to send OTP", "error");
        } finally {
            setSendingOtp(false);
        }
    };

    const handleVerifyParams = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await AuthService.verifyOtp({
                email,
                code: otp,
                type: selectedMethod
            });
            showToast("Verification Successful!", "success");
            navigate('/login'); // Redirect to login (or dashboard if token was returned? No, verify returns success, user logs in again to check new status)
            // Requirement: "now the status is account verified... when user tries to log in again he will be asked..."
        } catch (err: any) {
            showToast(err.response?.data || "Verification failed", "error");
        } finally {
            setLoading(false);
        }
    };

    // Render Logic
    const renderSelect = () => (
        <div className="space-y-4">
            <h2 className="text-xl font-semibold text-center">Verify Your Account</h2>
            <p className="text-center text-muted-foreground text-sm">
                Additional verification is required. Please choose a method.
            </p>
            <div className="grid grid-cols-2 gap-4">
                <button
                    type="button"
                    onClick={() => setSelectedMethod('Email')}
                    className={`p-4 border rounded-lg flex flex-col items-center gap-2 hover:bg-accent ${selectedMethod === 'Email' ? 'ring-2 ring-primary' : ''}`}
                >
                    <span className="font-medium">Email</span>
                </button>
                <button
                    type="button"
                    onClick={() => setSelectedMethod('Phone')}
                    className={`p-4 border rounded-lg flex flex-col items-center gap-2 hover:bg-accent ${selectedMethod === 'Phone' ? 'ring-2 ring-primary' : ''}`}
                >
                    <span className="font-medium">Phone</span>
                </button>
            </div>
            <button
                onClick={handleRequestOtp}
                disabled={sendingOtp}
                className="w-full bg-primary text-primary-foreground h-10 rounded-md font-medium mt-4 flex items-center justify-center gap-2"
            >
                {sendingOtp && <Loader2 className="animate-spin h-4 w-4" />}
                Continue
            </button>
        </div>
    );

    const renderOtpInput = () => (
        <form onSubmit={handleVerifyParams} className="space-y-6">
            <div className="text-center">
                <ShieldCheck className="mx-auto h-12 w-12 text-primary mb-4" />
                <h2 className="text-2xl font-bold tracking-tight">Enter Verifiction Code</h2>
                <p className="text-sm text-muted-foreground mt-2">
                    We sent a 4-digit code to your {selectedMethod.toLowerCase()}.
                    <br />
                    {/* Mock masking */}
                    {selectedMethod === 'Email' ? (
                        <span className="font-medium">{email.replace(/(.{2})(.*)(@.*)/, "$1***$3")}</span>
                    ) : (
                        <span className="font-medium">{maskPhoneNumber(phoneFromState)}</span>
                    )}
                </p>
            </div>

            <div className="flex justify-center gap-2">
                <input
                    type="text"
                    maxLength={4}
                    className="flex h-12 w-32 rounded-md border border-input bg-background px-3 py-2 text-center text-2xl tracking-widest ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={otp}
                    onChange={(e) => setOtp(e.target.value.replace(/[^0-9]/g, ''))}
                    placeholder="0000"
                    autoFocus
                />
            </div>

            <button
                disabled={loading || otp.length !== 4}
                className="w-full bg-primary text-primary-foreground h-10 rounded-md font-medium flex items-center justify-center gap-2"
            >
                {loading ? <Loader2 className="animate-spin h-4 w-4" /> : "Verify"}
            </button>

            {!otpSent && step === 'enter-otp' && (
                <div className="text-center">
                    <button
                        type="button"
                        onClick={handleRequestOtp}
                        disabled={sendingOtp}
                        className="text-sm text-primary hover:underline"
                    >
                        {sendingOtp ? "Sending..." : "Send Code"}
                    </button>
                </div>
            )}

            {otpSent && (
                <div className="text-center text-sm text-muted-foreground">
                    Didn't receive code?{' '}
                    <button type="button" onClick={handleRequestOtp} className="text-primary hover:underline">Resend</button>
                </div>
            )}
        </form>
    );

    return (
        <>
            {step === 'select' ? renderSelect() : renderOtpInput()}
        </>
    );
}

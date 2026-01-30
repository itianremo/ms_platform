import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { UserPlus, Eye, EyeOff, Loader2 } from 'lucide-react';
import CountrySelect, { COUNTRIES, Country } from '../components/CountrySelect';

const RegisterPage = () => {
    const [email, setEmail] = useState('');
    const [selectedCountry, setSelectedCountry] = useState<Country>(COUNTRIES[0]); // Default US
    const [phoneNumber, setPhoneNumber] = useState(''); // Just the number

    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState<Record<string, string>>({});

    const { register } = useAuth();
    const navigate = useNavigate();

    const validateEmail = (email: string) => {
        if (!email) return "Email is required";
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return "Invalid email address";
        return "";
    };

    const validatePhone = (number: string) => {
        if (!number) return "Phone number is required";
        if (!/^\d{7,15}$/.test(number.replace(/\D/g, ''))) return "Invalid phone number length";
        return "";
    };

    const validatePassword = (pass: string) => {
        if (!pass) return "Password is required";
        if (pass.length < 6) return "Password must be at least 6 characters";
        return "";
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        // Validation
        const newErrors: Record<string, string> = {};
        const emailErr = validateEmail(email);
        if (emailErr) newErrors.email = emailErr;

        const phoneErr = validatePhone(phoneNumber);
        if (phoneErr) newErrors.phone = phoneErr;

        const passErr = validatePassword(password);
        if (passErr) newErrors.password = passErr;

        if (password !== confirmPassword) {
            newErrors.confirmPassword = "Passwords do not match";
        }

        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors);
            return;
        }

        setErrors({});
        setLoading(true);

        const fullPhone = `${selectedCountry.dialCode}${phoneNumber.replace(/^0+/, '')}`; // Remove leading zeros if any

        try {
            await register(email, fullPhone, password, 'Both');
            navigate('/verify', { state: { email, phone: fullPhone, type: 'PendingAccountVerification' } });
        } catch (error) {
            // General error (handled by toast in context usually, but we can set form error if needed)
            // For now, layout likely handles global toasts, but we can display a generic error here if we want
            setErrors({ form: "Registration failed. Please try again." });
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            {errors.form && (
                <div className="p-3 text-sm text-red-500 bg-red-50 border border-red-200 rounded-md">
                    {errors.form}
                </div>
            )}

            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Work Email</label>
                <input
                    type="email"
                    className={`flex h-10 w-full rounded-md border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${errors.email ? 'border-red-500 focus-visible:ring-red-500' : 'border-input'}`}
                    value={email}
                    onChange={(e) => {
                        setEmail(e.target.value);
                        if (errors.email) setErrors({ ...errors, email: '' });
                    }}
                    placeholder="you@company.com"
                />
                {errors.email && <p className="text-xs text-red-500">{errors.email}</p>}
            </div>

            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Phone Number</label>
                <div className="flex gap-2">
                    <div className="w-[110px] flex-shrink-0">
                        <CountrySelect value={selectedCountry} onChange={setSelectedCountry} />
                    </div>
                    <input
                        type="tel"
                        className={`flex-1 h-10 w-full rounded-md border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${errors.phone ? 'border-red-500 focus-visible:ring-red-500' : 'border-input'}`}
                        value={phoneNumber}
                        onChange={(e) => {
                            setPhoneNumber(e.target.value.replace(/\D/g, '')); // Number only input
                            if (errors.phone) setErrors({ ...errors, phone: '' });
                        }}
                        placeholder="1234567890"
                    />
                </div>
                {errors.phone && <p className="text-xs text-red-500">{errors.phone}</p>}
            </div>

            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Password</label>
                <div className="relative">
                    <input
                        type={showPassword ? "text" : "password"}
                        className={`flex h-10 w-full rounded-md border bg-background px-3 py-2 pr-10 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${errors.password ? 'border-red-500 focus-visible:ring-red-500' : 'border-input'}`}
                        value={password}
                        onChange={(e) => {
                            setPassword(e.target.value);
                            if (errors.password) setErrors({ ...errors, password: '' });
                        }}
                    />
                    <button
                        type="button"
                        onClick={() => setShowPassword(!showPassword)}
                        className="absolute right-3 top-2.5 text-muted-foreground hover:text-foreground"
                    >
                        {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                </div>
                {errors.password && <p className="text-xs text-red-500">{errors.password}</p>}
            </div>

            <div className="space-y-2">
                <label className="text-sm font-medium leading-none text-muted-foreground">Confirm Password</label>
                <div className="relative">
                    <input
                        type={showConfirmPassword ? "text" : "password"}
                        className={`flex h-10 w-full rounded-md border bg-background px-3 py-2 pr-10 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${errors.confirmPassword ? 'border-red-500 focus-visible:ring-red-500' : 'border-input'}`}
                        value={confirmPassword}
                        onChange={(e) => {
                            setConfirmPassword(e.target.value);
                            if (errors.confirmPassword) setErrors({ ...errors, confirmPassword: '' });
                        }}
                    />
                    <button
                        type="button"
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        className="absolute right-3 top-2.5 text-muted-foreground hover:text-foreground"
                    >
                        {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                    </button>
                </div>
                {errors.confirmPassword && <p className="text-xs text-red-500">{errors.confirmPassword}</p>}
            </div>

            <button
                disabled={loading}
                className="mt-6 inline-flex w-full items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2 gap-2"
            >
                {loading ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : (
                    <UserPlus size={20} />
                )}
                <span>{loading ? 'Creating Account...' : 'Create Account'}</span>
            </button>

            <div className="mt-4 text-center text-sm text-muted-foreground">
                Already have an account?{' '}
                <Link to="/login" className="font-medium text-primary hover:underline">Sign In</Link>
            </div>
        </form>
    );
};

export default RegisterPage;

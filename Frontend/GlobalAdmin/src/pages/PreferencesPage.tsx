import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../components/theme-provider';
import { useToast } from '../context/ToastContext';
import { UserService } from '../services/userService';
import { AuthService } from '../services/authService';
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from "../components/ui/card";
import { User, Lock, Bell, Palette, Camera, Loader2, Eye, EyeOff, CheckCircle2, Monitor, Trash2, Shield, Save, KeyRound, ShieldCheck } from 'lucide-react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';
import { cn, getInitials } from '../lib/utils';
import { resizeImage } from '../lib/imageUtils';
import { Switch } from '../components/ui/switch';

type Tab = 'profile' | 'account' | 'logins' | 'appearance' | 'notifications';

const PreferencesPage = () => {
    const { user, updateUser, logout } = useAuth();
    const { theme, setTheme } = useTheme();
    const { showToast } = useToast();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<Tab>('profile');
    const [loading, setLoading] = useState(false);

    const handleVerify = async (type: 'Email' | 'Phone') => {
        if (!user?.email) return;
        setLoading(true);
        try {
            await api.post('/auth/api/auth/otp/request', { email: user.email, type });
            showToast(`Verification code sent to your ${type.toLowerCase()}`, "success");
            navigate('/verify', { state: { email: user.email, type: type === 'Email' ? 2 : 1 } });
        } catch (err) {
            showToast("Failed to request verification", "error");
        } finally {
            setLoading(false);
        }
    };

    // Profile State
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [bio, setBio] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('');
    const [phone, setPhone] = useState('');
    const [dob, setDob] = useState('');
    const [gender, setGender] = useState('');
    const [userPrefs, setUserPrefs] = useState<{ theme?: string, notifications?: any }>({});
    const fileInputRef = React.useRef<HTMLInputElement>(null);

    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showCurrent, setShowCurrent] = useState(false);
    const [showNew, setShowNew] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);

    useEffect(() => {
        if (user?.id) {
            if (!name) setName(user.name);
            if (!email) setEmail(user.email);
            if (!phone) setPhone(user.phone || '');
            loadProfile();
        }
    }, [user?.id]);

    const loadProfile = async () => {
        // Always try to set from context first/fallback
        if (user) {
            if (!name) setName(user.name);
            if (!phone) setPhone(user.phone || '');
        }

        if (!user?.id) return;
        setLoading(true);
        try {
            const profile = await UserService.getProfile(user.id);
            if (profile) {
                setName(profile.displayName);
                setBio(profile.bio || '');
                setAvatarUrl(profile.avatarUrl || '');
                setAvatarUrl(profile.avatarUrl || '');
                // Prefer DB column, fallback to Auth Token claim
                setPhone(user?.phone || '');

                if (profile.dateOfBirth) {
                    setDob(new Date(profile.dateOfBirth).toISOString().split('T')[0]);
                } else {
                    setDob('');
                }
                setGender(profile.gender || '');
                try {
                    const prefs = JSON.parse(profile.customDataJson || '{}');
                    setUserPrefs(prefs);
                } catch (e) { }
            }
        } catch (err) {
            // silent fail, rely on user context
            console.warn("Profile load failed", err);
        } finally {
            setLoading(false);
        }
    };

    const handleSaveProfile = async () => {
        if (!user?.id) return;
        setLoading(true);
        let updated = false;

        try {
            // 1. Update Basic Profile (Excluding Phone/Email)
            // Always update to capture new fields (dob/gender) that aren't in User Context
            if (true || name !== user!.name || bio !== (user as any).bio || avatarUrl !== user!.avatarUrl) {
                await UserService.updateProfile({
                    userId: user!.id,
                    appId: '00000000-0000-0000-0000-000000000000',
                    displayName: name,
                    bio,
                    avatarUrl,
                    customDataJson: JSON.stringify({ ...userPrefs }),
                    dateOfBirth: dob || undefined,
                    gender: gender || undefined
                });
                updated = true;
            }

            // 2. Update Contact Info (Email/Phone) if changed
            if (email !== user.email || phone !== user.phone) {
                await AuthService.updateContact({
                    userId: user.id,
                    newEmail: email !== user.email ? email : undefined,
                    newPhone: phone !== user.phone ? phone : undefined
                });

                showToast("Contact info updated. Verification required.", "success");

                // Force Logout for security/verification
                setTimeout(() => {
                    logout();
                    navigate('/login');
                }, 1500);
                return; // Stop here, logout pending
            }

            if (updated) {
                if (updateUser) {
                    updateUser({ name, bio: bio as any, avatarUrl });
                }
                showToast("Profile updated successfully", "success");
            } else if (email === user.email && phone === user.phone) {
                showToast("No changes to save.", "info");
            }

        } catch (err: any) {
            if (err.response?.status === 409) {
                showToast("Email already in use.", "error");
            } else {
                showToast("Failed to update profile", "error");
            }
        } finally {
            setLoading(false);
        }
    };

    const getStatusIcon = (currentValue: string, originalValue: string | undefined, isVerified: boolean | undefined) => {
        if (currentValue !== originalValue) {
            return <Loader2 className="h-5 w-5 text-yellow-500 animate-spin" />;
        }
        if (isVerified) return <CheckCircle2 className="h-5 w-5 text-green-500" />;
        return <Loader2 className="h-5 w-5 text-gray-400 opacity-0" />;
    };

    const handleImageClick = () => {
        fileInputRef.current?.click();
    };

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            try {
                const resized = await resizeImage(file, 200, 200);
                setAvatarUrl(resized); // Sets base64 string
            } catch (err) {
                console.error(err);
                showToast("Failed to process image", "error");
            }
        }
    };

    const handleChangePassword = async () => {
        if (newPassword !== confirmPassword) {
            showToast("New passwords do not match", "error");
            return;
        }
        if (!currentPassword || !newPassword) {
            showToast("Please fill in all password fields", "error");
            return;
        }

        if (!user?.id) return;

        setLoading(true);
        try {
            await api.post('/auth/api/Auth/change-password', {
                userId: user.id,
                oldPassword: currentPassword,
                newPassword: newPassword
            });
            showToast("Password updated! logging out...", "success");
            setCurrentPassword('');
            setNewPassword('');
            setConfirmPassword('');

            // Wait slightly for toast then logout
            setTimeout(() => {
                logout();
            }, 1000);
        } catch (err: any) {
            if (err.response?.status === 401) {
                showToast("Invalid old password", "error");
            } else {
                showToast("Failed to update password", "error");
            }
        } finally {
            setLoading(false);
        }
    };

    const handleThemeChange = (newTheme: 'light' | 'dark') => {
        setTheme(newTheme);
        setUserPrefs(prev => ({ ...prev, theme: newTheme }));
    };

    const NavItem = ({ id, label, icon: Icon }: { id: Tab, label: string, icon: any }) => (
        <button
            onClick={() => setActiveTab(id)}
            className={cn(
                "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                activeTab === id
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
            )}
        >
            <Icon size={18} />
            {label}
        </button>
    );

    const handleSaveSingleContact = async (type: 'Email' | 'Phone') => {
        if (!user?.id) return;
        setLoading(true);

        const newValue = type === 'Email' ? email : phone;
        const previousValue = type === 'Email' ? user.email : user.phone;

        if (newValue === previousValue) {
            // If not changed, trigger verify flow for existing value
            handleVerify(type);
            setLoading(false);
            return;
        }

        try {
            await AuthService.updateContact({
                userId: user.id,
                newEmail: type === 'Email' ? email : undefined,
                newPhone: type === 'Phone' ? phone : undefined
            });

            showToast(`${type} updated. Verification required. Logging out...`, "success");

            setTimeout(() => {
                logout();
                navigate('/login');
            }, 1500);

        } catch (err: any) {
            if (err.response?.status === 409) {
                showToast(`${type} already in use.`, "error");
            } else {
                showToast(`Failed to update ${type}`, "error");
            }
            setLoading(false);
        }
    };

    return (
        <div className="flex flex-col md:flex-row gap-8 p-4 md:p-8 animate-fade-in max-w-6xl mx-auto">
            {/* Sidebar Navigation */}
            <aside className="w-full md:w-64 flex-shrink-0 space-y-4">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight mb-1">Preferences</h2>
                    <p className="text-muted-foreground text-sm">Manage your account preferences.</p>
                </div>
                <nav className="space-y-1">
                    <NavItem id="profile" label="Profile" icon={User} />
                    <NavItem id="account" label="Security" icon={Lock} />
                    <NavItem id="logins" label="Logins" icon={Monitor} />
                    <NavItem id="appearance" label="Appearance" icon={Palette} />
                    <NavItem id="notifications" label="Notifications" icon={Bell} />
                </nav>
            </aside>

            {/* Content Area */}
            <div className="flex-1">
                {activeTab === 'profile' && (
                    <Card>
                        <CardHeader>
                            <CardTitle>Profile Information</CardTitle>
                            <CardDescription>Update your personal details and public profile.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="flex items-center gap-6">
                                <input
                                    type="file"
                                    ref={fileInputRef}
                                    className="hidden"
                                    accept="image/*"
                                    onChange={handleFileChange}
                                />
                                <div className="relative group cursor-pointer" onClick={handleImageClick}>
                                    <div className="h-24 w-24 rounded-full overflow-hidden border border-blue-500 dark:border-gray-700 bg-muted flex items-center justify-center text-3xl font-bold text-muted-foreground transition-all group-hover:border-primary">
                                        {avatarUrl ? (
                                            <img src={avatarUrl} alt="Avatar" className="h-full w-full object-cover" />
                                        ) : (
                                            getInitials(name || user?.name || '')
                                        )}
                                    </div>
                                    <div className="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity rounded-full">
                                        <Camera className="text-white" size={24} />
                                    </div>
                                </div>
                                <div className="space-y-1">
                                    <h3 className="font-medium text-lg">{name || user?.name}</h3>
                                    <div className="flex gap-2">
                                        <p className="text-sm text-muted-foreground">Click image to upload new photo</p>
                                        {avatarUrl && (
                                            <button
                                                onClick={() => setAvatarUrl('')}
                                                className="p-1 px-2 text-xs font-bold text-white bg-red-600 rounded hover:bg-red-700 transition-colors"
                                                title="Remove Photo"
                                            >
                                                <Trash2 size={14} />
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>

                            <div className="space-y-4">
                                <div className="space-y-2">
                                    <Label htmlFor="name">Display Name</Label>
                                    <Input id="name" value={name} onChange={e => setName(e.target.value)} placeholder="Full Name" />
                                </div>
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                        <Label htmlFor="email">Email Address</Label>
                                        <div className="flex gap-2 items-center">
                                            <Input id="email" value={email} onChange={e => setEmail(e.target.value)} />

                                            {email !== user?.email ? (
                                                <Button size="icon" onClick={() => handleSaveSingleContact('Email')} disabled={loading} title="Verify Email">
                                                    <ShieldCheck className="h-4 w-4" />
                                                </Button>
                                            ) : user?.isEmailVerified ? (
                                                <CheckCircle2 className="h-5 w-5 text-green-500" />
                                            ) : (
                                                <div className="flex items-center gap-2">
                                                    <Lock className="h-4 w-4 text-gray-400" />
                                                    <Button type="button" variant="outline" size="icon" onClick={() => handleVerify('Email')} disabled={loading} title="Verify Email">
                                                        <ShieldCheck className="h-4 w-4" />
                                                    </Button>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                    <div className="space-y-2">
                                        <Label htmlFor="phone">Phone Number</Label>
                                        <div className="flex gap-2 items-center">
                                            <Input id="phone" value={phone} onChange={e => setPhone(e.target.value)} placeholder="+1 (555) 000-0000" />

                                            {phone !== user?.phone ? (
                                                <Button size="icon" onClick={() => handleSaveSingleContact('Phone')} disabled={loading} title="Verify Phone">
                                                    <ShieldCheck className="h-4 w-4" />
                                                </Button>
                                            ) : user?.isPhoneVerified ? (
                                                <CheckCircle2 className="h-5 w-5 text-green-500" />
                                            ) : (
                                                <div className="flex items-center gap-2">
                                                    <Lock className="h-4 w-4 text-gray-400" />
                                                    <Button type="button" variant="outline" size="icon" onClick={() => handleVerify('Phone')} disabled={loading} title="Verify Phone">
                                                        <ShieldCheck className="h-4 w-4" />
                                                    </Button>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="bio">Bio</Label>
                                    <textarea
                                        id="bio"
                                        className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                        rows={3}
                                        value={bio}
                                        onChange={(e) => setBio(e.target.value)}
                                        placeholder="Tell us a little about yourself"
                                    />
                                </div>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                        <Label htmlFor="dob">Date of Birth</Label>
                                        <Input
                                            id="dob"
                                            type="date"
                                            value={dob}
                                            onChange={e => setDob(e.target.value)}
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <Label htmlFor="gender">Gender</Label>
                                        <select
                                            id="gender"
                                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                            value={gender}
                                            onChange={e => setGender(e.target.value)}
                                        >
                                            <option value="">Select Gender</option>
                                            <option value="Male">Male</option>
                                            <option value="Female">Female</option>
                                            <option value="Prefer not to say">Prefer not to say</option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                        <CardFooter>
                            <Button onClick={handleSaveProfile} disabled={loading} title="Save Changes">
                                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {!loading && <Save className="h-4 w-4" />}
                            </Button>
                        </CardFooter>
                    </Card>
                )}

                {activeTab === 'account' && (
                    <Card>
                        <CardHeader>
                            <CardTitle>Security</CardTitle>
                            <CardDescription>Manage your password and security settings.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="grid gap-2">
                                <Label htmlFor="current">Current Password</Label>
                                <div className="relative">
                                    <Lock className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                                    <Input
                                        id="current"
                                        type={showCurrent ? "text" : "password"}
                                        className="pl-9 pr-9"
                                        value={currentPassword}
                                        onChange={e => setCurrentPassword(e.target.value)}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowCurrent(!showCurrent)}
                                        className="absolute right-2.5 top-2.5 text-muted-foreground hover:text-foreground"
                                    >
                                        {showCurrent ? <EyeOff size={16} /> : <Eye size={16} />}
                                    </button>
                                </div>
                            </div>
                            <div className="grid gap-2">
                                <Label htmlFor="new">New Password</Label>
                                <div className="relative">
                                    <Lock className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                                    <Input
                                        id="new"
                                        type={showNew ? "text" : "password"}
                                        className="pl-9 pr-9"
                                        value={newPassword}
                                        onChange={e => setNewPassword(e.target.value)}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowNew(!showNew)}
                                        className="absolute right-2.5 top-2.5 text-muted-foreground hover:text-foreground"
                                    >
                                        {showNew ? <EyeOff size={16} /> : <Eye size={16} />}
                                    </button>
                                </div>
                            </div>
                            <div className="grid gap-2">
                                <Label htmlFor="confirm">Confirm New Password</Label>
                                <div className="relative">
                                    <Lock className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                                    <Input
                                        id="confirm"
                                        type={showConfirm ? "text" : "password"}
                                        className="pl-9 pr-9"
                                        value={confirmPassword}
                                        onChange={e => setConfirmPassword(e.target.value)}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowConfirm(!showConfirm)}
                                        className="absolute right-2.5 top-2.5 text-muted-foreground hover:text-foreground"
                                    >
                                        {showConfirm ? <EyeOff size={16} /> : <Eye size={16} />}
                                    </button>
                                </div>
                            </div>
                            <div className="p-4 text-sm font-bold text-blue-600 bg-blue-50 border border-blue-200 rounded-md dark:bg-blue-950/20 dark:border-blue-900">
                                Warning: Changing your password will sign you out of all other sessions.
                            </div>
                        </CardContent>
                        <CardFooter>
                            <Button variant="default" onClick={handleChangePassword} disabled={loading} title="Update Password">
                                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {!loading && <KeyRound className="h-4 w-4" />}
                            </Button>
                        </CardFooter>
                    </Card>
                )}

                {activeTab === 'logins' && (
                    <SessionsTab />
                )}

                {activeTab === 'appearance' && (
                    <Card>
                        <CardHeader>
                            <CardTitle>Appearance</CardTitle>
                            <CardDescription>Customize the interface look and feel.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="space-y-4">
                                <Label>Theme Preference</Label>
                                <div className="grid grid-cols-2 gap-4 max-w-sm">
                                    <div
                                        className={cn(
                                            "cursor-pointer rounded-lg border-2 p-2 hover:border-primary transition-all",
                                            theme === 'light' ? "border-primary bg-primary/5" : "border-muted"
                                        )}
                                        onClick={() => handleThemeChange('light')}
                                    >
                                        <div className="space-y-2 rounded-md bg-[#ecedef] p-2">
                                            <div className="space-y-2 rounded-md bg-white p-2 shadow-sm">
                                                <div className="h-2 w-[80px] rounded-lg bg-[#ecedef]" />
                                                <div className="h-2 w-[100px] rounded-lg bg-[#ecedef]" />
                                            </div>
                                            <div className="flex items-center space-x-2 rounded-md bg-white p-2 shadow-sm">
                                                <div className="h-4 w-4 rounded-full bg-[#ecedef]" />
                                                <div className="h-2 w-[100px] rounded-lg bg-[#ecedef]" />
                                            </div>
                                        </div>
                                        <div className="mt-2 text-center text-sm font-medium">Light</div>
                                    </div>
                                    <div
                                        className={cn(
                                            "cursor-pointer rounded-lg border-2 p-2 hover:border-primary transition-all",
                                            theme === 'dark' ? "border-primary bg-primary/5" : "border-muted"
                                        )}
                                        onClick={() => handleThemeChange('dark')}
                                    >
                                        <div className="space-y-2 rounded-md bg-slate-950 p-2">
                                            <div className="space-y-2 rounded-md bg-slate-800 p-2 shadow-sm">
                                                <div className="h-2 w-[80px] rounded-lg bg-slate-400" />
                                                <div className="h-2 w-[100px] rounded-lg bg-slate-400" />
                                            </div>
                                            <div className="flex items-center space-x-2 rounded-md bg-slate-800 p-2 shadow-sm">
                                                <div className="h-4 w-4 rounded-full bg-slate-400" />
                                                <div className="h-2 w-[100px] rounded-lg bg-slate-400" />
                                            </div>
                                        </div>
                                        <div className="mt-2 text-center text-sm font-medium">Dark</div>
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                        <CardFooter>
                            <Button onClick={handleSaveProfile} disabled={loading} title="Save Preferences">
                                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {!loading && <Save className="h-4 w-4" />}
                            </Button>
                        </CardFooter>
                    </Card>
                )}

                {activeTab === 'notifications' && (
                    <Card>
                        <CardHeader>
                            <CardTitle>Notifications</CardTitle>
                            <CardDescription>Choose what you want to be notified about.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="flex items-center justify-between space-x-2 rounded-lg border p-4">
                                <Label htmlFor="email-notifs" className="flex flex-col space-y-1">
                                    <span>Email Alerts</span>
                                    <span className="font-normal text-xs text-muted-foreground">Receive updates via email.</span>
                                </Label>
                                <Switch
                                    id="email-notifs"
                                    checked={userPrefs.notifications?.email !== false}
                                    onCheckedChange={(checked) => setUserPrefs(p => ({ ...p, notifications: { ...p.notifications, email: checked } }))}
                                />
                            </div>
                            <div className="flex items-center justify-between space-x-2 rounded-lg border p-4">
                                <Label htmlFor="push-notifs" className="flex flex-col space-y-1">
                                    <span>Browser Push</span>
                                    <span className="font-normal text-xs text-muted-foreground">Receive real-time push notifications.</span>
                                </Label>
                                <Switch
                                    id="push-notifs"
                                    checked={userPrefs.notifications?.push !== false}
                                    onCheckedChange={(checked) => setUserPrefs(p => ({ ...p, notifications: { ...p.notifications, push: checked } }))}
                                />
                            </div>
                        </CardContent>
                        <CardFooter>
                            <Button onClick={handleSaveProfile} disabled={loading} title="Save Preferences">
                                {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                {!loading && <Save className="h-4 w-4" />}
                            </Button>
                        </CardFooter>
                    </Card>
                )}
            </div>
        </div>
    );
};

const SessionsTab = () => {
    const { showToast } = useToast();
    const [sessions, setSessions] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    const fetchSessions = async () => {
        try {
            const res = await api.get('/auth/api/auth/sessions'); // Ensure route matches Gateway
            setSessions(res.data);
        } catch (err) {
            console.error("Failed to fetch sessions", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSessions();
    }, []);

    const handleRevoke = async (sessionId: string) => {
        try {
            await api.delete(`/auth/api/auth/sessions/${sessionId}`);
            setSessions(prev => prev.filter(s => s.id !== sessionId));
            showToast("Session revoked.", "success");
        } catch (err) {
            showToast("Failed to revoke session.", "error");
        }
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>Logins</CardTitle>
                <CardDescription>View and manage your active sessions across devices and applications.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                {loading ? (
                    <div className="flex justify-center p-4">
                        <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                    </div>
                ) : sessions.length === 0 ? (
                    <div className="text-center text-muted-foreground p-4">No active sessions found.</div>
                ) : (
                    <div className="space-y-4">
                        {sessions.map((session) => (
                            <div key={session.id} className="flex items-center justify-between p-4 border rounded-lg bg-card hover:bg-accent/5 transition-colors">
                                <div className="flex items-center gap-4">
                                    <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                                        <Monitor size={20} />
                                    </div>
                                    <div>
                                        <div className="font-medium flex items-center gap-2">
                                            {session.appName || 'Unknown App'}
                                            {session.isCurrent && (
                                                <span className="text-[10px] font-bold bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400 px-2 py-0.5 rounded-full uppercase tracking-wide">Current</span>
                                            )}
                                        </div>
                                        <div className="text-sm text-muted-foreground">
                                            {session.deviceInfo || 'Unknown Device'} • {session.ipAddress || 'Unknown IP'}
                                        </div>
                                        <div className="text-xs text-muted-foreground/70 mt-0.5">
                                            {new Date(session.createdAt).toLocaleString()}
                                        </div>
                                    </div>
                                </div>
                                {!session.isCurrent && (
                                    <Button variant="ghost" size="icon" className="text-muted-foreground hover:text-destructive hover:bg-destructive/10" onClick={() => handleRevoke(session.id)} title="Revoke Session">
                                        <Trash2 size={18} />
                                    </Button>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
};

export default PreferencesPage;

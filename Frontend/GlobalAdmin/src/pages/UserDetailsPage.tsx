import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Input } from "../components/ui/input";
import { Switch } from "../components/ui/switch";
import { Label } from "../components/ui/label";
import { ArrowLeft, Save, Shield, User, Activity, Lock, Key, Edit, CheckCircle2, MoreHorizontal, Info, XCircle } from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";
import { UserService, UserDto, UserProfile } from '../services/userService';
import { useToast } from '../context/ToastContext';
import { useAuth } from '../context/AuthContext';
import { RoleService, RoleDto } from '../services/roleService';
import { AuditService, AuditLog } from '../services/AuditService';
import { RoleBadge } from '../components/users/RoleBadge';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "../components/ui/dropdown-menu";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    DialogFooter,
} from "../components/ui/dialog";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "../components/ui/select";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../components/ui/table";
import { AppService, AppConfig } from '../services/appService';
import { UserSubscriptionsTab } from '../components/users/UserSubscriptionsTab';
import { EditAppProfileDialog } from '../components/users/EditAppProfileDialog';

// Helper for Provider Logos
const ProviderLogo = ({ provider }: { provider: string }) => {
    if (!provider) return null;
    const p = provider.toLowerCase();

    if (p === 'google') return (
        <svg className="w-6 h-6" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
            <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
            <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
            <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
        </svg>
    );

    if (p === 'microsoft') return (
        <svg className="w-6 h-6" viewBox="0 0 23 23" xmlns="http://www.w3.org/2000/svg">
            <path fill="#f35325" d="M1 1h10v10H1z" />
            <path fill="#81bc06" d="M12 1h10v10H12z" />
            <path fill="#05a6f0" d="M1 12h10v10H1z" />
            <path fill="#ffba08" d="M12 12h10v10H12z" />
        </svg>
    );

    if (p === 'facebook') return (
        <svg className="w-6 h-6" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" fill="#1877F2">
            <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
        </svg>
    );

    if (p === 'twitter') return (
        <svg className="w-6 h-6" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
            <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
        </svg>
    );

    if (p === 'apple') return (
        <svg className="w-6 h-6" viewBox="0 0 384 512" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
            <path d="M318.7 268.7c-.2-36.7 16.4-64.4 50-84.8-18.8-26.9-47.2-41.9-84.7-44.6-35.5-2.8-74.3 20.7-88.5 20.7-15 0-49.4-19.7-76.4-19.7C63.3 141.2 4 184.8 4 273.5q0 39.3 14.4 81.2c12.8 36.7 59 126.7 107.2 125.2 25.2-.6 43-17.9 75.8-17.9 31.8 0 48.3 17.9 76.4 17.9 48.6-.7 90.4-82.5 102.6-119.3-65.2-30.7-61.7-90-61.7-91.9zm-56.6-164.2c27.3-32.4 24.8-61.9 24-72.5-24.1 1.4-52 16.4-67.9 34.9-17.5 19.8-27.8 44.3-25.6 71.9 26.1 2 52.3-11.4 69.5-34.3z" />
        </svg>
    );

    if (p === 'linkedin') return (
        <svg className="w-6 h-6" viewBox="0 0 24 24" fill="#0077b5" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z" />
        </svg>
    );

    // Fallback
    return (
        <div className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center font-bold text-gray-500">
            {p[0].toUpperCase()}
        </div>
    );
};

const GlobalUserStatus = {
    0: 'PendingAccountVerification',
    1: 'PendingMobileVerification',
    2: 'PendingEmailVerification',
    3: 'PendingAdminApproval',
    4: 'Active',
    5: 'Banned',
    6: 'SoftDeleted',
    7: 'ProfileIncomplete'
};


const UserDetailsPage = () => {
    const { userId } = useParams<{ userId: string }>();
    const { userId: id } = useParams<{ userId: string }>(); // Renamed to 'id' for consistency with diff
    const navigate = useNavigate();
    const { showToast } = useToast();
    const [user, setUser] = useState<UserDto | null>(null);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [allApps, setAllApps] = useState<AppConfig[]>([]);
    const [selectedSubscriptionAppId, setSelectedSubscriptionAppId] = useState<string>("");

    // Default to System App ("Global Admin Dashboard") or just Global Context
    const SYSTEM_APP_ID = "00000000-0000-0000-0000-000000000001";
    const [selectedProfileAppId, setSelectedProfileAppId] = useState<string>(SYSTEM_APP_ID);

    // Edit State
    const [isEditing, setIsEditing] = useState(false);
    const [editForm, setEditForm] = useState<Partial<UserProfile> & { email?: string; phone?: string }>({}); // Changed type to Partial<UserProfile>

    useEffect(() => {
        if (id) {
            fetchData(id);
        }
        fetchApps(); // Keep fetchApps here as it's independent of userId
    }, [id]);

    // Reload profile when selected app context changes
    useEffect(() => {
        if (id && selectedProfileAppId) {
            loadProfile(id, selectedProfileAppId);
        }
    }, [selectedProfileAppId, id]);

    const fetchApps = async () => {
        try {
            const apps = await AppService.getAllApps();
            setAllApps(apps);
        } catch (e) { console.error(e); }
    }

    const loadProfile = async (userId: string, appId: string) => {
        try {
            const profileData = await UserService.getProfile(userId, appId);
            setProfile(profileData);
        } catch (error) {
            console.error(error);
            setProfile(null);
        }
    };

    const fetchData = async (userId: string) => {
        setLoading(true);
        try {
            const users = await UserService.getAllUsers();
            const foundUser = users.find(u => u.id === userId);

            if (foundUser) {
                setUser(foundUser);

                // Load profile for selected app
                // Ensure selectedProfileAppId is valid or fallback to Global
                const appIdToLoad = selectedProfileAppId || SYSTEM_APP_ID;
                await loadProfile(userId, appIdToLoad);

            } else {
                showToast("User not found", "error");
                navigate("/users");
            }
            setLoading(false);
        } catch (error) {
            console.error(error);
            showToast("Failed to load user data", "error");
            setLoading(false);
        }
    };

    const handleStatusChange = async (newStatus: number) => {
        if (!user) return;
        try {
            await UserService.setUserStatus(user.id, newStatus);
            showToast("Status updated successfully", "success");
            setUser({ ...user, status: newStatus });
        } catch (error) {
            showToast("Failed to update status", "error");
        }
    };

    const handlePasswordReset = async () => {
        if (!user) return;
        try {
            await UserService.sendPasswordReset(user.email);
            showToast("Password reset email sent successfully", "success");
        } catch (error) {
            showToast("Failed to send password reset email", "error");
        }
    };

    const toggleEditMode = () => {
        if (!isEditing) {
            setEditForm({
                email: user?.email || '',
                phone: user?.phone || ''
            });
        }
        setIsEditing(!isEditing);
    };

    // Note: Actual saving of Email/Phone would go here if implemented in backend
    // For now we just toggle UI state for these fields.

    const handleVerifyAction = async (type: 'email' | 'phone', verified: boolean) => {
        if (!user) return;
        try {
            await UserService.verifyUserIdentity(user.id, type, verified);
            showToast(`${type} status updated`, "success");
            if (type === 'email') setUser({ ...user, isEmailVerified: verified });
            if (type === 'phone') setUser({ ...user, isPhoneVerified: verified });
        } catch (e) {
            showToast("Failed to update verification status", "error");
        }
    };

    const handleRequestOtp = async (type: 'email' | 'phone') => {
        if (!user) return;
        try {
            await UserService.sendPasswordReset(user.email);
            showToast("OTP sent", "success");
        } catch (e) {
            showToast("Failed to send OTP", "error");
        }
    };

    if (loading) return <div>Loading...</div>;
    if (!user) return <div>User not found</div>;

    const statusLabel = GlobalUserStatus[user.status as keyof typeof GlobalUserStatus] || 'Unknown';

    const deriveName = (u: UserDto) => {
        const isValidName = (n?: string) => n && n !== 'N/A' && n !== 'n/a';
        if (isValidName(u.displayName)) return u.displayName;
        if (isValidName(u.firstName) && isValidName(u.lastName)) return `${u.firstName} ${u.lastName}`;
        if (isValidName(u.firstName)) return u.firstName;
        const email = u.email || '';
        if (!email.includes('@')) return email || 'Unknown User';
        const [localPart, domainPart] = email.split('@');
        const domainName = domainPart.split('.')[0];
        const capitalize = (s: string) => s ? s.charAt(0).toUpperCase() + s.slice(1) : '';
        return `${capitalize(localPart)} ${capitalize(domainName)}`;
    };

    const headerTitle = deriveName(user);

    const getInitials = (name: string) => {
        if (!name || name === 'N/A' || name.startsWith('N/')) return "U";
        const parts = name.split(' ');
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        }
        return name.substring(0, 2).toUpperCase();
    };
    return (
        <div className="p-4 md:p-8 space-y-6 animate-fade-in">
            <div className="flex flex-col gap-6 md:flex-row md:items-start md:justify-between bg-card p-6 rounded-lg border shadow-sm">
                <div className="flex items-start gap-4">
                    <Avatar className="h-20 w-20 border-2 border-background shadow-sm">
                        <AvatarImage src={profile?.avatarUrl} alt={headerTitle} />
                        <AvatarFallback className="text-xl font-bold text-primary">{getInitials(headerTitle || 'U')}</AvatarFallback>
                    </Avatar>
                    <div className="space-y-1">
                        <h1 className="text-2xl font-bold text-card-foreground">{headerTitle}</h1>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground font-mono">
                            <Key className="h-3 w-3" /> {user.id}
                        </div>
                        <div className="flex items-center gap-2 mt-2">
                            <Select
                                value={(user.status ?? 0).toString()}
                                onValueChange={(v) => handleStatusChange(parseInt(v))}
                            >
                                <SelectTrigger className={`h-7 text-xs font-semibold border px-2.5 rounded-full w-auto gap-2 ${user.status === 4 ? 'bg-green-50 text-green-700 border-green-200' : 'bg-yellow-50 text-yellow-700 border-yellow-200'}`}>
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    {Object.entries(GlobalUserStatus)
                                        .filter(([k]) => !isNaN(Number(k))) // Keep numeric keys
                                        .map(([k, v]) => (
                                            <SelectItem key={k} value={k.toString()}>{v}</SelectItem>
                                        ))
                                    }
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-3 text-sm border-t md:border-t-0 md:border-l pt-4 md:pt-0 md:pl-8">
                    {/* Email */}
                    <div className="space-y-1">
                        <Label className="text-xs text-muted-foreground uppercase tracking-wider">Email</Label>
                        <div className="flex items-center gap-2">
                            <Input
                                value={isEditing ? editForm.email : user.email}
                                onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                                disabled={!isEditing}
                                className="h-8 w-64 bg-transparent border-transparent hover:border-input focus:border-input transition-colors disabled:opacity-100 disabled:cursor-text"
                            />
                            <div className="flex items-center gap-2">
                                <Switch
                                    checked={user.isEmailVerified}
                                    onCheckedChange={(c) => handleVerifyAction('email', c)}
                                    className="scale-75" // Make it small
                                />
                                <span className={`text-xs ${user.isEmailVerified ? 'text-green-600 font-medium' : 'text-yellow-600'}`}>
                                    {user.isEmailVerified ? 'Verified' : 'Unverified'}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Phone */}
                    <div className="space-y-1">
                        <Label className="text-xs text-muted-foreground uppercase tracking-wider">Phone</Label>
                        <div className="flex items-center gap-2">
                            <Input
                                value={isEditing ? editForm.phone : (user.phone || "")}
                                onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                                disabled={!isEditing}
                                className="h-8 w-64 bg-transparent border-transparent hover:border-input focus:border-input transition-colors disabled:opacity-100 disabled:cursor-text"
                                placeholder="N/A"
                            />
                            <div className="flex items-center gap-2">
                                <Switch
                                    checked={user.isPhoneVerified}
                                    onCheckedChange={(c) => handleVerifyAction('phone', c)}
                                    className="scale-75"
                                />
                                <span className={`text-xs ${user.isPhoneVerified ? 'text-green-600 font-medium' : 'text-yellow-600'}`}>
                                    {user.isPhoneVerified ? 'Verified' : 'Unverified'}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Last Activity */}
                    <div className="space-y-1 md:col-span-2">
                        <Label className="text-xs text-muted-foreground uppercase tracking-wider">Last Activity</Label>
                        <div className="flex items-center gap-2">
                            <span className="text-sm font-medium">
                                {user.lastLoginUtc ? formatDistanceToNow(new Date(user.lastLoginUtc), { addSuffix: true }) : 'Never'}
                            </span>
                            {user.lastLoginUtc && (
                                <span className="text-xs text-muted-foreground bg-muted px-1.5 py-0.5 rounded">
                                    {user.lastLoginAppId ? allApps.find(a => a.id.toLowerCase() === user.lastLoginAppId?.toLowerCase())?.name ?? 'Unknown App' : 'System'}
                                </span>
                            )}
                        </div>
                    </div>

                    {/* Joined/Reset Actions */}
                    <div className="md:col-span-2 flex gap-2 mt-2 items-center">
                        {isEditing ? (
                            <>
                                <Button size="sm" onClick={toggleEditMode} className="h-7 text-xs bg-green-600 hover:bg-green-700">
                                    <Save className="mr-2 h-3 w-3" /> Save Changes
                                </Button>
                                <Button variant="ghost" size="sm" onClick={() => setIsEditing(false)} className="h-7 text-xs">
                                    Cancel
                                </Button>
                            </>
                        ) : (
                            <Button variant="outline" size="sm" onClick={toggleEditMode} className="h-7 text-xs">
                                <Edit className="mr-2 h-3 w-3" /> Edit Contact Info
                            </Button>
                        )}
                        <Button variant="ghost" size="sm" onClick={handlePasswordReset} className="h-7 text-xs">
                            <Lock className="mr-2 h-3 w-3" /> Reset Password
                        </Button>
                    </div>
                </div>
            </div>

            <Tabs defaultValue="apps" className="w-full">
                <TabsList className="flex w-full justify-start border-b rounded-none bg-transparent p-0 mb-6">
                    <TabsTrigger value="apps" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent pb-3 px-6">Applications</TabsTrigger>
                    <TabsTrigger value="security" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent pb-3 px-6">Security & Logins</TabsTrigger>
                    {/* Removed Permissions Tab */}
                    <TabsTrigger value="audit" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent pb-3 px-6">Audit Logs</TabsTrigger>
                </TabsList>

                <TabsContent value="apps" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Applications</CardTitle>
                            <CardDescription>Manage access to specific applications.</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <MembershipManager user={user} apps={allApps} onUpdate={() => fetchData(user.id)} />
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="security" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Security Settings</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="space-y-4">
                                <h3 className="text-sm font-medium">Linked Accounts</h3>
                                <div className="grid gap-4 md:grid-cols-2">
                                    {['Google', 'Microsoft', 'Facebook', 'LinkedIn', 'Twitter', 'Apple'].map(provider => {
                                        const isLinked = user.linkedProviders?.includes(provider);
                                        return (
                                            <div key={provider} className="flex items-center justify-between border p-3 rounded-md">
                                                <div className="flex items-center gap-3">
                                                    <div className="flex items-center justify-center">
                                                        <ProviderLogo provider={provider} />
                                                    </div>
                                                    <span className="text-sm font-medium">{provider}</span>
                                                </div>
                                                {isLinked ? (
                                                    <Button variant="ghost" size="sm" className="text-red-600 hover:text-red-700 hover:bg-red-50" onClick={async () => {
                                                        if (confirm(`Unlink ${provider}?`)) {
                                                            try {
                                                                await UserService.unlinkExternalAccount(user.id, provider);
                                                                showToast(`${provider} unlinked`, "success");
                                                                fetchData(user.id);
                                                            } catch {
                                                                showToast(`Failed to unlink ${provider}`, "error");
                                                            }
                                                        }
                                                    }}>
                                                        Unlink
                                                    </Button>
                                                ) : (
                                                    <span className="text-xs text-muted-foreground italic">Not Linked</span>
                                                )}
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>


                        </CardContent>
                    </Card>
                </TabsContent>

                {/* Removed Permissions TabsContent */}



                <TabsContent value="audit" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Audit Logs</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <AuditLogViewer userId={user.id} />
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs >
        </div >
    );
};


const MembershipManager = ({ user, apps, onUpdate }: { user: UserDto, apps: AppConfig[], onUpdate: () => void }) => {
    // const [apps, setApps] = useState<AppConfig[]>([]); // Removed local state, using prop
    const [selectedApp, setSelectedApp] = useState<string>("");
    const [loading, setLoading] = useState(false);
    const { showToast } = useToast();
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);

    // New State for Edit Dialog
    const [editingProfileAppId, setEditingProfileAppId] = useState<string | null>(null);

    // useEffect(() => {
    //     loadApps();
    // }, []);

    // const loadApps = async () => { ... } // Removed

    const handleAddToApp = async () => {
        if (!selectedApp) return;
        setLoading(true);
        try {
            await UserService.addUserToApp(user.id, selectedApp);
            showToast("User added to app", "success");
            setIsAddDialogOpen(false);
            onUpdate();
        } catch (error) {
            showToast("Failed to add user to app", "error");
        } finally {
            setLoading(false);
        }
    };

    const handleRemoveFromApp = async (appId: string) => {
        if (!confirm("Are you sure you want to remove this user from the app?")) return;
        try {
            await UserService.removeUserFromApp(user.id, appId);
            showToast("User removed from app", "success");
            onUpdate();
        } catch (error) {
            showToast("Failed to remove user", "error");
        }
    };

    const handleRoleChange = async (appId: string, roleName: string) => {
        try {
            await UserService.assignRole(user.id, appId, roleName);
            showToast("Role updated", "success");
            onUpdate();
        } catch (error) {
            showToast("Failed to update role", "error");
        }
    };

    const handleStatusChange = async (appId: string, status: number) => {
        try {
            await UserService.updateAppStatus(user.id, appId, status);
            showToast("App status updated", "success");
            onUpdate();
        } catch (error) {
            showToast("Failed to update status", "error");
        }
    };

    // Calculate available apps (not already a member)
    const storedMemberships = user.memberships || [];
    const availableApps = apps.filter(a => !storedMemberships.some(m => m.appId === a.id));

    const { user: currentUser } = useAuth();
    const isSuperAdmin = currentUser?.roles?.includes('SuperAdmin');

    // ... items ...

    const [managingAppId, setManagingAppId] = useState<string | null>(null);

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">Current Memberships</h3>
                <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
                    <DialogTrigger asChild>
                        <Button size="sm"><Shield className="mr-2 h-4 w-4" /> Add to App</Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Add User to Application</DialogTitle>
                            <DialogDescription>Select an application to grant access to.</DialogDescription>
                        </DialogHeader>
                        <div className="py-4">
                            <Label>Application</Label>
                            <Select onValueChange={setSelectedApp} value={selectedApp} disabled={availableApps.length === 0}>
                                <SelectTrigger>
                                    <SelectValue placeholder={availableApps.length === 0 ? "No available apps" : "Select App"} />
                                </SelectTrigger>
                                <SelectContent>
                                    {availableApps.length > 0 ? (
                                        availableApps.map(app => (
                                            <SelectItem key={app.id} value={app.id}>{app.name}</SelectItem>
                                        ))
                                    ) : (
                                        <div className="p-2 text-sm text-muted-foreground text-center">User is already in all apps</div>
                                    )}
                                </SelectContent>
                            </Select>
                        </div>
                        <DialogFooter>
                            <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>Cancel</Button>
                            <Button onClick={handleAddToApp} disabled={loading || !selectedApp}>Add User</Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>


            <div className="border rounded-md">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>App Name</TableHead>
                            <TableHead>Role</TableHead>
                            <TableHead>Permissions</TableHead>
                            <TableHead>Last Login</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {storedMemberships.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-4 text-muted-foreground">
                                    No memberships found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            storedMemberships.map(m => {
                                const appName = apps.find(a => a.id.toLowerCase() === m.appId.toLowerCase())?.name || `Unknown App (${m.appId.substring(0, 8)})`;
                                const isTargetSuperAdmin = m.roleName === 'SuperAdmin';
                                const canEdit = isSuperAdmin || !isTargetSuperAdmin;

                                return (
                                    <TableRow key={m.appId}>
                                        <TableCell className="font-medium">
                                            <div className="flex flex-col">
                                                <span>{appName}</span>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className={!canEdit ? "opacity-50 pointer-events-none" : ""}>
                                                <RoleSelector
                                                    appId={m.appId}
                                                    currentRoleName={m.roleName}
                                                    onRoleChange={(roleName) => handleRoleChange(m.appId, roleName)}
                                                />
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <RolePermissionsDisplay appId={m.appId} roleName={m.roleName} />
                                        </TableCell>
                                        <TableCell>
                                            <span className="text-xs text-muted-foreground">
                                                {m.lastLogin ? new Date(m.lastLogin).toLocaleString() : 'Never'}
                                            </span>
                                        </TableCell>
                                        <TableCell>
                                            <Select
                                                defaultValue={m.status.toString()}
                                                onValueChange={(v: string) => handleStatusChange(m.appId, parseInt(v))}
                                                disabled={!canEdit}
                                            >
                                                <SelectTrigger className="w-[100px] h-8 text-xs">
                                                    <SelectValue />
                                                </SelectTrigger>
                                                <SelectContent>
                                                    <SelectItem value="0">Active</SelectItem>
                                                    <SelectItem value="1">Banned</SelectItem>
                                                    <SelectItem value="2">Pending</SelectItem>
                                                </SelectContent>
                                            </Select>
                                        </TableCell>
                                        <TableCell className="text-right flex items-center justify-end gap-1">
                                            <Button variant="ghost" size="icon" className="h-8 w-8 text-blue-600" onClick={() => setEditingProfileAppId(m.appId)} title="Edit App Profile">
                                                <Edit className="h-4 w-4" />
                                            </Button>
                                            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setManagingAppId(m.appId)} title="Manage Subscriptions">
                                                <Activity className="h-4 w-4" />
                                            </Button>
                                            <Button variant="ghost" size="icon" className={`h-8 w-8 text-red-500 hover:text-red-700 hover:bg-red-50 ${!canEdit ? "opacity-50 pointer-events-none" : ""}`} onClick={() => handleRemoveFromApp(m.appId)} title="Remove Access" disabled={!canEdit}>
                                                <Shield className="h-4 w-4" />
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                );
                            })
                        )}
                    </TableBody>
                </Table>
            </div>

            {/* Edit Profile Dialog */}
            {editingProfileAppId && (
                <EditAppProfileDialog
                    isOpen={!!editingProfileAppId}
                    onClose={() => setEditingProfileAppId(null)}
                    userId={user.id}
                    appId={editingProfileAppId}
                    appName={apps.find(a => a.id.toLowerCase() === editingProfileAppId.toLowerCase())?.name || 'App'}
                    onProfileUpdated={onUpdate}
                />
            )}

            <Dialog open={!!managingAppId} onOpenChange={(open) => !open && setManagingAppId(null)}>
                <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>Manage Subscriptions</DialogTitle>
                        <DialogDescription>Grant or modify subscriptions for this application.</DialogDescription>
                    </DialogHeader>
                    {managingAppId && <UserSubscriptionsTab appId={managingAppId} userId={user.id} userRole={user.memberships?.find(m => m.appId === managingAppId)?.roleName} />}
                </DialogContent>
            </Dialog>
        </div>
    );
};


const RoleSelector = ({ appId, currentRoleName, onRoleChange }: { appId: string, currentRoleName: string, onRoleChange: (role: string) => void }) => {
    const [roles, setRoles] = useState<RoleDto[]>([]);
    const [loading, setLoading] = useState(false);

    const loadRoles = async () => {
        setLoading(true);
        const data = await RoleService.getAllRoles(appId);
        setRoles(data);
        setLoading(false);
    };

    return (
        <Select onValueChange={onRoleChange} value={currentRoleName} onOpenChange={(open: boolean) => { if (open && roles.length === 0) loadRoles(); }}>
            <SelectTrigger className="w-[140px] h-8">
                <SelectValue placeholder={currentRoleName}>{currentRoleName}</SelectValue>
            </SelectTrigger>
            <SelectContent>
                {loading ? (
                    <div className="p-2 text-xs text-muted-foreground">Loading...</div>
                ) : (
                    roles.length > 0 ? roles.map(r => (
                        <SelectItem key={r.id} value={r.name}>{r.name}</SelectItem>
                    )) : <div className="p-2 text-xs text-muted-foreground">No roles found</div>
                )}
            </SelectContent>
        </Select>
    );
};

const RolePermissionsDisplay = ({ appId, roleName }: { appId: string, roleName: string }) => {
    const [permissions, setPermissions] = useState<string[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchPermissions = async () => {
            if (!roleName) return;
            setLoading(true);
            try {
                // Optimally we should cache this or lift state, but for now fetch per row/role change
                const roles = await RoleService.getAllRoles(appId);
                const role = roles.find(r => r.name === roleName);
                if (role && role.permissions) {
                    setPermissions(role.permissions);
                } else {
                    setPermissions([]);
                }
            } catch {
                setPermissions([]);
            } finally {
                setLoading(false);
            }
        };
        fetchPermissions();
    }, [appId, roleName]);

    if (loading) return <span className="text-xs text-muted-foreground">Loading...</span>;
    if (permissions.length === 0) return <span className="text-xs text-muted-foreground">-</span>;

    return (
        <div className="flex flex-wrap gap-1">
            {permissions.slice(0, 3).map(p => (
                <span key={p} className="text-[10px] px-1.5 py-0.5 bg-secondary text-secondary-foreground rounded border">{p}</span>
            ))}
            {permissions.length > 3 && (
                <span className="text-[10px] px-1.5 py-0.5 text-muted-foreground">+{permissions.length - 3}</span>
            )}
        </div>
    );
};


const AuditLogViewer = ({ userId, userName }: { userId: string, userName?: string }) => {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        loadLogs();
    }, [userId]);

    const loadLogs = async () => {
        // Prevent fetch if no userId (e.g. initial render)
        if (!userId) return;

        setLoading(true);
        // We use userId param to fetch logs related to this user
        const data = await AuditService.getLogs(undefined, userId);
        setLogs(data);
        setLoading(false);
    };

    if (loading) return <div className="p-4 text-center">Loading logs...</div>;
    if (logs.length === 0) return <div className="p-4 text-center text-muted-foreground">No audit history found.</div>;

    return (
        <div className="border rounded-md">
            <Table>
                <TableHeader>
                    <TableRow>
                        <TableHead>Time</TableHead>
                        <TableHead>Action</TableHead>
                        <TableHead>Changes</TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {logs.map(log => (
                        <TableRow key={log.id}>
                            <TableCell className="whitespace-nowrap text-xs">{new Date(log.timestamp).toLocaleString()}</TableCell>
                            <TableCell>
                                <div className="font-medium text-sm">{log.action}</div>
                                <div className="text-xs text-muted-foreground">{log.entityName}</div>
                            </TableCell>
                            <TableCell className="max-w-md truncate text-xs font-mono bg-muted/50 p-1 rounded" title={log.changesJson}>
                                {log.changesJson?.substring(0, 100)}{log.changesJson?.length > 100 ? '...' : ''}
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </div>
    );
};

export default UserDetailsPage;

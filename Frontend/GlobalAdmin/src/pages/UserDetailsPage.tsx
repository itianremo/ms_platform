import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Input } from "../components/ui/input";
import { Switch } from "../components/ui/switch";
import { Label } from "../components/ui/label";
import { ArrowLeft, Save, Shield, User, Activity, Lock, Key, Edit, CheckCircle2, MoreHorizontal, Info } from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";
import { UserService, UserDto, UserProfile } from '../services/userService';
import { useToast } from '../context/ToastContext';
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
    const navigate = useNavigate();
    const { showToast } = useToast();
    const [user, setUser] = useState<UserDto | null>(null);
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [allApps, setAllApps] = useState<AppConfig[]>([]);
    const [selectedSubscriptionAppId, setSelectedSubscriptionAppId] = useState<string>("");

    // Edit State
    const [isEditing, setIsEditing] = useState(false);
    const [editForm, setEditForm] = useState({ email: '', phone: '' });

    useEffect(() => {
        if (userId) fetchData(userId);
        fetchApps();
    }, [userId]);

    const fetchApps = async () => {
        try {
            const apps = await AppService.getAllApps();
            setAllApps(apps);
        } catch (e) { console.error(e); }
    }

    const fetchData = async (id: string) => {
        setLoading(true);
        try {
            const users = await UserService.getAllUsers(); // Inefficient but reused for now as we don't have getById explicitly in Service
            const foundUser = users.find(u => u.id === id);

            if (foundUser) {
                setUser(foundUser);
                const userProfile = await UserService.getProfile(id);
                setProfile(userProfile);
            } else {
                showToast("User not found", "error");
                navigate('/users');
            }
        } catch (error) {
            showToast("Failed to fetch user details", "error");
        } finally {
            setLoading(false);
        }
    };

    const handleStatusChange = async (newStatus: number) => {
        if (!user) return;
        try {
            await UserService.setUserStatus(user.id, newStatus);
            showToast("Status updated successfully", "success");
            // Optimistic update or refetch
            setUser({ ...user, status: newStatus });
            // fetchData(user.id); // Uncomment if backend response is complex
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
        if (isEditing) {
            handleSaveProfile();
        } else {
            setEditForm({ email: user?.email || '', phone: user?.phone || '' });
            setIsEditing(true);
        }
    };

    const handleSaveProfile = async () => {
        if (!user) return;
        // Basic validation or API call for core user details updates setup here
        // Currently UserService.updateProfile updates the UserProfile entity (bio, avatar, etc.)
        // Updating Email/Phone usually requires specific endpoints or admin override
        // Assuming we update UserProfile for Birthdate and maybe mock or notify for Email/Phone

        try {
            // Update Profile
            if (profile) {
                await UserService.updateProfile({
                    userId: user.id,
                    appId: "00000000-0000-0000-0000-000000000000", // Global
                    displayName: user.displayName || '',
                    bio: profile.bio,
                    avatarUrl: profile.avatarUrl,
                    customDataJson: profile.customDataJson || '{}',
                    dateOfBirth: profile.dateOfBirth,
                    gender: profile.gender
                });
            }
            // Ideally call API to update Email/Phone if changed

            showToast("Profile updated", "success");
            setIsEditing(false);
            fetchData(user.id);
        } catch (error) {
            showToast("Failed to save profile", "error");
        }
    };

    const handleVerifyAction = async (type: 'email' | 'phone', verified: boolean) => {
        if (!user) return;
        try {
            await UserService.verifyUserIdentity(user.id, type, verified);
            showToast(`${type} status updated`, "success");
            // Optimistic update
            if (type === 'email') setUser({ ...user, isEmailVerified: verified });
            if (type === 'phone') setUser({ ...user, isPhoneVerified: verified });
        } catch (e) {
            showToast("Failed to update verification status", "error");
        }
    };

    const handleRequestOtp = async (type: 'email' | 'phone') => {
        if (!user) return;
        try {
            // Reusing sendPasswordReset (OTP Request) logic but maybe specifying type if supported
            // Assuming endpoint handles type. Existing UserService.sendPasswordReset uses type: 0 (Email)
            await UserService.sendPasswordReset(user.email); // Sends OTP
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

        // Fallback to Email Logic
        const email = u.email || '';
        if (!email.includes('@')) return email || 'Unknown User';

        const [localPart, domainPart] = email.split('@');
        const domainName = domainPart.split('.')[0];

        // Capitalize for better display
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
            <div className="flex items-center gap-4">
                <Button variant="ghost" onClick={() => navigate('/users')}>
                    <ArrowLeft className="mr-2 h-4 w-4" /> Back to Users
                </Button>
                <Avatar className="h-16 w-16">
                    <AvatarImage src={profile?.avatarUrl} alt={headerTitle} />
                    <AvatarFallback className="text-lg">{getInitials(headerTitle)}</AvatarFallback>
                </Avatar>
                <div className="flex flex-col">
                    <h1 className="text-2xl font-bold">{headerTitle}</h1>
                    <span className="text-xs text-muted-foreground">{user.id}</span>
                </div>
                <span className={`px-2 py-1 rounded-full text-xs font-semibold ${user.status === 4 ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}`}>
                    {statusLabel}
                </span>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                {/* Omitted TabsContent for brevity, assuming they are inside */}


                <TabsList className="flex w-full justify-start border-b rounded-none bg-transparent p-0">
                    <TabsTrigger value="overview" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent">Overview</TabsTrigger>
                    <TabsTrigger value="security" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent">Security</TabsTrigger>
                    <TabsTrigger value="apps" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent">Apps</TabsTrigger>
                    <TabsTrigger value="audit" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent">Audit</TabsTrigger>
                </TabsList>

                <TabsContent value="apps" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Applications</CardTitle>
                            <CardDescription>Manage access to specific applications.</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <MembershipManager user={user} onUpdate={() => fetchData(user.id)} />
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
                                    <div className="flex items-center justify-between border p-3 rounded-md">
                                        <div className="flex items-center gap-3">
                                            <div className="h-8 w-8 bg-red-100 rounded-full flex items-center justify-center text-red-600 font-bold">G</div>
                                            <span className="text-sm font-medium">Google</span>
                                        </div>
                                        <Button variant="outline" size="sm" onClick={() => window.location.href = '/auth/api/ExternalAuth/link/Google'}>
                                            Connect
                                        </Button>
                                    </div>
                                    <div className="flex items-center justify-between border p-3 rounded-md">
                                        <div className="flex items-center gap-3">
                                            <div className="h-8 w-8 bg-blue-100 rounded-full flex items-center justify-center text-blue-600 font-bold">M</div>
                                            <span className="text-sm font-medium">Microsoft</span>
                                        </div>
                                        <Button variant="outline" size="sm" onClick={() => window.location.href = '/auth/api/ExternalAuth/link/Microsoft'}>
                                            Connect
                                        </Button>
                                    </div>
                                </div>
                            </div>

                            <div className="pt-4 border-t">
                                <Button variant="outline" onClick={handlePasswordReset}><Key className="mr-2 h-4 w-4" /> Send Password Reset Email</Button>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>





                <TabsContent value="overview" className="space-y-4">
                    <div className="flex justify-end">
                        <Button variant={isEditing ? "default" : "outline"} onClick={toggleEditMode} size="sm">
                            {isEditing ? <CheckCircle2 className="mr-2 h-4 w-4" /> : <Edit className="mr-2 h-4 w-4" />}
                            {isEditing ? "Save Changes" : "Edit Profile"}
                        </Button>
                    </div>
                    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Unique ID</CardTitle>
                                <User className="h-4 w-4 text-muted-foreground" />
                            </CardHeader>
                            <CardContent>
                                <div className="text-xs font-mono text-muted-foreground truncate" title={user.id}>{user.id}</div>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Email</CardTitle>
                                <Activity className="h-4 w-4 text-muted-foreground" />
                            </CardHeader>
                            <CardContent>
                                {isEditing ? (
                                    <Input
                                        value={editForm.email}
                                        onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                                        className="h-8"
                                    />
                                ) : (
                                    <div className="text-lg font-bold truncate" title={user.email}>{user.email}</div>
                                )}
                                <div className="flex items-center gap-2 mt-1">
                                    <span className={`text-xs ${user.isEmailVerified ? "text-green-600" : "text-yellow-600"}`}>
                                        {user.isEmailVerified ? "Verified" : "Unverified"}
                                    </span>
                                    {/* Admin Verification Controls */}
                                    <DropdownMenu>
                                        <DropdownMenuTrigger asChild>
                                            <Button variant="ghost" size="icon" className="h-4 w-4"><MoreHorizontal className="h-3 w-3" /></Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end">
                                            <DropdownMenuItem onClick={() => handleVerifyAction('email', !user.isEmailVerified)}>
                                                {user.isEmailVerified ? "Mark Unverified" : "Mark Verified"}
                                            </DropdownMenuItem>
                                            {!user.isEmailVerified && (
                                                <DropdownMenuItem onClick={() => handleRequestOtp('email')}>
                                                    Request OTP
                                                </DropdownMenuItem>
                                            )}
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </div>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Phone</CardTitle>
                                <Activity className="h-4 w-4 text-muted-foreground" />
                            </CardHeader>
                            <CardContent>
                                {isEditing ? (
                                    <Input
                                        value={editForm.phone}
                                        onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                                        className="h-8"
                                    />
                                ) : (
                                    <div className="text-lg font-bold truncate">{user.phone || "N/A"}</div>
                                )}
                                <div className="flex items-center gap-2 mt-1">
                                    <span className={`text-xs ${user.isPhoneVerified ? "text-green-600" : "text-yellow-600"}`}>
                                        {user.isPhoneVerified ? "Verified" : "Unverified"}
                                    </span>
                                    <DropdownMenu>
                                        <DropdownMenuTrigger asChild>
                                            <Button variant="ghost" size="icon" className="h-4 w-4"><MoreHorizontal className="h-3 w-3" /></Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end">
                                            <DropdownMenuItem onClick={() => handleVerifyAction('phone', !user.isPhoneVerified)}>
                                                {user.isPhoneVerified ? "Mark Unverified" : "Mark Verified"}
                                            </DropdownMenuItem>
                                            {!user.isPhoneVerified && (
                                                <DropdownMenuItem onClick={() => handleRequestOtp('phone')}>
                                                    Request OTP
                                                </DropdownMenuItem>
                                            )}
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </div>
                            </CardContent>
                        </Card>
                        <Card>
                            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                                <CardTitle className="text-sm font-medium">Date of Birth</CardTitle>
                                <Activity className="h-4 w-4 text-muted-foreground" />
                            </CardHeader>
                            <CardContent>
                                {isEditing ? (
                                    <Input
                                        type="date"
                                        value={profile?.dateOfBirth ? new Date(profile.dateOfBirth).toISOString().split('T')[0] : ''}
                                        onChange={(e) => setProfile(prev => prev ? { ...prev, dateOfBirth: e.target.value } : null)}
                                        className="h-8"
                                    />
                                ) : (
                                    <div className="text-lg font-bold text-sm truncate">
                                        {profile?.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString() : "N/A"}
                                    </div>
                                )}
                            </CardContent>
                        </Card>
                    </div>
                </TabsContent>

                <TabsContent value="subscriptions" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Subscriptions</CardTitle>
                            <CardDescription>Manage user subscriptions for specific apps.</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <div className="mb-4">
                                <Label>Select Application</Label>
                                <Select value={selectedSubscriptionAppId} onValueChange={setSelectedSubscriptionAppId}>
                                    <SelectTrigger className="w-[300px]">
                                        <SelectValue placeholder="Select App to manage" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {user.memberships?.map(m => {
                                            // Ideally we need App Name here. user.memberships might not have it populated fully?
                                            // The memberships array in UserDto usually has AppId. 
                                            // We might need to fetch apps list to map ID to Name if UserDto doesn't have it.
                                            // In MembershipManager we calculate names from 'apps'. 
                                            // We should fetch apps in UserDetailsPage or reusing available data.
                                            // For now, let's use ID if name missing, but 'MembershipManager' fetches 'apps'.
                                            // Let's lift 'apps' state up? Or just fetch here too.
                                            return <SelectItem key={m.appId} value={m.appId}>{m.appId}</SelectItem>
                                        })}
                                    </SelectContent>
                                </Select>
                            </div>

                            {selectedSubscriptionAppId ? (
                                <UserSubscriptionsTab appId={selectedSubscriptionAppId} userId={user.id} />
                            ) : (
                                <div className="text-muted-foreground text-sm">Please select an application to view subscriptions.</div>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

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
            </Tabs>
        </div>
    );
};


const MembershipManager = ({ user, onUpdate }: { user: UserDto, onUpdate: () => void }) => {
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [selectedApp, setSelectedApp] = useState<string>("");
    const [loading, setLoading] = useState(false);
    const { showToast } = useToast();
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);

    useEffect(() => {
        loadApps();
    }, []);

    const loadApps = async () => {
        try {
            const allApps = await AppService.getAllApps();
            setApps(allApps);
        } catch (error) {
            console.error(error);
        }
    };

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
                            <Select onValueChange={setSelectedApp} value={selectedApp}>
                                <SelectTrigger>
                                    <SelectValue placeholder="Select App" />
                                </SelectTrigger>
                                <SelectContent>
                                    {availableApps.map(app => (
                                        <SelectItem key={app.id} value={app.id}>{app.name}</SelectItem>
                                    ))}
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
                            <TableHead>Last Login</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {storedMemberships.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="text-center py-4 text-muted-foreground">
                                    No memberships found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            storedMemberships.map(m => {
                                const appName = apps.find(a => a.id === m.appId)?.name || 'Unknown App';
                                return (
                                    <TableRow key={m.appId}>
                                        <TableCell className="font-medium">{appName}</TableCell>
                                        <TableCell>
                                            <div className="flex items-center gap-2">
                                                {/* Text removed as requested, now using DDL + Badge Info */}
                                                <RoleSelector
                                                    appId={m.appId}
                                                    currentRoleName={m.roleName}
                                                    onRoleChange={(roleName) => handleRoleChange(m.appId, roleName)}
                                                />
                                                <RoleBadge roleName={m.roleName} appId={m.appId} showIcon={true} />
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <span className="text-xs text-muted-foreground">Never</span> {/* Placeholder for Last Login */}
                                        </TableCell>
                                        <TableCell>
                                            <Select
                                                defaultValue={m.status.toString()}
                                                onValueChange={(v: string) => handleStatusChange(m.appId, parseInt(v))}
                                            >
                                                <SelectTrigger className="w-[120px] h-8">
                                                    <SelectValue />
                                                </SelectTrigger>
                                                <SelectContent>
                                                    <SelectItem value="0">Active</SelectItem>
                                                    <SelectItem value="1">Banned</SelectItem>
                                                    <SelectItem value="2">Pending</SelectItem>
                                                </SelectContent>
                                            </Select>
                                        </TableCell>
                                        <TableCell className="text-right flex items-center justify-end gap-2">
                                            <Button variant="outline" size="sm" onClick={() => setManagingAppId(m.appId)}>
                                                Subscriptions
                                            </Button>
                                            <Button variant="ghost" size="sm" className="text-red-500 hover:text-red-700 hover:bg-red-50" onClick={() => handleRemoveFromApp(m.appId)}>
                                                Remove
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                );
                            })
                        )}
                    </TableBody>
                </Table>
            </div>

            <Dialog open={!!managingAppId} onOpenChange={(open) => !open && setManagingAppId(null)}>
                <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>Manage Subscriptions</DialogTitle>
                        <DialogDescription>Grant or modify subscriptions for this application.</DialogDescription>
                    </DialogHeader>
                    {managingAppId && <UserSubscriptionsTab appId={managingAppId} userId={user.id} />}
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
        <Select onValueChange={onRoleChange} defaultValue={currentRoleName} onOpenChange={(open: boolean) => { if (open && roles.length === 0) loadRoles(); }}>
            <SelectTrigger className="w-[140px] h-8">
                <SelectValue placeholder={currentRoleName} />
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

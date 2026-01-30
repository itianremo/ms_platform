import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Input } from "../components/ui/input";
import { Switch } from "../components/ui/switch";
import { Label } from "../components/ui/label";
import { ArrowLeft, Save, Shield, User, Activity, Lock, Key } from 'lucide-react';
import { UserService, UserDto, UserProfile } from '../services/userService';
import { useToast } from '../context/ToastContext';
import { RoleService, RoleDto } from '../services/roleService';
import { AuditService, AuditLog } from '../services/AuditService';

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

    useEffect(() => {
        if (userId) fetchData(userId);
    }, [userId]);

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
            fetchData(user.id);
        } catch (error) {
            showToast("Failed to update status", "error");
        }
    };

    const handleVerificationChange = async (type: 'email' | 'phone', verified: boolean) => {
        if (!user) return;
        try {
            await UserService.verifyUserIdentity(user.id, type, verified);
            showToast(`${type} verification updated`, "success");
            fetchData(user.id);
        } catch (error) {
            showToast("Failed to update verification", "error");
        }
    };

    const handleReactivate = async () => {
        if (!user) return;
        try {
            await UserService.reactivateUser(user.email);
            showToast("Reactivation email sent", "success");
        } catch (error) {
            showToast("Failed to send reactivation email", "error");
        }
    }

    if (loading || !user) return <div className="p-8">Loading...</div>;

    const statusLabel = GlobalUserStatus[user.status as keyof typeof GlobalUserStatus] || 'Unknown';

    return (
        <div className="p-4 md:p-8 space-y-6 animate-fade-in">
            <div className="flex items-center gap-4">
                <Button variant="ghost" onClick={() => navigate('/users')}>
                    <ArrowLeft className="mr-2 h-4 w-4" /> Back to Users
                </Button>
                <h1 className="text-2xl font-bold">{user.displayName || user.firstName}</h1>
                <span className={`px-2 py-1 rounded-full text-xs font-semibold ${user.status === 4 ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'}`}>
                    {statusLabel}
                </span>
            </div>

            <Tabs defaultValue="overview" className="w-full">
                <TabsList className="grid w-full grid-cols-4 lg:w-[400px]">
                    <TabsTrigger value="overview">Overview</TabsTrigger>
                    <TabsTrigger value="security">Security</TabsTrigger>
                    <TabsTrigger value="memberships">Apps</TabsTrigger>
                    <TabsTrigger value="audit">Audit</TabsTrigger>
                </TabsList>

                <TabsContent value="overview" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Profile Information</CardTitle>
                            <CardDescription>Basic user details and identity validation.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <Label>Email</Label>
                                    <div className="flex items-center gap-2">
                                        <Input value={user.email} readOnly />
                                        <Switch
                                            checked={user.isEmailVerified}
                                            onCheckedChange={(c: boolean) => handleVerificationChange('email', c)}
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <Label>Phone</Label>
                                    <div className="flex items-center gap-2">
                                        <Input value={user.phone || ''} readOnly placeholder="No phone" />
                                        <Switch
                                            checked={user.isPhoneVerified}
                                            onCheckedChange={(c: boolean) => handleVerificationChange('phone', c)}
                                        />
                                    </div>
                                </div>
                                <div className="space-y-2">
                                    <Label>Display Name</Label>
                                    <Input value={profile?.displayName || user.displayName || ''} readOnly />
                                </div>
                            </div>

                            <div className="pt-4 border-t">
                                <Label className="mb-2 block">Global Status Management</Label>
                                <div className="flex gap-2 flex-wrap">
                                    <Button size="sm" variant={user.status === 4 ? "default" : "outline"} onClick={() => handleStatusChange(4)}>Active</Button>
                                    <Button size="sm" variant={user.status === 5 ? "destructive" : "outline"} onClick={() => handleStatusChange(5)}>Ban</Button>
                                    <Button size="sm" variant={user.status === 3 ? "secondary" : "outline"} onClick={() => handleStatusChange(3)}>Pending Approval</Button>
                                    {user.status === 6 && (
                                        <Button size="sm" className="bg-orange-500 hover:bg-orange-600" onClick={handleReactivate}>Send Reactivation Email</Button>
                                    )}
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="security" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>Security Settings</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <Button variant="outline"><Key className="mr-2 h-4 w-4" /> Send Password Reset</Button>
                            {/* MFA status could go here */}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="memberships" className="space-y-4">
                    <Card>
                        <CardHeader>
                            <CardTitle>App Memberships</CardTitle>
                            <CardDescription>Manage access to specific applications.</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <MembershipManager user={user} onUpdate={() => fetchData(user.id)} />
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
                                            <RoleSelector
                                                appId={m.appId}
                                                currentRoleName={m.roleName || m.roleId}
                                                onRoleChange={(roleName) => handleRoleChange(m.appId, roleName)}
                                            />
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
                                        <TableCell className="text-right">
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


const AuditLogViewer = ({ userId }: { userId: string }) => {
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

import React, { useEffect, useState } from 'react';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../components/ui/table";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../components/ui/card";
import { Plus, MoreHorizontal, CheckCircle2, User as UserIcon, Shield, Lock, Trash2, Edit } from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";
import { UserService } from '../services/userService';
import type { UserDto } from '../services/userService';
import { AppService } from '../services/appService';
import type { AppConfig } from '../services/appService';
import { useToast } from '../context/ToastContext';
import { cn } from '../lib/utils';
import { formatDistanceToNow } from 'date-fns';

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
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { APP_ID } from '../config';

const UsersPage = () => {
    const [users, setUsers] = useState<UserDto[]>([]);
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();
    const [filter, setFilter] = useState<'all' | 'pending'>('all');
    const [selectedUsers, setSelectedUsers] = useState<Set<string>>(new Set());

    // Create User State
    const [open, setOpen] = useState(false);
    const [formData, setFormData] = useState({ email: '', password: '', firstName: '', lastName: '', phone: '' });

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            const [usersData, appsData, profilesData] = await Promise.all([
                UserService.getAllUsers(),
                AppService.getAllApps(),
                UserService.getAllProfiles()
            ]);

            // Merge profiles into users
            const mergedUsers = usersData.map(user => {
                const profile = profilesData.find(p => p.userId === user.id);
                return {
                    ...user,
                    displayName: profile?.displayName || user.displayName // Prefer profile name
                };
            });

            setUsers(mergedUsers);
            setApps(appsData);
        } catch (error) {
            console.error("Failed to fetch data", error);
            showToast("Failed to load users", "error");
        } finally {
            setLoading(false);
        }
    };

    const toggleUser = (userId: string) => {
        const newSelected = new Set(selectedUsers);
        if (newSelected.has(userId)) {
            newSelected.delete(userId);
        } else {
            newSelected.add(userId);
        }
        setSelectedUsers(newSelected);
    };

    const toggleAll = () => {
        if (selectedUsers.size === filteredUsers.length) {
            setSelectedUsers(new Set());
        } else {
            setSelectedUsers(new Set(filteredUsers.map(u => u.id)));
        }
    };

    const handleBulkAction = async (action: 'activate' | 'deactivate' | 'delete') => {
        if (!confirm(`Are you sure you want to ${action} ${selectedUsers.size} users?`)) return;

        setLoading(true);
        const errors: string[] = [];
        let successCount = 0;

        for (const userId of selectedUsers) {
            try {
                if (action === 'activate') {
                    await UserService.setUserStatus(userId, 1); // 1 = Active
                } else if (action === 'deactivate') {
                    await UserService.setUserStatus(userId, 2); // 2 = Inactive/Banned
                } else if (action === 'delete') {
                    // await UserService.deleteUser(userId);
                }
                successCount++;
            } catch (err) {
                errors.push(userId);
            }
        }

        setLoading(false);
        if (errors.length > 0) {
            showToast(`Completed with errors. Failed: ${errors.length}`, "error");
        } else {
            showToast(`Successfully ${action}d ${successCount} users`, "success");
        }
        setSelectedUsers(new Set());
        fetchData();
    };

    const handleSubmit = async () => {
        setLoading(true);
        try {
            await UserService.createUser({ ...formData, appId: APP_ID });
            showToast("User created successfully", "success");
            setOpen(false);
            setFormData({ email: '', password: '', firstName: '', lastName: '', phone: '' });
            fetchData();
        } catch (error) {
            showToast("Failed to create user", "error");
        } finally {
            setLoading(false);
        }
    };

    const filteredUsers = users.filter(user => {
        if (filter === 'pending') return user.status === 3; // PendingAdminApproval
        return true;
    });

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Users</h2>
                    <p className="text-muted-foreground">Manage system users and their roles.</p>
                </div>
                <div className="flex space-x-2">
                    <Button
                        variant={filter === 'all' ? "default" : "outline"}
                        onClick={() => setFilter('all')}
                    >
                        All Users
                    </Button>
                    <Button
                        variant={filter === 'pending' ? "default" : "outline"}
                        onClick={() => setFilter('pending')}
                    >
                        Pending Support
                    </Button>

                    {/* Bulk Actions */}
                    {selectedUsers.size > 0 && (
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="secondary" className="bg-muted">
                                    <Shield className="mr-2 h-4 w-4" />
                                    Actions ({selectedUsers.size})
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent>
                                <DropdownMenuLabel>Bulk Actions</DropdownMenuLabel>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem onClick={() => handleBulkAction('activate')}>
                                    <CheckCircle2 className="mr-2 h-4 w-4 text-green-500" /> Activate Selected
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={() => handleBulkAction('deactivate')} className="text-orange-500">
                                    <Lock className="mr-2 h-4 w-4" /> Deactivate Selected
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    )}

                    <Dialog open={open} onOpenChange={setOpen}>
                        <DialogTrigger asChild>
                            <Button className="h-10 w-10 p-0 rounded-full" title="Create User">
                                <Plus size={20} />
                                <span className="sr-only">Create User</span>
                            </Button>
                        </DialogTrigger>
                        <DialogContent>
                            <DialogHeader>
                                <DialogTitle>Create New User</DialogTitle>
                                <DialogDescription>Add a new user to the system.</DialogDescription>
                            </DialogHeader>
                            <div className="grid gap-4 py-4">
                                <div className="grid gap-2">
                                    <Label>Email</Label>
                                    <Input value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} placeholder="user@example.com" />
                                </div>
                                <div className="grid gap-2">
                                    <Label>Password</Label>
                                    <Input value={formData.password} onChange={(e) => setFormData({ ...formData, password: e.target.value })} type="password" placeholder="********" />
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="grid gap-2">
                                        <Label>First Name</Label>
                                        <Input value={formData.firstName} onChange={(e) => setFormData({ ...formData, firstName: e.target.value })} />
                                    </div>
                                    <div className="grid gap-2">
                                        <Label>Last Name</Label>
                                        <Input value={formData.lastName} onChange={(e) => setFormData({ ...formData, lastName: e.target.value })} />
                                    </div>
                                </div>
                                <div className="grid gap-2">
                                    <Label>Phone (Optional)</Label>
                                    <Input value={formData.phone} onChange={(e) => setFormData({ ...formData, phone: e.target.value })} placeholder="+1234567890" />
                                </div>
                            </div>
                            <DialogFooter>
                                <Button variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
                                <Button onClick={handleSubmit} disabled={loading}>
                                    {loading ? "Creating..." : "Create User"}
                                </Button>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>{filter === 'pending' ? 'Pending Approvals' : 'All Users'}</CardTitle>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="w-[40px]">
                                    <input
                                        type="checkbox"
                                        className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                        checked={filteredUsers.length > 0 && selectedUsers.size === filteredUsers.length}
                                        onChange={toggleAll}
                                    />
                                </TableHead>
                                <TableHead>User / Email</TableHead>
                                <TableHead>Joined</TableHead>
                                <TableHead>Last Usage</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {filteredUsers.map((user) => (
                                <TableRow key={user.id} data-state={selectedUsers.has(user.id) ? "selected" : undefined}>
                                    <TableCell>
                                        <input
                                            type="checkbox"
                                            className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                            checked={selectedUsers.has(user.id)}
                                            onChange={() => toggleUser(user.id)}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <Avatar className="h-9 w-9">
                                                <AvatarImage src={user.photoUrl} alt={user.displayName || "User"} />
                                                <AvatarFallback>
                                                    {(user.displayName || user.firstName || 'U').substring(0, 2).toUpperCase()}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div className="flex flex-col">
                                                <span className="font-medium text-sm">{user.displayName || `${user.firstName} ${user.lastName}`}</span>
                                                <span className="text-xs text-muted-foreground">{user.email}</span>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-xs text-muted-foreground">Unknown</span>
                                    </TableCell>
                                    <TableCell>
                                        {(() => {
                                            if (!user.lastLoginUtc) return <span className="text-muted-foreground text-xs">Never</span>;
                                            const app = user.lastLoginAppId ? apps.find(a => a.id.toLowerCase() === user.lastLoginAppId?.toLowerCase()) : null;
                                            const appName = app ? app.name : 'System';
                                            return (
                                                <div className="flex flex-col">
                                                    <span className="font-medium text-xs">{appName}</span>
                                                    <span className="text-[10px] text-muted-foreground">
                                                        {formatDistanceToNow(new Date(user.lastLoginUtc), { addSuffix: true })}
                                                    </span>
                                                </div>
                                            );
                                        })()}
                                    </TableCell>
                                    <TableCell>
                                        <span className={cn(
                                            "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
                                            user.isActive
                                                ? "bg-green-100 text-green-800"
                                                : "bg-red-100 text-red-800"
                                        )}>
                                            {user.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" size="icon">
                                                    <MoreHorizontal className="h-4 w-4" />
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                                <DropdownMenuItem>
                                                    <Edit className="mr-2 h-4 w-4" /> Edit
                                                </DropdownMenuItem>
                                                <DropdownMenuSeparator />
                                                <DropdownMenuItem className="text-destructive">
                                                    <Trash2 className="mr-2 h-4 w-4" /> Deactivate
                                                </DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>
        </div >
    );
};

export default UsersPage;

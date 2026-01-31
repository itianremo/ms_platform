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
import { UserService, UserDto } from '../services/userService';
import { RoleService, RoleDto } from '../services/roleService';
import { useToast } from '../context/ToastContext';
import { cn } from '../lib/utils';
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

const UsersPage = () => {
    const [users, setUsers] = useState<UserDto[]>([]);
    const [roles, setRoles] = useState<RoleDto[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();
    const [filter, setFilter] = useState<'all' | 'pending'>('all');
    const [selectedUsers, setSelectedUsers] = useState<Set<string>>(new Set());

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
                    // Logic for activation (e.g. Set Status to Active or Unban)
                    await UserService.setUserStatus(userId, 1); // 1 = Active
                } else if (action === 'deactivate') {
                    await UserService.setUserStatus(userId, 2); // 2 = Banned? Or Inactive?
                } else if (action === 'delete') {
                    // await UserService.deleteUser(userId); // Not implemented yet
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

    // Create User State
    const [open, setOpen] = useState(false);
    const [formData, setFormData] = useState({ email: '', password: '', firstName: '', lastName: '', phone: '' });

    const handleSubmit = async () => {
        setLoading(true);
        try {
            await UserService.createUser(formData);
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

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            const [usersData, rolesData, profilesData] = await Promise.all([
                UserService.getAllUsers(),
                RoleService.getAllRoles(),
                UserService.getAllProfiles()
            ]);

            // Merge profiles into users
            const mergedUsers = usersData.map(user => {
                const profile = profilesData.find(p => p.userId === user.id);
                return {
                    ...user,
                    displayName: profile?.displayName || user.displayName // Prefer profile name, fallback to existing or undefined
                };
            });

            setUsers(mergedUsers);
            setRoles(rolesData);
        } catch (error) {
            showToast("Failed to fetch data", "error");
        } finally {
            setLoading(false);
        }
    };

    const getRolePermissions = (roleName: string) => {
        const role = roles.find(r => r.name === roleName);
        return role ? role.permissions : [];
    };

    // ... useEffect ...

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
                                <DropdownMenuSeparator />
                                <DropdownMenuItem onClick={() => handleBulkAction('delete')} className="text-destructive">
                                    <Trash2 className="mr-2 h-4 w-4" /> Delete Selected
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
                                <DialogDescription>Add a new user to the system. A default profile will be created.</DialogDescription>
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
                                        <Input value={formData.firstName} onChange={(e) => setFormData({ ...formData, firstName: e.target.value })} placeholder="John" />
                                    </div>
                                    <div className="grid gap-2">
                                        <Label>Last Name</Label>
                                        <Input value={formData.lastName} onChange={(e) => setFormData({ ...formData, lastName: e.target.value })} placeholder="Doe" />
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
                    <CardDescription>
                        {filter === 'pending'
                            ? "Users waiting for admin approval."
                            : "A list of all users registered in the system."}
                    </CardDescription>
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
                                <TableHead>Name</TableHead>
                                <TableHead>Email</TableHead>
                                <TableHead>Phone</TableHead>
                                <TableHead>Roles</TableHead>
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
                                    <TableCell className="font-medium">
                                        <div className="flex items-center gap-2">
                                            <Avatar className="h-8 w-8">
                                                <AvatarImage src={undefined} alt={user.displayName} /> {/* No profile data in list yet */}
                                                <AvatarFallback className="text-xs">
                                                    {(() => {
                                                        const isValidName = (n?: string) => n && n !== 'N/A' && n !== 'n/a';
                                                        const name = isValidName(user.displayName) ? user.displayName : (isValidName(user.firstName) && isValidName(user.lastName) ? `${user.firstName} ${user.lastName}` : '');

                                                        if (name) {
                                                            const parts = name.split(' ');
                                                            if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
                                                            return name.substring(0, 2).toUpperCase();
                                                        }
                                                        // Fallback to Email Initials
                                                        if (user.email && user.email.includes('@')) {
                                                            const [local, domain] = user.email.split('@');
                                                            const domainName = domain.split('.')[0];
                                                            if (local && domainName) {
                                                                return (local[0] + domainName[0]).toUpperCase();
                                                            }
                                                            return user.email.substring(0, 2).toUpperCase();
                                                        }
                                                        return 'U';
                                                    })()}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div className="flex flex-col">
                                                <span>
                                                    {(() => {
                                                        const isValidName = (n?: string) => n && n !== 'N/A' && n !== 'n/a';
                                                        if (isValidName(user.displayName)) return user.displayName;
                                                        if (isValidName(user.firstName) && isValidName(user.lastName)) return `${user.firstName} ${user.lastName}`;
                                                        if (isValidName(user.firstName)) return user.firstName;

                                                        // Email Fallback
                                                        if (user.email && user.email.includes('@')) {
                                                            const [local, domain] = user.email.split('@');
                                                            const domainName = domain.split('.')[0];
                                                            const capitalize = (s: string) => s ? s.charAt(0).toUpperCase() + s.slice(1) : '';
                                                            return `${capitalize(local)} ${capitalize(domainName)}`;
                                                        }
                                                        return 'Unknown User';
                                                    })()}
                                                </span>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            {user.email}
                                            {user.isEmailVerified && <span title="Verified"><CheckCircle2 className="h-4 w-4 text-green-500" /></span>}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            {user.phone || '-'}
                                            {user.isPhoneVerified && <span title="Verified"><CheckCircle2 className="h-4 w-4 text-green-500" /></span>}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex gap-1 flex-wrap">
                                            {user.roles.map(roleName => {
                                                const permissions = getRolePermissions(roleName);
                                                return (
                                                    <div key={roleName} className="group relative">
                                                        <span className="cursor-help inline-flex items-center rounded-md border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80">
                                                            {roleName}
                                                        </span>
                                                        {/* Tooltip */}
                                                        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 hidden group-hover:block w-48 z-50">
                                                            <div className="bg-popover text-popover-foreground text-xs rounded-md border shadow-md p-2">
                                                                <p className="font-bold mb-1 border-b pb-1">{roleName} Permissions:</p>
                                                                {permissions.length > 0 ? (
                                                                    <ul className="list-disc list-inside space-y-0.5 text-[10px] text-muted-foreground">
                                                                        {permissions.map(p => (
                                                                            <li key={p}>{p}</li>
                                                                        ))}
                                                                    </ul>
                                                                ) : (
                                                                    <span className="text-muted-foreground italic">No specific permissions</span>
                                                                )}
                                                            </div>
                                                            {/* Arrow */}
                                                            <div className="absolute left-1/2 -translate-x-1/2 top-full -mt-1 h-2 w-2 bg-popover border-b border-r border-popover transform rotate-45"></div>
                                                        </div>
                                                    </div>
                                                );
                                            })}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <span className={cn(
                                            "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
                                            user.isActive
                                                ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400"
                                                : "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400"
                                        )}>
                                            {user.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" size="icon">
                                                    <MoreHorizontal className="h-4 w-4" />
                                                    <span className="sr-only">Actions</span>
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                                <DropdownMenuItem onClick={() => window.location.href = `/users/${user.id}`}>
                                                    <Edit className="mr-2 h-4 w-4" /> Edit User
                                                </DropdownMenuItem>
                                                <DropdownMenuItem>
                                                    <Lock className="mr-2 h-4 w-4" /> Reset Password
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

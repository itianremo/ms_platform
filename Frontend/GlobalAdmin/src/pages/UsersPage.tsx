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

const UsersPage = () => {
    const [users, setUsers] = useState<UserDto[]>([]);
    const [roles, setRoles] = useState<RoleDto[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();
    const [filter, setFilter] = useState<'all' | 'pending'>('all');

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
                    <Button
                        variant={filter === 'pending' ? "default" : "outline"}
                        onClick={() => setFilter('pending')}
                        className="relative"
                    >
                        Pending Approvals
                        {users.filter(u => u.status === 3).length > 0 && (
                            <span className="absolute -top-2 -right-2 bg-red-500 text-white text-[10px] rounded-full h-5 w-5 flex items-center justify-center">
                                {users.filter(u => u.status === 3).length}
                            </span>
                        )}
                    </Button>
                    <Button className="h-10 w-10 p-0 rounded-full" title="Create User">
                        <Plus size={20} />
                        <span className="sr-only">Create User</span>
                    </Button>
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
                                <TableRow key={user.id}>
                                    <TableCell className="font-medium">
                                        <div className="flex items-center gap-2">
                                            <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                                                <UserIcon size={14} />
                                            </div>
                                            <span>
                                                {user.displayName || (user.firstName && user.lastName ? `${user.firstName} ${user.lastName}` : 'N/A')}
                                            </span>
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
        </div>
    );
};

export default UsersPage;

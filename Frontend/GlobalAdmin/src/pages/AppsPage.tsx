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
import { Plus, Globe, Edit, Power, Trash2, MoreHorizontal, Loader2, Shield, CheckCircle2, Lock, LayoutGrid } from 'lucide-react';
import { AppService, AppConfig } from '../services/appService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { cn } from '../lib/utils';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "../components/ui/dialog";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "../components/ui/dropdown-menu";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";
import ExternalAuthConfigTab from '../components/apps/ExternalAuthConfigTab';
import AppPackagesTab from '../components/apps/AppPackagesTab';

const AppsPage = () => {
    const { user } = useAuth();
    const { showToast } = useToast();
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [loading, setLoading] = useState(true);

    // Permission Check
    const canManageApps = user?.roles?.some(r => r === 'ManageApps' || r === 'AccessAll') || false;

    // Selection & Filter
    const [selectedApps, setSelectedApps] = useState<Set<string>>(new Set());
    const [filter, setFilter] = useState<'all' | 'active' | 'inactive'>('all');

    // Dialog States
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [isEditOpen, setIsEditOpen] = useState(false);
    const [selectedApp, setSelectedApp] = useState<AppConfig | null>(null);
    const [actionLoading, setActionLoading] = useState(false);

    // Form States
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        baseUrl: '',
        defaultUserProfileJson: '{}'
    });

    useEffect(() => {
        fetchApps();
    }, []);

    const fetchApps = async () => {
        try {
            const data = await AppService.getAllApps();
            setApps(data);
        } catch (error) {
            showToast("Failed to fetch applications", "error");
        } finally {
            setLoading(false);
        }
    };

    const toggleApp = (appId: string) => {
        const newSelected = new Set(selectedApps);
        if (newSelected.has(appId)) newSelected.delete(appId);
        else newSelected.add(appId);
        setSelectedApps(newSelected);
    };

    const toggleAll = () => {
        if (selectedApps.size === filteredApps.length) setSelectedApps(new Set());
        else setSelectedApps(new Set(filteredApps.map(a => a.id)));
    };

    const handleCreate = async () => {
        if (!formData.name || !formData.baseUrl) {
            showToast("Name and Base URL are required", "error");
            return;
        }

        setActionLoading(true);
        try {
            await AppService.createApp(formData);
            showToast("Application created successfully", "success");
            setIsCreateOpen(false);
            setFormData({ name: '', description: '', baseUrl: '', defaultUserProfileJson: '{}' });
            fetchApps();
        } catch (error) {
            showToast("Failed to create application", "error");
        } finally {
            setActionLoading(false);
        }
    };

    const handleEditStart = (app: AppConfig) => {
        if (!canManageApps) return;
        setSelectedApp(app);
        setFormData({
            name: app.name,
            description: app.description,
            baseUrl: app.baseUrl,
            defaultUserProfileJson: (app as any).defaultUserProfileJson || '{}'
        });
        setIsEditOpen(true);
    };

    const handleUpdate = async () => {
        if (!selectedApp) return;
        if (!formData.name || !formData.baseUrl) {
            showToast("Name and Base URL are required", "error");
            return;
        }

        // Validate JSON
        try {
            JSON.parse(formData.defaultUserProfileJson);
        } catch (e) {
            showToast("Invalid JSON in Default User Settings", "error");
            return;
        }

        setActionLoading(true);
        try {
            await AppService.updateApp(selectedApp.id, formData);
            showToast("Application updated successfully", "success");
            setIsEditOpen(false);
            setSelectedApp(null);
            fetchApps();
        } catch (error) {
            showToast("Failed to update application", "error");
        } finally {
            setActionLoading(false);
        }
    };

    const handleToggleStatus = async (app: AppConfig) => {
        if (!canManageApps) return;
        const newStatus = !app.isActive;
        const action = newStatus ? "activated" : "deactivated";

        try {
            await AppService.toggleStatus(app.id, newStatus);
            showToast(`Application ${action} successfully`, "success");
            setApps(prev => prev.map(a => a.id === app.id ? { ...a, isActive: newStatus } : a));
        } catch (error) {
            showToast(`Failed to ${action} application`, "error");
            fetchApps();
        }
    };

    const handleBulkAction = async (action: 'activate' | 'deactivate' | 'delete') => {
        if (!canManageApps) return;
        if (!confirm(`Are you sure you want to ${action} ${selectedApps.size} applications?`)) return;

        setLoading(true);
        const errors: string[] = [];
        let successCount = 0;

        for (const appId of selectedApps) {
            try {
                if (action === 'activate') await AppService.toggleStatus(appId, true);
                else if (action === 'deactivate') await AppService.toggleStatus(appId, false);
                else if (action === 'delete') await AppService.deleteApp(appId);
                successCount++;
            } catch (err) {
                errors.push(appId);
            }
        }

        setLoading(false);
        if (errors.length > 0) showToast(`Completed with errors. Failed: ${errors.length}`, "error");
        else showToast(`Successfully ${action}d ${successCount} apps`, "success");

        setSelectedApps(new Set());
        fetchApps();
    };

    const filteredApps = apps.filter(app => {
        if (filter === 'active') return app.isActive;
        if (filter === 'inactive') return !app.isActive;
        return true;
    });

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <Loader2 className="animate-spin h-8 w-8 text-primary" />
        </div>
    );

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Applications</h2>
                    <p className="text-muted-foreground">Manage registered applications and tenants.</p>
                </div>

                <div className="flex space-x-2">
                    <div className="flex bg-muted rounded-lg p-1">
                        <Button variant={filter === 'all' ? 'secondary' : 'ghost'} size="sm" onClick={() => setFilter('all')}>All</Button>
                        <Button variant={filter === 'active' ? 'secondary' : 'ghost'} size="sm" onClick={() => setFilter('active')}>Active</Button>
                        <Button variant={filter === 'inactive' ? 'secondary' : 'ghost'} size="sm" onClick={() => setFilter('inactive')}>Inactive</Button>
                    </div>

                    {selectedApps.size > 0 && canManageApps && (
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="secondary" className="bg-muted">
                                    <Shield className="mr-2 h-4 w-4" />
                                    Actions ({selectedApps.size})
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

                    {canManageApps && (
                        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
                            <DialogTrigger asChild>
                                <Button className="gap-2" onClick={() => setFormData({ name: '', description: '', baseUrl: '', defaultUserProfileJson: '{}' })}>
                                    <Plus size={16} />
                                    New App
                                </Button>
                            </DialogTrigger>
                            <DialogContent>
                                <DialogHeader>
                                    <DialogTitle>Create New Application</DialogTitle>
                                    <DialogDescription>Add a new micro-frontend application to the platform.</DialogDescription>
                                </DialogHeader>
                                <div className="space-y-4 py-4">
                                    <div className="space-y-2">
                                        <Label>Application Name</Label>
                                        <Input
                                            placeholder="e.g. User Management"
                                            value={formData.name}
                                            onChange={e => setFormData({ ...formData, name: e.target.value })}
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>Base URL</Label>
                                        <Input
                                            placeholder="https://app.example.com"
                                            value={formData.baseUrl}
                                            onChange={e => setFormData({ ...formData, baseUrl: e.target.value })}
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>Description</Label>
                                        <Input
                                            placeholder="Brief description of the app"
                                            value={formData.description}
                                            onChange={e => setFormData({ ...formData, description: e.target.value })}
                                        />
                                    </div>
                                </div>
                                <DialogFooter>
                                    <Button variant="outline" onClick={() => setIsCreateOpen(false)}>Cancel</Button>
                                    <Button onClick={handleCreate} disabled={actionLoading}>
                                        {actionLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                        Create App
                                    </Button>
                                </DialogFooter>
                            </DialogContent>
                        </Dialog>
                    )}
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Registered Apps</CardTitle>
                    <CardDescription>
                        A list of all applications and their status.
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
                                        checked={filteredApps.length > 0 && selectedApps.size === filteredApps.length}
                                        onChange={toggleAll}
                                    />
                                </TableHead>
                                <TableHead>Application</TableHead>
                                <TableHead>Base URL</TableHead>
                                <TableHead>Description</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {filteredApps.map((app) => (
                                <TableRow key={app.id}>
                                    <TableCell>
                                        <input
                                            type="checkbox"
                                            className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                            checked={selectedApps.has(app.id)}
                                            onChange={() => toggleApp(app.id)}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <Avatar className="h-9 w-9 rounded-lg border">
                                                <AvatarImage src="" /> {/* Todo: App Icon URL */}
                                                <AvatarFallback className="rounded-lg bg-primary/10 text-primary">
                                                    {app.name.substring(0, 2).toUpperCase()}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div className="font-medium text-foreground">{app.name}</div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <a
                                            href={app.baseUrl}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="flex items-center gap-1 text-primary hover:underline text-sm"
                                        >
                                            <Globe size={14} />
                                            {app.baseUrl}
                                        </a>
                                    </TableCell>
                                    <TableCell className="text-muted-foreground">{app.description}</TableCell>
                                    <TableCell>
                                        <span className={cn(
                                            "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
                                            app.isActive
                                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                                : "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400"
                                        )}>
                                            {app.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="ghost" className="h-8 w-8 p-0">
                                                    <span className="sr-only">Open menu</span>
                                                    <MoreHorizontal className="h-4 w-4" />
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent align="end">
                                                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                                                <DropdownMenuItem onClick={() => handleEditStart(app)} disabled={!canManageApps}>
                                                    <Edit className="mr-2 h-4 w-4" /> Edit Details
                                                </DropdownMenuItem>
                                                <DropdownMenuSeparator />
                                                <DropdownMenuItem
                                                    onClick={() => handleToggleStatus(app)}
                                                    disabled={!canManageApps}
                                                    className={app.isActive ? "text-red-600" : "text-green-600"}
                                                >
                                                    <Power className="mr-2 h-4 w-4" />
                                                    {app.isActive ? "Deactivate" : "Activate"}
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

            {/* Edit Dialog */}
            <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
                <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>Edit Application</DialogTitle>
                        <DialogDescription>Update application details and configurations.</DialogDescription>
                    </DialogHeader>

                    <div className="py-2">
                        <Tabs defaultValue="details" className="w-full">
                            <TabsList className="grid w-full grid-cols-4">
                                <TabsTrigger value="details">Details</TabsTrigger>
                                <TabsTrigger value="auth">External Auth</TabsTrigger>
                                <TabsTrigger value="defaults">Defaults</TabsTrigger>
                                <TabsTrigger value="packages">Packages</TabsTrigger>
                            </TabsList>

                            <TabsContent value="details" className="space-y-4 py-4">
                                <div className="space-y-2">
                                    <Label>Application Name</Label>
                                    <Input
                                        value={formData.name}
                                        onChange={e => setFormData({ ...formData, name: e.target.value })}
                                    />
                                </div>
                                <div className="space-y-2">
                                    <Label>Base URL</Label>
                                    <Input
                                        value={formData.baseUrl}
                                        onChange={e => setFormData({ ...formData, baseUrl: e.target.value })}
                                    />
                                </div>
                                <div className="space-y-2">
                                    <Label>Description</Label>
                                    <Input
                                        value={formData.description}
                                        onChange={e => setFormData({ ...formData, description: e.target.value })}
                                    />
                                </div>
                                <div className="pt-4 flex justify-end">
                                    <Button onClick={handleUpdate} disabled={actionLoading}>
                                        {actionLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                        Save Details
                                    </Button>
                                </div>
                            </TabsContent>

                            <TabsContent value="auth">
                                {selectedApp && (
                                    <ExternalAuthConfigTab
                                        app={selectedApp}
                                        onSave={() => fetchApps()}
                                    />
                                )}
                            </TabsContent>

                            <TabsContent value="defaults" className="py-4 space-y-4">
                                <div className="space-y-2">
                                    <Label>Default User Settings (JSON)</Label>
                                    <textarea
                                        className="flex min-h-[150px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 font-mono"
                                        value={formData.defaultUserProfileJson}
                                        onChange={e => setFormData({ ...formData, defaultUserProfileJson: e.target.value })}
                                        placeholder="{}"
                                    />
                                    <p className="text-xs text-muted-foreground">
                                        These settings will be applied to new users added to this application properly default customDataJson.
                                    </p>
                                </div>
                                <div className="pt-4 flex justify-end">
                                    <Button onClick={handleUpdate} disabled={actionLoading}>
                                        {actionLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                        Save Defaults
                                    </Button>
                                </div>
                            </TabsContent>

                            <TabsContent value="packages" className="py-4">
                                {selectedApp && <AppPackagesTab app={selectedApp} />}
                            </TabsContent>
                        </Tabs>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default AppsPage;

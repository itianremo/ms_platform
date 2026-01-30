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
import { Plus, Globe, Edit, Power, Trash2, MoreHorizontal, Loader2 } from 'lucide-react';
import { AppService, AppConfig } from '../services/appService';
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
import ExternalAuthConfigTab from '../components/apps/ExternalAuthConfigTab';

const AppsPage = () => {
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();

    // Dialog States
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [isEditOpen, setIsEditOpen] = useState(false);
    const [selectedApp, setSelectedApp] = useState<AppConfig | null>(null);
    const [actionLoading, setActionLoading] = useState(false);

    // Form States
    const [formData, setFormData] = useState({ name: '', description: '', baseUrl: '' });

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
            setFormData({ name: '', description: '', baseUrl: '' });
            fetchApps();
        } catch (error) {
            showToast("Failed to create application", "error");
        } finally {
            setActionLoading(false);
        }
    };

    const handleEditStart = (app: AppConfig) => {
        setSelectedApp(app);
        setFormData({ name: app.name, description: app.description, baseUrl: app.baseUrl });
        setIsEditOpen(true);
    };

    const handleUpdate = async () => {
        if (!selectedApp) return;
        if (!formData.name || !formData.baseUrl) {
            showToast("Name and Base URL are required", "error");
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
        // Optimistic update
        const newStatus = !app.isActive;
        const action = newStatus ? "activated" : "deactivated";

        try {
            await AppService.toggleStatus(app.id, newStatus);
            showToast(`Application ${action} successfully`, "success");

            // Update local state
            setApps(prev => prev.map(a => a.id === app.id ? { ...a, isActive: newStatus } : a));
        } catch (error) {
            showToast(`Failed to ${action} application`, "error");
            fetchApps(); // Revert on failure
        }
    };

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>
    );

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Applications</h2>
                    <p className="text-muted-foreground">Manage registered applications and tenants.</p>
                </div>

                <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
                    <DialogTrigger asChild>
                        <Button className="gap-2" onClick={() => setFormData({ name: '', description: '', baseUrl: '' })}>
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
                                <TableHead className="w-[200px]">Name</TableHead>
                                <TableHead>Base URL</TableHead>
                                <TableHead>Description</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {apps.map((app) => (
                                <TableRow key={app.id}>
                                    <TableCell className="font-medium text-foreground">
                                        {app.name}
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
                                                <DropdownMenuItem onClick={() => handleEditStart(app)}>
                                                    <Edit className="mr-2 h-4 w-4" /> Edit Details
                                                </DropdownMenuItem>
                                                <DropdownMenuSeparator />
                                                <DropdownMenuItem onClick={() => handleToggleStatus(app)} className={app.isActive ? "text-red-600" : "text-green-600"}>
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
                <DialogContent className="max-w-2xl">
                    <DialogHeader>
                        <DialogTitle>Edit Application</DialogTitle>
                        <DialogDescription>Update application details and configurations.</DialogDescription>
                    </DialogHeader>

                    <div className="py-2">
                        <Tabs defaultValue="details" className="w-full">
                            <TabsList className="grid w-full grid-cols-2">
                                <TabsTrigger value="details">General Details</TabsTrigger>
                                <TabsTrigger value="auth">External Auth</TabsTrigger>
                                <TabsTrigger value="payment">Payment</TabsTrigger>
                            </TabsList>

                            <TabsContent value="details" className="space-y-4 py-4">
                                {/* ... existing details form ... */}
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

                            <TabsContent value="payment" className="py-4">
                                <div className="p-4 border border-dashed rounded text-center text-muted-foreground">
                                    <p>Payment Gateway Configuration (Stripe/PayPal)</p>
                                    <p className="text-xs mt-2">Coming Soon: Configure per-tenant API keys.</p>
                                </div>
                            </TabsContent>
                        </Tabs>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default AppsPage;

import { useEffect, useState } from 'react';
import { AppService, SubscriptionPackage } from '../../services/appService';
import { Button } from '../ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../ui/card';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from '../ui/dialog';
import { Label } from '../ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../ui/table';
import { Input } from '../ui/input';
import { Loader2, Plus, Ban, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';

import { useAuth } from '../../context/AuthContext';

interface UserSubscriptionsTabProps {
    appId: string;
    userId: string;
    userRole?: string;
}

interface Subscription {
    id: string;
    packageName: string;
    startDate: string;
    endDate: string;
    isActive: boolean;
    pricePaid: number;
}

export function UserSubscriptionsTab({ appId, userId, userRole }: UserSubscriptionsTabProps) {
    const { user: currentUser } = useAuth();
    const isSuperAdmin = currentUser?.roles?.includes('SuperAdmin');
    const isTargetAdmin = userRole ? (userRole.includes('Admin') || userRole === 'SuperAdmin') : false;
    const canEdit = isSuperAdmin || !isTargetAdmin;

    const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
    const [packages, setPackages] = useState<SubscriptionPackage[]>([]);
    const [loading, setLoading] = useState(true);
    const [grantDialogOpen, setGrantDialogOpen] = useState(false);

    // Form State
    const [selectedPackage, setSelectedPackage] = useState<string>('');
    const [customStartDate, setCustomStartDate] = useState<string>('');
    const [customEndDate, setCustomEndDate] = useState<string>('');

    const loadData = async () => {
        setLoading(true);
        try {
            const [subs, pkgs] = await Promise.all([
                AppService.getUserSubscriptions(appId, userId),
                AppService.getPackages(appId)
            ]);
            setSubscriptions(subs);
            setPackages(pkgs);
        } catch (error) {
            toast.error('Failed to load subscriptions');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, [appId, userId]);

    const handleGrant = async () => {
        if (!selectedPackage) return;
        try {
            await AppService.grantSubscription(appId, userId, selectedPackage, customStartDate || undefined, customEndDate || undefined);
            toast.success('Subscription granted');
            setGrantDialogOpen(false);
            loadData();
        } catch (error) {
            toast.error('Failed to grant subscription');
        }
    };

    const handleStatusChange = async (sub: Subscription) => {
        try {
            await AppService.changeSubscriptionStatus(appId, sub.id, !sub.isActive);
            toast.success(`Subscription ${sub.isActive ? 'cancelled' : 'activated'}`);
            loadData();
        } catch (error) {
            toast.error('Failed to update status');
        }
    };

    if (loading) return <Loader2 className="h-8 w-8 animate-spin" />;

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <div>
                    <CardTitle>User Subscriptions</CardTitle>
                    <CardDescription>Manage user access levels</CardDescription>
                </div>
                <Dialog open={grantDialogOpen} onOpenChange={setGrantDialogOpen}>
                    <DialogTrigger asChild>
                        <Button disabled={!canEdit}><Plus className="mr-2 h-4 w-4" /> Grant Subscription</Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Grant Subscription</DialogTitle>
                            <DialogDescription>Select a package and optional custom dates.</DialogDescription>
                        </DialogHeader>
                        <div className="grid gap-4 py-4">
                            <div className="grid gap-2">
                                <Label>Package</Label>
                                <Select value={selectedPackage} onValueChange={setSelectedPackage}>
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select package" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {packages.map(p => (
                                            <SelectItem key={p.id} value={p.id}>{p.name} - {p.currency} {p.price}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div className="grid gap-2">
                                    <Label>Start Date (Optional)</Label>
                                    <Input type="date" value={customStartDate} onChange={e => setCustomStartDate(e.target.value)} />
                                </div>
                                <div className="grid gap-2">
                                    <Label>End Date (Optional)</Label>
                                    <Input type="date" value={customEndDate} onChange={e => setCustomEndDate(e.target.value)} />
                                </div>
                            </div>
                        </div>
                        <DialogFooter>
                            <Button onClick={handleGrant}>Grant</Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Package</TableHead>
                            <TableHead>Start</TableHead>
                            <TableHead>End</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {subscriptions.map((sub) => (
                            <TableRow key={sub.id}>
                                <TableCell className="font-medium">{sub.packageName}</TableCell>
                                <TableCell>{new Date(sub.startDate).toLocaleDateString()}</TableCell>
                                <TableCell>{new Date(sub.endDate).toLocaleDateString()}</TableCell>
                                <TableCell>
                                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${sub.isActive ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800"}`}>
                                        {sub.isActive ? "Active" : "Inactive"}
                                    </span>
                                </TableCell>
                                <TableCell>
                                    <Button variant="ghost" size="icon" onClick={() => handleStatusChange(sub)} disabled={!canEdit}>
                                        {sub.isActive ? <Ban className="h-4 w-4 text-red-500" /> : <CheckCircle className="h-4 w-4 text-green-500" />}
                                    </Button>
                                </TableCell>
                            </TableRow>
                        ))}
                        {subscriptions.length === 0 && <TableRow><TableCell colSpan={5} className="text-center text-muted-foreground">No subscriptions found</TableCell></TableRow>}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}

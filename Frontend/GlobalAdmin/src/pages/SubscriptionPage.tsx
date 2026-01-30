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
import { Plus, CheckCircle, XCircle } from 'lucide-react';
import { SubscriptionService, SubscriptionPlan } from '../services/subscriptionService';
import { useToast } from '../context/ToastContext';
import { cn } from '../lib/utils';

const SubscriptionPage = () => {
    const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();

    useEffect(() => {
        loadPlans();
    }, []);

    const loadPlans = async () => {
        try {
            const data = await SubscriptionService.getPlans();
            setPlans(data);
        } catch (error) {
            showToast("Failed to load plans", "error");
        } finally {
            setLoading(false);
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
                    <h2 className="text-3xl font-bold tracking-tight">Subscription Plans</h2>
                    <p className="text-muted-foreground">Manage tenant subscription tiers and pricing.</p>
                </div>
                <Button className="gap-2">
                    <Plus size={16} />
                    Create Plan
                </Button>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Available Plans</CardTitle>
                    <CardDescription>
                        List of all subscription plans available to tenants.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="w-[300px]">Plan Name</TableHead>
                                <TableHead>Price</TableHead>
                                <TableHead>Active Tenants</TableHead>
                                <TableHead>Status</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {plans.map((p) => (
                                <TableRow key={p.id}>
                                    <TableCell>
                                        <div>
                                            <div className="font-semibold text-foreground">{p.name}</div>
                                            <div className="text-xs text-muted-foreground">{p.description}</div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <span className="font-mono text-foreground font-medium">
                                            {p.price === 0 ? 'Free' : `$${p.price.toFixed(2)} / ${p.interval}`}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <span className="inline-flex items-center rounded-md border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80">
                                            {p.tenantCount} active
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <span className={cn(
                                            "inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium",
                                            p.isActive
                                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                                : "bg-muted text-muted-foreground"
                                        )}>
                                            {p.isActive ? <CheckCircle size={12} /> : <XCircle size={12} />}
                                            {p.isActive ? 'Active' : 'Archived'}
                                        </span>
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

export default SubscriptionPage;

import React, { useEffect, useState } from 'react';
import { Package, AlertCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import api from '../services/api';

interface PlanDto {
    id: string;
    name: string;
    amount: number;
    currency: string;
    interval: string;
    providerPlanId: string;
}

export default function PackagesPage() {
    const [plans, setPlans] = useState<PlanDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchPlans = async () => {
            try {
                // Fetch from Gateway routing to Payments.API
                const response = await api.get('/payments/api/Payments/plans');
                if (Array.isArray(response.data)) {
                    setPlans(response.data);
                } else if (response.data?.items) {
                    setPlans(response.data.items);
                } else {
                    setPlans([]);
                }
            } catch (err: any) {
                console.error("Failed to load packages", err);
                setError(err.message || 'Failed to load packages');
            } finally {
                setLoading(false);
            }
        };

        fetchPlans();
    }, []);

    if (loading) {
        return <div className="p-8 text-center text-muted-foreground animate-pulse">Loading packages...</div>;
    }

    if (error) {
        return (
            <div className="p-8 space-y-8 animate-fade-in flex flex-col items-center justify-center h-[80vh]">
                <div className="bg-destructive/10 p-8 rounded-lg text-center max-w-md border border-destructive">
                    <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
                    <h2 className="text-2xl font-bold mb-2 text-destructive">Error Loading Packages</h2>
                    <p className="text-muted-foreground mb-6">
                        {error}
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between space-y-2">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Subscription Packages</h2>
                    <p className="text-muted-foreground">
                        View active subscription plans available in the system.
                    </p>
                </div>
            </div>

            {plans.length === 0 && !loading ? (
                <div className="p-8 text-center bg-card rounded-lg border border-border">
                    <p className="text-muted-foreground">No active packages found.</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {plans.map(plan => (
                        <Card key={plan.id} className="relative overflow-hidden flex flex-col">
                            <CardHeader className="pb-4">
                                <div className="flex justify-between items-start">
                                    <div className="bg-primary/10 p-2 rounded-lg">
                                        <Package className="h-6 w-6 text-primary" />
                                    </div>
                                    <span className="inline-flex items-center rounded-full bg-emerald-100 px-2.5 py-0.5 text-xs font-semibold text-emerald-800 tracking-wide uppercase">
                                        Active
                                    </span>
                                </div>
                                <CardTitle className="text-xl mt-4">{plan.name}</CardTitle>
                                <CardDescription className="text-sm font-mono text-muted-foreground mt-1">
                                    {plan.providerPlanId || "No Provider ID"}
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="flex-1 flex flex-col justify-end">
                                <div className="mt-2 flex items-baseline gap-1">
                                    <span className="text-3xl font-bold">{(Math.round(plan.amount * 100) / 100).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                                    <span className="text-sm font-medium text-muted-foreground uppercase">{plan.currency || 'USD'}</span>
                                    <span className="text-sm font-medium text-muted-foreground ml-1">/{plan.interval || 'month'}</span>
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}
        </div>
    );
}

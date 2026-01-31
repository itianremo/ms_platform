import React, { useEffect, useState } from 'react';
import { AppConfig, AppService, SubscriptionPackage } from '../../services/appService';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../ui/table";
import { Loader2 } from 'lucide-react';

interface AppPackagesTabProps {
    app: AppConfig;
}

const AppPackagesTab: React.FC<AppPackagesTabProps> = ({ app }) => {
    const [packages, setPackages] = useState<SubscriptionPackage[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchPackages = async () => {
            setLoading(true);
            try {
                const data = await AppService.getPackages(app.id);
                setPackages(data);
                setError(null);
            } catch (err) {
                setError("Failed to load packages.");
            } finally {
                setLoading(false);
            }
        };

        if (app.id) {
            fetchPackages();
        }
    }, [app.id]);

    if (loading) {
        return <div className="flex justify-center py-8"><Loader2 className="animate-spin h-6 w-6 text-primary" /></div>;
    }

    if (error) {
        return <div className="text-red-500 py-4 text-center">{error}</div>;
    }

    if (packages.length === 0) {
        return <div className="text-muted-foreground py-8 text-center">No subscription packages found for this application.</div>;
    }

    return (
        <div className="space-y-4">
            <h3 className="text-lg font-medium">Subscription Packages</h3>
            <div className="border rounded-md">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name</TableHead>
                            <TableHead>Price</TableHead>
                            <TableHead>Discount</TableHead>
                            <TableHead>Final</TableHead>
                            <TableHead>Period</TableHead>
                            <TableHead>Description</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {packages.map((pkg) => {
                            const finalPrice = pkg.price - (pkg.discount || 0);
                            return (
                                <TableRow key={pkg.id}>
                                    <TableCell className="font-medium">{pkg.name}</TableCell>
                                    <TableCell>{pkg.currency} {pkg.price.toFixed(2)}</TableCell>
                                    <TableCell className="text-green-600">
                                        {pkg.discount > 0 ? `-${pkg.currency} ${pkg.discount.toFixed(2)}` : '-'}
                                    </TableCell>
                                    <TableCell className="font-bold">{pkg.currency} {finalPrice.toFixed(2)}</TableCell>
                                    <TableCell>{pkg.period === 0 ? 'Monthly' : (pkg.period === 99 ? 'Lifetime' : `${pkg.period} Months`)}</TableCell>
                                    <TableCell className="text-muted-foreground whitespace-pre-wrap">{pkg.description}</TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
};

export default AppPackagesTab;

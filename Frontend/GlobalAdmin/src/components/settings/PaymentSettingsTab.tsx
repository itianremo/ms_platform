import { useEffect, useState } from 'react';
import { useToast } from '../../context/ToastContext';
import { SettingsService } from '../../services/settingsService';
import { AppService } from '../../services/appService';
import { CreditCard, Globe, Landmark, Save, ShoppingBag, Smartphone, Check, Terminal, Shield } from 'lucide-react';
import { Button } from "../ui/button";
import { Switch } from "../ui/switch";

type PaymentProvider = 'stripe' | 'paypal' | 'bank' | 'amazon' | 'google' | 'apple';

interface PaymentConfigDef {
    id: PaymentProvider;
    name: string;
    icon: any;
    description: string;
    fields: { key: string; label: string; type: string; value: string; placeholder?: string; colSpan?: number }[];
}

const PROVIDERS: PaymentConfigDef[] = [
    {
        id: 'google',
        name: 'Google Pay',
        icon: Smartphone,
        description: 'Simple and secure checkout with Google Pay.',
        fields: [
            { key: 'merchantId', label: 'Merchant ID', type: 'text', value: '', colSpan: 2 },
            { key: 'merchantName', label: 'Merchant Name', type: 'text', value: '', colSpan: 2 },
            { key: 'environment', label: 'Environment (TEST/PRODUCTION)', type: 'switch', value: 'TEST', colSpan: 2 }
        ]
    },
    {
        id: 'stripe',
        name: 'Stripe',
        icon: CreditCard,
        description: 'Accept credit cards and other payment methods.',
        fields: [
            { key: 'publishableKey', label: 'Publishable Key', type: 'text', value: '', placeholder: 'pk_test_...', colSpan: 2 },
            { key: 'secretKey', label: 'Secret Key', type: 'password', value: '', placeholder: 'sk_test_...', colSpan: 2 },
            { key: 'webhookSecret', label: 'Webhook Secret', type: 'password', value: '', placeholder: 'whsec_...', colSpan: 2 }
        ]
    },
    {
        id: 'paypal',
        name: 'PayPal',
        icon: Globe,
        description: 'Connect with PayPal for global payments.',
        fields: [
            { key: 'clientId', label: 'Client ID', type: 'text', value: '', colSpan: 2 },
            { key: 'clientSecret', label: 'Client Secret', type: 'password', value: '', colSpan: 2 },
            { key: 'mode', label: 'Sandbox Mode', type: 'switch', value: 'sandbox', colSpan: 2 }
        ]
    },
    {
        id: 'apple',
        name: 'Apple Pay',
        icon: Smartphone,
        description: 'Easy and secure payments with Apple Pay.',
        fields: [
            { key: 'merchantIdentifier', label: 'Merchant Identifier', type: 'text', value: '', colSpan: 2 },
            { key: 'displayName', label: 'Display Name', type: 'text', value: '', colSpan: 2 },
            { key: 'certificatePath', label: 'Certificate Path', type: 'text', value: '', colSpan: 2 },
            { key: 'domainVerification', label: 'Domain Verification File', type: 'file', value: '', colSpan: 2 }
        ]
    },
    {
        id: 'amazon',
        name: 'Amazon Pay',
        icon: ShoppingBag,
        description: 'Pay with Amazon account.',
        fields: [
            { key: 'merchantId', label: 'Merchant ID', type: 'text', value: '', colSpan: 1 },
            { key: 'clientId', label: 'Client ID', type: 'text', value: '', colSpan: 1 },
            { key: 'accessKey', label: 'Access Key', type: 'text', value: '', colSpan: 2 },
            { key: 'secretKey', label: 'Secret Key', type: 'password', value: '', colSpan: 2 },
            { key: 'region', label: 'Region', type: 'text', value: '', colSpan: 1 },
            { key: 'isSandbox', label: 'Sandbox Mode', type: 'switch', value: 'true', colSpan: 1 }
        ]
    },
    {
        id: 'bank',
        name: 'Bank Transfer',
        icon: Landmark,
        description: 'Manual bank transfer instructions.',
        fields: [
            { key: 'bankName', label: 'Bank Name', type: 'bank-select', value: '' },
            { key: 'accountName', label: 'Account Name', type: 'text', value: '' },
            { key: 'accountNumber', label: 'Account Number', type: 'text', value: '' },
            { key: 'swiftCode', label: 'SWIFT/BIC', type: 'text', value: '' }
        ]
    }
];

const PaymentSettingsTab = () => {
    const { showToast } = useToast();
    const [activeProviderId, setActiveProviderId] = useState<PaymentProvider>('stripe'); // UI selection, not activation status
    const [configs, setConfigs] = useState<Record<string, any>>({});
    const [actives, setActives] = useState<Record<string, boolean>>({}); // Activation status map
    const [loading, setLoading] = useState(false);

    const [selectedAppId, setSelectedAppId] = useState<string>('');

    useEffect(() => {
        loadSystemApp();
    }, []);

    const loadSystemApp = async () => {
        try {
            const data = await AppService.getAllApps();
            const systemApp = data.find(a => a.name === 'Global Dashboard' || a.name === 'System' || a.name === 'Admin Panel');
            if (systemApp) {
                setSelectedAppId(systemApp.id);
                loadConfigs(systemApp.id);
            } else if (data.length > 0) {
                setSelectedAppId(data[0].id);
                loadConfigs(data[0].id);
            }
        } catch (error) {
            console.error("Failed to load apps", error);
        }
    };

    const loadConfigs = async (appId: string) => {
        if (!appId) return;
        try {
            const data = await SettingsService.getPaymentConfigs(appId);
            const finalActives: Record<string, boolean> = {};
            const finalConfigs: Record<string, any> = {};

            PROVIDERS.forEach(p => {
                finalActives[p.id] = false;
                finalConfigs[p.id] = {};
            });

            if (data && Array.isArray(data)) {
                data.forEach((p: any) => {
                    let mappedId = p.providerId.toLowerCase();
                    if (mappedId.includes('amazon')) mappedId = 'amazon';
                    else if (mappedId.includes('google')) mappedId = 'google';
                    else if (mappedId.includes('apple')) mappedId = 'apple';
                    else if (mappedId.includes('bank')) mappedId = 'bank';

                    const def = PROVIDERS.find(d => d.id === mappedId);
                    if (def) {
                        finalActives[def.id] = p.isActive;
                        finalConfigs[def.id] = p.configs || {};
                    }
                });
            }
            setActives(finalActives);
            setConfigs(finalConfigs);
        } catch (error) {
            console.error("Error loading payment configs", error);
        }
    };

    const handleInputChange = (providerId: string, fieldKey: string, value: string) => {
        setConfigs(prev => ({
            ...prev,
            [providerId]: {
                ...prev[providerId],
                [fieldKey]: value
            }
        }));
    };

    const toggleActive = (e: React.MouseEvent, providerId: string) => {
        e.stopPropagation(); // Prevent card selection when clicking toggle
        setActives(prev => ({
            ...prev,
            [providerId]: !prev[providerId]
        }));
    };

    const handleSave = async () => {
        if (!selectedAppId) return;
        setLoading(true);
        try {
            await SettingsService.savePaymentConfigs({ actives, configs }, selectedAppId);
            showToast("Payment configurations saved successfully!", "success");
        } catch (error) {
            showToast("Failed to save payment configurations.", "error");
        } finally {
            setLoading(false);
        }
    };

    if (!selectedAppId) return <div>Loading System App...</div>;

    const currentProvider = PROVIDERS.find(p => p.id === activeProviderId);

    return (
        <div className="space-y-6 animate-fade-in py-2">
            <div className="flex items-center justify-between">
                <div>
                    <h3 className="text-lg font-medium">Payment Gateways</h3>
                    <p className="text-sm text-muted-foreground">Configure global payment providers and gateway settings.</p>
                </div>
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    <Save size={16} />
                    {loading ? 'Saving...' : 'Save Changes'}
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 items-start">

                {/* Left Column: Provider List */}
                <div className="md:col-span-1 space-y-3">
                    <h4 className="text-sm font-semibold text-muted-foreground mb-2 px-1">Providers</h4>
                    {PROVIDERS.map(provider => {
                        const isActive = actives[provider.id];
                        return (
                            <div
                                key={provider.id}
                                onClick={() => setActiveProviderId(provider.id)}
                                className={`cursor-pointer p-3 rounded-xl border transition-all duration-200 ${activeProviderId === provider.id
                                        ? 'border-primary shadow-sm bg-primary/5 ring-1 ring-primary/20'
                                        : 'border-border hover:bg-accent/50 hover:border-accent-foreground/20'
                                    }`}
                            >
                                <div className="flex items-center gap-3">
                                    <div className={`p-2.5 rounded-lg ${activeProviderId === provider.id ? 'bg-primary text-white' : 'bg-muted text-muted-foreground'}`}>
                                        <provider.icon size={18} />
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center justify-between">
                                            <h3 className="font-medium text-sm">{provider.name}</h3>
                                            <Switch
                                                checked={isActive}
                                                onCheckedChange={(c) => {
                                                    setActives(prev => ({ ...prev, [provider.id]: c }));
                                                }}
                                                // We need to capture click to prevent parent onClick if using native elements, but Radix Switch handles events well usually.
                                                // However, simpler to just use onCheckedChange here.
                                                className="scale-75"
                                            />
                                        </div>
                                        <p className="text-xs text-muted-foreground line-clamp-1 mt-0.5">{provider.description}</p>
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>

                {/* Right Column: Configuration Form */}
                <div className="md:col-span-3 space-y-6">
                    {currentProvider && (
                        <div className="glass-panel p-6 rounded-xl border border-border">
                            <div className="flex items-center gap-2 mb-6 border-b border-border pb-4">
                                <Shield size={18} className="text-muted-foreground" />
                                <h3 className="font-semibold text-base">Configuration: {currentProvider.name}</h3>
                                {actives[currentProvider.id] ? (
                                    <span className="ml-auto text-xs px-2 py-0.5 rounded-full bg-green-500/10 text-green-500 font-medium border border-green-500/20">Active</span>
                                ) : (
                                    <span className="ml-auto text-xs px-2 py-0.5 rounded-full bg-gray-500/10 text-gray-500 font-medium border border-gray-500/20">Inactive</span>
                                )}
                            </div>

                            <div className={`grid grid-cols-1 md:grid-cols-2 gap-6 ${!actives[currentProvider.id] ? 'opacity-70' : ''}`}>
                                {currentProvider.fields.map(field => {
                                    const val = configs[currentProvider.id]?.[field.key] || '';
                                    const colSpanInfo = field.colSpan ? `col-span-${field.colSpan}` : 'col-span-2';

                                    if (field.type === 'switch') {
                                        let isChecked = false;
                                        if (field.key === 'isSandbox' || field.key === 'mode') {
                                            isChecked = String(val).toLowerCase() === 'true' || String(val).toLowerCase() === 'sandbox';
                                        } else if (field.key === 'environment') {
                                            isChecked = String(val).toUpperCase() === 'TEST';
                                        }

                                        const handleSwitchChange = (checked: boolean) => {
                                            let newValue: any = checked;
                                            if (currentProvider.id === 'paypal' && field.key === 'mode') newValue = checked ? 'sandbox' : 'live';
                                            else if (currentProvider.id === 'google' && field.key === 'environment') newValue = checked ? 'TEST' : 'PRODUCTION';
                                            else if (currentProvider.id === 'amazon' && field.key === 'isSandbox') newValue = checked ? 'true' : 'false';
                                            handleInputChange(currentProvider.id, field.key, newValue);
                                        };

                                        return (
                                            <div key={field.key} className={`${colSpanInfo} flex items-center justify-between p-3 rounded-lg border border-border bg-card/50`}>
                                                <div className="flex flex-col">
                                                    <label className="text-sm font-medium text-foreground">
                                                        {field.label}
                                                    </label>
                                                    <span className="text-xs text-muted-foreground">
                                                        {isChecked ? (field.key === 'environment' ? 'Test Environment' : 'Sandbox Mode') : (field.key === 'environment' ? 'Production Environment' : 'Live Mode')}
                                                    </span>
                                                </div>
                                                <Switch
                                                    checked={isChecked}
                                                    onCheckedChange={handleSwitchChange}
                                                    disabled={!actives[currentProvider.id]}
                                                />
                                            </div>
                                        );
                                    }

                                    return (
                                        <div key={field.key} className={colSpanInfo}>
                                            <label className="text-sm font-medium mb-1.5 block text-foreground">{field.label}</label>
                                            <input
                                                type={field.type === 'file' ? 'text' : field.type} // Simple file handling for now (text path)
                                                className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                                placeholder={field.placeholder || ''}
                                                value={val}
                                                onChange={(e) => handleInputChange(currentProvider.id, field.key, e.target.value)}
                                                disabled={!actives[currentProvider.id]}
                                            />
                                            {field.type === 'file' && <p className="text-xs text-muted-foreground mt-1">File upload not fully supported in demo. Enter path.</p>}
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default PaymentSettingsTab;

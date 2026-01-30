import { useEffect, useState } from 'react';
import { useToast } from '../context/ToastContext';
import { SettingsService, PaymentConfig } from '../services/settingsService';
import { AppService, AppConfig } from '../services/appService';
import { CreditCard, Globe, Landmark, Save, ShoppingBag, Smartphone, Upload, Check } from 'lucide-react';
import { Button } from "../components/ui/button";
import { Switch } from "../components/ui/switch";

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

const PaymentConfigPage = () => {
    const { showToast } = useToast();
    const [configs, setConfigs] = useState<Record<string, any>>({});
    // Initialize all as active by default to ensure keys exist for saving
    const [actives, setActives] = useState<Record<string, boolean>>(() => {
        const defaults: Record<string, boolean> = {};
        PROVIDERS.forEach(p => defaults[p.id] = false);
        return defaults;
    });
    const [loading, setLoading] = useState(false);

    // Apps State
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [selectedAppId, setSelectedAppId] = useState<string>(''); // Will default after fetch

    // Bank State
    const [bankList, setBankList] = useState<any[]>([]);
    const [bankSearch, setBankSearch] = useState('');
    const [isBankDropdownOpen, setIsBankDropdownOpen] = useState(false);

    useEffect(() => {
        loadApps();
        loadBanks();
    }, []);

    // Load Configs whenever App ID changes
    useEffect(() => {
        if (selectedAppId) {
            loadConfigs();
        }
    }, [selectedAppId]);

    const loadApps = async () => {
        try {
            const data = await AppService.getAllApps();
            setApps(data);
            if (data.length > 0) {
                // Default to Global Dashboard or first app
                const defaultApp = data.find(a => a.name === 'Global Dashboard') || data.find(a => a.name.includes('Global')) || data[0];
                setSelectedAppId(defaultApp.name);
            }
        } catch (error) {
            console.error("Failed to load apps", error);
            showToast("Failed to load apps.", "error");
        }
    };

    const loadBanks = async () => {
        const banks = await SettingsService.getBanks();
        setBankList(banks);
    };

    const loadConfigs = async () => {
        if (!selectedAppId) return;
        try {
            const data = await SettingsService.getPaymentConfigs(selectedAppId);

            // 1. Initialize Clean Defaults
            const finalActives: Record<string, boolean> = {};
            const finalConfigs: Record<string, any> = {};

            PROVIDERS.forEach(p => {
                finalActives[p.id] = false; // Default inactive
                finalConfigs[p.id] = {};    // Default empty configs
            });

            // 2. Map Loaded Data
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

            // 3. Replace State (No merging with prev)
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

    const toggleActive = (providerId: string) => {
        setActives(prev => ({
            ...prev,
            [providerId]: !prev[providerId]
        }));
    };

    const handleSave = async () => {
        if (!selectedAppId) {
            showToast("No App Selected.", "error");
            return;
        }
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

    return (
        <div className="space-y-8 animate-fade-in p-2">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold mb-2">Payment Gateways</h1>
                    <p className="text-muted-foreground">Configure how you accept payments from customers.</p>
                </div>
                <div className="flex items-center gap-4">
                    {/* App Selector */}
                    <div className="w-56">
                        <select
                            className="w-full p-2 rounded-md border border-gray-200 dark:border-gray-700 bg-white dark:bg-slate-900 text-sm"
                            value={selectedAppId}
                            onChange={(e) => setSelectedAppId(e.target.value)}
                        >
                            {apps.map(app => (
                                <option key={app.id} value={app.name}>{app.name}</option>
                            ))}
                        </select>
                    </div>

                    <Button onClick={handleSave} disabled={loading} className="gap-2">
                        <Save size={16} />
                        {loading ? 'Saving...' : 'Save Changes'}
                    </Button>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {PROVIDERS.map(provider => {
                    const isActive = actives[provider.id];
                    return (
                        <div
                            key={provider.id}
                            className={`glass-panel p-6 rounded-xl border transition-all duration-300 ${isActive
                                ? 'border-primary/40 shadow-lg ring-1 ring-primary/10'
                                : 'border-border opacity-90'
                                }`}
                        >
                            {/* Header */}
                            <div className="flex items-start justify-between mb-6">
                                <div className="flex items-center gap-3">
                                    <div className={`p-3 rounded-lg ${isActive ? 'bg-primary/20 text-primary' : 'bg-gray-100 dark:bg-slate-800 text-gray-500'}`}>
                                        <provider.icon size={24} />
                                    </div>
                                    <div>
                                        <h3 className="font-semibold text-lg">{provider.name}</h3>
                                        <p className="text-xs text-gray-500 dark:text-gray-400 line-clamp-1">{provider.description}</p>
                                    </div>
                                </div>
                                <label className="relative inline-flex items-center cursor-pointer">
                                    <input
                                        type="checkbox"
                                        className="sr-only peer"
                                        checked={isActive}
                                        onChange={() => toggleActive(provider.id)}
                                    />
                                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary/30 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary"></div>
                                </label>
                            </div>

                            {/* Fields */}
                            <div className={`grid grid-cols-2 gap-4 transition-all duration-300 ${!isActive ? 'opacity-50 grayscale-[0.5] pointer-events-none' : ''}`}>
                                {provider.fields.map(field => {
                                    const val = configs[provider.id]?.[field.key] || '';
                                    const colSpanInfo = field.colSpan ? `col-span-${field.colSpan}` : 'col-span-2';

                                    if (field.type === 'switch') {
                                        // Handle Boolean Toggles (Small Switch)
                                        // Specific logic for PayPal Mode and Google Env
                                        let isChecked = false;
                                        if (field.key === 'isSandbox' || field.key === 'mode') {
                                            // Handle various boolean representations (string/bool)
                                            isChecked = String(val).toLowerCase() === 'true' || String(val).toLowerCase() === 'sandbox';
                                        } else if (field.key === 'environment') {
                                            isChecked = String(val).toUpperCase() === 'TEST';
                                        }

                                        const handleSwitchChange = (checked: boolean) => {
                                            let newValue: any = checked;
                                            if (provider.id === 'paypal' && field.key === 'mode') {
                                                newValue = checked ? 'sandbox' : 'live';
                                            } else if (provider.id === 'google' && field.key === 'environment') {
                                                newValue = checked ? 'TEST' : 'PRODUCTION';
                                            }
                                            // Handle Amazon boolean explicitly
                                            else if (provider.id === 'amazon' && field.key === 'isSandbox') {
                                                newValue = checked ? 'true' : 'false';
                                            }

                                            handleInputChange(provider.id, field.key, newValue);
                                        };

                                        return (
                                            <div key={field.key} className={`${colSpanInfo} flex items-center justify-between p-2 rounded-lg border border-gray-100 dark:border-slate-800 bg-gray-50/50 dark:bg-slate-900/50`}>
                                                <div className="flex flex-col">
                                                    <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                                                        {field.label}
                                                    </label>
                                                    <span className="text-xs text-muted-foreground">
                                                        {isChecked ? (field.key === 'environment' ? 'Test Environment' : 'Sandbox Mode') : (field.key === 'environment' ? 'Production Environment' : 'Live Mode')}
                                                    </span>
                                                </div>
                                                <Switch
                                                    checked={isChecked}
                                                    onCheckedChange={handleSwitchChange}
                                                    className="scale-75 origin-right" // Make it smaller
                                                    disabled={!isActive}
                                                />
                                            </div>
                                        );
                                    }

                                    if (field.type === 'file') {
                                        return (
                                            <div key={field.key} className={colSpanInfo}>
                                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">
                                                    {field.label}
                                                </label>
                                                <div className="flex gap-2">
                                                    <Button
                                                        variant="outline"
                                                        disabled={!isActive}
                                                        className="w-full justify-start text-muted-foreground font-normal"
                                                        onClick={() => document.getElementById(`file-${provider.id}-${field.key}`)?.click()}
                                                    >
                                                        <Upload className="mr-2 h-4 w-4" />
                                                        {val ? 'Verification File Uploaded' : 'Upload Verification File'}
                                                    </Button>
                                                    <input
                                                        type="file"
                                                        id={`file-${provider.id}-${field.key}`}
                                                        className="hidden"
                                                        accept=".txt"
                                                        onChange={(e) => {
                                                            const file = e.target.files?.[0];
                                                            if (file) {
                                                                const reader = new FileReader();
                                                                reader.onload = (ev) => {
                                                                    handleInputChange(provider.id, field.key, ev.target?.result as string);
                                                                };
                                                                reader.readAsText(file);
                                                            }
                                                        }}
                                                    />
                                                    {val && <div className="flex items-center text-green-500"><Check size={20} /></div>}
                                                </div>
                                                {val && <p className="text-xs text-muted-foreground mt-1 truncate">Content length: {val.length} chars</p>}
                                            </div>
                                        );
                                    }

                                    if (field.type === 'bank-select') {
                                        const currentVal = configs[provider.id]?.[field.key] || '';

                                        // Filter banks
                                        const filteredBanks = bankList.filter(b =>
                                            b.name.toLowerCase().includes(bankSearch.toLowerCase()) ||
                                            (b.swiftCode && b.swiftCode.toLowerCase().includes(bankSearch.toLowerCase()))
                                        );

                                        return (
                                            <div key={field.key} className={`${colSpanInfo} relative`}>
                                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">
                                                    {field.label}
                                                </label>

                                                {/* Input acts as search/display */}
                                                <div className="relative">
                                                    <input
                                                        type="text"
                                                        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50 transition-shadow disabled:bg-gray-50 dark:disabled:bg-slate-800"
                                                        placeholder="Search bank..."
                                                        value={isBankDropdownOpen ? bankSearch : currentVal}
                                                        onFocus={() => {
                                                            setIsBankDropdownOpen(true);
                                                            setBankSearch(currentVal); // Seed search with current val
                                                        }}
                                                        onChange={(e) => setBankSearch(e.target.value)}
                                                        disabled={!isActive}
                                                    />
                                                    {isBankDropdownOpen && (
                                                        <div className="absolute z-10 w-full mt-1 max-h-60 overflow-auto bg-white dark:bg-slate-900 border border-gray-200 dark:border-slate-700 rounded-md shadow-lg">
                                                            {filteredBanks.length === 0 ? (
                                                                <div className="p-2 text-sm text-gray-500">No banks found.</div>
                                                            ) : (
                                                                filteredBanks.map(bank => (
                                                                    <button
                                                                        key={bank.id}
                                                                        className="w-full text-left px-3 py-2 text-sm hover:bg-gray-100 dark:hover:bg-slate-800 focus:bg-gray-100 dark:focus:bg-slate-800 outline-none"
                                                                        onClick={() => {
                                                                            handleInputChange(provider.id, field.key, bank.name);
                                                                            // Also helper to auto-fill swift if possible
                                                                            if (bank.swiftCode) {
                                                                                handleInputChange(provider.id, 'swiftCode', bank.swiftCode);
                                                                            }
                                                                            setIsBankDropdownOpen(false);
                                                                            setIsBankDropdownOpen(false);
                                                                            setBankSearch('');
                                                                        }}
                                                                    >
                                                                        <div className="font-medium">{bank.name}</div>
                                                                        {bank.swiftCode && <div className="text-xs text-gray-500">SWIFT: {bank.swiftCode}</div>}
                                                                    </button>
                                                                ))
                                                            )}
                                                            {/* Backdrop to close */}
                                                            <div
                                                                className="fixed inset-0 z-[-1]"
                                                                onClick={() => {
                                                                    setIsBankDropdownOpen(false);
                                                                    setBankSearch('');
                                                                }}
                                                            ></div>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        );
                                    }

                                    return (
                                        <div key={field.key} className={colSpanInfo}>
                                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">
                                                {field.label}
                                            </label>
                                            <input
                                                type={field.type}
                                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50 transition-shadow disabled:bg-gray-50 dark:disabled:bg-slate-800"
                                                placeholder={field.placeholder || ''}
                                                value={configs[provider.id]?.[field.key] || ''}
                                                onChange={(e) => handleInputChange(provider.id, field.key, e.target.value)}
                                                disabled={!isActive}
                                            />
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    );
                })}
            </div>


        </div >
    );
};

export default PaymentConfigPage;

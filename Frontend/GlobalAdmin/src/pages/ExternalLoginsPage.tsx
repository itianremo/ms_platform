import { useEffect, useState } from 'react';
import { AppService, AppConfig } from '../services/appService';
import { useToast } from '../context/ToastContext';
import { Button } from "../components/ui/button";
import { Loader2, Save, Globe, Lock } from 'lucide-react';
import { cn } from '../lib/utils';

interface AuthProviderConfig {
    clientId: string;
    clientSecret: string;
    enabled: boolean;
}

interface ExternalAuthConfigs {
    google?: AuthProviderConfig;
    facebook?: AuthProviderConfig;
    apple?: AuthProviderConfig;
    [key: string]: AuthProviderConfig | undefined;
}

const ExternalLoginsPage = () => {
    const { showToast } = useToast();
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [selectedAppId, setSelectedAppId] = useState<string>('');
    const [config, setConfig] = useState<ExternalAuthConfigs>({});
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        fetchApps();
    }, []);

    useEffect(() => {
        if (selectedAppId) {
            loadAppConfig(selectedAppId);
        } else {
            setConfig({});
        }
    }, [selectedAppId]);

    const fetchApps = async () => {
        try {
            const data = await AppService.getAllApps();
            setApps(data);
            if (data.length > 0) {
                setSelectedAppId(data[0].id);
            }
        } catch (error) {
            showToast("Failed to fetch applications", "error");
        } finally {
            setLoading(false);
        }
    };

    const loadAppConfig = async (appId: string) => {
        // Since we don't have a direct "GetConfig" endpoint, we rely on the App object having it.
        // BUT, GetAllApps might not return the heavy JSON?
        // Let's assume GetAllApps returns it or we fetch details.
        // Actually, AppService.getAllApps returns AppConfig[] which maps to backend DTO.
        // We might need to ensure the backend DTO includes ExternalAuthProvidersJson.
        // Let's check GetAllAppsQueryHandler. If it maps Domain -> DTO, we need to check the DTO.
        // Assuming it does for now. If empty, we start blank.

        const app = apps.find(a => a.id === appId);
        if (app && (app as any).externalAuthProvidersJson) {
            try {
                const parsed = JSON.parse((app as any).externalAuthProvidersJson);
                setConfig(parsed);
            } catch {
                setConfig({});
            }
        } else {
            // Need to fetch individual app details if not in list?
            // Or assume empty.
            setConfig({});
        }
    };

    const handleConfigChange = (provider: string, field: keyof AuthProviderConfig, value: any) => {
        setConfig(prev => ({
            ...prev,
            [provider]: {
                ...prev[provider] || { clientId: '', clientSecret: '', enabled: false },
                [field]: value
            }
        }));
    };

    const handleSave = async () => {
        if (!selectedAppId) return;
        setSaving(true);
        try {
            const json = JSON.stringify(config);
            await AppService.updateExternalAuth(selectedAppId, json);
            showToast("External Login settings saved successfully", "success");

            // Update local state
            setApps(prev => prev.map(a => a.id === selectedAppId ? { ...a, externalAuthProvidersJson: json } : a) as AppConfig[]);
        } catch (error) {
            showToast("Failed to save settings", "error");
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div className="p-8">Loading apps...</div>;

    return (
        <div className="space-y-8 animate-fade-in p-2">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold mb-2">External Logins</h1>
                    <p className="text-muted-foreground">Configure Social Login providers (Google, Facebook, Apple) per application.</p>
                </div>
                <div className="flex gap-2">
                    <Button onClick={handleSave} disabled={saving || !selectedAppId} className="gap-2">
                        <Save size={16} />
                        {saving ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        {saving ? 'Saving...' : 'Save Changes'}
                    </Button>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
                {/* App Selector Sidebar */}
                <div className="glass-panel p-4 rounded-xl border border-border lg:col-span-1 h-fit">
                    <h3 className="font-semibold mb-4 px-2">Select Application</h3>
                    <div className="space-y-1">
                        {apps.map(app => (
                            <button
                                key={app.id}
                                onClick={() => setSelectedAppId(app.id)}
                                className={cn(
                                    "w-full text-left px-3 py-2 rounded-md text-sm transition-colors",
                                    selectedAppId === app.id
                                        ? "bg-primary text-primary-foreground font-medium"
                                        : "hover:bg-accent text-muted-foreground hover:text-foreground"
                                )}
                            >
                                {app.name}
                            </button>
                        ))}
                    </div>
                </div>

                {/* Configuration Form */}
                <div className="glass-panel p-8 rounded-xl border border-border lg:col-span-3">
                    {!selectedAppId ? (
                        <div className="text-center text-muted-foreground py-10">Select an application to configure</div>
                    ) : (
                        <div className="space-y-8">
                            {['google', 'facebook', 'apple'].map(provider => (
                                <div key={provider} className="border rounded-lg p-6 bg-card/50">
                                    <div className="flex items-center justify-between mb-4">
                                        <div className="flex items-center gap-3">
                                            <div className="p-2 rounded-full bg-primary/10 text-primary capitalize font-bold">
                                                {provider[0]}
                                            </div>
                                            <h3 className="font-semibold text-lg capitalize">{provider}</h3>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <label className="text-sm font-medium">Enable</label>
                                            <input
                                                type="checkbox"
                                                checked={config[provider]?.enabled || false}
                                                onChange={e => handleConfigChange(provider, 'enabled', e.target.checked)}
                                                className="w-4 h-4"
                                            />
                                        </div>
                                    </div>

                                    {config[provider]?.enabled && (
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 animate-in fade-in slide-in-from-top-2">
                                            <div>
                                                <label className="block text-sm font-medium mb-1.5 capitalize">{provider} Client ID</label>
                                                <input
                                                    type="text"
                                                    className="w-full px-3 py-2 rounded-lg border bg-background"
                                                    value={config[provider]?.clientId || ''}
                                                    onChange={e => handleConfigChange(provider, 'clientId', e.target.value)}
                                                    placeholder={`Enter ${provider} Client ID`}
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-sm font-medium mb-1.5 capitalize">{provider} Client Secret</label>
                                                <div className="relative">
                                                    <input
                                                        type="password"
                                                        className="w-full px-3 py-2 rounded-lg border bg-background pr-10"
                                                        value={config[provider]?.clientSecret || ''}
                                                        onChange={e => handleConfigChange(provider, 'clientSecret', e.target.value)}
                                                        placeholder="********"
                                                    />
                                                    <Lock className="absolute right-3 top-2.5 h-4 w-4 text-muted-foreground" />
                                                </div>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ExternalLoginsPage;

import React, { useEffect, useState } from 'react';
import { useToast } from '../../context/ToastContext';
import { AppService, AppConfig } from '../../services/appService';
import { Button } from "../ui/button";
import { Switch } from "../ui/switch";
import { Loader2, Save, Smartphone, Facebook, Globe, Lock } from 'lucide-react';
import { cn } from '../../lib/utils';

interface AuthProviderConfig {
    clientId: string;
    clientSecret: string;
    enabled: boolean;
    // Apple specific
    teamId?: string;
    keyId?: string;
    serviceId?: string;
}

interface ExternalAuthConfigs {
    google?: AuthProviderConfig;
    facebook?: AuthProviderConfig;
    apple?: AuthProviderConfig;
    [key: string]: AuthProviderConfig | undefined;
}

interface ExternalAuthConfigTabProps {
    app: AppConfig;
    onSave?: () => void;
}

const ExternalAuthConfigTab = ({ app, onSave }: ExternalAuthConfigTabProps) => {
    const { showToast } = useToast();
    const [config, setConfig] = useState<ExternalAuthConfigs>({});
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (app && (app as any).externalAuthProvidersJson) {
            try {
                const parsed = JSON.parse((app as any).externalAuthProvidersJson);
                setConfig(parsed);
            } catch {
                setConfig({});
            }
        }
    }, [app]);

    const handleConfigChange = (provider: string, field: string, value: any) => {
        setConfig(prev => ({
            ...prev,
            [provider]: {
                ...prev[provider] || { clientId: '', clientSecret: '', enabled: false },
                [field]: value
            }
        }));
    };

    const handleSave = async () => {
        setLoading(true);
        try {
            const json = JSON.stringify(config);
            await AppService.updateExternalAuth(app.id, json);
            showToast("External Login settings saved successfully", "success");
            if (onSave) onSave();
        } catch (error) {
            showToast("Failed to save settings", "error");
        } finally {
            setLoading(false);
        }
    };

    const PROVIDERS = [
        { id: 'google', name: 'Google', icon: Globe, color: 'bg-red-500', fields: ['clientId', 'clientSecret'] },
        { id: 'facebook', name: 'Facebook', icon: Facebook, color: 'bg-blue-600', fields: ['clientId', 'clientSecret'] }, // FB calls them App ID/Secret usually, but mapping to generic keys is easier or labelling them UI side
        { id: 'microsoft', name: 'Microsoft', icon: Globe, color: 'bg-orange-500', fields: ['clientId', 'clientSecret', 'tenantId'] },
        { id: 'linkedin', name: 'LinkedIn', icon: Globe, color: 'bg-blue-700', fields: ['clientId', 'clientSecret'] },
        { id: 'twitter', name: 'Twitter', icon: Globe, color: 'bg-sky-500', fields: ['clientId', 'clientSecret'] },
        { id: 'apple', name: 'Apple', icon: Smartphone, color: 'bg-black', fields: ['serviceId', 'teamId', 'keyId'] } // Apple is complex, simplified for MVP
    ];

    return (
        <div className="space-y-6 pt-4">
            <div>
                <h3 className="text-lg font-medium">Social Logins</h3>
                <p className="text-sm text-muted-foreground">Enable and configure external identity providers for this application.</p>
            </div>

            <div className="space-y-4">
                {PROVIDERS.map(provider => {
                    const isEnabled = config[provider.id]?.enabled || false;

                    return (
                        <div key={provider.id} className="border rounded-xl p-4 bg-card/50 transition-all hover:bg-card">
                            <div className="flex items-center justify-between mb-4">
                                <div className="flex items-center gap-3">
                                    <div className={`p-2 rounded-lg text-white ${provider.color}`}>
                                        <provider.icon size={20} />
                                    </div>
                                    <div>
                                        <h4 className="font-semibold">{provider.name} Login</h4>
                                        <p className="text-xs text-muted-foreground">{isEnabled ? 'Enabled' : 'Disabled'}</p>
                                    </div>
                                </div>
                                <Switch
                                    checked={isEnabled}
                                    onCheckedChange={(checked) => handleConfigChange(provider.id, 'enabled', checked)}
                                />
                            </div>

                            {isEnabled && (
                                <div className="grid grid-cols-1 gap-4 animate-in slide-in-from-top-2 pt-2 border-t mt-2">
                                    {provider.fields.map(field => {
                                        let label = 'Client ID';
                                        if (field === 'clientSecret') label = 'Client Secret';
                                        if (field === 'serviceId') label = 'Service ID';
                                        if (field === 'teamId') label = 'Team ID';
                                        if (field === 'keyId') label = 'Key ID';

                                        const type = field.toLowerCase().includes('secret') || field.includes('private') ? 'password' : 'text';

                                        return (
                                            <div key={field}>
                                                <label className="text-xs font-medium mb-1.5 block capitalize">{label}</label>
                                                <div className="relative">
                                                    <input
                                                        type={type}
                                                        className="w-full px-3 py-2 rounded-md border text-sm bg-background"
                                                        value={(config[provider.id] as any)?.[field] || ''}
                                                        onChange={e => handleConfigChange(provider.id, field, e.target.value)}
                                                        placeholder={`Enter ${label}`}
                                                    />
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            <div className="flex justify-end pt-4 border-t">
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    {loading ? <Loader2 size={16} className="animate-spin" /> : <Save size={16} />}
                    Save Configs
                </Button>
            </div>
        </div>
    );
};

export default ExternalAuthConfigTab;

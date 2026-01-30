import { useEffect, useState } from 'react';
import { useToast } from '../../context/ToastContext';
import { SettingsService } from '../../services/settingsService';
import { Check, MessageSquare, Terminal, Server, Globe, Save, Smartphone } from 'lucide-react';
import { Button } from "../ui/button";

type GatewayType = 'twilio' | 'aws_sns' | 'infobip';

const SmsSettingsTab = () => {
    const { showToast } = useToast();
    const [activeGateway, setActiveGateway] = useState<GatewayType>('twilio');
    const [configs, setConfigs] = useState<Record<string, any>>({});
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        loadConfigs();
    }, []);

    const loadConfigs = async () => {
        try {
            const data = await SettingsService.getSmsConfigs();
            const loadedConfigs: Record<string, any> = {};
            data.forEach((item: any) => {
                loadedConfigs[item.gatewayId] = item.configs || {};
            });
            setConfigs(loadedConfigs);

            const active = data.find((g: any) => g.isActive);
            if (active) setActiveGateway(active.gatewayId as GatewayType);
        } catch (error) {
            console.error("Failed to load SMS configs", error);
        }
    };

    const handleInputChange = (gatewayId: string, fieldKey: string, value: string) => {
        setConfigs(prev => ({
            ...prev,
            [gatewayId]: {
                ...prev[gatewayId],
                [fieldKey]: value
            }
        }));
    };

    const handleSave = async () => {
        setLoading(true);
        try {
            await SettingsService.saveSmsConfigs({ activeGateway, configs });
            showToast(`SMS Configuration saved successfully! Active Gateway: ${activeGateway.toUpperCase()}`, 'success');
        } catch (error) {
            showToast("Failed to save configuration.", "error");
        } finally {
            setLoading(false);
        }
    };

    const GATEWAYS = [
        { id: 'twilio', name: 'Twilio', icon: MessageSquare, description: 'Cloud communications platform for SMS, Voice, and Video.', fields: ['Account SID', 'Auth Token', 'Phone Number'] },
        { id: 'aws_sns', name: 'AWS SNS', icon: Server, description: 'Amazon Simple Notification Service.', fields: ['Access Key ID', 'Secret Access Key', 'Region'] },
        { id: 'infobip', name: 'Infobip', icon: Globe, description: 'Global omnichannel communication platform.', fields: ['API Key', 'Base URL'] }
    ];

    return (
        <div className="space-y-6 animate-fade-in py-2">
            <div className="flex items-center justify-between">
                <div>
                    <h3 className="text-lg font-medium">SMS Gateway Configuration</h3>
                    <p className="text-sm text-muted-foreground">Select and configure the SMS gateway provider for system notifications.</p>
                </div>
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    <Save size={16} />
                    {loading ? 'Saving...' : 'Save Changes'}
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 items-start">

                {/* Left Column: Getaway Selection */}
                <div className="md:col-span-1 space-y-3">
                    <h4 className="text-sm font-semibold text-muted-foreground mb-2 px-1">Gateway Provider</h4>
                    {GATEWAYS.map(gateway => (
                        <div
                            key={gateway.id}
                            onClick={() => setActiveGateway(gateway.id as GatewayType)}
                            className={`cursor-pointer p-3 rounded-xl border transition-all duration-200 ${activeGateway === gateway.id
                                    ? 'border-primary shadow-sm bg-primary/5 ring-1 ring-primary/20'
                                    : 'border-border hover:bg-accent/50 hover:border-accent-foreground/20'
                                }`}
                        >
                            <div className="flex items-center gap-3">
                                <div className={`p-2.5 rounded-lg ${activeGateway === gateway.id ? 'bg-primary text-white' : 'bg-muted text-muted-foreground'}`}>
                                    <gateway.icon size={18} />
                                </div>
                                <div className="flex-1">
                                    <div className="flex items-center justify-between">
                                        <h3 className="font-medium text-sm">{gateway.name}</h3>
                                        {activeGateway === gateway.id && <Check size={14} className="text-primary" />}
                                    </div>
                                    <p className="text-xs text-muted-foreground line-clamp-1 mt-0.5">{gateway.description}</p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Right Column: Configuration Form */}
                <div className="md:col-span-3 space-y-6">
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-2 mb-6 border-b border-border pb-4">
                            <Terminal size={18} className="text-muted-foreground" />
                            <h3 className="font-semibold text-base">Configuration: {GATEWAYS.find(g => g.id === activeGateway)?.name}</h3>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {GATEWAYS.find(g => g.id === activeGateway)?.fields.map(field => {
                                const fieldKey = field.toLowerCase().replace(/\s+/g, '_');
                                return (
                                    <div key={fieldKey}>
                                        <label className="block text-sm font-medium mb-1.5 text-foreground">{field}</label>
                                        <input
                                            type="text"
                                            className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 transition-shadow"
                                            placeholder={`Enter ${field}`}
                                            value={configs[activeGateway]?.[fieldKey] || ''}
                                            onChange={(e) => handleInputChange(activeGateway, fieldKey, e.target.value)}
                                        />
                                    </div>
                                );
                            })}
                        </div>
                    </div>

                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-2 mb-4">
                            <Smartphone size={18} className="text-muted-foreground" />
                            <h3 className="font-semibold text-base">Test Gateway</h3>
                        </div>
                        <div className="flex gap-3 items-end">
                            <div className="flex-1 max-w-sm">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Target Phone Number</label>
                                <input
                                    type="text"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="+1234567890"
                                    id="test-phone"
                                />
                            </div>
                            <Button
                                variant="secondary"
                                onClick={async () => {
                                    const phone = (document.getElementById('test-phone') as HTMLInputElement).value;
                                    if (!phone) return showToast("Enter a phone number", "error");

                                    try {
                                        await SettingsService.testSms(phone, "This is a test message from GlobalAdmin.");
                                        showToast("Test SMS sent successfully!", "success");
                                    } catch (err) {
                                        showToast("Failed to send test SMS.", "error");
                                    }
                                }}
                            >
                                Send Test SMS
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </div >
    );
};

export default SmsSettingsTab;

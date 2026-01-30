import { useEffect, useState } from 'react';
import { useToast } from '../context/ToastContext';
import { SettingsService } from '../services/settingsService';
import { Check, MessageSquare, Terminal, Server, Globe, Save } from 'lucide-react';
import { Button } from "../components/ui/button";

type GatewayType = 'twilio' | 'aws_sns' | 'infobip';

const SmsConfigPage = () => {
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
            // Map array to config object
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
        { id: 'aws_sns', name: 'AWS SNS', icon: Server, description: 'Amazon Simple Notification Service for high-throughput SMS.', fields: ['Access Key ID', 'Secret Access Key', 'Region'] },
        { id: 'infobip', name: 'Infobip', icon: Globe, description: 'Global omnichannel communication platform.', fields: ['API Key', 'Base URL'] }
    ];

    return (
        <div className="space-y-8 animate-fade-in p-2">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold mb-2">SMS Gateway Configuration</h1>
                    <p className="text-muted-foreground">Select and configure the SMS gateway provider for system notifications.</p>
                </div>
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    <Save size={16} />
                    {loading ? 'Saving...' : 'Save Changes'}
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {GATEWAYS.map(gateway => (
                    <div
                        key={gateway.id}
                        onClick={() => setActiveGateway(gateway.id as GatewayType)}
                        className={`cursor-pointer glass-panel p-6 rounded-xl border transition-all duration-300 transform hover:-translate-y-1 ${activeGateway === gateway.id
                            ? 'border-primary shadow-lg ring-1 ring-primary/20 bg-primary/5'
                            : 'border-border hover:border-gray-300 dark:hover:border-slate-600'
                            }`}
                    >
                        <div className="flex items-start justify-between mb-4">
                            <div className={`p-3 rounded-lg ${activeGateway === gateway.id ? 'bg-primary text-white' : 'bg-gray-100 dark:bg-slate-800 text-gray-500'}`}>
                                <gateway.icon size={24} />
                            </div>
                            {activeGateway === gateway.id && (
                                <span className="bg-primary text-white text-xs px-2 py-1 rounded-full flex items-center gap-1">
                                    <Check size={12} /> Active
                                </span>
                            )}
                        </div>
                        <h3 className="font-semibold text-lg mb-1">{gateway.name}</h3>
                        <p className="text-xs text-gray-500 dark:text-gray-400 line-clamp-2">{gateway.description}</p>
                    </div>
                ))}
            </div>

            <div className="glass-panel p-8 rounded-xl border border-border">
                <div className="flex items-center gap-3 mb-6 border-b border-border pb-4">
                    <Terminal size={20} className="text-gray-400" />
                    <h3 className="font-semibold text-lg">Configuration: {GATEWAYS.find(g => g.id === activeGateway)?.name}</h3>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {GATEWAYS.find(g => g.id === activeGateway)?.fields.map(field => {
                        const fieldKey = field.toLowerCase().replace(/\s+/g, '_');
                        return (
                            <div key={fieldKey}>
                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">{field}</label>
                                <input
                                    type="text" // In real app, check for 'password'/'token' in name to hide
                                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50 transition-shadow"
                                    placeholder={`Enter ${field}`}
                                    value={configs[activeGateway]?.[fieldKey] || ''}
                                    onChange={(e) => handleInputChange(activeGateway, fieldKey, e.target.value)}
                                />
                            </div>
                        );
                    })}
                </div>

                <div className="mt-8 pt-6 border-t border-border">
                    <h4 className="text-md font-medium mb-4">Test Configuration</h4>
                    <div className="flex gap-4 items-end">
                        <div className="flex-1 max-w-sm">
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Target Phone Number</label>
                            <input
                                type="text"
                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
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
                                    // Use SettingsService to test
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
        </div >
    );
};

export default SmsConfigPage;

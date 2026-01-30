import { useEffect, useState } from 'react';
import { useToast } from '../context/ToastContext';
import { SettingsService, SmtpConfig } from '../services/settingsService';
import { Mail, Server, Send, Save, Eye, EyeOff, Loader2 } from 'lucide-react';
import { Button } from "../components/ui/button";

const SmtpConfigPage = () => {
    const { showToast } = useToast();
    const [config, setConfig] = useState<SmtpConfig>({
        provider: 'custom',
        host: '',
        port: '587',
        username: '',
        password: '',
        fromAddress: '',
        fromName: '',
        enableSsl: true
    });
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [testToEmail, setTestToEmail] = useState('');

    useEffect(() => {
        loadConfig();
    }, []);

    const loadConfig = async () => {
        try {
            const data = await SettingsService.getSmtpConfig();
            if (data) setConfig(data);
        } catch (error) {
            console.error("Failed to load SMTP config", error);
        }
    };

    const handleInputChange = (key: string, value: any) => {
        setConfig(prev => ({ ...prev, [key]: value }));
    };

    const handleSave = async () => {
        setLoading(true);
        try {
            await SettingsService.saveSmtpConfig(config);
            showToast("SMTP Configuration saved successfully!", "success");
        } catch (error) {
            showToast("Failed to save SMTP configuration.", "error");
        } finally {
            setLoading(false);
        }
    };

    const handleTestEmail = async () => {
        if (!config.fromAddress) {
            showToast("Please enter a 'From Address' to test.", "error");
            return;
        }
        if (!testToEmail) {
            showToast("Please enter a 'Receiver Email' to test.", "error");
            return;
        }
        showToast(`Sending test email to ${testToEmail}...`, "info");
        try {
            await SettingsService.testSmtpConfig(testToEmail);
            showToast("Test email sent successfully!", "success");
        } catch (error) {
            showToast("Failed to send test email.", "error");
        }
    };

    return (
        <div className="space-y-8 animate-fade-in p-2">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold mb-2">SMTP Settings</h1>
                    <p className="text-muted-foreground">Configure email delivery settings for system notifications and alerts.</p>
                </div>
                <div className="flex gap-2">
                    <Button onClick={handleSave} disabled={loading} className="gap-2">
                        <Save size={16} />
                        {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        {loading ? 'Saving...' : 'Save Changes'}
                    </Button>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Configuration Panel */}
                <div className="lg:col-span-2 glass-panel p-8 rounded-xl border border-border">
                    {/* ... (Server Config content unchanged) ... */}
                    <div className="flex items-center gap-3 mb-6">
                        <div className="p-3 rounded-lg bg-primary/20 text-primary">
                            <Server size={24} />
                        </div>
                        <h3 className="font-semibold text-lg">Server Configuration</h3>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="col-span-2">
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">SMTP Provider</label>
                            <select
                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                value={config.provider}
                                onChange={(e) => handleInputChange('provider', e.target.value)}
                            >
                                <option value="custom">Custom SMTP</option>
                                <option value="sendgrid">SendGrid</option>
                                <option value="mailgun">Mailgun</option>
                                <option value="aws_ses">AWS SES</option>
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Host</label>
                            <input
                                type="text"
                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                placeholder="smtp.example.com"
                                value={config.host}
                                onChange={(e) => handleInputChange('host', e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Port</label>
                            <input
                                type="number"
                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                placeholder="587"
                                value={config.port}
                                onChange={(e) => handleInputChange('port', e.target.value)}
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Username</label>
                            <input
                                type="text"
                                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                placeholder="user@example.com"
                                value={config.username || ''}
                                onChange={(e) => handleInputChange('username', e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Password</label>
                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    className="w-full px-3 py-2 pr-10 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="********"
                                    value={config.password || ''}
                                    onChange={(e) => handleInputChange('password', e.target.value)}
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-3 top-2.5 text-muted-foreground hover:text-foreground"
                                >
                                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                </button>
                            </div>
                        </div>

                        <div className="col-span-2 flex items-center gap-2 mt-2">
                            <input
                                type="checkbox"
                                id="ssl"
                                checked={config.enableSsl}
                                onChange={(e) => handleInputChange('enableSsl', e.target.checked)}
                                className="w-4 h-4 text-primary bg-white border-gray-300 rounded focus:ring-primary"
                            />
                            <label htmlFor="ssl" className="text-sm font-medium text-gray-700 dark:text-gray-300">Enable SSL/TLS</label>
                        </div>
                    </div>
                </div>

                {/* Sender Settings & Test */}
                <div className="space-y-6">
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="p-3 rounded-lg bg-green-500/10 text-green-500">
                                <Mail size={24} />
                            </div>
                            <h3 className="font-semibold text-lg">Sender Details</h3>
                        </div>
                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">From Name</label>
                                <input
                                    type="text"
                                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="System Admin"
                                    value={config.fromName}
                                    onChange={(e) => handleInputChange('fromName', e.target.value)}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">From Address</label>
                                <input
                                    type="email"
                                    className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="admin@globaldashboard.com"
                                    value={config.fromAddress}
                                    onChange={(e) => handleInputChange('fromAddress', e.target.value)}
                                />
                            </div>
                        </div>
                    </div>

                    {/* Test Email Section */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="p-3 rounded-lg bg-blue-500/10 text-blue-500">
                                <Send size={24} />
                            </div>
                            <h3 className="font-semibold text-lg">Test Configuration</h3>
                        </div>
                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium mb-1.5 text-gray-700 dark:text-gray-300">Receiver Email</label>
                                <div className="flex gap-2">
                                    <input
                                        type="email"
                                        className="flex-1 px-3 py-2 rounded-lg border border-gray-200 dark:border-slate-700 bg-white dark:bg-slate-900 focus:outline-none focus:ring-2 focus:ring-primary/50"
                                        placeholder="test@example.com"
                                        value={testToEmail}
                                        onChange={(e) => setTestToEmail(e.target.value)}
                                    />
                                    <Button variant="outline" onClick={handleTestEmail} className="gap-2">
                                        <Send size={16} />
                                        Send
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SmtpConfigPage;

import { useEffect, useState } from 'react';
import { useToast } from '../../context/ToastContext';
import { SettingsService, SmtpConfig } from '../../services/settingsService';
import { Mail, Server, Send, Save, Eye, EyeOff, Loader2, Check, Cloud } from 'lucide-react';
import { Button } from "../ui/button";

const SmtpSettingsTab = () => {
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
        if (key === 'provider') {
            // Pre-fill defaults based on provider
            let defaults = {};
            switch (value) {
                case 'sendgrid':
                    defaults = { host: 'smtp.sendgrid.net', port: '587' };
                    break;
                case 'mailgun':
                    defaults = { host: 'smtp.mailgun.org', port: '587' };
                    break;
                case 'aws_ses':
                    defaults = { host: 'email-smtp.us-east-1.amazonaws.com', port: '587' };
                    break;
                case 'custom':
                    defaults = { host: '', port: '587' }; 
                    break;
            }
            setConfig(prev => ({ ...prev, [key]: value, ...defaults }));
        } else {
            setConfig(prev => ({ ...prev, [key]: value }));
        }
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

    const PROVIDERS = [
        { id: 'custom', name: 'Custom SMTP', icon: Server, description: 'Connect to any standard SMTP server.' },
        { id: 'sendgrid', name: 'SendGrid', icon: Mail, description: 'Twilio SendGrid Email API.' },
        { id: 'mailgun', name: 'Mailgun', icon: Mail, description: 'Mailgun Email Delivery Service.' },
        { id: 'aws_ses', name: 'AWS SES', icon: Cloud, description: 'Amazon Simple Email Service.' }
    ];

    return (
        <div className="space-y-6 animate-fade-in py-2">
            <div className="flex items-center justify-between">
                <div>
                    <h3 className="text-lg font-medium">SMTP Settings</h3>
                    <p className="text-sm text-muted-foreground">Configure email delivery settings for system notifications and alerts.</p>
                </div>
                <div className="flex gap-2">
                    <Button onClick={handleSave} disabled={loading} className="gap-2">
                        <Save size={16} />
                        {loading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        {loading ? 'Saving...' : 'Save Changes'}
                    </Button>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 items-start">

                {/* Left Column: Provider Selection */}
                <div className="md:col-span-1 space-y-3">
                    <h4 className="text-sm font-semibold text-muted-foreground mb-2 px-1">Provider</h4>
                    {PROVIDERS.map(provider => (
                        <div
                            key={provider.id}
                            onClick={() => handleInputChange('provider', provider.id)}
                            className={`cursor-pointer p-3 rounded-xl border transition-all duration-200 ${config.provider === provider.id
                                    ? 'border-primary shadow-sm bg-primary/5 ring-1 ring-primary/20'
                                    : 'border-border hover:bg-accent/50 hover:border-accent-foreground/20'
                                }`}
                        >
                            <div className="flex items-center gap-3">
                                <div className={`p-2.5 rounded-lg ${config.provider === provider.id ? 'bg-primary text-white' : 'bg-muted text-muted-foreground'}`}>
                                    <provider.icon size={18} />
                                </div>
                                <div className="flex-1">
                                    <div className="flex items-center justify-between">
                                        <h3 className="font-medium text-sm">{provider.name}</h3>
                                        {config.provider === provider.id && <Check size={14} className="text-primary" />}
                                    </div>
                                    <p className="text-xs text-muted-foreground line-clamp-1 mt-0.5">{provider.description}</p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Right Column: Configuration Form */}
                <div className="md:col-span-3 space-y-6">
                    {/* Server Config */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-2 mb-6 border-b border-border pb-4">
                            <Server size={18} className="text-muted-foreground" />
                            <h3 className="font-semibold text-base">Server Configuration</h3>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="col-span-2 md:col-span-1">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Host Address</label>
                                <input
                                    type="text"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="smtp.example.com"
                                    value={config.host}
                                    onChange={(e) => handleInputChange('host', e.target.value)}
                                />
                            </div>
                            <div className="col-span-2 md:col-span-1">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Port</label>
                                <input
                                    type="number"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="587"
                                    value={config.port}
                                    onChange={(e) => handleInputChange('port', e.target.value)}
                                />
                            </div>

                            <div className="col-span-2 md:col-span-1">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Username</label>
                                <input
                                    type="text"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="user@example.com"
                                    value={config.username || ''}
                                    onChange={(e) => handleInputChange('username', e.target.value)}
                                />
                            </div>
                            <div className="col-span-2 md:col-span-1">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Password</label>
                                <div className="relative">
                                    <input
                                        type={showPassword ? "text" : "password"}
                                        className="w-full px-3 py-2 pr-10 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                        placeholder="********"
                                        value={config.password || ''}
                                        onChange={(e) => handleInputChange('password', e.target.value)}
                                    />
                                    <button
                                        type="button"
                                        onClick={() => setShowPassword(!showPassword)}
                                        className="absolute right-3 top-2.5 text-muted-foreground hover:text-foreground"
                                    >
                                        {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                                    </button>
                                </div>
                            </div>

                            <div className="col-span-2 flex items-center gap-2 mt-2">
                                <input
                                    type="checkbox"
                                    id="ssl"
                                    checked={config.enableSsl}
                                    onChange={(e) => handleInputChange('enableSsl', e.target.checked)}
                                    className="w-4 h-4 text-primary bg-background border-input rounded focus:ring-primary"
                                />
                                <label htmlFor="ssl" className="text-sm font-medium text-foreground cursor-pointer">Enable SSL/TLS Encryption</label>
                            </div>
                        </div>
                    </div>

                    {/* Sender Details */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-2 mb-6 border-b border-border pb-4">
                            <Mail size={18} className="text-muted-foreground" />
                            <h3 className="font-semibold text-base">Sender Identity</h3>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label className="block text-sm font-medium mb-1.5 text-foreground">From Name</label>
                                <input
                                    type="text"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="System Admin"
                                    value={config.fromName}
                                    onChange={(e) => handleInputChange('fromName', e.target.value)}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1.5 text-foreground">From Address</label>
                                <input
                                    type="email"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="admin@globaldashboard.com"
                                    value={config.fromAddress}
                                    onChange={(e) => handleInputChange('fromAddress', e.target.value)}
                                />
                            </div>
                        </div>
                    </div>

                    {/* Test Config */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <div className="flex items-center gap-2 mb-4">
                            <Send size={18} className="text-muted-foreground" />
                            <h3 className="font-semibold text-base">Test Email</h3>
                        </div>
                        <div className="flex gap-3 items-end">
                            <div className="flex-1">
                                <label className="block text-sm font-medium mb-1.5 text-foreground">Receiver Email</label>
                                <input
                                    type="email"
                                    className="w-full px-3 py-2 rounded-md border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                                    placeholder="test@example.com"
                                    value={testToEmail}
                                    onChange={(e) => setTestToEmail(e.target.value)}
                                />
                            </div>
                            <Button variant="secondary" onClick={handleTestEmail} className="gap-2">
                                <Send size={16} />
                                Send Test
                            </Button>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    );
};

export default SmtpSettingsTab;

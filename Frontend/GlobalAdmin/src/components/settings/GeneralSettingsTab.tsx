import React, { useState, useEffect } from 'react';
import { Settings, Moon, Sun, Bell, Layout } from 'lucide-react';
import { Button } from "../ui/button";
import { SettingsService } from '../../services/settingsService';
import { useToast } from '../../context/ToastContext';
import { useAuth } from '../../context/AuthContext';
import { UI_DEFAULTS } from '../../config/uiDefaults';
import { Switch } from "../ui/switch";

const GeneralSettingsTab = () => {
    const { user } = useAuth();
    const { showToast } = useToast();
    const [loading, setLoading] = useState(true);

    const [theme, setTheme] = useState(UI_DEFAULTS.THEME);
    const [collapsed, setCollapsed] = useState(UI_DEFAULTS.SIDEBAR_COLLAPSED);
    const [notificationChannels, setNotificationChannels] = useState({
        sms: true,
        email: true,
        push: true
    });

    useEffect(() => {
        loadConfig();
    }, []);

    const loadConfig = async () => {
        setLoading(true);
        const config = await SettingsService.getGeneralConfig();
        if (config) {
            if (config.defaultTheme) setTheme(config.defaultTheme);
            if (config.defaultSidebarCollapsed !== undefined) setCollapsed(config.defaultSidebarCollapsed);
            if (config.notificationChannels) setNotificationChannels(config.notificationChannels);
        }
        setLoading(false);
    };

    const handleSave = async () => {
        try {
            const config = {
                defaultTheme: theme,
                defaultSidebarCollapsed: collapsed,
                notificationChannels: notificationChannels
            };

            await SettingsService.saveGeneralConfig(config, user?.name || 'System Admin');
            showToast('System settings saved successfully. All admins notified.', 'success');
        } catch (error) {
            console.error(error);
            showToast('Failed to save settings.', 'error');
        }
    };

    return (
        <div className="animate-fade-in space-y-6 py-2">
            <div className="flex justify-between items-center">
                <div>
                    <h3 className="text-lg font-medium">General Preferences</h3>
                    <p className="text-sm text-muted-foreground">Configure global system defaults and appearance.</p>
                </div>
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    <Settings size={16} />
                    {loading ? 'Saving...' : 'Save Changes'}
                </Button>
            </div>

            {/* Vertical Stack Layout */}
            <div className="space-y-6 max-w-2xl">
                {/* Theme Config */}
                <div className="glass-panel p-6 rounded-xl border border-border">
                    <h3 className="text-base font-semibold mb-4 flex items-center gap-2">
                        <Moon size={18} className="text-primary" />
                        Default Appearance
                    </h3>
                    <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-card/50">
                        <div>
                            <span className="block font-medium text-sm">Dark Mode Default</span>
                            <span className="text-xs text-muted-foreground">New users will start in dark mode</span>
                        </div>
                        <Switch
                            checked={theme === 'dark'}
                            onCheckedChange={(c) => setTheme(c ? 'dark' : 'light')}
                        />
                    </div>
                </div>

                {/* Sidebar Config */}
                <div className="glass-panel p-6 rounded-xl border border-border">
                    <h3 className="text-base font-semibold mb-4 flex items-center gap-2">
                        <Layout size={18} className="text-primary" />
                        Layout Defaults
                    </h3>
                    <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-card/50">
                        <div>
                            <span className="block font-medium text-sm">Collapsed Sidebar</span>
                            <span className="text-xs text-muted-foreground">Start with sidebar minimized</span>
                        </div>
                        <Switch
                            checked={collapsed}
                            onCheckedChange={(c) => setCollapsed(c)}
                        />
                    </div>
                </div>

                {/* Notifications Config */}
                <div className="glass-panel p-6 rounded-xl border border-border">
                    <h3 className="text-base font-semibold mb-4 flex items-center gap-2">
                        <Bell size={18} className="text-primary" />
                        System Notifications
                    </h3>
                    <div className="space-y-4">
                        <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-card/50">
                            <div>
                                <span className="block font-medium text-sm">Enable SMS</span>
                                <span className="text-xs text-muted-foreground">Send notifications via SMS gateways</span>
                            </div>
                            <Switch
                                checked={notificationChannels.sms}
                                onCheckedChange={(c) => setNotificationChannels({ ...notificationChannels, sms: c })}
                            />
                        </div>
                        <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-card/50">
                            <div>
                                <span className="block font-medium text-sm">Enable Email (SMTP)</span>
                                <span className="text-xs text-muted-foreground">Send system emails using SMTP</span>
                            </div>
                            <Switch
                                checked={notificationChannels.email}
                                onCheckedChange={(c) => setNotificationChannels({ ...notificationChannels, email: c })}
                            />
                        </div>
                        <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-card/50">
                            <div>
                                <span className="block font-medium text-sm">Browser Push</span>
                                <span className="text-xs text-muted-foreground">Web push notifications</span>
                            </div>
                            <Switch
                                checked={notificationChannels.push}
                                onCheckedChange={(c) => setNotificationChannels({ ...notificationChannels, push: c })}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default GeneralSettingsTab;

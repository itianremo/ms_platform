import React, { useState, useEffect } from 'react';
import { Settings, Moon, Sun, Bell, Layout } from 'lucide-react';
import { Button } from "../components/ui/button";
import { SettingsService } from '../services/settingsService';
import { useToast } from '../context/ToastContext';
import { useAuth } from '../context/AuthContext';
import { UI_DEFAULTS } from '../config/uiDefaults';

const GeneralConfigPage = () => {
    const { user } = useAuth();
    const { showToast } = useToast();
    const [loading, setLoading] = useState(true);

    // Config State
    const [theme, setTheme] = useState(UI_DEFAULTS.THEME); // 'dark' or 'light'
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
        <div className="animate-fade-in space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">General Settings</h1>
                    <p className="text-gray-500 dark:text-gray-400">Configure global system defaults.</p>
                </div>
                <Button onClick={handleSave} disabled={loading} className="gap-2">
                    <Settings size={16} />
                    {loading ? 'Saving...' : 'Save Changes'}
                </Button>
            </div>

            {loading ? (
                <div className="p-12 text-center text-gray-500">Loading configurations...</div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Theme Config */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                            <Moon size={20} className="text-primary" />
                            Default Appearance
                        </h3>
                        <div className="space-y-4">
                            <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-gray-50 dark:bg-white/5">
                                <div>
                                    <span className="block font-medium">Dark Mode Default</span>
                                    <span className="text-xs text-gray-500">New users will start in dark mode</span>
                                </div>
                                <div className="relative inline-block w-12 mr-2 align-middle select-none transition duration-200 ease-in">
                                    <input
                                        type="checkbox"
                                        name="toggle"
                                        id="theme-toggle"
                                        className="toggle-checkbox absolute block w-6 h-6 rounded-full bg-white border-4 appearance-none cursor-pointer"
                                        checked={theme === 'dark'}
                                        onChange={(e) => setTheme(e.target.checked ? 'dark' : 'light')}
                                    />
                                    <label htmlFor="theme-toggle" className="toggle-label block overflow-hidden h-6 rounded-full bg-gray-300 cursor-pointer"></label>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Sidebar Config */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                            <Layout size={20} className="text-primary" />
                            Layout Defaults
                        </h3>
                        <div className="space-y-4">
                            <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-gray-50 dark:bg-white/5">
                                <div>
                                    <span className="block font-medium">Collapsed Sidebar</span>
                                    <span className="text-xs text-gray-500">Start with sidebar minimized</span>
                                </div>
                                <input
                                    type="checkbox"
                                    className="w-5 h-5 text-primary rounded focus:ring-primary border-gray-300"
                                    checked={collapsed}
                                    onChange={(e) => setCollapsed(e.target.checked)}
                                />
                            </div>
                        </div>
                    </div>

                    {/* Notifications Config */}
                    <div className="glass-panel p-6 rounded-xl border border-border md:col-span-2">
                        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                            <Bell size={20} className="text-primary" />
                            System Notifications
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <label className="flex items-center space-x-3 p-4 border border-border rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-white/5">
                                <input
                                    type="checkbox"
                                    className="form-checkbox h-5 w-5 text-primary"
                                    checked={notificationChannels.sms}
                                    onChange={(e) => setNotificationChannels({ ...notificationChannels, sms: e.target.checked })}
                                />
                                <span className="font-medium">Enable SMS</span>
                            </label>
                            <label className="flex items-center space-x-3 p-4 border border-border rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-white/5">
                                <input
                                    type="checkbox"
                                    className="form-checkbox h-5 w-5 text-primary"
                                    checked={notificationChannels.email}
                                    onChange={(e) => setNotificationChannels({ ...notificationChannels, email: e.target.checked })}
                                />
                                <span className="font-medium">Enable Email (SMTP)</span>
                            </label>
                            <label className="flex items-center space-x-3 p-4 border border-border rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-white/5">
                                <input
                                    type="checkbox"
                                    className="form-checkbox h-5 w-5 text-primary"
                                    checked={notificationChannels.push}
                                    onChange={(e) => setNotificationChannels({ ...notificationChannels, push: e.target.checked })}
                                />
                                <span className="font-medium">Browser Push</span>
                            </label>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default GeneralConfigPage;

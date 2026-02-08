import React, { useState, useEffect } from 'react';
import { Moon, Layout, Save, Loader2 } from 'lucide-react';
import { Button } from "../components/ui/button";
import { AppService } from '../services/appService';
import { useToast } from '../context/ToastContext';
import { useAuth } from '../context/AuthContext';

// Hardcoded Default for UI if not loaded
const UI_DEFAULTS = {
    theme: 'light',
    sidebarCollapsed: false
};

const GeneralConfigPage = () => {
    const { user } = useAuth();
    const { showToast } = useToast();
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // State
    const [theme, setTheme] = useState<'light' | 'dark' | 'system'>('light');
    const [collapsed, setCollapsed] = useState(false);

    // FitIt App ID
    const APP_ID = "11111111-1111-1111-1111-111111111111";

    useEffect(() => {
        loadConfig();
    }, []);

    const loadConfig = async () => {
        setLoading(true);
        try {
            // Load App Default Settings
            const app = await AppService.getAppById(APP_ID);
            if (app.themeJson) {
                try {
                    const themeData = JSON.parse(app.themeJson);
                    setTheme(themeData.theme || UI_DEFAULTS.theme);
                    setCollapsed(themeData.sidebarCollapsed || UI_DEFAULTS.sidebarCollapsed);
                } catch {
                    setTheme(UI_DEFAULTS.theme as any);
                    setCollapsed(UI_DEFAULTS.sidebarCollapsed);
                }
            }
        } catch (error) {
            console.error("Failed to load configs", error);
            showToast("Failed to load settings", "error");
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        setSaving(true);
        try {
            // Sync to Apps API (For Persistent Theme / Public Access)
            try {
                // Fetch current app to get latest state details
                const currentApp = await AppService.getAppById(APP_ID);

                // Update ThemeJson with BOTH theme and sidebar state
                const themeData = {
                    theme: theme,
                    sidebarCollapsed: collapsed
                };

                await AppService.updateApp(APP_ID, {
                    name: currentApp.name,
                    description: currentApp.description,
                    baseUrl: currentApp.baseUrl,
                    themeJson: JSON.stringify(themeData)
                });

                showToast('App defaults saved successfully.', 'success');
            } catch (syncError) {
                console.error("Failed to sync app config", syncError);
                showToast('Failed to save app config.', 'error');
            }
        } catch (error) {
            console.error(error);
            showToast('Failed to save settings.', 'error');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in max-w-5xl mx-auto">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">App Configuration</h2>
                    <p className="text-muted-foreground">Manage default appearance and settings for this application.</p>
                </div>
                <div className="flex items-center gap-2">
                    <Button onClick={handleSave} disabled={loading || saving}>
                        {saving ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Save className="mr-2 h-4 w-4" />}
                        Save Changes
                    </Button>
                </div>
            </div>

            {loading ? (
                <div className="p-12 text-center text-gray-500">Loading configurations...</div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Default Appearance (Merged) */}
                    <div className="glass-panel p-6 rounded-xl border border-border">
                        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                            <Moon size={20} className="text-primary" />
                            Default Appearance
                        </h3>
                        <div className="space-y-6">
                            {/* Dark Mode Toggle */}
                            <div className="flex items-center justify-between p-3 rounded-lg border border-border bg-gray-50 dark:bg-white/5">
                                <div>
                                    <span className="block font-medium">Dark Mode Default</span>
                                    <span className="text-xs text-gray-500">New users and public pages will start in dark mode</span>
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

                            {/* Sidebar Toggle */}
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
                </div>
            )}
        </div>
    );
};

export default GeneralConfigPage;

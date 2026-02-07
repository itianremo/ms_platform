
import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../ui/card";
import { Label } from "../ui/label";
import { Switch } from "../ui/switch";
import { useUserPreferences } from '../../context/UserPreferencesContext';
import { useTheme } from '../theme-provider';
import { cn } from '../../lib/utils';
import { Moon, Sun, Sidebar } from 'lucide-react';

export const AppearanceSettingsCard = () => {
    const { preferences, updatePreferences } = useUserPreferences();
    const { theme } = useTheme();

    const handleThemeChange = (newTheme: 'light' | 'dark') => {
        updatePreferences({ theme: newTheme });
    };

    const handleSidebarChange = (checked: boolean) => {
        updatePreferences({ sidebarCollapsed: checked });
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>Appearance</CardTitle>
                <CardDescription>Customize the interface look and feel.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
                <div className="space-y-4">
                    <Label>Theme Preference</Label>
                    <div className="grid grid-cols-2 gap-4 max-w-sm">
                        <div
                            className={cn(
                                "cursor-pointer rounded-lg border-2 p-2 hover:border-primary transition-all flex flex-col items-center justify-center gap-2",
                                theme === 'light' ? "border-primary bg-primary/5" : "border-muted"
                            )}
                            onClick={() => handleThemeChange('light')}
                        >
                            <Sun className="h-6 w-6" />
                            <span className="font-medium text-sm">Light</span>
                        </div>
                        <div
                            className={cn(
                                "cursor-pointer rounded-lg border-2 p-2 hover:border-primary transition-all flex flex-col items-center justify-center gap-2",
                                theme === 'dark' ? "border-primary bg-primary/5" : "border-muted"
                            )}
                            onClick={() => handleThemeChange('dark')}
                        >
                            <Moon className="h-6 w-6" />
                            <span className="font-medium text-sm">Dark</span>
                        </div>
                    </div>
                </div>

                <div className="flex items-center justify-between space-x-2 rounded-lg border p-4">
                    <div className="flex items-center space-x-4">
                        <Sidebar className="h-5 w-5 text-muted-foreground" />
                        <Label htmlFor="sidebar-collapse" className="flex flex-col space-y-1">
                            <span>Collapse Sidebar</span>
                            <span className="font-normal text-xs text-muted-foreground">Minimize the sidebar to icon-only mode.</span>
                        </Label>
                    </div>
                    <Switch
                        id="sidebar-collapse"
                        checked={preferences.sidebarCollapsed}
                        onCheckedChange={handleSidebarChange}
                    />
                </div>
            </CardContent>
        </Card>
    );
};

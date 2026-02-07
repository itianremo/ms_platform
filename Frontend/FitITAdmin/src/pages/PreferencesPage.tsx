import React from 'react';
import { AppearanceSettingsCard } from '../components/AppearanceSettingsCard';

const PreferencesPage = () => {
    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Settings</h2>
                <p className="text-muted-foreground">Manage your admin preferences.</p>
            </div>

            <div className="grid gap-6">
                <AppearanceSettingsCard />
            </div>
        </div>
    );
};

export default PreferencesPage;

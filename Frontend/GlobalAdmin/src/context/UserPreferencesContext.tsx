
import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { UserService, UserProfile } from '../services/userService';
import { AppService } from '../services/appService';
import { useAuth } from './AuthContext';
import { useTheme } from '../components/theme-provider';

interface UserPreferences {
    theme: 'light' | 'dark' | 'system';
    sidebarCollapsed: boolean;
}

interface UserPreferencesContextType {
    preferences: UserPreferences;
    updatePreferences: (newPrefs: Partial<UserPreferences>) => Promise<void>;
    loading: boolean;
}

const UserPreferencesContext = createContext<UserPreferencesContextType | undefined>(undefined);

export const UserPreferencesProvider = ({ children }: { children: React.ReactNode }) => {
    const { user } = useAuth();
    const { setTheme } = useTheme();
    const [preferences, setPreferences] = useState<UserPreferences>({
        theme: 'system',
        sidebarCollapsed: false
    });
    const [loading, setLoading] = useState(true);
    const [profile, setProfile] = useState<UserProfile | null>(null);

    // Initial Load
    useEffect(() => {
        loadPreferences();
    }, [user]);

    const loadAppDefaults = async () => {
        try {
            const SYSTEM_APP_ID = "00000000-0000-0000-0000-000000000001";
            const app = await AppService.getAppById(SYSTEM_APP_ID);

            if (app.themeJson) {
                try {
                    const themeData = JSON.parse(app.themeJson);
                    setPreferences(prev => ({
                        ...prev,
                        ...themeData
                    }));
                    if (themeData.theme) {
                        setTheme(themeData.theme);
                    }
                } catch { } // Ignore parse errors
            }
        } catch (e) {
            console.error("Failed to load app defaults", e);
        }
    };

    const loadPreferences = async () => {
        // If not logged in, load from AppConfig
        if (!user) {
            // Reset to defaults first to avoid stale state from previous user
            setPreferences({
                theme: 'system',
                sidebarCollapsed: false
            });
            await loadAppDefaults();
            setLoading(false);
            return;
        }

        setLoading(true);

        const SYSTEM_APP_ID = "00000000-0000-0000-0000-000000000001";

        try {
            const p = await UserService.getProfile(user.id, SYSTEM_APP_ID);
            setProfile(p);

            if (p && p.customDataJson) {
                try {
                    const data = JSON.parse(p.customDataJson);
                    setPreferences(prev => ({ ...prev, ...data }));

                    if (data.theme) {
                        setTheme(data.theme);
                    }
                } catch (e) {
                    console.error("Failed to parse preferences", e);
                }
            }
        } catch (error) {
            console.error("Failed to load preferences", error);
        } finally {
            setLoading(false);
        }
    };

    const updatePreferences = async (newPrefs: Partial<UserPreferences>) => {
        if (!user || !profile) return;

        // Optimistic Update
        const updated = { ...preferences, ...newPrefs };
        setPreferences(updated);

        // Apply Theme Side-effect
        if (newPrefs.theme) {
            setTheme(newPrefs.theme);
        }

        try {
            const SYSTEM_APP_ID = "00000000-0000-0000-0000-000000000001";

            // Merge with existing customDataJson
            let currentData = {};
            try {
                currentData = JSON.parse(profile.customDataJson || '{}');
            } catch { }

            const mergedData = { ...currentData, ...newPrefs };

            await UserService.updateProfile({
                userId: user.id,
                appId: SYSTEM_APP_ID,
                displayName: profile.displayName,
                bio: profile.bio,
                avatarUrl: profile.avatarUrl,
                gender: profile.gender,
                dateOfBirth: profile.dateOfBirth,
                customDataJson: JSON.stringify(mergedData)
            });

            // Update local profile state to reflect saved data (optional)
            setProfile({ ...profile, customDataJson: JSON.stringify(mergedData) });

        } catch (error) {
            console.error("Failed to save preferences", error);
            // Revert? For now, just log.
        }
    };

    return (
        <UserPreferencesContext.Provider value={{ preferences, updatePreferences, loading }}>
            {children}
        </UserPreferencesContext.Provider>
    );
};

export const useUserPreferences = () => {
    const context = useContext(UserPreferencesContext);
    if (context === undefined) {
        throw new Error('useUserPreferences must be used within a UserPreferencesProvider');
    }
    return context;
};

import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';
import { UI_DEFAULTS } from '../config/uiDefaults';
import { SettingsService } from '../services/settingsService';
import { useAuth } from './AuthContext';
import { UserService } from '../services/userService';

interface ThemeContextType {
    isDark: boolean;
    toggleTheme: () => void;
    setTheme: (isDark: boolean) => void;
}

const ThemeContext = createContext<ThemeContextType | null>(null);

export const ThemeProvider = ({ children }: { children: React.ReactNode }) => {


    // ...

    const { user } = useAuth();

    // Initialize from localStorage or default from Config
    const [isDark, setIsDark] = useState(() => {
        const saved = localStorage.getItem('theme');
        if (saved) return saved === 'dark';
        return UI_DEFAULTS.THEME === 'dark';
    });

    useEffect(() => {
        const initTheme = async () => {
            let targetTheme = null;

            // 1. If User Logged In, try User Prefs
            if (user?.id) {
                try {
                    const profile = await UserService.getProfile(user.id);
                    const prefs = JSON.parse(profile?.customDataJson || '{}');
                    if (prefs.theme) {
                        targetTheme = prefs.theme;
                    }
                } catch (e) {
                    console.warn("ThemeContext: Failed to load user prefs", e);
                }
            }

            // 2. If no user pref, try System Config (if not already in LS?)
            // Actually, if we just logged in, we want to OVERRIDE LS with Cloud Prefs.
            // If we are anon, we stick to LS or System.

            if (!targetTheme && !localStorage.getItem('theme')) {
                try {
                    const config = await SettingsService.getGeneralConfig();
                    if (config?.defaultTheme) targetTheme = config.defaultTheme;
                } catch (e) { }
            }

            if (targetTheme) {
                setIsDark(targetTheme === 'dark');
            }
        };
        initTheme();
    }, [user]);

    useEffect(() => {
        // Apply theme class to body
        if (isDark) {
            document.body.classList.remove('light');
            document.documentElement.classList.add('dark'); // Ensure Tailwind dark mode works
        } else {
            document.body.classList.add('light');
            document.documentElement.classList.remove('dark');
        }
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
    }, [isDark]);

    const toggleTheme = () => {
        setIsDark(prev => !prev);
    };

    const setTheme = (val: boolean) => {
        setIsDark(val);
    };

    return (
        <ThemeContext.Provider value={{ isDark, toggleTheme, setTheme }}>
            {children}
        </ThemeContext.Provider>
    );
};

export const useTheme = () => {
    const context = useContext(ThemeContext);
    if (!context) throw new Error('useTheme must be used within a ThemeProvider');
    return context;
};

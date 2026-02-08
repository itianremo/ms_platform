import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Bell, Moon, Sun, LogOut, User, Activity } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from './theme-provider';
import { useUserPreferences } from '../context/UserPreferencesContext';
import { NotificationDrawer } from './NotificationDrawer';
import type { Notification } from '../types/notification';
import { NotificationService } from '../services/NotificationService';
import { UserService } from '../services/userService';
import { cn } from '../lib/utils';
import { APP_ID } from '../config';

export const Header = () => {
    const { user, logout } = useAuth();
    const { theme, setTheme } = useTheme();
    const { updatePreferences } = useUserPreferences();

    // Notification State
    const [isNotificationOpen, setIsNotificationOpen] = useState(false);
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [profileOpen, setProfileOpen] = useState(false);

    const unreadCount = Array.isArray(notifications) ? notifications.filter(n => !n.isRead).length : 0;

    useEffect(() => {
        fetchNotifications();
        fetchUserProfile();

        // Poll every minute
        const interval = setInterval(fetchNotifications, 60000);
        return () => clearInterval(interval);
    }, []);

    const fetchNotifications = async () => {
        try {
            if (user?.id) {
                const data = await NotificationService.getMyNotifications(user.id);
                // Map service type to component type
                const mapped: Notification[] = data.map((n: any) => ({
                    ...n,
                    userId: user.id,
                    type: 'info' // Default type
                }));
                setNotifications(mapped);
            }
        } catch (err) {
            console.error("Failed to fetch notifications", err);
        }
    };

    const fetchUserProfile = async () => {
        if (!user?.id) return;
        try {
            // Use FitIT App ID
            const profileData = await UserService.getProfile(user.id, APP_ID);
            if (profileData) {
                // Determine if we need to update Auth Context (if it supports it)
                // For now just relying on profile data if needed locally
            }
        } catch (err) {
            // Ignore
        }
    };

    const handleToggleTheme = () => {
        const newTheme = theme === 'dark' ? 'light' : 'dark';
        setTheme(newTheme);
        updatePreferences({ theme: newTheme });
    };

    const markAsRead = async (id: string) => {
        try {
            await NotificationService.markAsRead(id);
            setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
        } catch (err) { console.error(err); }
    };

    const markAllRead = async () => {
        if (!user?.id) return;
        try {
            await NotificationService.markAllAsRead(user.id);
            setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
        } catch (err) { console.error(err); }
    };

    const getInitials = (name: string) => {
        return name
            .split(' ')
            .filter(n => n)
            .map(n => n[0])
            .slice(0, 2)
            .join('')
            .toUpperCase();
    };

    return (
        <>
            <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-border bg-card px-6 shadow-sm">
                <div className="flex items-center gap-2 font-bold text-xl text-primary">
                    <Activity className="h-6 w-6" />
                    <span className="hidden md:inline-block">FitIT Admin</span>
                </div>

                <div className="flex items-center gap-2">
                    <button
                        onClick={handleToggleTheme}
                        className="flex h-9 w-9 items-center justify-center rounded-full text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
                    >
                        {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
                    </button>

                    <button
                        onClick={() => setIsNotificationOpen(true)}
                        className="relative flex h-9 w-9 items-center justify-center rounded-full text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
                    >
                        <Bell size={20} />
                        {unreadCount > 0 && (
                            <span className="absolute top-1.5 right-1.5 h-2 w-2 rounded-full bg-destructive border border-card"></span>
                        )}
                    </button>

                    <div className="relative ml-2">
                        <button
                            onClick={() => setProfileOpen(!profileOpen)}
                            className="flex items-center gap-3 hover:opacity-80 transition-opacity"
                        >
                            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground border border-border">
                                {getInitials(user?.email || 'Admin')}
                            </div>
                        </button>

                        {profileOpen && (
                            <>
                                <div className="fixed inset-0 z-40" onClick={() => setProfileOpen(false)} />
                                <div className="absolute right-0 top-full mt-2 w-56 rounded-md border border-border bg-popover p-1 shadow-md animate-in fade-in zoom-in-95 z-50">
                                    <div className="px-2 py-1.5 text-xs text-muted-foreground font-medium border-b border-border mb-1">
                                        {user?.email}
                                    </div>
                                    <Link
                                        to="/settings"
                                        onClick={() => setProfileOpen(false)}
                                        className="flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none hover:bg-accent hover:text-accent-foreground"
                                    >
                                        <User size={16} className="text-muted-foreground" /> Settings
                                    </Link>
                                    <div className="my-1 h-px bg-muted" />
                                    <button
                                        onClick={() => { setProfileOpen(false); logout(); }}
                                        className="flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm text-destructive outline-none hover:bg-destructive/10 hover:text-destructive"
                                    >
                                        <LogOut size={16} /> Sign Out
                                    </button>
                                </div>
                            </>
                        )}
                    </div>
                </div>
            </header>

            <NotificationDrawer
                isOpen={isNotificationOpen}
                onClose={() => setIsNotificationOpen(false)}
                notifications={notifications}
                onMarkRead={markAsRead}
                onMarkAllRead={markAllRead}
            />
        </>
    );
};

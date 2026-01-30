import React, { useState, useEffect } from 'react';
import { UI_DEFAULTS } from '../config/uiDefaults';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Sidebar } from '../components/Sidebar';
import { useTheme } from '../context/ThemeContext';
import { NotificationDrawer } from '../components/NotificationDrawer';
import { Notification } from '../types/notification';
import axios from 'axios';
import {
    Bell, Moon, Sun, LogOut, User
} from 'lucide-react';
import { cn } from '../lib/utils';
import { UserService } from '../services/userService';

const DashboardLayout = () => {
    const { user, logout, updateUser } = useAuth();
    // Initialize from Config
    const [collapsed, setCollapsed] = useState(UI_DEFAULTS.SIDEBAR_COLLAPSED);
    const [profileOpen, setProfileOpen] = useState(false);


    // Theme Context
    const { isDark, toggleTheme, setTheme } = useTheme();
    const [profile, setProfile] = useState<any>(null); // For storing full profile data

    // Notification State
    const [isNotificationOpen, setIsNotificationOpen] = useState(false);
    const [notifications, setNotifications] = useState<Notification[]>([]);

    // Derived State
    const unreadCount = Array.isArray(notifications) ? notifications.filter(n => !n.isRead).length : 0;

    useEffect(() => {
        // Fetch Notifications
        fetchNotifications();

        // Fetch User Profile & Preferences
        fetchUserProfile();

        // Poll every minute
        const interval = setInterval(fetchNotifications, 60000);
        return () => clearInterval(interval);
    }, []);

    const fetchNotifications = async () => {
        try {
            // Gateway routes /notifications/* to Notifications Service
            // Service endpoint is /api/notifications
            const res = await axios.get('/notifications/api/notifications');
            if (Array.isArray(res.data)) {
                setNotifications(res.data);
            } else {
                setNotifications([]);
                console.error("Notifications API returned non-array:", res.data);
            }
        } catch (err) {
            console.error("Failed to fetch notifications", err);
            setNotifications([]); // Safe fallback
        }
    };

    const fetchUserProfile = async () => {
        if (!user?.id) return;
        try {
            // Fetch latest profile from Users Service
            const profileData = await UserService.getProfile(user.id);
            if (profileData) {
                // Update Global Auth State so header reflects latest name/avatar
                if (updateUser) {
                    updateUser({
                        name: profileData.displayName,
                        avatarUrl: profileData.avatarUrl,
                        phone: user?.phone || ''
                    });
                }
            }
        } catch (err) {
            console.log("Profile fetch failed", err);
        }
    };

    const savePreferences = async (newCollapsed: boolean, newDark: boolean) => {
        try {
            // Construct preference object
            const prefs = { collapsed: newCollapsed, theme: newDark ? 'dark' : 'light' };
            console.log("Saving prefs:", prefs);
            // Logic to call backend:
            // await axios.put('/api/users/profile', { ...profile, customDataJson: JSON.stringify(prefs) });
        } catch (err) {
            console.error("Failed to save prefs", err);
        }
    };

    // Wrappers
    const handleSetCollapsed = (val: boolean) => {
        setCollapsed(val);
        savePreferences(val, isDark);
    };

    const handleToggleTheme = () => {
        const newDark = !isDark;
        setTheme(newDark); // Update Global State (Context handles DOM)
        savePreferences(collapsed, newDark);
    };

    const markAsRead = async (id: string) => {
        try {
            await axios.put(`/notifications/api/notifications/${id}/read`);
            setNotifications(prev => Array.isArray(prev) ? prev.map(n => n.id === id ? { ...n, isRead: true } : n) : []);
        } catch (err) {
            console.error("Failed to mark read", err);
        }
    };

    const markAllRead = async () => {
        try {
            await axios.put(`/notifications/api/notifications/read-all`);
            setNotifications(prev => Array.isArray(prev) ? prev.map(n => ({ ...n, isRead: true })) : []);
        } catch (err) {
            console.error("Failed to mark all read", err);
        }
    };

    // User Avatar Initials Logic
    const getInitials = (name: string) => {
        return name
            .split(' ')
            .filter(n => n) // Filter empty parts
            .map(n => n[0])
            .slice(0, 2)
            .join('')
            .toUpperCase();
    };

    return (
        <div className="flex min-h-screen bg-background">
            <Sidebar collapsed={collapsed} setCollapsed={handleSetCollapsed} />

            <NotificationDrawer
                isOpen={isNotificationOpen}
                onClose={() => setIsNotificationOpen(false)}
                notifications={notifications}
                onMarkRead={markAsRead}
                onMarkAllRead={markAllRead}
            />

            {/* Main Content Wrapper */}
            <div className={cn(
                "flex flex-1 flex-col overflow-hidden transition-all duration-300",
                collapsed ? "ml-[80px]" : "ml-[260px]"
            )}>

                {/* Header */}
                <header className="flex h-16 items-center justify-between border-b border-border bg-card px-8 shadow-sm">
                    <h2 className="text-xl font-semibold tracking-tight">Overview</h2>

                    <div className="flex items-center gap-2">
                        <button
                            onClick={handleToggleTheme}
                            className="flex h-9 w-9 items-center justify-center rounded-full text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
                        >
                            {isDark ? <Sun size={20} /> : <Moon size={20} />}
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

                        {/* Profile Dropdown */}
                        <div className="relative ml-2">
                            <button
                                onClick={() => setProfileOpen(!profileOpen)}
                                className="flex items-center gap-3 hover:opacity-80 transition-opacity"
                            >
                                {user?.avatarUrl ? (
                                    <img
                                        src={user.avatarUrl}
                                        alt="Profile"
                                        className="h-9 w-9 rounded-full object-cover border border-blue-500 dark:border-gray-700"
                                    />
                                ) : (
                                    <div
                                        key={user?.name}
                                        className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground border border-blue-500 dark:border-gray-700"
                                    >
                                        {getInitials(user?.name || 'Admin User')}
                                    </div>
                                )}

                                <div className="text-left leading-tight hidden sm:block">
                                    <div className="text-sm font-semibold">{user?.name || 'Super Admin'}</div>
                                </div>
                            </button>

                            {profileOpen && (
                                <>
                                    <div
                                        className="fixed inset-0 z-40"
                                        onClick={() => setProfileOpen(false)}
                                    />
                                    <div className="absolute right-0 top-full mt-2 w-56 rounded-md border border-border bg-popover p-1 shadow-md animate-in fade-in zoom-in-95 z-50">
                                        <div className="px-2 py-1.5 text-xs text-muted-foreground font-medium border-b border-border mb-1">
                                            {user?.email || 'admin@globaldashboard.com'}
                                        </div>
                                        <Link
                                            to="/preferences"
                                            onClick={() => setProfileOpen(false)}
                                            className="flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none hover:bg-accent hover:text-accent-foreground"
                                        >
                                            <User size={16} className="text-muted-foreground" /> Preferences
                                        </Link>
                                        <div className="my-1 h-px bg-muted" />
                                        <button
                                            onClick={() => {
                                                setProfileOpen(false);
                                                logout();
                                            }}
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

                {/* Page Content */}
                <main className="flex-1 overflow-y-auto bg-muted/40 p-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};

export default DashboardLayout;

import React, { useState, useEffect } from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Sidebar } from '../components/Sidebar';
import { useTheme } from '../components/theme-provider';
import { useUserPreferences } from '../context/UserPreferencesContext';
import { NotificationDrawer } from '../components/NotificationDrawer';
import type { Notification } from '../types/notification';
import { NotificationService } from '../services/NotificationService';
import { UserService } from '../services/userService';
import { Bell, Moon, Sun, LogOut, User } from 'lucide-react';
import { cn } from '../lib/utils';
import { APP_ID } from '../config';

const DashboardLayout = () => {
    const { user, logout } = useAuth();
    const { preferences, updatePreferences } = useUserPreferences();
    const collapsed = preferences.sidebarCollapsed;
    const { theme, setTheme } = useTheme();

    const [profileOpen, setProfileOpen] = useState(false);
    const [isNotificationOpen, setIsNotificationOpen] = useState(false);
    const [notifications, setNotifications] = useState<Notification[]>([]);

    const unreadCount = Array.isArray(notifications) ? notifications.filter(n => !n.isRead).length : 0;

    useEffect(() => {
        fetchNotifications();
        fetchUserProfile();
        const interval = setInterval(fetchNotifications, 60000);
        return () => clearInterval(interval);
    }, []);

    const fetchNotifications = async () => {
        try {
            if (user?.id) {
                const data = await NotificationService.getMyNotifications(user.id);
                const mapped: Notification[] = data.map((n: any) => ({
                    ...n,
                    userId: user.id,
                    type: 'info'
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
            await UserService.getProfile(user.id, APP_ID);
        } catch (err) { }
    };

    const handleSetCollapsed = (val: boolean) => {
        updatePreferences({ sidebarCollapsed: val });
    };

    const handleToggleTheme = () => {
        const newDark = theme !== 'dark';
        updatePreferences({ theme: newDark ? 'dark' : 'light' });
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
        <div className="flex min-h-screen bg-background text-foreground">
            <Sidebar collapsed={collapsed} setCollapsed={handleSetCollapsed} />

            <NotificationDrawer
                isOpen={isNotificationOpen}
                onClose={() => setIsNotificationOpen(false)}
                notifications={notifications}
                onMarkRead={markAsRead}
                onMarkAllRead={markAllRead}
            />

            <div className={cn(
                "flex flex-1 flex-col overflow-hidden transition-all duration-300",
                collapsed ? "ml-[80px]" : "ml-[260px]"
            )}>
                {/* Header built in */}
                <header className="flex h-16 items-center justify-between border-b border-border bg-card px-8 shadow-sm">
                    <h2 className="text-xl font-semibold tracking-tight">Wissler Admin Data Overview</h2>

                    <div className="flex items-center gap-2">

                        <button
                            onClick={handleToggleTheme}
                            className="relative flex h-9 w-9 items-center justify-center rounded-full text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
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
                                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground border border-blue-500 dark:border-gray-700">
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

                <main className="flex-1 overflow-y-auto bg-muted/40 p-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};

export default DashboardLayout;

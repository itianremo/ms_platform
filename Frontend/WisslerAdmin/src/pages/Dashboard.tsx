import React, { useEffect, useState } from 'react';
import { Users, Activity, CreditCard, Bell, TrendingUp, AlertCircle, Server, Database, HardDrive, UserPlus, FileText, MessageSquare } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { cn } from '../lib/utils';
import { AnalyticsService, type AppUserStats } from '../services/AnalyticsService';
import { AppService } from '../services/appService';
import { AuditService, type AuditLog } from '../services/AuditService';
import { NotificationService, type Notification } from '../services/NotificationService';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";

// Mock Auth Hook if missing, or use real one if available
import { useAuth } from '../context/AuthContext';

export default function Dashboard() {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState({
        totalUsers: 0,
        activeUsers: 0,
        newUsersLast24h: 0,
        pendingAlerts: 0,
        appUserStats: [] as AppUserStats[]
    });
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [notifications, setNotifications] = useState<Notification[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                // Fetch independent data in parallel where possible, but handle failures gracefully
                const [dashboardStats, appStats, auditLogs, notifsResult] = await Promise.allSettled([
                    AnalyticsService.getDashboardStats(),
                    AnalyticsService.getAppUserStats(),
                    AuditService.getLogs(),
                    user ? NotificationService.getMyNotifications(user.id) : Promise.resolve([])
                ]);

                // Fetch Apps separately to ensure it doesn't block critical stats if it fails
                let appsData: any[] = [];
                try {
                    appsData = await AppService.getAllApps();
                } catch (e) {
                    console.error("Failed to load apps list", e);
                }

                // Process Results
                const totalUsers = dashboardStats.status === 'fulfilled' ? dashboardStats.value.totalUsers : 0;
                const activeUsers = dashboardStats.status === 'fulfilled' ? dashboardStats.value.activeUsers : 0;
                const newUsersLast24h = dashboardStats.status === 'fulfilled' ? dashboardStats.value.newUsersLast24h : 0;

                const rawAppStats = appStats.status === 'fulfilled' ? appStats.value : [];
                const logsData = auditLogs.status === 'fulfilled' ? auditLogs.value : [];
                const notifs = notifsResult.status === 'fulfilled' ? notifsResult.value : [];

                // Map IDs to Names for App Stats
                const mappedAppStats = rawAppStats.map(stat => {
                    const app = appsData.find(a => a.id && stat.appId && a.id.toLowerCase() === stat.appId.toLowerCase());
                    return {
                        ...stat,
                        appName: app ? app.name : stat.appName || `App ${stat.appId.substring(0, 8)}`
                    };
                });

                setStats({
                    totalUsers,
                    activeUsers,
                    newUsersLast24h,
                    pendingAlerts: notifs.filter(n => !n.isRead).length,
                    appUserStats: mappedAppStats
                });

                setLogs(logsData.slice(0, 5));
                setNotifications(notifs.slice(0, 5));

            } catch (error) {
                console.error("Critical failure loading dashboard data", error);
            } finally {
                setLoading(false);
            }
        };

        if (user) {
            fetchData();
        }
    }, [user]);

    // Permission Logic
    const hasRole = (roleName: string) => {
        if (!user || !user.roles) return false;
        return user.roles.includes('SuperAdmin') || user.roles.includes(roleName);
    };

    const isAdmin = hasRole('SuperAdmin') || hasRole('FitITAdmin') || hasRole('WisslerAdmin');
    const canViewStats = isAdmin || hasRole('ManageUsers');

    if (!canViewStats) {
        return (
            <div className="p-8 space-y-8 animate-fade-in flex flex-col items-center justify-center h-[80vh]">
                <div className="bg-muted p-8 rounded-lg text-center max-w-md">
                    <AlertCircle className="h-12 w-12 text-yellow-500 mx-auto mb-4" />
                    <h2 className="text-2xl font-bold mb-2">Access Restricted</h2>
                    <p className="text-muted-foreground mb-6">
                        You do not have permission to view the dashboard statistics.
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between space-y-2">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">
                        Overview of your system performance and activity.
                    </p>
                </div>
            </div>

            {/* Stat Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Total Users</CardTitle>
                        <Users className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : stats.totalUsers.toLocaleString()}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <TrendingUp className="mr-1 h-3 w-3 text-emerald-500" />
                            <span className="text-emerald-500">Live Count</span>
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Active Users</CardTitle>
                        <Activity className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : stats.activeUsers.toLocaleString()}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <span className="text-emerald-500">Online now</span>
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">New Users (24h)</CardTitle>
                        <UserPlus className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : stats.newUsersLast24h}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <span className="text-blue-500">Fast Growth</span>
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Pending Alerts</CardTitle>
                        <Bell className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : stats.pendingAlerts}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <span className="text-amber-500">Unread notifications</span>
                        </p>
                    </CardContent>
                </Card>
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-7 gap-4">
                <Card className="col-span-1 md:col-span-2 lg:col-span-4">
                    <CardHeader>
                        <CardTitle>User Role Distribution</CardTitle>
                        <CardDescription>
                            Admins vs Visitors per Application
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="h-[350px]">
                        {loading ? (
                            <div className="flex h-full items-center justify-center">Loading...</div>
                        ) : (
                            <ResponsiveContainer width="100%" height="100%">
                                <BarChart data={stats.appUserStats}>
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} />
                                    <XAxis dataKey="appName" stroke="#888888" fontSize={12} tickLine={false} axisLine={false} />
                                    <YAxis stroke="#888888" fontSize={12} tickLine={false} axisLine={false} tickFormatter={(value) => `${value}`} />
                                    <Tooltip
                                        cursor={{ fill: 'transparent' }}
                                        contentStyle={{ backgroundColor: 'white', borderRadius: '8px', border: '1px solid #e5e7eb', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                                    />
                                    <Legend />
                                    <Bar dataKey="visitorCount" name="Visitors" fill="#3b82f6" radius={[4, 4, 0, 0]} stackId="a" />
                                    <Bar dataKey="adminCount" name="Admins" fill="#f59e0b" radius={[4, 4, 0, 0]} stackId="a" />
                                </BarChart>
                            </ResponsiveContainer>
                        )}
                    </CardContent>
                </Card>

                {/* Recent Activity & Notifications Column */}
                <div className="col-span-1 md:col-span-2 lg:col-span-3 flex flex-col gap-4 h-[350px]">
                    {/* Recent Activity */}
                    <Card className="flex-1 overflow-hidden flex flex-col">
                        <CardHeader className="py-3">
                            <CardTitle className="text-base flex items-center">
                                <FileText className="mr-2 h-4 w-4 text-muted-foreground" />
                                Recent Activity
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="overflow-y-auto flex-1 p-0 px-6 pb-2">
                            <div className="space-y-4 pt-2">
                                {logs.length === 0 ? (
                                    <p className="text-sm text-muted-foreground text-center py-4">No recent activity.</p>
                                ) : (
                                    logs.map((log, i) => (
                                        <div key={i} className="flex items-start space-x-2 pb-2 border-b last:border-0 last:pb-0">
                                            <div className="w-2 h-2 mt-1.5 rounded-full bg-blue-500 shrink-0" />
                                            <div className="space-y-0.5">
                                                <p className="text-sm font-medium leading-none">{log.action}</p>
                                                <p className="text-xs text-muted-foreground">
                                                    {log.entityName} • {new Date(log.timestamp).toLocaleTimeString()}
                                                </p>
                                            </div>
                                        </div>
                                    ))
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Notifications */}
                    <Card className="flex-1 overflow-hidden flex flex-col">
                        <CardHeader className="py-3">
                            <CardTitle className="text-base flex items-center">
                                <Bell className="mr-2 h-4 w-4 text-muted-foreground" />
                                Recent Notifications
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="overflow-y-auto flex-1 p-0 px-6 pb-2">
                            <div className="space-y-4 pt-2">
                                {notifications.length === 0 ? (
                                    <p className="text-sm text-muted-foreground text-center py-4">No new notifications.</p>
                                ) : (
                                    notifications.map((notif, i) => (
                                        <div key={i} className="flex items-start space-x-2 pb-2 border-b last:border-0 last:pb-0">
                                            <div className={`w-2 h-2 mt-1.5 rounded-full ${notif.isRead ? 'bg-gray-300' : 'bg-red-500'} shrink-0`} />
                                            <div className="space-y-0.5 w-full">
                                                <div className="flex justify-between">
                                                    <p className="text-sm font-medium leading-none truncate">{notif.title}</p>
                                                    <span className="text-[10px] text-muted-foreground">{new Date(notif.createdAt).toLocaleDateString()}</span>
                                                </div>
                                                <p className="text-xs text-muted-foreground line-clamp-1">
                                                    {notif.message}
                                                </p>
                                            </div>
                                        </div>
                                    ))
                                )}
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}

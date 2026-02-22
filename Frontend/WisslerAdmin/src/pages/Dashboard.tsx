import React, { useEffect, useState } from 'react';
import { Users, Activity, CreditCard, Bell, TrendingUp, AlertCircle, Server, Database, HardDrive, UserPlus, FileText, MessageSquare, Heart } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { cn } from '../lib/utils';
import { AnalyticsService, type AppUserStats, type RevenueStats } from '../services/AnalyticsService';
import { AppService } from '../services/appService';
import { AuditService, type AuditLog } from '../services/AuditService';
import { NotificationService, type Notification } from '../services/NotificationService';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, PieChart, Pie, Cell } from 'recharts';
import { APP_ID } from '../config';
import { Avatar, AvatarFallback, AvatarImage } from "../components/ui/avatar";

// Mock Auth Hook if missing, or use real one if available
import { useAuth } from '../context/AuthContext';

export default function Dashboard() {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [startDate, setStartDate] = useState<string>('');
    const [endDate, setEndDate] = useState<string>('');
    const [stats, setStats] = useState({
        totalUsers: 0,
        activeUsers: 0,
        revenue: 0,
        chartData: [] as { date: string; amount: number }[],
        pendingAlerts: 0,
        appUserStats: [] as AppUserStats[],
        totalMatches: 0,
        demographics: [] as { region: string; count: number }[]
    });
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [notifications, setNotifications] = useState<Notification[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                // Fetch independent data in parallel where possible, but handle failures gracefully
                const [dashboardStats, appStats, auditLogsResult, notifsResult, revenueResult] = await Promise.allSettled([
                    AnalyticsService.getDashboardStats(startDate || undefined, endDate || undefined),
                    AnalyticsService.getAppUserStats(),
                    AuditService.getLogs(1, 10),
                    user ? NotificationService.getMyNotifications(user.id) : Promise.resolve([]),
                    AnalyticsService.getRevenueStats(startDate || undefined, endDate || undefined)
                ]);

                // Fetch Apps separately to ensure it doesn't block critical stats if it fails
                let appsData: any[] = [];
                try {
                    appsData = await AppService.getAllApps();
                } catch (e) {
                    console.error("Failed to load apps list", e);
                }

                // Process Results
                const finalTotalUsers = dashboardStats.status === 'fulfilled' ? dashboardStats.value.totalUsers : 0;
                const finalActiveUsers = dashboardStats.status === 'fulfilled' ? dashboardStats.value.activeUsers : 0;
                const revenue = revenueResult.status === 'fulfilled' ? revenueResult.value.currentMonthRevenue : 0;
                const chartData = revenueResult.status === 'fulfilled' ? revenueResult.value.chartData : [];

                const rawAppStats = appStats.status === 'fulfilled' ? appStats.value : [];
                // Handle PagedResult
                const logsData = auditLogsResult.status === 'fulfilled' ? auditLogsResult.value.items : [];
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
                    totalUsers: Math.max(finalTotalUsers, rawAppStats.find(s => s.appId.toLowerCase() === APP_ID.toLowerCase())?.visitorCount || 0),
                    activeUsers: finalActiveUsers,
                    revenue: revenue,
                    chartData: chartData,
                    pendingAlerts: notifs.filter(n => !n.isRead).length,
                    appUserStats: mappedAppStats,
                    totalMatches: dashboardStats.status === 'fulfilled' ? dashboardStats.value.totalMatches : 0,
                    demographics: dashboardStats.status === 'fulfilled' ? dashboardStats.value.demographics || [] : []
                });

                setLogs(logsData);
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
    }, [user, startDate, endDate]);

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
            <div className="flex flex-col sm:flex-row sm:items-center justify-between space-y-2 sm:space-y-0 pb-4">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
                    <p className="text-muted-foreground">
                        Overview of your system performance and activity.
                    </p>
                </div>
                <div className="flex flex-wrap items-center gap-3">
                    <div className="flex flex-col">
                        <label className="text-xs text-muted-foreground mb-1">Start Date</label>
                        <input
                            type="date"
                            className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                        />
                    </div>
                    <div className="flex flex-col">
                        <label className="text-xs text-muted-foreground mb-1">End Date</label>
                        <input
                            type="date"
                            className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                        />
                    </div>
                    {(startDate || endDate) && (
                        <div className="flex flex-col justify-end h-full mt-[18px]">
                            <Button variant="ghost" size="sm" onClick={() => { setStartDate(''); setEndDate(''); }}>Clear</Button>
                        </div>
                    )}
                </div>
            </div>

            {/* Stat Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <Link to="/users" className="block focus:outline-none">
                    <Card className="hover:border-primary/50 transition-colors cursor-pointer h-full">
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
                </Link>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Active Users</CardTitle>
                        <Activity className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : stats.activeUsers.toLocaleString()}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <span className="text-emerald-500">Online today</span>
                        </p>
                    </CardContent>
                </Card>

                <Link to="/matches" className="block focus:outline-none">
                    <Card className="hover:border-primary/50 transition-colors cursor-pointer h-full">
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-sm font-medium">Total Matches</CardTitle>
                            <Heart className="h-4 w-4 text-muted-foreground" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{loading ? "..." : stats.totalMatches.toLocaleString()}</div>
                            <p className="text-xs text-muted-foreground flex items-center mt-1">
                                <TrendingUp className="mr-1 h-3 w-3 text-emerald-500" />
                                <span className="text-emerald-500">+12% this month</span>
                            </p>
                        </CardContent>
                    </Card>
                </Link>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Current Month Revenue</CardTitle>
                        <CreditCard className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{loading ? "..." : `$${stats.revenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`}</div>
                        <p className="text-xs text-muted-foreground flex items-center mt-1">
                            <span className="text-emerald-500">From successful payments</span>
                        </p>
                    </CardContent>
                </Card>
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-7 gap-4">
                <div className="col-span-1 md:col-span-2 lg:col-span-4 flex flex-col gap-4">

                    <Card className="flex-1">
                        <CardHeader>
                            <CardTitle>Profile Demographics</CardTitle>
                            <CardDescription>Distribution of profiles by region</CardDescription>
                        </CardHeader>
                        <CardContent className="h-[250px]">
                            <ResponsiveContainer width="100%" height="100%">
                                <PieChart>
                                    <Pie
                                        data={stats.demographics.length > 0 ? stats.demographics.map(d => ({ name: d.region, value: d.count })) : [{ name: 'No Demographics Data', value: 1 }]}
                                        cx="50%"
                                        cy="50%"
                                        innerRadius={60}
                                        outerRadius={80}
                                        paddingAngle={5}
                                        dataKey="value"
                                    >
                                        {stats.demographics.length > 0
                                            ? stats.demographics.map((entry, index) => <Cell key={`cell-${index}`} fill={['#3b82f6', '#10b981', '#f59e0b', '#6366f1', '#ec4899', '#8b5cf6'][index % 6]} />)
                                            : <Cell fill="#cccccc" />
                                        }
                                    </Pie>
                                    <Tooltip contentStyle={{ borderRadius: '8px' }} />
                                    <Legend />
                                </PieChart>
                            </ResponsiveContainer>
                        </CardContent>
                    </Card>
                </div>

                {/* Recent Activity & Notifications Column */}
                <div className="col-span-1 md:col-span-2 lg:col-span-3 flex flex-col gap-4 h-[350px]">
                    {/* Recent Audit Logs */}
                    <Card className="flex-1 overflow-hidden flex flex-col">
                        <CardHeader className="py-3 flex flex-row items-center justify-between">
                            <CardTitle className="text-base flex items-center">
                                <FileText className="mr-2 h-4 w-4 text-muted-foreground" />
                                Recent Audit Logs
                            </CardTitle>
                            <Link to="/audit-logs" className="text-xs text-primary hover:underline">
                                Show All
                            </Link>
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
                                                <p className="text-xs text-muted-foreground font-mono">
                                                    {log.entityName} • {new Date(log.timestamp).toISOString().replace('T', ' ').replace('Z', '')}
                                                </p>
                                            </div>
                                        </div>
                                    ))
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Payments Chart */}
                    <Card className="flex-1 overflow-hidden flex flex-col">
                        <CardHeader className="py-3">
                            <CardTitle className="text-base flex items-center">
                                <CreditCard className="mr-2 h-4 w-4 text-muted-foreground" />
                                Payment Activity
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="overflow-hidden flex-1 p-0 px-2 pb-2 h-[140px]">
                            {stats.chartData.length === 0 ? (
                                <p className="text-sm text-muted-foreground text-center py-4">No recent payments.</p>
                            ) : (
                                <ResponsiveContainer width="100%" height="100%">
                                    <BarChart data={stats.chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                                        <XAxis dataKey="date" tick={{ fontSize: 10 }} tickLine={false} axisLine={false} />
                                        <YAxis tick={{ fontSize: 10 }} tickLine={false} axisLine={false} tickFormatter={(val) => `$${val}`} />
                                        <Tooltip cursor={{ fill: 'transparent' }} contentStyle={{ borderRadius: '8px', fontSize: '12px' }} />
                                        <Bar dataKey="amount" fill="#10b981" radius={[4, 4, 0, 0]} />
                                    </BarChart>
                                </ResponsiveContainer>
                            )}
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}

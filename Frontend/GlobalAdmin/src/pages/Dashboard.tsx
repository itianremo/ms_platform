import React, { useEffect, useState } from 'react';
import { Users, Activity, CreditCard, Bell, TrendingUp, AlertCircle, Server, Database, HardDrive, UserPlus } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { cn } from '../lib/utils';
import { AnalyticsService } from '../services/AnalyticsService';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function Dashboard() {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState({
        totalUsers: 0,
        activeUsers: 0,
        newUsersLast24h: 0,
        revenue: '$0', // Still mocked
        pendingAlerts: 0, // Still mocked
        usersPerApp: [] as { appName: string; count: number }[]
    });

    useEffect(() => {
        const fetchStats = async () => {
            try {
                const data = await AnalyticsService.getDashboardStats();
                setStats({
                    totalUsers: data.totalUsers,
                    activeUsers: data.activeUsers,
                    newUsersLast24h: data.newUsersLast24h,
                    revenue: '$0', // Keep mock
                    pendingAlerts: 0, // Keep mock
                    usersPerApp: data.usersPerApp || []
                });
            } catch (error) {
                console.error("Failed to load dashboard stats", error);
            } finally {
                setLoading(false);
            }
        };
        fetchStats();
    }, []);

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
                        You do not have permission to view the global dashboard statistics.
                        Please contact your system administrator to request access.
                    </p>
                    <div className="text-sm text-muted-foreground bg-background p-4 rounded border">
                        <p>User ID: {user?.id}</p>
                        <p>Email: {user?.email}</p>
                        <p>Roles: {user?.roles?.join(', ') || 'None'}</p>
                    </div>
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

            {/* Bento Grid Layout */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {/* Stat Cards */}
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
                            <span className="text-emerald-500">Currently Active</span>
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
                            <span className="text-amber-500">Requires attention</span>
                        </p>
                    </CardContent>
                </Card>

                {/* Second Row: Charts (Span 2) & Quick Actions */}
                <Card className="col-span-1 md:col-span-2 lg:col-span-3">
                    <CardHeader>
                        <CardTitle>User Distribution</CardTitle>
                        <CardDescription>
                            Users per application.
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="h-[300px]">
                        {loading ? (
                            <div className="flex h-full items-center justify-center">Loading...</div>
                        ) : stats.usersPerApp.length === 0 ? (
                            <div className="flex h-full items-center justify-center text-muted-foreground">No data available</div>
                        ) : (
                            <ResponsiveContainer width="100%" height="100%">
                                <BarChart data={stats.usersPerApp}>
                                    <CartesianGrid strokeDasharray="3 3" />
                                    <XAxis dataKey="appName" />
                                    <YAxis allowDecimals={false} />
                                    <Tooltip
                                        contentStyle={{ backgroundColor: 'white', borderRadius: '8px', border: '1px solid #e5e7eb' }}
                                    />
                                    <Bar dataKey="count" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                                </BarChart>
                            </ResponsiveContainer>
                        )}
                    </CardContent>
                </Card>

                {/* Right Column: Status & Actions */}
                <div className="grid grid-cols-1 gap-4 lg:col-span-1">
                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">System Health</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="space-y-1">
                                <div className="flex items-center justify-between text-xs">
                                    <span className="text-muted-foreground flex items-center"><Server className="w-3 h-3 mr-1" /> CPU</span>
                                    <span className="font-medium">45%</span>
                                </div>
                                <div className="h-1.5 w-full rounded-full bg-secondary">
                                    <div className="h-full rounded-full bg-blue-500 w-[45%]" />
                                </div>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center justify-between text-xs">
                                    <span className="text-muted-foreground flex items-center"><HardDrive className="w-3 h-3 mr-1" /> Memory</span>
                                    <span className="font-medium">62%</span>
                                </div>
                                <div className="h-1.5 w-full rounded-full bg-secondary">
                                    <div className="h-full rounded-full bg-purple-500 w-[62%]" />
                                </div>
                            </div>
                            <div className="space-y-1">
                                <div className="flex items-center justify-between text-xs">
                                    <span className="text-muted-foreground flex items-center"><Database className="w-3 h-3 mr-1" /> Storage</span>
                                    <span className="font-medium">28%</span>
                                </div>
                                <div className="h-1.5 w-full rounded-full bg-secondary">
                                    <div className="h-full rounded-full bg-emerald-500 w-[28%]" />
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">Quick Actions</CardTitle>
                        </CardHeader>
                        <CardContent className="grid grid-cols-2 gap-2">
                            <Button variant="outline" className="h-20 flex-col gap-2" asChild>
                                <Link to="/users">
                                    <UserPlus className="h-5 w-5 text-blue-500" />
                                    <span className="text-xs">Add User</span>
                                </Link>
                            </Button>
                            <Button variant="outline" className="h-20 flex-col gap-2" asChild>
                                <Link to="/sms-configs">
                                    <AlertCircle className="h-5 w-5 text-orange-500" />
                                    <span className="text-xs">SMS Config</span>
                                </Link>
                            </Button>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}

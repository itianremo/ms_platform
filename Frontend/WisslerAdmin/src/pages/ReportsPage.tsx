import React, { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../components/ui/table';
import { Search, Plus, Trash2, Eye, EyeOff, Activity, Users, AlertOctagon, TrendingUp, BarChart2 } from 'lucide-react';
import { Badge } from '../components/ui/badge';
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts';

type ReportReason = { id: string; title: string; isActive: boolean; usageCount: number };
type ReportedUser = { id: string; name: string; email: string; totalReports: number; lastReportDate: string };

const MOCK_REASONS: ReportReason[] = [
    { id: '1', title: 'Inappropriate Content', isActive: true, usageCount: 45 },
    { id: '2', title: 'Fake Profile', isActive: true, usageCount: 32 },
    { id: '3', title: 'Harassment', isActive: true, usageCount: 18 },
    { id: '4', title: 'Spam', isActive: false, usageCount: 0 }
];

const MOCK_USERS: ReportedUser[] = [
    { id: 'u1', name: 'John Doe', email: 'john@example.com', totalReports: 12, lastReportDate: '2026-02-21' },
    { id: 'u2', name: 'Jane Smith', email: 'jane@example.com', totalReports: 4, lastReportDate: '2026-02-19' },
    { id: 'u3', name: 'Mike Johnson', email: 'mike@example.com', totalReports: 1, lastReportDate: '2026-02-10' },
];

export default function ReportsPage() {
    const [activeTab, setActiveTab] = useState<'users' | 'reasons'>('users');
    const [reasons, setReasons] = useState<ReportReason[]>(MOCK_REASONS);
    const [users, setUsers] = useState<ReportedUser[]>(MOCK_USERS);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [sortCol, setSortCol] = useState<'totalReports' | 'lastReportDate' | 'name'>('totalReports');
    const [sortDesc, setSortDesc] = useState(true);

    // Sort logic
    const sortedUsers = [...users].sort((a, b) => {
        const valA: unknown = a[sortCol];
        const valB: unknown = b[sortCol];
        if (sortDesc) return (valA as number | string) < (valB as number | string) ? 1 : -1;
        return (valA as number | string) > (valB as number | string) ? 1 : -1;
    });

    const handleSort = (col: typeof sortCol) => {
        if (sortCol === col) setSortDesc(!sortDesc);
        else { setSortCol(col); setSortDesc(true); }
    };

    const toggleReasonActive = (id: string) => {
        setReasons(reasons.map(r => r.id === id ? { ...r, isActive: !r.isActive } : r));
    };

    const deleteReason = (id: string) => {
        // Only allow deleting unused reasons based on requirements
        const reason = reasons.find(r => r.id === id);
        if (reason && reason.usageCount === 0) {
            setReasons(reasons.filter(r => r.id !== id));
        } else {
            alert("Cannot delete a reason that has been used. You can deactivate it instead.");
        }
    };

    const COLORS = ['#ef4444', '#f59e0b', '#3b82f6', '#8b5cf6', '#10b981'];

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in max-w-7xl mx-auto">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Reported Users Module</h2>
                    <p className="text-muted-foreground">Manage user reports and specific report reasons.</p>
                </div>
                <div className="flex bg-muted p-1 rounded-md">
                    <button
                        onClick={() => setActiveTab('users')}
                        className={`px-4 py-2 text-sm font-medium rounded-sm transition-colors ${activeTab === 'users' ? 'bg-background shadow font-semibold' : 'text-muted-foreground hover:text-foreground'}`}
                    >
                        Reported Users
                    </button>
                    <button
                        onClick={() => setActiveTab('reasons')}
                        className={`px-4 py-2 text-sm font-medium rounded-sm transition-colors ${activeTab === 'reasons' ? 'bg-background shadow font-semibold' : 'text-muted-foreground hover:text-foreground'}`}
                    >
                        Manage Reasons
                    </button>
                </div>
            </div>

            {activeTab === 'users' && (
                <div className="space-y-6">
                    {/* Analytics Top Section */}
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <Card className="md:col-span-1">
                            <CardHeader className="pb-2">
                                <CardTitle className="text-base flex items-center gap-2"><AlertOctagon className="h-5 w-5 text-red-500" /> Total Reports</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="text-3xl font-bold text-red-600">95</div>
                                <p className="text-xs text-muted-foreground flex items-center mt-1">
                                    <TrendingUp className="mr-1 h-3 w-3" /> +14% this month
                                </p>
                            </CardContent>
                        </Card>
                        <Card className="md:col-span-2">
                            <CardHeader className="pb-2">
                                <CardTitle className="text-base flex items-center gap-2"><BarChart2 className="h-5 w-5 text-blue-500" /> Reports By Reason</CardTitle>
                            </CardHeader>
                            <CardContent className="h-[140px] flex items-center">
                                <ResponsiveContainer width="100%" height="100%">
                                    <PieChart>
                                        <Pie data={reasons.filter(r => r.usageCount > 0)} dataKey="usageCount" nameKey="title" cx="50%" cy="50%" outerRadius={50}>
                                            {reasons.map((entry, index) => (
                                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                            ))}
                                        </Pie>
                                        <Tooltip />
                                        <Legend verticalAlign="middle" align="right" layout="vertical" />
                                    </PieChart>
                                </ResponsiveContainer>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Paginated Table */}
                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle>Reported Profiles</CardTitle>
                            <div className="flex gap-2 items-center">
                                <span className="text-sm text-muted-foreground">Rows:</span>
                                <select
                                    className="border rounded p-1 text-sm bg-background"
                                    value={pageSize}
                                    onChange={(e) => setPageSize(Number(e.target.value))}
                                >
                                    <option value={10}>10</option>
                                    <option value={25}>25</option>
                                    <option value={50}>50</option>
                                    <option value={100}>100</option>
                                    <option value={99999}>All</option>
                                </select>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead className="cursor-pointer hover:bg-muted/50" onClick={() => handleSort('name')}>User {sortCol === 'name' && (sortDesc ? '↓' : '↑')}</TableHead>
                                        <TableHead className="cursor-pointer hover:bg-muted/50 text-center" onClick={() => handleSort('totalReports')}>Total Reports {sortCol === 'totalReports' && (sortDesc ? '↓' : '↑')}</TableHead>
                                        <TableHead className="cursor-pointer hover:bg-muted/50 text-right" onClick={() => handleSort('lastReportDate')}>Last Report {sortCol === 'lastReportDate' && (sortDesc ? '↓' : '↑')}</TableHead>
                                        <TableHead className="text-right">Action</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {sortedUsers.map(user => (
                                        <TableRow key={user.id}>
                                            <TableCell className="font-medium">
                                                {user.name} <br />
                                                <span className="text-xs text-muted-foreground font-normal">{user.email}</span>
                                            </TableCell>
                                            <TableCell className="text-center">
                                                <Badge variant="destructive" className="px-2 py-0">{user.totalReports}</Badge>
                                            </TableCell>
                                            <TableCell className="text-right text-sm text-muted-foreground">
                                                {user.lastReportDate}
                                            </TableCell>
                                            <TableCell className="text-right">
                                                <Button size="sm" variant="outline">View Reports</Button>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                    {users.length === 0 && (
                                        <TableRow><TableCell colSpan={4} className="text-center text-muted-foreground py-8">No reported users.</TableCell></TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        </CardContent>
                    </Card>
                </div>
            )}

            {activeTab === 'reasons' && (
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between pb-6">
                        <div>
                            <CardTitle>Global Report Reasons</CardTitle>
                            <p className="text-sm text-muted-foreground mt-1">Configure the reasons users can select when reporting others.</p>
                        </div>
                        <Button className="bg-emerald-600 hover:bg-emerald-700" onClick={() => alert('WIP: Open Add Reason Dialog')}><Plus className="h-4 w-4 mr-2" /> Add Reason</Button>
                    </CardHeader>
                    <CardContent>
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Reason Title</TableHead>
                                    <TableHead className="text-center">Usage Count</TableHead>
                                    <TableHead className="text-center">Status</TableHead>
                                    <TableHead className="text-right">Actions</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {reasons.map(reason => (
                                    <TableRow key={reason.id} className={!reason.isActive ? "opacity-60" : ""}>
                                        <TableCell className="font-medium">{reason.title}</TableCell>
                                        <TableCell className="text-center font-mono">{reason.usageCount}</TableCell>
                                        <TableCell className="text-center">
                                            {reason.isActive
                                                ? <Badge className="bg-green-100 text-green-800 hover:bg-green-100">Active</Badge>
                                                : <Badge variant="secondary">Hidden</Badge>
                                            }
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <Button size="sm" variant="outline" title="View Analytics" onClick={() => alert(`Showing analytics for: ${reason.title}`)}>
                                                    <Activity className="h-4 w-4 text-blue-500" />
                                                </Button>
                                                <Button size="sm" variant="outline" title={reason.isActive ? "Deactivate (Hide)" : "Activate (Show)"} onClick={() => toggleReasonActive(reason.id)}>
                                                    {reason.isActive ? <EyeOff className="h-4 w-4 text-amber-600" /> : <Eye className="h-4 w-4 text-green-600" />}
                                                </Button>
                                                <Button size="sm" variant="outline" title="Delete" onClick={() => deleteReason(reason.id)} disabled={reason.usageCount > 0}>
                                                    <Trash2 className="h-4 w-4 text-destructive" />
                                                </Button>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </CardContent>
                </Card>
            )}
        </div>
    );
}

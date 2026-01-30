import React, { useEffect, useState } from 'react';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../components/ui/table";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "../components/ui/card";
import { AuditService, AuditLog } from '../services/AuditService';
import { useToast } from '../context/ToastContext';
import { Loader2, FileJson, User, AppWindow } from 'lucide-react';
import { format } from 'date-fns';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "../components/ui/dialog";

const AuditLogsPage = () => {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
    const { showToast } = useToast();

    useEffect(() => {
        fetchLogs();
    }, []);

    const fetchLogs = async () => {
        try {
            const data = await AuditService.getLogs();
            setLogs(data);
        } catch (error) {
            showToast("Failed to fetch audit logs", "error");
        } finally {
            setLoading(false);
        }
    };

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <Loader2 className="animate-spin h-8 w-8 text-primary" />
        </div>
    );

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Audit Logs</h2>
                <p className="text-muted-foreground">View system activity and changes.</p>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>System Activity</CardTitle>
                    <CardDescription>
                        A chronological record of operations performed within the platform.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="w-[180px]">Timestamp</TableHead>
                                <TableHead>Action</TableHead>
                                <TableHead>Entity</TableHead>
                                <TableHead>User / App</TableHead>
                                <TableHead className="text-right">Details</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {logs.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="text-center h-24 text-muted-foreground">
                                        No audit logs found.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                logs.map((log) => (
                                    <TableRow key={log.id} className="group">
                                        <TableCell className="text-sm text-muted-foreground whitespace-nowrap">
                                            {format(new Date(log.timestamp), 'MMM d, yyyy HH:mm:ss')}
                                        </TableCell>
                                        <TableCell className="font-medium">
                                            {log.action}
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex flex-col">
                                                <span className="font-medium text-xs">{log.entityName}</span>
                                                <span className="text-xs text-muted-foreground">{log.entityId}</span>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex items-center gap-2 text-sm">
                                                {log.userId && (
                                                    <span className="flex items-center gap-1 bg-blue-50 text-blue-700 px-2 py-0.5 rounded text-xs dark:bg-blue-900/30 dark:text-blue-300">
                                                        <User size={12} /> User {log.userId}
                                                    </span>
                                                )}
                                                {log.appId && (
                                                    <span className="flex items-center gap-1 bg-purple-50 text-purple-700 px-2 py-0.5 rounded text-xs dark:bg-purple-900/30 dark:text-purple-300">
                                                        <AppWindow size={12} /> App {log.appId}
                                                    </span>
                                                )}
                                            </div>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <button
                                                onClick={() => setSelectedLog(log)}
                                                className="ghost-button p-2 hover:bg-muted rounded-full"
                                                title="View JSON Details"
                                            >
                                                <FileJson size={16} className="text-muted-foreground group-hover:text-primary" />
                                            </button>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>

            <Dialog open={!!selectedLog} onOpenChange={(open) => !open && setSelectedLog(null)}>
                <DialogContent className="max-w-2xl">
                    <DialogHeader>
                        <DialogTitle>Audit Detail</DialogTitle>
                        <DialogDescription>
                            Raw change data for request {selectedLog?.id}
                        </DialogDescription>
                    </DialogHeader>
                    <div className="bg-slate-950 text-slate-50 p-4 rounded-md overflow-x-auto text-xs font-mono max-h-[60vh] overflow-y-auto">
                        <pre>
                            {selectedLog && JSON.stringify(JSON.parse(selectedLog.changesJson || "{}"), null, 2)}
                        </pre>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default AuditLogsPage;

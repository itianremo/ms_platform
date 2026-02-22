import React, { useEffect, useState } from 'react';
import { FileText, AlertCircle, Search } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { AuditService, type AuditLog } from '../services/AuditService';

export default function AuditLogsPage() {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(20);

    const fetchLogs = async (currentPage: number) => {
        setLoading(true);
        try {
            const response = await AuditService.getLogs(currentPage, pageSize);
            if (response && Array.isArray(response.items)) {
                setLogs(response.items);
            } else if (Array.isArray(response)) {
                setLogs(response as any);
            } else {
                setLogs([]);
            }
        } catch (err: any) {
            console.error("Failed to load audit logs", err);
            setError(err.message || 'Failed to load audit logs');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLogs(page);
    }, [page]);

    if (error) {
        return (
            <div className="p-8 space-y-8 animate-fade-in flex flex-col items-center justify-center h-[80vh]">
                <div className="bg-destructive/10 p-8 rounded-lg text-center max-w-md border border-destructive">
                    <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
                    <h2 className="text-2xl font-bold mb-2 text-destructive">Error Loading Logs</h2>
                    <p className="text-muted-foreground mb-6">{error}</p>
                </div>
            </div>
        );
    }

    return (
        <div className="p-4 md:p-8 pt-6 space-y-8 animate-fade-in">
            <div className="flex items-center justify-between space-y-2">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Audit Logs</h2>
                    <p className="text-muted-foreground">
                        System-wide activity and security events.
                    </p>
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-lg flex items-center gap-2">
                        <FileText className="h-5 w-5 text-muted-foreground" />
                        Log Entries
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    {loading && logs.length === 0 ? (
                        <div className="py-8 text-center text-muted-foreground animate-pulse">Loading logs...</div>
                    ) : logs.length === 0 ? (
                        <div className="py-8 text-center text-muted-foreground">No audit logs found.</div>
                    ) : (
                        <div className="space-y-4">
                            {logs.map((log, i) => (
                                <div key={i} className="flex flex-col sm:flex-row sm:items-center justify-between p-4 rounded-lg border border-border bg-card/50 hover:bg-card transition-colors">
                                    <div className="flex items-start space-x-3">
                                        <div className="mt-1 h-2 w-2 rounded-full bg-blue-500 shrink-0" />
                                        <div className="space-y-1">
                                            <p className="text-sm font-semibold leading-none">{log.action}</p>
                                            <div className="flex items-center text-xs text-muted-foreground font-mono space-x-2">
                                                <span>{log.entityName}</span>
                                                {log.entityId && (
                                                    <>
                                                        <span>•</span>
                                                        <span>ID: {log.entityId.substring(0, 8)}...</span>
                                                    </>
                                                )}
                                                {log.userId && (
                                                    <>
                                                        <span>•</span>
                                                        <span>User: {log.userId.substring(0, 8)}...</span>
                                                    </>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                    <div className="mt-2 sm:mt-0 text-right">
                                        <p className="text-xs text-muted-foreground whitespace-nowrap">
                                            {new Date(log.timestamp).toLocaleString()}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    {/* Simple Pagination Controls */}
                    <div className="mt-6 flex items-center justify-between border-t py-4">
                        <button
                            disabled={page === 1 || loading}
                            onClick={() => setPage(p => Math.max(1, p - 1))}
                            className="px-3 py-1 bg-secondary text-secondary-foreground rounded-md text-sm disabled:opacity-50"
                        >
                            Previous
                        </button>
                        <span className="text-sm text-muted-foreground font-medium">Page {page}</span>
                        <button
                            disabled={logs.length < pageSize || loading}
                            onClick={() => setPage(p => p + 1)}
                            className="px-3 py-1 bg-secondary text-secondary-foreground rounded-md text-sm disabled:opacity-50"
                        >
                            Next
                        </button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

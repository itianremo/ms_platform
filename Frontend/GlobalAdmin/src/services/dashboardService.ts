import { UserService } from './userService';
import { AppService } from './appService';
import { AuditService } from './AuditService';

export interface DashboardStats {
    totalUsers: number;
    activeSessions: number;
    revenue: string;
    pendingAlerts: number;
    recentActivity: ActivityItem[];
}

export interface ActivityItem {
    id: string;
    title: string;
    time: string;
    type: 'info' | 'warning' | 'success';
}

export const DashboardService = {
    getStats: async (): Promise<DashboardStats> => {
        try {
            // Parallel fetch
            const [users, apps, logs] = await Promise.all([
                UserService.getAllUsers().catch(() => []),
                AppService.getAllApps().catch(() => []),
                AuditService.getLogs().catch(() => [])
            ]);

            // Map logs to Recent Activity
            // Take top 5 recent logs
            const recentActivity: ActivityItem[] = logs.slice(0, 5).map(log => ({
                id: log.id,
                title: `${log.action} on ${log.entityName} ${log.entityId || ''}`,
                time: new Date(log.timestamp).toLocaleString(), // Simple format for now, UI uses text
                type: 'info' // Default to info, logic could be refined
            }));

            // Mocked values for now where no service exists
            return {
                totalUsers: users.length,
                activeSessions: apps.filter(a => a.isActive).length, // Using Active Apps as proxy for now
                revenue: '$12,450', // Mock
                pendingAlerts: 3,   // Mock
                recentActivity
            };
        } catch (error) {
            console.error("Failed to aggregate dashboard stats", error);
            throw error;
        }
    }
};

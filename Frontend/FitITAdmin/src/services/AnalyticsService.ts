import api from './api';

export interface DashboardStats {
    totalUsers: number;
    activeUsers: number;
    newUsersLast24h: number;
    usersPerApp: { appName: string; count: number }[];
}

export const AnalyticsService = {
    getDashboardStats: async (): Promise<DashboardStats> => {
        try {
            // Gateway Proxy: /users -> Users.API
            const response = await api.get('/users/api/Users/dashboard/stats');
            return response.data;
        } catch (error) {
            console.error("Failed to fetch dashboard stats", error);
            // Return safe default
            return {
                totalUsers: 0,
                activeUsers: 0,
                newUsersLast24h: 0,
                usersPerApp: []
            };
        }
    },

    getAppUserStats: async (): Promise<AppUserStats[]> => {
        try {
            // Auth API: /api/Analytics/app-user-stats
            const response = await api.get('/auth/api/Analytics/app-user-stats');
            return response.data;
        } catch (error) {
            console.error("Failed to fetch app user stats", error);
            return [];
        }
    }
};

export interface AppUserStats {
    appId: string;
    appName: string;
    adminCount: number;
    visitorCount: number;
}

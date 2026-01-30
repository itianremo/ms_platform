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
            const response = await api.get('/users/api/Analytics/dashboard');
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
    }
};

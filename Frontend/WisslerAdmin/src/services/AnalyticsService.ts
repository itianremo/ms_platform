import api from './api';
import { APP_ID } from '../config';

export interface DashboardStats {
    totalUsers: number;
    activeUsers: number;
    newUsersLast24h: number;
    usersPerApp: { appName: string; count: number }[];
    totalMatches: number;
    demographics: { region: string; count: number }[];
}

export interface RevenueStats {
    currentMonthRevenue: number;
    chartData: { date: string; amount: number }[];
}

export const AnalyticsService = {
    getDashboardStats: async (startDate?: string, endDate?: string): Promise<DashboardStats> => {
        try {
            // Gateway Proxy: /users -> Users.API
            let url = `/users/api/Users/dashboard/stats?appId=${APP_ID}`;
            if (startDate) url += `&startDate=${startDate}`;
            if (endDate) url += `&endDate=${endDate}`;
            const response = await api.get(url);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch dashboard stats", error);
            // Return safe default
            return {
                totalUsers: 0,
                activeUsers: 0,
                newUsersLast24h: 0,
                usersPerApp: [],
                totalMatches: 0,
                demographics: []
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
    },

    getRevenueStats: async (startDate?: string, endDate?: string): Promise<RevenueStats> => {
        try {
            let url = `/payments/api/Payments/analytics`;
            const params = new URLSearchParams();
            if (startDate) params.append('startDate', startDate);
            if (endDate) params.append('endDate', endDate);

            if (params.toString()) {
                url += `?${params.toString()}`;
            }

            const response = await api.get(url);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch revenue stats", error);
            return {
                currentMonthRevenue: 0,
                chartData: []
            };
        }
    }
};

export interface AppUserStats {
    appId: string;
    appName: string;
    adminCount: number;
    visitorCount: number;
}

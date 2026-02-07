import api from './api';

export interface Notification {
    id: string;
    title: string;
    message: string;
    isRead: boolean;
    createdAt: string;
    link?: string;
}

export const NotificationService = {
    getMyNotifications: async (userId: string): Promise<Notification[]> => {
        // Backend expects X-User-Id header or query param. API interceptor might handle header?
        // Let's pass query param to be safe based on controller logic.
        const response = await api.get(`/notifications/api/Notifications?userId=${userId}`);
        return response.data;
    },

    getUnread: async (userId: string): Promise<Notification[]> => {
        const response = await api.get(`/notifications/api/Notifications/unread?userId=${userId}`);
        return response.data;
    },

    markAsRead: async (id: string): Promise<void> => {
        await api.put(`/notifications/api/Notifications/${id}/read`);
    },

    markAllAsRead: async (userId: string): Promise<void> => {
        await api.put(`/notifications/api/Notifications/read-all?userId=${userId}`);
    }
};

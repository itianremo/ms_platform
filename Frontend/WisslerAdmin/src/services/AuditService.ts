import api from './api';

export interface AuditLog {
    id: string;
    action: string;
    entityName: string;
    entityId: string;
    userId?: string;
    appId?: string;
    changesJson: string;
    timestamp: string;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export const AuditService = {
    getLogs: async (page: number = 1, pageSize: number = 25, appId?: string, userId?: string): Promise<PagedResult<AuditLog>> => {
        try {
            const params = new URLSearchParams();
            params.append('page', page.toString());
            params.append('pageSize', pageSize.toString());
            if (appId) params.append('appId', appId);
            if (userId) params.append('userId', userId);

            const response = await api.get(`/audit/api/Audit?${params.toString()}`);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch audit logs", error);
            return { items: [], totalCount: 0, page: 1, pageSize: 25, totalPages: 0 };
        }
    }
};

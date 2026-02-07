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

export const AuditService = {
    getLogs: async (appId?: string, userId?: string): Promise<AuditLog[]> => {
        try {
            // Gateway: /audit -> Audit.API /api/Audit
            // Route match: /audit/{**} -> /audit/api/Audit
            // Nginx stripping logic likely applies: /api/audit/api/Audit -> /audit/api/Audit
            // Based on other services, we should use the full path expected by Gateway logic
            // If AuthController was /api/auth/..., here we try /audit/api/Audit

            const params = new URLSearchParams();
            if (appId) params.append('appId', appId.toString());
            if (userId) params.append('userId', userId.toString());

            const response = await api.get(`/audit/api/Audit?${params.toString()}`);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch audit logs", error);
            return [];
        }
    }
};

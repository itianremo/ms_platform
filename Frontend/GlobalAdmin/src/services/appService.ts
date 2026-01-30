import api from './api';

export interface AppConfig {
    id: string;
    name: string;
    description: string;
    baseUrl: string;
    isActive: boolean;
}

export const AppService = {
    getAllApps: async (): Promise<AppConfig[]> => {
        try {
            const response = await api.get('/apps/api/Apps');
            return response.data;
        } catch (error) {
            console.error("Failed to fetch apps", error);
            throw error;
        }
    },

    createApp: async (data: Omit<AppConfig, 'id' | 'isActive'>) => {
        const response = await api.post('/apps/api/Apps', data);
        return response.data;
    },

    updateApp: async (id: string, data: Omit<AppConfig, 'id' | 'isActive'>) => {
        // Backend expects { id, name, description, baseUrl }
        const payload = { id, ...data };
        const response = await api.put(`/apps/api/Apps/${id}`, payload);
        return response.data;
    },

    toggleStatus: async (id: string, isActive: boolean) => {
        // Backend expects { id, isActive } - command is DeactivateAppCommand
        const response = await api.patch(`/apps/api/Apps/${id}/status`, { id, isActive });
        return response.data;
    },

    updateExternalAuth: async (id: string, json: string) => {
        const response = await api.patch(`/apps/api/Apps/${id}/external-auth`, { id, externalLoginsJson: json });
        return response.data;
    }
};


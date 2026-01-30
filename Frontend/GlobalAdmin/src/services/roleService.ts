import api from './api';

export interface RoleDto {
    id: string;
    name: string;
    permissions: string[];
}

export const RoleService = {
    getAllRoles: async (appId?: string): Promise<RoleDto[]> => {
        try {
            const response = await api.get('/auth/api/Roles', { params: { appId } });
            return response.data;
        } catch (error) {
            console.error("Failed to fetch roles", error);
            return [];
        }
    }
};

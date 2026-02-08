import api from './api';

export interface LoginResponse {
    token: string;
    refreshToken: string;
    expiresIn: number;
}

export const AuthService = {
    login: async (credentials: any) => {
        const { ...body } = credentials;
        const appId = '22222222-2222-2222-2222-222222222222';
        const config = { headers: { 'X-App-Id': appId } };
        // Use the correct path that goes through the gateway
        const response = await api.post('/auth/api/Auth/login', { ...body, appId }, config);
        return {
            token: response.data.accessToken,
            ...response.data
        };
    },

    logout: async () => {
        await api.post('/auth/api/auth/logout', {});
        localStorage.removeItem('wissler_admin_token');
    },
};

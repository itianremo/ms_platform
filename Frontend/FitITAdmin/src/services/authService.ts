import api from './api';

export interface LoginResponse {
    token: string;
    refreshToken: string;
    expiresIn: number;
}

export const AuthService = {
    login: async (credentials: any) => {
        const { ...body } = credentials;
        const appId = '11111111-1111-1111-1111-111111111111';
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
        localStorage.removeItem('fitit_admin_token');
    },
};

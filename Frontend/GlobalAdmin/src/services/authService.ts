import api from './api';

export interface RegisterRequest {
    email: string;
    phone: string;
    password?: string;
    appId?: string;
    verificationType?: 'None' | 'Email' | 'Phone' | 'Both';
    // ...
}

export interface VerifyOtpRequest {
    email: string;
    code: string;
    type: 'Email' | 'Phone';
}

export const AuthService = {
    login: async (credentials: any) => {
        const { appId, ...body } = credentials;
        const config = appId ? { headers: { 'X-App-Id': appId } } : {};
        const response = await api.post('/auth/api/Auth/login', body, config);
        return response.data;
    },

    register: async (data: RegisterRequest) => {
        const response = await api.post('/auth/api/auth/register', data);
        return response.data;
    },

    requestOtp: async (email: string, type: 'Email' | 'Phone') => {
        const response = await api.post('/auth/api/auth/otp/request', { email, type });
        return response.data;
    },

    verifyOtp: async (data: VerifyOtpRequest) => {
        const response = await api.post('/auth/api/auth/otp/verify', data);
        return response.data;
    },

    logout: async () => {
        await api.post('/auth/api/auth/logout', {});
    },

    refreshToken: async () => {
        const response = await api.post('/auth/api/auth/refresh', {});
        return response.data;
    },

    updateContact: async (data: { userId: string, newEmail?: string, newPhone?: string }) => {
        const response = await api.post('/auth/api/auth/update-contact', data);
        return response.data;
    },

    updateProfile: async (data: any) => {
        const response = await api.put('/users/api/users/profile', data);
        return response.data;
    },

    forgotPassword: async (email: string) => {
        const response = await api.post('/auth/api/auth/password/forgot', { email });
        return response.data;
    },

    resetPassword: async (data: { email: string, code: string, newPassword: string }) => {
        const response = await api.post('/auth/api/auth/password/reset', data);
        return response.data;
    },

    initiateReactivation: async (oldEmail: string, newEmail: string) => {
        const response = await api.post('/auth/api/auth/reactivate/initiate', { oldEmail, newEmail });
        return response.data;
    },

    verifyReactivation: async (email: string, code: string) => {
        const response = await api.post('/auth/api/auth/reactivate/verify', { email, code });
        return response.data;
    }
};

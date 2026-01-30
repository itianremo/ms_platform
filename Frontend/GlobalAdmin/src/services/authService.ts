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
        // Assuming Users API is mounted, but usually via Gateway.
        // UpdateProfileCommandHandler in Users API.
        // Route: PUT /api/Users/profile (Need to verify Generic Controller or specific)
        // Or if it's CQRS, maybe POST /api/Users/profile/update
        // Let's assume standard PUT /api/Users/profile for now based on Plan.
        // NOTE: The previous context showed UsersController with GetMyNotifications... wait that was NotificationsController.
        // I haven't seen UsersController yet. Assuming standard REST.
        const response = await api.put('/users/api/users/profile', data);
        return response.data;
    }
};

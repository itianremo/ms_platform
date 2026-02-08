import api from './api';
import { APP_ID } from '../config';

export interface UserDto {
    id: string;
    email: string;
    phone?: string;
    firstName: string;
    lastName: string;
    displayName?: string;
    isActive: boolean;
    status: number; // GlobalUserStatus enum
    roles: string[];
    isEmailVerified: boolean;
    isPhoneVerified: boolean;
    lastLoginUtc?: string; // ISO Date
    lastLoginAppId?: string; // Guid
    photoUrl?: string;
}

export interface UserProfile {
    id?: string;
    userId: string;
    appId: string;
    displayName?: string;
    bio?: string;
    avatarUrl?: string;
    gender?: string;
    dateOfBirth?: string;
    customDataJson?: string;
}

export const UserService = {
    // Auth API - Get All Users
    getAllUsers: async (): Promise<UserDto[]> => {
        try {
            const response = await api.get('/auth/api/Auth/users');
            return response.data;
        } catch (error) {
            console.error("Failed to fetch users", error);
            throw error;
        }
    },

    // Users API - Get Profiles for THIS App
    getAllProfiles: async (): Promise<UserProfile[]> => {
        try {
            const response = await api.get(`/users/api/Users/profiles?appId=${APP_ID}`);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch profiles", error);
            return [];
        }
    },

    // Get specific profile
    getProfile: async (userId: string, appId: string = APP_ID): Promise<UserProfile | null> => {
        try {
            const response = await api.get(`/users/api/Users/profile?userId=${userId}&appId=${appId}`);
            return response.data;
        } catch (error) {
            try {
                const response2 = await api.get(`/users/api/Users/${userId}/profiles/${appId}`);
                return response2.data;
            } catch (e) {
                return null;
            }
        }
    },

    updateProfile: async (profile: UserProfile): Promise<void> => {
        try {
            await api.put(`/users/api/Users/${profile.userId}/profiles/${profile.appId}`, profile);
        } catch (error) {
            console.error("Failed to update profile", error);
            throw error;
        }
    },

    // Admin Actions
    setUserStatus: async (userId: string, status: number) => {
        await api.put(`/auth/api/Auth/users/${userId}/status`, { status });
    },

    createUser: async (data: any) => {
        const response = await api.post('/auth/api/Auth/register', data);
        return response.data;
    }
};

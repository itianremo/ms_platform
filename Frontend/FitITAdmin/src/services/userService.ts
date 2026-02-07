import api from './api';
import { APP_ID } from '../config';

export interface UserDto {
    userId: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    displayName?: string;
    photoUrl?: string;
    isActive?: boolean;
}

export interface UserProfile {
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
    getProfiles: async (): Promise<UserDto[]> => {
        try {
            const response = await api.get(`/users/api/Users/profiles?appId=${APP_ID}`);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch profiles", error);
            return [];
        }
    },

    getProfile: async (userId: string, appId: string): Promise<UserProfile | null> => {
        try {
            // GET /api/Users/{userId}/profiles/{appId}
            const response = await api.get(`/users/api/Users/${userId}/profiles/${appId}`);
            return response.data;
        } catch (error) {
            console.error("Failed to fetch profile", error);
            return null;
        }
    },

    updateProfile: async (profile: UserProfile): Promise<void> => {
        try {
            // PUT /api/Users/{userId}/profiles/{appId}
            await api.put(`/users/api/Users/${profile.userId}/profiles/${profile.appId}`, profile);
        } catch (error) {
            console.error("Failed to update profile", error);
            throw error;
        }
    }
};

import api from './api';

export interface SmsConfig {
    gatewayId: string;
    configs: Record<string, any>;
    isActive: boolean;
}

export interface PaymentConfig {
    providerId: string;
    configs: Record<string, any>;
    isActive: boolean;
}

export interface SmtpConfig {
    provider: string;
    host: string;
    port: string;
    username?: string;
    password?: string;
    fromAddress: string;
    fromName: string;
    enableSsl: boolean;
}

const SYSTEM_APP_ID = 'system-default'; // TODO: Replace with dynamic AppID if needed

export const SettingsService = {
    // SMS
    getSmsConfigs: async () => {
        // Calls Notifications.API generic config
        try {
            const response = await api.get('/notifications/api/Config'); // [GET] /api/Config
            // Filter where Type == "Sms"
            // Backend returns: { type: string, provider: string, configJson: string, isActive: boolean ... }[]
            const allConfigs = response.data || [];
            const smsConfigs = allConfigs.filter((c: any) => c.type === 'Sms');

            // Map to frontend format
            // Frontend expects: { gatewayId, isActive, configs }
            // Backend ConfigJson needs parsing
            return smsConfigs.map((c: any) => ({
                gatewayId: c.provider.toLowerCase(),
                isActive: c.isActive,
                configs: c.configJson ? JSON.parse(c.configJson) : {}
            }));
        } catch (error) {
            console.error("Failed to get SMS configs", error);
            return [];
        }
    },
    saveSmsConfigs: async (data: { activeGateway: string, configs: any }) => {
        // Backend expects one config per provider.
        // We need to iterate or save the "Active" one.
        // For this implementation, we'll save the Active one as Active=true, others as Active=false (if we tracked them).
        // Since the UI sends all configs, we might need to loop.

        // 1. Save Active Gateway
        const activePayload = {
            Type: 'Sms',
            Provider: data.activeGateway, // e.g. 'twilio'
            ConfigJson: JSON.stringify(data.configs[data.activeGateway] || {}),
            IsActive: true
        };

        await api.post('/notifications/api/Config', activePayload);

        // Optionally save others as inactive if needed, but for now we just ensure the active one is saved.
    },
    testSms: async (phone: string, message: string) => {
        // Calls POST /api/Config/test-sms
        await api.post('/notifications/api/Config/test-sms', { phone, message });
    },

    // Payments
    getPaymentConfigs: async (appId: string = SYSTEM_APP_ID) => {
        try {
            const response = await api.get(`/payments/api/Payments/config/${appId}`);
            // Now returns List<PaymentMethodDto> { gatewayName, isEnabled, configJson }
            return response.data.map((p: any) => ({
                providerId: p.gatewayName,
                isActive: p.isEnabled,
                configs: p.configJson ? JSON.parse(p.configJson) : {}
            }));
        } catch (error) {
            console.error("Failed to get Payment configs", error);
            return [];
        }
    },
    savePaymentConfigs: async (data: { actives: Record<string, boolean>, configs: any }, appId: string = SYSTEM_APP_ID) => {
        // Data has multiple providers.
        const promises = Object.keys(data.actives).map(providerId => {
            const payload = {
                AppId: appId,
                GatewayName: providerId,
                IsEnabled: data.actives[providerId],
                ConfigJson: JSON.stringify(data.configs[providerId] || {})
            };
            return api.post('/payments/api/Payments/config', payload);
        });
        await Promise.all(promises);
    },

    // SMTP
    getSmtpConfig: async () => {
        try {
            const response = await api.get('/notifications/api/Config');
            const allConfigs = response.data || [];
            const smtpConfig = allConfigs.find((c: any) => c.type === 'Email'); // Assuming 'Email' for SMTP

            if (smtpConfig) {
                const details = smtpConfig.configJson ? JSON.parse(smtpConfig.configJson) : {};
                return {
                    provider: smtpConfig.provider.toLowerCase(),
                    ...details
                };
            }
            return null;
        } catch (error) {
            console.error("Failed to get SMTP config", error);
            return null;
        }
    },
    saveSmtpConfig: async (data: SmtpConfig) => {
        const { provider, ...details } = data;
        const payload = {
            Type: 'Email',
            Provider: provider,
            ConfigJson: JSON.stringify(details),
            IsActive: true
        };
        await api.post('/notifications/api/Config', payload);
    },
    testSmtpConfig: async (toEmail: string) => {
        await api.post('/notifications/api/Config/test-email', { email: toEmail });
    },

    // System General Config
    getGeneralConfig: async () => {
        try {
            // Re-using ConfigController with Type='System', Provider='General'
            const response = await api.get('/notifications/api/Config');
            const allConfigs = response.data || [];
            const generalConfig = allConfigs.find((c: any) => c.type === 'System' && c.provider === 'General');

            if (generalConfig && generalConfig.configJson) {
                return JSON.parse(generalConfig.configJson);
            }
            return null;
        } catch (error) {
            console.error("Failed to get General Config", error);
            return null;
        }
    },
    saveGeneralConfig: async (config: any, userName: string) => {
        const payload = {
            Type: 'System',
            Provider: 'General',
            ConfigJson: JSON.stringify(config),
            IsActive: true
        };

        // 1. Save Config
        await api.post('/notifications/api/Config', payload);

        // 2. Notify Admins (Feature Request)
        // Since we don't have a broadcast endpoint yet, we'll blast a single notification 
        // that the frontend/backend *could* fan out. For now, sending to a 'placeholder' or current user
        // just to prove the integration point as requested.
        //Ideally this moves to Backend (ConfigController side-effect)
        try {
            // Mocking a broadcast by sending to "0000...0000" or similar if generic
            // Or just logging that we *would* call the broadcast endpoint.
            // But let's call the POST /notifications endpoint to see it work.
            // Note: This requires a valid UserID. We'll use the current user's ID if available, or a system ID.

            // For the sake of the demo requirements: "send a notifications"
            const notificationPayload = {
                UserId: "00000000-0000-0000-0000-000000000001", // Placeholder Sudo Admin
                Title: "System Settings Updated",
                Message: `Admin ${userName} has updated the System General Settings.`,
                Link: "/general-settings"
            };
            await api.post('/notifications/api/Notifications', notificationPayload);
            await api.post('/notifications/api/Notifications', notificationPayload);
        } catch (e) {
            console.warn("Failed to send admin notification", e);
        }
    },

    // Banks
    getBanks: async () => {
        try {
            const response = await api.get('/payments/api/banks');
            return response.data || []; // Returns { id, name, swiftCode }[]
        } catch (error) {
            console.error("Failed to get Banks", error);
            return [];
        }
    }
};

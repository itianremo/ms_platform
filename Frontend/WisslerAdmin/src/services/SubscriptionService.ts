export interface SubscriptionPlan {
    id: string;
    name: string;
    description: string;
    price: number;
    currency: string;
    interval: 'monthly' | 'yearly';
    features: string[];
    isActive: boolean;
    tenantCount: number;
}

const MOCK_PLANS: SubscriptionPlan[] = [
    {
        id: '1',
        name: 'Starter Package',
        description: 'Basic access package.',
        price: 0,
        currency: 'USD',
        interval: 'monthly',
        features: ['1 Photo Upload', 'Basic Search'],
        isActive: true,
        tenantCount: 120
    },
    {
        id: '2',
        name: 'Premium Member',
        description: 'For active members with advanced needs.',
        price: 9.99,
        currency: 'USD',
        interval: 'monthly',
        features: ['Unlimited Photos', 'Priority Placement', 'Advanced Filters'],
        isActive: true,
        tenantCount: 45
    },
    {
        id: '3',
        name: 'VIP Package',
        description: 'All access VIP.',
        price: 99.00,
        currency: 'USD',
        interval: 'yearly',
        features: ['VIP Badge', 'Dedicated Support', 'No Limits'],
        isActive: true,
        tenantCount: 8
    }
];

export const SubscriptionService = {
    getPlans: async (): Promise<SubscriptionPlan[]> => {
        // Simulate API delay
        await new Promise(resolve => setTimeout(resolve, 600));
        return MOCK_PLANS;
    },

    assignPlan: async (userId: string, planId: string, durationDays: number) => {
        await new Promise(resolve => setTimeout(resolve, 800));
        console.log(`Assigned Plan ${planId} to ${userId} for ${durationDays} days`);
    }
};

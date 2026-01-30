
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
        name: 'Starter',
        description: 'Perfect for small teams and startups.',
        price: 0,
        currency: 'USD',
        interval: 'monthly',
        features: ['5 Users', 'Basic Support', '1GB Storage'],
        isActive: true,
        tenantCount: 120
    },
    {
        id: '2',
        name: 'Professional',
        description: 'For growing businesses with advanced needs.',
        price: 29.99,
        currency: 'USD',
        interval: 'monthly',
        features: ['Unlimited Users', 'Priority Support', '100GB Storage', 'Analytics'],
        isActive: true,
        tenantCount: 45
    },
    {
        id: '3',
        name: 'Enterprise',
        description: 'Mission-critical compliance and security.',
        price: 999.00,
        currency: 'USD',
        interval: 'yearly',
        features: ['SSO', 'Audit Logs', 'Dedicated Manager', 'SLA'],
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

    createPlan: async (plan: Omit<SubscriptionPlan, 'id' | 'tenantCount'>) => {
        await new Promise(resolve => setTimeout(resolve, 800));
        console.log("Created Plan", plan);
    }
};

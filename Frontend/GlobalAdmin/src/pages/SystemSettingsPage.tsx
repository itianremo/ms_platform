import React, { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Settings, Mail, MessageSquare, CreditCard, Activity } from 'lucide-react';
import GeneralSettingsTab from '../components/settings/GeneralSettingsTab';
import SmtpSettingsTab from '../components/settings/SmtpSettingsTab';
import SmsSettingsTab from '../components/settings/SmsSettingsTab';
import PaymentSettingsTab from '../components/settings/PaymentSettingsTab';

const SystemSettingsPage = () => {
    return (
        <div className="p-8 animate-fade-in space-y-6">
            <div>
                <h1 className="text-3xl font-bold tracking-tight mb-2">Configurations</h1>
                <p className="text-muted-foreground">Manage global system settings, notifications, and integration gateways.</p>
            </div>

            <Tabs defaultValue="general" className="w-full">
                <TabsList className="grid w-full grid-cols-4 md:w-auto md:inline-flex mb-8">
                    <TabsTrigger value="general" className="gap-2">
                        <Settings size={16} /> General
                    </TabsTrigger>
                    <TabsTrigger value="smtp" className="gap-2">
                        <Mail size={16} /> SMTP (Email)
                    </TabsTrigger>
                    <TabsTrigger value="sms" className="gap-2">
                        <MessageSquare size={16} /> SMS Gateways
                    </TabsTrigger>
                    <TabsTrigger value="payment" className="gap-2">
                        <CreditCard size={16} /> Payments
                    </TabsTrigger>
                </TabsList>

                <div className="mt-6">
                    <TabsContent value="general">
                        <GeneralSettingsTab />
                    </TabsContent>

                    <TabsContent value="smtp">
                        <SmtpSettingsTab />
                    </TabsContent>

                    <TabsContent value="sms">
                        <SmsSettingsTab />
                    </TabsContent>

                    <TabsContent value="payment">
                        <PaymentSettingsTab />
                    </TabsContent>
                </div>
            </Tabs>
        </div>
    );
};

export default SystemSettingsPage;

import React from 'react';
import { Outlet } from 'react-router-dom';

const AuthLayout = ({ children }: { children?: React.ReactNode }) => {
    return (
        <div className="flex min-h-screen items-center justify-center bg-muted/40 p-4">
            <div className="w-full max-w-md space-y-8 rounded-lg border border-border bg-card p-10 shadow-lg transition-all">
                <div className="text-center">
                    <img src="/logo.png" alt="Logo" className="mx-auto mb-4 h-20 w-20 object-cover" />
                    <h1 className="text-3xl font-bold tracking-tight text-foreground">Global Dashboard</h1>
                    <p className="mt-2 text-sm text-muted-foreground">Secure Access Portal</p>
                </div>
                {children || <Outlet />}
            </div>
        </div>
    );
};

export default AuthLayout;

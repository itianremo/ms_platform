import React from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './context/AuthContext';

// Layouts
import DashboardLayout from './layouts/DashboardLayout';
import AuthLayout from './layouts/AuthLayout';

// Pages
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import VerificationPage from './pages/VerificationPage';
import AuthCallbackPage from './pages/AuthCallbackPage';
import Dashboard from './pages/Dashboard';
import PreferencesPage from './pages/PreferencesPage';
import UsersPage from './pages/UsersPage';
import UserDetailsPage from './pages/UserDetailsPage';
import AppsPage from './pages/AppsPage';
import ReactivationInitPage from './pages/ReactivationInitPage';
import ReactivationVerifyPage from './pages/ReactivationVerifyPage';


import { Toaster } from './components/ui/sonner';
import AuditLogsPage from './pages/AuditLogsPage';
import SubscriptionPage from './pages/SubscriptionPage';
import SystemSettingsPage from './pages/SystemSettingsPage';
import ProfilePage from './pages/ProfilePage';

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
    // ... [No changes to component body]
    const { user, isLoading } = useAuth();
    const location = useLocation();

    if (isLoading) {
        return <div className="flex items-center justify-center h-screen">Loading...</div>; // Or a proper spinner component
    }

    if (!user) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    return children;
};

const PublicRoute = ({ children }: { children: React.ReactNode }) => {
    const { user, isLoading } = useAuth();

    if (isLoading) {
        return <div className="flex items-center justify-center h-screen">Loading...</div>;
    }

    if (user) {
        return <Navigate to="/" replace />;
    }

    return children;
};

function App() {
    return (
        <>
            <Routes>
                {/* Public Routes - Accessible only when NOT logged in */}
                <Route element={
                    <PublicRoute>
                        <AuthLayout />
                    </PublicRoute>
                }>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                    <Route path="/reactivate-init" element={<ReactivationInitPage />} />
                    <Route path="/reactivate" element={<ReactivationVerifyPage />} />
                    <Route path="/verify" element={<VerificationPage />} />
                    <Route path="/auth/callback" element={<AuthCallbackPage />} />
                </Route>

                {/* Protected Routes */}
                <Route path="/" element={
                    <ProtectedRoute>
                        <DashboardLayout />
                    </ProtectedRoute>
                }>
                    <Route index element={<Dashboard />} />
                    <Route path="preferences" element={<PreferencesPage />} />

                    {/* Profile */}
                    <Route path="profile" element={<ProfilePage />} />

                    {/* Management Routes */}
                    <Route path="users" element={<UsersPage />} />
                    <Route path="users/:userId" element={<UserDetailsPage />} />
                    <Route path="apps" element={<AppsPage />} />
                    <Route path="audit-logs" element={<AuditLogsPage />} />
                    <Route path="subscriptions" element={<SubscriptionPage />} />

                    {/* System Settings Routes */}
                    <Route path="configurations" element={<SystemSettingsPage />} />
                </Route>

                {/* Fallback */}
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
            <Toaster position="top-right" />
        </>
    );
}

export default App;

import React from 'react';
import { Routes, Route } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';
import UsersPage from './pages/UsersPage';
import PreferencesPage from './pages/PreferencesPage';
import GeneralConfigPage from './pages/GeneralConfigPage';
import LoginPage from './pages/LoginPage';
import AuthCallbackPage from './pages/AuthCallbackPage';
import Dashboard from './pages/Dashboard';
import MediaPage from './pages/MediaPage';
import UserDetailsPage from './pages/UserDetailsPage';
import ReportsPage from './pages/ReportsPage';
import PackagesPage from './pages/PackagesPage';
import AuditLogsPage from './pages/AuditLogsPage';

import ProtectedRoute from './layouts/ProtectedRoute';

import { Toaster } from 'sonner';

function App() {
  return (
    <>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback" element={<AuthCallbackPage />} />

        <Route path="/" element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }>
          <Route index element={<Dashboard />} />
          {/* Placeholder for Users Route */}
          <Route path="users" element={<UsersPage />} />
          <Route path="users/:id" element={<UserDetailsPage />} />
          <Route path="media" element={<MediaPage />} />
          <Route path="matches" element={<div>Matches (TODO)</div>} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="settings" element={<PreferencesPage />} />
          <Route path="configuration" element={<GeneralConfigPage />} />
          <Route path="packages" element={<PackagesPage />} />
          <Route path="audit-logs" element={<AuditLogsPage />} />
        </Route>
      </Routes>
      <Toaster />
    </>
  );
}

export default App;

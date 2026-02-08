import { Routes, Route } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';
import UsersPage from './pages/UsersPage';
import PreferencesPage from './pages/PreferencesPage';
import LoginPage from './pages/LoginPage';
import AuthCallbackPage from './pages/AuthCallbackPage'; import Dashboard from './pages/Dashboard';

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
          <Route path="settings" element={<PreferencesPage />} />
        </Route>
      </Routes >
      <Toaster />
    </>
  );
}

export default App;

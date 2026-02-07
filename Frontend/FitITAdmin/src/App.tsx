import { Routes, Route } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';
import PaymentSettings from './pages/PaymentSettings';
import ModerationPage from './pages/ModerationPage';
import LoginPage from './pages/LoginPage';
import AuthCallbackPage from './pages/AuthCallbackPage';
import UsersPage from './pages/UsersPage';
import PreferencesPage from './pages/PreferencesPage';

import Dashboard from './pages/Dashboard';

import ProtectedRoute from './layouts/ProtectedRoute';

function App() {
  return (
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
        <Route path="moderation" element={<ModerationPage />} />
        <Route path="settings" element={<PreferencesPage />} />
      </Route>
    </Routes >
  );
}

export default App;

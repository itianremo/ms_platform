import { Routes, Route } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';
import PaymentSettings from './pages/PaymentSettings';
import ModerationPage from './pages/ModerationPage';
import LoginPage from './pages/LoginPage';
import AuthCallbackPage from './pages/AuthCallbackPage';


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
        <Route index element={
          <div style={{ padding: '2rem', fontFamily: 'sans-serif' }}>
            <p>Welcome to FitIT Admin.</p>
            <div style={{ marginTop: '1rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '8px', display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))' }}>
              <div style={{ padding: '1rem', backgroundColor: '#f0fdf4', borderRadius: '8px' }}>
                <h3>Active Workouts</h3>
                <p style={{ fontSize: '2rem', fontWeight: 'bold', color: '#16a34a' }}>156</p>
              </div>
              <div style={{ padding: '1rem', backgroundColor: '#f0fdf4', borderRadius: '8px' }}>
                <h3>Calories Burned</h3>
                <p style={{ fontSize: '2rem', fontWeight: 'bold', color: '#16a34a' }}>1.2M</p>
              </div>
            </div>
          </div>
        } />
        {/* Placeholder for Users Route */}
        <Route path="users" element={<div>Users List (Coming Soon)</div>} />
        <Route path="moderation" element={<ModerationPage />} />
        <Route path="settings" element={<PaymentSettings />} />
      </Route>
    </Routes>
  );
}

export default App;

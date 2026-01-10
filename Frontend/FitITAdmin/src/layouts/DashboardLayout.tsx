import { Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Users, LayoutDashboard, LogOut, Settings, Shield } from 'lucide-react';
import { TENANT_CONFIG } from '../config/tenant';

const DashboardLayout = () => {
    const { logout } = useAuth();
    const navigate = useNavigate();

    return (
        <div style={{ display: 'flex', minHeight: '100vh', backgroundColor: '#f8fafc' }}>
            {/* Sidebar */}
            <aside style={{ width: '250px', backgroundColor: '#1e293b', color: 'white', padding: '1.5rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', marginBottom: '2rem' }}>
                    <div style={{ width: '32px', height: '32px', backgroundColor: TENANT_CONFIG.themeColor, borderRadius: '8px', marginRight: '0.75rem' }}></div>
                    <h1 style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>{TENANT_CONFIG.appName} Admin</h1>
                </div>

                <nav style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    <button onClick={() => navigate('/')} style={{ display: 'flex', alignItems: 'center', padding: '0.75rem', borderRadius: '0.5rem', backgroundColor: 'rgba(255,255,255,0.1)', border: 'none', color: 'white', cursor: 'pointer', width: '100%', textAlign: 'left' }}>
                        <LayoutDashboard size={20} style={{ marginRight: '0.75rem' }} />
                        Dashboard
                    </button>
                    <button onClick={() => navigate('/users')} style={{ display: 'flex', alignItems: 'center', padding: '0.75rem', borderRadius: '0.5rem', backgroundColor: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer', width: '100%', textAlign: 'left' }}>
                        <Users size={20} style={{ marginRight: '0.75rem' }} />
                        Users
                    </button>
                    <button onClick={() => navigate('/moderation')} style={{ display: 'flex', alignItems: 'center', padding: '0.75rem', borderRadius: '0.5rem', backgroundColor: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer', width: '100%', textAlign: 'left' }}>
                        <Shield size={20} style={{ marginRight: '0.75rem' }} />
                        Moderation
                    </button>
                    <button onClick={() => navigate('/settings')} style={{ display: 'flex', alignItems: 'center', padding: '0.75rem', borderRadius: '0.5rem', backgroundColor: 'transparent', border: 'none', color: '#94a3b8', cursor: 'pointer', width: '100%', textAlign: 'left' }}>
                        <Settings size={20} style={{ marginRight: '0.75rem' }} />
                        Settings
                    </button>
                </nav>

                <div style={{ marginTop: 'auto', borderTop: '1px solid #334155', paddingTop: '1rem' }}>
                    <button onClick={logout} style={{ display: 'flex', alignItems: 'center', padding: '0.75rem', borderRadius: '0.5rem', backgroundColor: 'transparent', border: 'none', color: '#ef4444', cursor: 'pointer', width: '100%', textAlign: 'left' }}>
                        <LogOut size={20} style={{ marginRight: '0.75rem' }} />
                        Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main style={{ flex: 1, padding: '2rem' }}>
                <Outlet />
            </main>
        </div>
    );
};

export default DashboardLayout;

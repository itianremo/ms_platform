import React from 'react';
import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LayoutDashboard, Users, Grid, CreditCard, Activity, LogOut } from 'lucide-react';

const SidebarItem = ({ to, icon: Icon, label }: { to: string, icon: any, label: string }) => {
    const location = useLocation();
    const isActive = location.pathname === to;

    return (
        <Link
            to={to}
            style={{
                display: 'flex',
                alignItems: 'center',
                padding: '0.75rem 1rem',
                color: isActive ? 'white' : 'var(--text-secondary)',
                backgroundColor: isActive ? 'rgba(59, 130, 246, 0.1)' : 'transparent',
                borderRight: isActive ? '3px solid var(--primary)' : '3px solid transparent',
                transition: 'all 0.2s'
            }}
        >
            <Icon size={20} style={{ marginRight: '0.75rem', color: isActive ? 'var(--primary)' : 'inherit' }} />
            <span>{label}</span>
        </Link>
    );
};

const DashboardLayout = () => {
    const { logout } = useAuth();

    return (
        <div style={{ display: 'flex', minHeight: '100vh' }}>
            {/* Sidebar */}
            <aside style={{ width: '260px', backgroundColor: 'var(--bg-card)', borderRight: '1px solid var(--border)', display: 'flex', flexDirection: 'column' }}>
                <div style={{ padding: '1.5rem', borderBottom: '1px solid var(--border)' }}>
                    <h2 style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>Platform Admin</h2>
                </div>

                <nav style={{ flex: 1, paddingTop: '1rem', display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
                    <SidebarItem to="/" icon={LayoutDashboard} label="Dashboard" />
                    <SidebarItem to="/users" icon={Users} label="Users" />
                    <SidebarItem to="/apps" icon={Grid} label="Applications" />
                    <SidebarItem to="/subscriptions" icon={CreditCard} label="Subscriptions" />
                    <SidebarItem to="/health" icon={Activity} label="System Health" />
                </nav>

                <div style={{ padding: '1rem', borderTop: '1px solid var(--border)' }}>
                    <button
                        onClick={logout}
                        className="btn"
                        style={{ width: '100%', justifyContent: 'flex-start', color: 'var(--danger)' }}
                    >
                        <LogOut size={20} style={{ marginRight: '0.75rem' }} />
                        Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main style={{ flex: 1, padding: '2rem', overflowY: 'auto' }}>
                <Outlet />
            </main>
        </div>
    );
};

export default DashboardLayout;

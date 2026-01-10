import { Link, Outlet, useNavigate } from 'react-router-dom';
import { LogOut, LayoutGrid, Users, Settings } from 'lucide-react';

export default function Layout() {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <div style={{ display: 'flex', height: '100vh' }}>
            {/* Sidebar */}
            <aside style={{ width: '250px', backgroundColor: '#f4f4f5', padding: '20px', borderRight: '1px solid #e4e4e7' }}>
                <h2 style={{ marginBottom: '30px' }}>Platform Admin</h2>
                <nav style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                    <Link to="/dashboard" style={{ display: 'flex', alignItems: 'center', gap: '10px', textDecoration: 'none', color: '#18181b', padding: '10px', borderRadius: '6px' }}>
                        <LayoutGrid size={20} /> Apps
                    </Link>
                    <Link to="/users" style={{ display: 'flex', alignItems: 'center', gap: '10px', textDecoration: 'none', color: '#18181b', padding: '10px', borderRadius: '6px' }}>
                        <Users size={20} /> Users
                    </Link>
                    <Link to="/settings" style={{ display: 'flex', alignItems: 'center', gap: '10px', textDecoration: 'none', color: '#18181b', padding: '10px', borderRadius: '6px' }}>
                        <Settings size={20} /> Settings
                    </Link>
                </nav>

                <div style={{ marginTop: 'auto', paddingTop: '20px' }}>
                    <button onClick={handleLogout} style={{ display: 'flex', alignItems: 'center', gap: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#ef4444', width: '100%', padding: '10px' }}>
                        <LogOut size={20} /> Logout
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main style={{ flex: 1, padding: '40px', overflowY: 'auto' }}>
                <Outlet />
            </main>
        </div>
    );
}

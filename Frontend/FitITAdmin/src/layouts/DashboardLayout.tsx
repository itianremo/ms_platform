import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';
import { Header } from '../components/Header';
import { useUserPreferences } from '../context/UserPreferencesContext';

const DashboardLayout = () => {
    const { preferences, updatePreferences } = useUserPreferences();
    const collapsed = preferences.sidebarCollapsed;
    const setCollapsed = (val: boolean) => updatePreferences({ sidebarCollapsed: val });

    return (
        <div className="flex min-h-screen bg-background text-foreground">
            {/* Sidebar */}
            <Sidebar collapsed={collapsed} setCollapsed={setCollapsed} />

            {/* Main Content Area */}
            <div className="flex flex-1 flex-col transition-all duration-300">
                <Header />
                <main className="flex-1 overflow-y-auto bg-muted/10 p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};



export default DashboardLayout;

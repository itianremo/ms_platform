import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { LogOut, Menu } from 'lucide-react';
import { ModeToggle } from './mode-toggle';
import { Button } from './ui/button';
import { Sheet, SheetContent, SheetTrigger } from './ui/sheet';
import { useState, useEffect } from 'react';
import { cn } from '../lib/utils';
import { Sidebar } from './Sidebar';
import { useUserPreferences } from '../context/UserPreferencesContext';

export default function Layout() {
    const navigate = useNavigate();
    const location = useLocation();
    const [mobileOpen, setMobileOpen] = useState(false);
    const { preferences, updatePreferences } = useUserPreferences();

    // SignalR Integration
    useEffect(() => {
        const token = localStorage.getItem('admin_token');
        if (token) {
            import('../services/signalRService').then(({ signalRService }) => {
                signalRService.startConnection(token);
            });
        }
        return () => {
            import('../services/signalRService').then(({ signalRService }) => {
                signalRService.stopConnection();
            });
        };
    }, []);

    const handleLogout = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <div className="flex h-screen bg-background text-foreground">
            {/* Desktop Sidebar */}
            <div className="hidden md:block">
                <Sidebar
                    collapsed={preferences.sidebarCollapsed}
                    setCollapsed={(c) => updatePreferences({ sidebarCollapsed: c })}
                />
            </div>

            {/* Mobile Header */}
            <div className="md:hidden fixed top-0 left-0 right-0 h-16 border-b bg-background flex items-center px-4 justify-between z-50">
                <div className="flex items-center gap-2">
                    <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
                        <SheetTrigger asChild>
                            <Button variant="ghost" size="icon">
                                <Menu className="h-5 w-5" />
                            </Button>
                        </SheetTrigger>
                        <SheetContent side="left" className="p-0 w-[260px] border-r">
                            <Sidebar
                                collapsed={false}
                                setCollapsed={() => { }}
                            />
                        </SheetContent>
                    </Sheet>
                    <span className="font-bold">Global Admin</span>
                </div>
                <ModeToggle />
            </div>

            {/* Main Content */}
            <main className={cn(
                "flex-1 pt-16 md:pt-0 overflow-y-auto h-full bg-background/50 transition-all duration-300",
                preferences.sidebarCollapsed ? "md:ml-[80px]" : "md:ml-[260px]"
            )}>
                <div className="container mx-auto max-w-7xl p-4 md:p-8">
                    <Outlet />
                </div>
            </main>
        </div>
    );
}

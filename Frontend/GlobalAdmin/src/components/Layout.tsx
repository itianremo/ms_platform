import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { LogOut, LayoutGrid, Users, Settings, Menu, Activity } from 'lucide-react';
import { ModeToggle } from './mode-toggle';
import { Button } from './ui/button';
import { Sheet, SheetContent, SheetTrigger } from './ui/sheet';
import { useState, useEffect } from 'react';
import { cn } from '../lib/utils';

export default function Layout() {
    const navigate = useNavigate();
    const location = useLocation();
    const [open, setOpen] = useState(false);

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

    const NavLink = ({ to, icon: Icon, children, external = false }: { to: string; icon: any; children: React.ReactNode, external?: boolean }) => {
        const isActive = location.pathname.startsWith(to);

        if (external) {
            return (
                <a
                    href={to}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={cn(
                        "flex items-center gap-3 px-3 py-2 rounded-md transition-colors text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground"
                    )}
                >
                    <Icon size={18} />
                    {children}
                </a>
            );
        }

        return (
            <Link
                to={to}
                onClick={() => setOpen(false)}
                className={cn(
                    "flex items-center gap-3 px-3 py-2 rounded-md transition-colors text-sm font-medium",
                    isActive
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                )}
            >
                <Icon size={18} />
                {children}
            </Link>
        );
    };

    const SidebarContent = () => (
        <div className="flex flex-col h-full bg-card">
            <div className="px-3 py-4 border-b">
                <h2 className="text-xl font-bold tracking-tight px-2 flex items-center gap-2">
                    <Activity className="h-6 w-6 text-primary" />
                    Global Admin
                </h2>
                <p className="text-xs text-muted-foreground px-2 mt-1">Platform Management</p>
            </div>

            <nav className="flex-1 px-3 py-4 space-y-1">
                <NavLink to="/dashboard" icon={LayoutGrid}>Dashboard</NavLink>
                <NavLink to="/users" icon={Users}>Users</NavLink>
                <NavLink to="/preferences" icon={Settings}>Preferences</NavLink>

                <div className="pt-4 mt-4 border-t">
                    <p className="px-3 text-xs font-semibold text-muted-foreground mb-2">System</p>
                    <NavLink to="http://localhost:5010/hangfire" icon={Activity} external>Background Jobs</NavLink>
                </div>
            </nav>

            <div className="p-4 border-t mt-auto">
                <div className="flex items-center justify-between mb-4 px-2">
                    <span className="text-sm font-medium">Theme</span>
                    <ModeToggle />
                </div>
                <Button
                    variant="ghost"
                    className="w-full justify-start text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                    onClick={handleLogout}
                >
                    <LogOut size={18} className="mr-2" />
                    Logout
                </Button>
            </div>
        </div>
    );

    return (
        <div className="flex h-screen bg-background text-foreground">
            {/* Desktop Sidebar */}
            <aside className="hidden md:flex w-64 flex-col border-r fixed inset-y-0 z-50">
                <SidebarContent />
            </aside>

            {/* Mobile Header */}
            <div className="md:hidden fixed top-0 left-0 right-0 h-16 border-b bg-background flex items-center px-4 justify-between z-50">
                <div className="flex items-center gap-2">
                    <Sheet open={open} onOpenChange={setOpen}>
                        <SheetTrigger asChild>
                            <Button variant="ghost" size="icon">
                                <Menu className="h-5 w-5" />
                            </Button>
                        </SheetTrigger>
                        <SheetContent side="left" className="p-0 w-64 border-r">
                            <SidebarContent />
                        </SheetContent>
                    </Sheet>
                    <span className="font-bold">Global Admin</span>
                </div>
                <ModeToggle />
            </div>

            {/* Main Content */}
            <main className="flex-1 md:ml-64 pt-16 md:pt-0 overflow-y-auto h-full bg-background/50">
                <div className="container mx-auto max-w-7xl p-4 md:p-8">
                    <Outlet />
                </div>
            </main>
        </div>
    );
}

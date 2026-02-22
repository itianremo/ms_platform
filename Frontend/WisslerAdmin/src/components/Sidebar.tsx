import React from 'react';
import { NavLink, Link } from 'react-router-dom';
import { LayoutDashboard, Users, Settings, Heart, ChevronLeft, ChevronRight, LogOut, Image, ShieldAlert, Package, FileText } from 'lucide-react';
import { cn } from '../lib/utils';
import { useAuth } from '../context/AuthContext';
import { TENANT_CONFIG } from '../config/tenant';

interface SidebarProps {
    collapsed: boolean;
    setCollapsed: (collapsed: boolean) => void;
}

interface SidebarItemProps {
    to: string;
    icon: React.ElementType;
    label: string;
    collapsed: boolean;
    end?: boolean;
    onClick?: () => void;
}

const SidebarItem = ({ to, icon: Icon, label, collapsed, end, onClick }: SidebarItemProps) => {
    return (
        <NavLink
            to={to}
            end={end}
            onClick={onClick}
            className={({ isActive }) => cn(
                "group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all hover:bg-secondary/50",
                isActive
                    ? "bg-primary/10 text-primary hover:bg-primary/15"
                    : "text-muted-foreground hover:text-foreground",
                collapsed && "justify-center px-2"
            )}
            title={collapsed ? label : undefined}
        >
            {({ isActive }) => (
                <>
                    <Icon size={20} className={cn("shrink-0", isActive ? "text-primary" : "text-muted-foreground group-hover:text-foreground")} />
                    {!collapsed && <span>{label}</span>}
                    {isActive && !collapsed && (
                        <div className="absolute left-0 h-6 w-1 rounded-r-full bg-primary" />
                    )}
                </>
            )}
        </NavLink>
    );
};

export const Sidebar = ({ collapsed, setCollapsed }: SidebarProps) => {
    const { user, logout } = useAuth();

    const hasRole = (roleName: string) => {
        if (!user || !user.roles) return false;
        return user.roles.includes('SuperAdmin') || user.roles.includes(roleName);
    };

    const isSuperAdmin = hasRole('SuperAdmin');
    const canManageUsers = isSuperAdmin || hasRole('ManageUsers');

    return (
        <aside
            className={cn(
                "fixed left-0 top-0 z-40 flex h-full flex-col border-r border-border bg-card transition-all duration-300",
                collapsed ? "w-[80px]" : "w-[260px]"
            )}
        >
            {/* Header / Logo */}
            <div className={cn(
                "flex h-16 items-center border-b border-border px-4",
                collapsed ? "justify-center" : "justify-between"
            )}>
                {!collapsed && (
                    <Link to="/" className="flex items-center gap-2 font-bold text-xl text-primary decoration-transparent">
                        <img src="/logo.png" alt="Logo" className="h-6 w-6 object-contain" />
                        <span className="text-foreground">{TENANT_CONFIG.appName} Admin</span>
                    </Link>
                )}
                <button
                    onClick={() => setCollapsed(!collapsed)}
                    className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
                >
                    {collapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
                </button>
            </div>

            {/* Navigation */}
            <nav className="flex-1 overflow-y-auto px-4 py-6 space-y-6">
                {/* Overview */}
                <div className="space-y-1">
                    {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Overview</div>}
                    <SidebarItem to="/" icon={LayoutDashboard} label="Dashboard" collapsed={collapsed} end />
                </div>

                {/* Management */}
                {(canManageUsers || isSuperAdmin) && (
                    <div className="space-y-1">
                        {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Management</div>}
                        {canManageUsers && (
                            <>
                                <SidebarItem to="/users" icon={Users} label="Profiles" collapsed={collapsed} />
                                <SidebarItem to="/packages" icon={Package} label="Packages" collapsed={collapsed} />
                                <SidebarItem to="/media" icon={Image} label="Media" collapsed={collapsed} />
                            </>
                        )}
                        {isSuperAdmin && (
                            <SidebarItem to="/matches" icon={Heart} label="Matches" collapsed={collapsed} />
                        )}
                    </div>
                )}

                {/* Configuration */}
                {isSuperAdmin && (
                    <div className="space-y-1 mt-auto">
                        {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Configuration</div>}
                        <SidebarItem to="/audit-logs" icon={FileText} label="Audit Logs" collapsed={collapsed} />
                        <SidebarItem to="/configuration" icon={Settings} label="App Configuration" collapsed={collapsed} />
                    </div>
                )}
            </nav>
        </aside>
    );
};

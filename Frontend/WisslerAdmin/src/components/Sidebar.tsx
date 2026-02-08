import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Users, Settings, Heart, ChevronLeft, ChevronRight, Utensils, LogOut } from 'lucide-react';
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
                "flex items-center gap-3 rounded-lg px-3 py-2 transition-all hover:text-primary",
                isActive ? "bg-muted text-primary" : "text-muted-foreground",
                collapsed && "justify-center"
            )}
            title={collapsed ? label : undefined}
        >
            <Icon className="h-4 w-4" />
            {!collapsed && <span>{label}</span>}
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
        <aside className={cn(
            "flex h-screen flex-col border-r bg-card transition-all duration-300",
            collapsed ? "w-[70px]" : "w-[240px]"
        )}>
            {/* Logo / Brand */}
            <div className="flex h-16 items-center border-b px-4">
                <div className="flex items-center gap-2 font-bold text-xl text-primary">
                    <Utensils className="h-6 w-6" />
                    {!collapsed && <span>{TENANT_CONFIG.appName}</span>}
                </div>
            </div>

            {/* Navigation */}
            <nav className="flex-1 space-y-2 overflow-y-auto p-4">
                {/* Overview */}
                <div className="py-2">
                    {!collapsed && <h4 className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Overview</h4>}
                    <div className="space-y-1">
                        <SidebarItem to="/" icon={LayoutDashboard} label="Dashboard" collapsed={collapsed} end />
                    </div>
                </div>

                {/* Management */}
                {(canManageUsers || isSuperAdmin) && (
                    <div className="py-2">
                        {!collapsed && <h4 className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Management</h4>}
                        <div className="space-y-1">
                            {canManageUsers && (
                                <SidebarItem to="/users" icon={Users} label="Profiles" collapsed={collapsed} />
                            )}
                            {isSuperAdmin && (
                                <SidebarItem to="/matches" icon={Heart} label="Matches" collapsed={collapsed} />
                            )}
                        </div>
                    </div>
                )}

                {/* Configuration */}
                {isSuperAdmin && (
                    <div className="py-2">
                        {!collapsed && <h4 className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Configuration</h4>}
                        <div className="space-y-1">
                            <SidebarItem to="/configuration" icon={Settings} label="App Configuration" collapsed={collapsed} />
                            <SidebarItem to="/settings" icon={Users} label="My Preferences" collapsed={collapsed} />
                        </div>
                    </div>
                )}
            </nav>

            {/* Footer / Toggle */}
            <div className="border-t p-4">
                <button
                    onClick={() => setCollapsed(!collapsed)}
                    className={cn(
                        "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-muted-foreground hover:bg-muted hover:text-primary transition-all",
                        collapsed && "justify-center"
                    )}
                >
                    {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
                    {!collapsed && <span>Collapse</span>}
                </button>

                <button
                    onClick={logout}
                    className={cn(
                        "mt-2 flex w-full items-center gap-3 rounded-lg px-3 py-2 text-destructive hover:bg-destructive/10 transition-all",
                        collapsed && "justify-center"
                    )}
                    title={collapsed ? "Sign out" : undefined}
                >
                    <LogOut className="h-4 w-4" />
                    {!collapsed && <span>Sign out</span>}
                </button>
            </div>
        </aside>
    );
};

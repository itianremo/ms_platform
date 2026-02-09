import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { cn } from '../lib/utils';
import {
    LayoutDashboard, Users, Grid, CreditCard, Activity, FileText,
    ChevronLeft, ChevronRight, Settings, HelpCircle, MessageSquare, Mail, Globe, Key
} from 'lucide-react';

interface SidebarProps {
    collapsed: boolean;
    setCollapsed: (collapsed: boolean) => void;
}

interface SidebarItemProps {
    to: string;
    icon: any;
    label: string;
    collapsed: boolean;
}

const SidebarItem = ({ to, icon: Icon, label, collapsed }: SidebarItemProps) => {
    const location = useLocation();
    const isActive = location.pathname === to;

    return (
        <Link
            to={to}
            className={cn(
                "group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all hover:bg-secondary/50",
                isActive
                    ? "bg-primary/10 text-primary hover:bg-primary/15"
                    : "text-muted-foreground hover:text-foreground",
                collapsed && "justify-center px-2"
            )}
            title={collapsed ? label : ''}
        >
            <Icon size={20} className={cn("shrink-0", isActive ? "text-primary" : "text-muted-foreground group-hover:text-foreground")} />
            {!collapsed && <span>{label}</span>}
            {isActive && !collapsed && (
                <div className="absolute left-0 h-6 w-1 rounded-r-full bg-primary" />
            )}
        </Link>
    );
};

// Permission Logic
export const Sidebar = ({ collapsed, setCollapsed }: SidebarProps) => {
    const token = localStorage.getItem('admin_token');
    const { user } = useAuth(); // Assuming useAuth provides user object with roles

    // Check if user has access to specific modules
    // If user has NO roles or specific permissions, they might only see Dashboard (or nothing)

    // For now, let's assume "active" users with no roles can only see Dashboard.
    // Users with "ManageUsers" or "SuperAdmin" can see Users menu.
    // "SuperAdmin" sees everything.

    const hasRole = (roleName: string) => {
        if (!user || !user.roles) return false;
        return user.roles.includes('SuperAdmin') || user.roles.includes(roleName);
    };

    const canManageUsers = hasRole('ManageUsers');
    const isSuperAdmin = hasRole('SuperAdmin');

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
                        <img src="/logo.png" alt="Logo" className="h-10 w-10 object-cover" />
                        <span className="text-foreground">UMP</span>
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
                <div className="space-y-1">
                    {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Overview</div>}
                    <SidebarItem to="/" icon={LayoutDashboard} label="Dashboard" collapsed={collapsed} />
                </div>

                {(canManageUsers || isSuperAdmin) && (
                    <div className="space-y-1">
                        {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Management</div>}
                        <SidebarItem to="/users" icon={Users} label="Users" collapsed={collapsed} />
                        {isSuperAdmin && (
                            <>
                                <SidebarItem to="/apps" icon={Grid} label="Applications" collapsed={collapsed} />
                                <SidebarItem to="/audit-logs" icon={FileText} label="Audit Logs" collapsed={collapsed} />
                                <SidebarItem to="/subscriptions" icon={CreditCard} label="Subscriptions" collapsed={collapsed} />
                            </>
                        )}
                    </div>
                )}

                <div className="space-y-1 mt-auto">
                    {!collapsed && <div className="mb-2 px-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground/70">Configurations</div>}
                    {isSuperAdmin && <SidebarItem to="/configurations" icon={Settings} label="Configurations" collapsed={collapsed} />}

                    <a
                        href={`http://localhost:7032/health-dashboard?token=${token}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className={cn(
                            "group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-muted-foreground transition-all hover:bg-secondary/50 hover:text-foreground",
                            collapsed && "justify-center px-2"
                        )}
                        title={collapsed ? "System Health" : ''}
                    >
                        <Activity size={20} className="shrink-0 group-hover:text-green-500" />
                        {!collapsed && <span>System Health</span>}
                    </a>
                </div>
            </nav>
        </aside>
    );
};

import React, { useState } from 'react';
import { Badge } from "../ui/badge";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "../ui/popover";
import { RoleService, RoleDto } from '../../services/roleService';
import { Info } from 'lucide-react';

interface RoleBadgeProps {
    roleName: string;
    appId: string;
    variant?: "default" | "secondary" | "destructive" | "outline";
    className?: string;
    showIcon?: boolean;
}

export const RoleBadge: React.FC<RoleBadgeProps> = ({ roleName, appId, variant = "secondary", className, showIcon = false }) => {
    const [permissions, setPermissions] = useState<string[]>([]);
    const [loading, setLoading] = useState(false);
    const [open, setOpen] = useState(false);

    const handleOpenChange = async (isOpen: boolean) => {
        setOpen(isOpen);
        if (isOpen && permissions.length === 0) {
            setLoading(true);
            try {
                const roles = await RoleService.getAllRoles(appId);
                const role = roles.find(r => r.name === roleName);
                if (role && role.permissions) {
                    setPermissions(role.permissions);
                } else {
                    setPermissions(["No specific permissions found."]); // Or fetch from logic if needed
                }
            } catch (err) {
                setPermissions(["Failed to load permissions."]);
            } finally {
                setLoading(false);
            }
        }
    };

    return (
        <Popover open={open} onOpenChange={handleOpenChange}>
            <PopoverTrigger asChild>
                <div className={`cursor-pointer inline-flex items-center gap-1 ${className}`}>
                    {roleName !== 'User' && roleName !== 'Visitor' ? (
                        <Badge variant={variant} className="hover:bg-primary/80">
                            {roleName}
                            {showIcon && <Info size={12} className="ml-1" />}
                        </Badge>
                    ) : (
                        <span className="text-sm text-muted-foreground flex items-center gap-1 hover:underline">
                            {roleName} <Info size={12} />
                        </span>
                    )}
                </div>
            </PopoverTrigger>
            <PopoverContent className="w-80">
                <div className="grid gap-4">
                    <div className="space-y-2">
                        <h4 className="font-medium leading-none">{roleName} Permissions</h4>
                        <p className="text-muted-foreground text-xs">
                            Access capabilities for this role.
                        </p>
                    </div>
                    {loading ? (
                        <div className="text-xs">Loading...</div>
                    ) : (
                        <div className="grid gap-2">
                            {permissions.length > 0 ? (
                                <div className="flex flex-wrap gap-1">
                                    {permissions.map((perm, i) => (
                                        <Badge key={i} variant="outline" className="text-xs">{perm}</Badge>
                                    ))}
                                </div>
                            ) : (
                                <div className="text-xs text-muted-foreground">No permissions assigned.</div>
                            )}
                        </div>
                    )}
                </div>
            </PopoverContent>
        </Popover>
    );
};

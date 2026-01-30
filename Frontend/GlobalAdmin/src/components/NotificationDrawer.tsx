import React from 'react';
import { X, CheckCheck, BellOff } from 'lucide-react';
import { Notification } from '../types/notification';
import moment from 'moment';
import { cn } from '../lib/utils';

interface NotificationDrawerProps {
    isOpen: boolean;
    onClose: () => void;
    notifications: Notification[];
    onMarkAllRead: () => void;
    onMarkRead: (id: string) => void;
}

export const NotificationDrawer: React.FC<NotificationDrawerProps> = ({
    isOpen, onClose, notifications, onMarkAllRead, onMarkRead
}) => {
    return (
        <>
            {/* Backdrop */}
            <div
                className={cn(
                    "fixed inset-0 z-40 bg-background/80 backdrop-blur-sm transition-all duration-300",
                    isOpen ? "opacity-100" : "opacity-0 pointer-events-none"
                )}
                onClick={onClose}
            />

            {/* Side Panel */}
            <div className={cn(
                "fixed inset-y-0 right-0 z-50 w-full sm:w-96 border-l border-border bg-card shadow-2xl transition-transform duration-300 ease-in-out",
                isOpen ? "translate-x-0" : "translate-x-full"
            )}>
                <div className="flex h-full flex-col">
                    {/* Header */}
                    <div className="flex items-center justify-between border-b border-border bg-muted/30 p-4">
                        <h3 className="font-semibold text-lg">Notifications</h3>
                        <div className="flex items-center gap-2">
                            {notifications.some(n => !n.isRead) && (
                                <button
                                    onClick={onMarkAllRead}
                                    className="flex items-center gap-1 rounded px-2 py-1 text-xs font-medium text-primary hover:bg-primary/10 transition-colors"
                                    title="Mark all as read"
                                >
                                    <CheckCheck size={14} /> Mark all read
                                </button>
                            )}
                            <button
                                onClick={onClose}
                                className="rounded-full p-1 text-muted-foreground hover:bg-secondary hover:text-foreground transition-colors"
                            >
                                <X size={20} />
                            </button>
                        </div>
                    </div>

                    {/* List */}
                    <div className="flex-1 overflow-y-auto p-4 space-y-3">
                        {notifications.length === 0 ? (
                            <div className="flex h-full flex-col items-center justify-center text-center text-muted-foreground">
                                <BellOff size={48} className="mb-4 opacity-20" />
                                <p>No updates</p>
                            </div>
                        ) : (
                            notifications.map(n => (
                                <div
                                    key={n.id}
                                    onClick={() => {
                                        if (!n.isRead) onMarkRead(n.id);
                                        onClose();
                                    }}
                                    className={cn(
                                        "relative group cursor-pointer rounded-lg border p-4 transition-all hover:bg-accent/50",
                                        n.isRead
                                            ? "bg-card border-border opacity-70"
                                            : "bg-primary/5 border-primary/20 shadow-sm"
                                    )}
                                >
                                    {/* Unread Indicator */}
                                    {!n.isRead && (
                                        <div className="absolute top-4 right-4 h-2 w-2 rounded-full bg-primary ring-2 ring-background"></div>
                                    )}

                                    <div className="pr-6">
                                        <h4 className={cn(
                                            "mb-1 text-sm font-medium",
                                            !n.isRead ? "text-foreground" : "text-muted-foreground"
                                        )}>
                                            {n.title}
                                        </h4>
                                        <p className="line-clamp-2 text-xs text-muted-foreground">
                                            {n.message}
                                        </p>
                                        <div className="mt-2 flex items-center gap-2 text-[10px] text-muted-foreground/70">
                                            <span>{moment(n.createdAt).fromNow()}</span>
                                            {n.link && <span className="text-primary font-medium">View Details</span>}
                                        </div>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>
        </>
    );
};

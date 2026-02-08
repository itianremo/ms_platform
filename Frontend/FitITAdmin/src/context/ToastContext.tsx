import { toast } from "sonner"

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export const useToast = () => {
    const showToast = (message: string, type: ToastType = 'info', duration = 3000) => {
        switch (type) {
            case 'success':
                toast.success(message, { duration });
                break;
            case 'error':
                toast.error(message, { duration });
                break;
            case 'warning':
                toast.warning(message, { duration });
                break;
            case 'info':
            default:
                toast.info(message, { duration });
                break;
        }
    };

    const removeToast = (id: string) => {
        toast.dismiss(id);
    };

    return { showToast, removeToast };
};
// Deprecated Provider for backward compatibility (no-op)
export const ToastProvider = ({ children }: { children: React.ReactNode }) => <>{children}</>;

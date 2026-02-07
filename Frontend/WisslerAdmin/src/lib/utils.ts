import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs))
}

export function getInitials(name: string) {
    if (!name) return 'U';
    return name
        .split(' ')
        .filter(n => n)
        .map(n => n[0])
        .slice(0, 2)
        .join('')
        .toUpperCase();
}

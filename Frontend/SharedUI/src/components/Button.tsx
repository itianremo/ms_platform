import React from 'react';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: 'primary' | 'secondary' | 'danger';
    size?: 'sm' | 'md' | 'lg';
}

export const Button: React.FC<ButtonProps> = ({
    children,
    variant = 'primary',
    size = 'md',
    style,
    ...props
}) => {
    const baseStyle: React.CSSProperties = {
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: '0.5rem',
        fontWeight: 500,
        cursor: 'pointer',
        border: 'none',
        transition: 'all 0.2s',
        ...style
    };

    const variants = {
        primary: { backgroundColor: '#3b82f6', color: 'white' },
        secondary: { backgroundColor: '#64748b', color: 'white' },
        danger: { backgroundColor: '#ef4444', color: 'white' }
    };

    const sizes = {
        sm: { padding: '0.25rem 0.5rem', fontSize: '0.875rem' },
        md: { padding: '0.5rem 1rem', fontSize: '1rem' },
        lg: { padding: '0.75rem 1.5rem', fontSize: '1.125rem' }
    };

    return (
        <button
            style={{ ...baseStyle, ...variants[variant], ...sizes[size] }}
            {...props}
        >
            {children}
        </button>
    );
};

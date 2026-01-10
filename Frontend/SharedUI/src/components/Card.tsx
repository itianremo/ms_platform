import React from 'react';

export const Card: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children, style, ...props }) => {
    return (
        <div
            style={{
                backgroundColor: '#1e293b',
                border: '1px solid #334155',
                borderRadius: '0.5rem',
                padding: '1.5rem',
                boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)',
                color: '#f8fafc',
                ...style
            }}
            {...props}
        >
            {children}
        </div>
    );
};

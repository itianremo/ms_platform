import React from 'react';

export const Input: React.FC<React.InputHTMLAttributes<HTMLInputElement>> = ({ style, ...props }) => {
    return (
        <input
            style={{
                width: '100%',
                padding: '0.75rem',
                borderRadius: '0.5rem',
                border: '1px solid #334155',
                backgroundColor: '#020617',
                color: '#f8fafc',
                outline: 'none',
                ...style
            }}
            {...props}
        />
    );
};

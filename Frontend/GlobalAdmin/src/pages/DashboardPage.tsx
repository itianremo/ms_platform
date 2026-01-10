import React from 'react';

const DashboardPage = () => {
    return (
        <div>
            <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold', marginBottom: '2rem' }}>Dashboard Overview</h1>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
                <StatCard title="Total Users" value="1,240" trend="+12%" />
                <StatCard title="Active Apps" value="8" trend="+1" />
                <StatCard title="Revenue (MTD)" value="$12,450" trend="+5.4%" />
                <StatCard title="System Status" value="Healthy" color="var(--success)" />
            </div>

            <div style={{ marginTop: '2rem' }} className="card">
                <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '1rem' }}>Recent Activity</h3>
                <p style={{ color: 'var(--text-secondary)' }}>Audit logs placeholder...</p>
            </div>
        </div>
    );
};

const StatCard = ({ title, value, trend, color }: any) => (
    <div className="card">
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', marginBottom: '0.5rem' }}>{title}</p>
        <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between' }}>
            <h2 style={{ fontSize: '1.5rem', fontWeight: 'bold', color: color || 'var(--text-primary)' }}>{value}</h2>
            {trend && <span style={{ fontSize: '0.875rem', color: 'var(--success)' }}>{trend}</span>}
        </div>
    </div>
);

export default DashboardPage;

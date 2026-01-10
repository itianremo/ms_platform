import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Table } from '../components/Table';
import { Plus } from 'lucide-react';

interface AppConfig {
    id: string;
    name: string;
    description: string;
    baseUrl: string;
    isActive: boolean;
}

const AppsPage = () => {
    const [apps, setApps] = useState<AppConfig[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchApps();
    }, []);

    const fetchApps = async () => {
        try {
            const response = await axios.get('http://localhost:7032/apps'); // Gateway Route
            setApps(response.data);
        } catch (error) {
            console.error('Failed to fetch apps', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold' }}>Applications</h1>
                <button className="btn btn-primary">
                    <Plus size={18} style={{ marginRight: '0.5rem' }} />
                    New App
                </button>
            </div>

            <div className="card">
                <Table
                    data={apps}
                    columns={[
                        { header: 'Name', accessor: 'name' },
                        { header: 'Base URL', accessor: 'baseUrl' },
                        { header: 'Description', accessor: 'description' },
                        {
                            header: 'Status',
                            accessor: (app) => (
                                <span style={{
                                    padding: '0.25rem 0.5rem',
                                    borderRadius: '9999px',
                                    fontSize: '0.75rem',
                                    backgroundColor: app.isActive ? 'rgba(34, 197, 94, 0.1)' : 'rgba(239, 68, 68, 0.1)',
                                    color: app.isActive ? 'var(--success)' : 'var(--danger)'
                                }}>
                                    {app.isActive ? 'Active' : 'Inactive'}
                                </span>
                            )
                        }
                    ]}
                />
            </div>
        </div>
    );
};

export default AppsPage;

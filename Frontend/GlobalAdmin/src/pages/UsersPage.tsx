import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Table } from '../components/Table';
import { Plus } from 'lucide-react';

interface UserDto {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    roles: string[];
}

const UsersPage = () => {
    const [users, setUsers] = useState<UserDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = async () => {
        try {
            const response = await axios.get('http://localhost:7032/auth/users'); // Gateway Route
            setUsers(response.data);
        } catch (error) {
            console.error('Failed to fetch users', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <h1 style={{ fontSize: '1.875rem', fontWeight: 'bold' }}>Users</h1>
                <button className="btn btn-primary">
                    <Plus size={18} style={{ marginRight: '0.5rem' }} />
                    Create User
                </button>
            </div>

            <div className="card">
                <Table
                    data={users}
                    columns={[
                        { header: 'Name', accessor: (u) => `${u.firstName} ${u.lastName}` },
                        { header: 'Email', accessor: 'email' },
                        { header: 'Roles', accessor: (u) => u.roles.join(', ') || 'No Roles' },
                        {
                            header: 'Status',
                            accessor: (u) => (
                                <span style={{
                                    padding: '0.25rem 0.5rem',
                                    borderRadius: '9999px',
                                    fontSize: '0.75rem',
                                    backgroundColor: u.isActive ? 'rgba(34, 197, 94, 0.1)' : 'rgba(239, 68, 68, 0.1)',
                                    color: u.isActive ? 'var(--success)' : 'var(--danger)'
                                }}>
                                    {u.isActive ? 'Active' : 'Inactive'}
                                </span>
                            )
                        }
                    ]}
                />
            </div>
        </div>
    );
};

export default UsersPage;

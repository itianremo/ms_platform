import { useEffect, useState } from 'react';
import api from '../services/api';

export default function Dashboard() {
    const [profile, setProfile] = useState<any>(null);

    useEffect(() => {
        // Fetch profile on load
        // This is a placeholder as we don't have a direct "me" endpoint yet, 
        // but we can assume we might use the Users service in the future.
        // For now, just show static content.
        setProfile({ name: 'Admin User' });
    }, []);

    return (
        <div style={{ padding: '20px' }}>
            <h1>Dashboard</h1>
            <p>Welcome, {profile?.name}</p>
        </div>
    );
}

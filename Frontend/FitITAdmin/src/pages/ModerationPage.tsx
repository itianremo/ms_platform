import React, { useState, useEffect } from 'react';

interface ChatMessage {
    id: string;
    senderId: string;
    content: string;
    timestamp: string;
    isFlagged: boolean;
}

interface MediaItem {
    id: string;
    fileName: string;
    url: string;
    reason: string;
}

const ModerationPage: React.FC = () => {
    const [activeTab, setActiveTab] = useState<'text' | 'media'>('text');
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [mediaItems, setMediaItems] = useState<MediaItem[]>([]);
    const [loading, setLoading] = useState(false);

    const appId = '00000000-0000-0000-0000-000000000000'; // Hardcoded for prototype

    useEffect(() => {
        fetchData();
    }, [activeTab]);

    const fetchData = () => {
        setLoading(true);
        if (activeTab === 'text') {
            fetch(`/api/chat/flagged?appId=${appId}`)
                .then(res => res.json())
                .then(data => setMessages(data || []))
                .catch(err => console.error(err))
                .finally(() => setLoading(false));
        } else {
            fetch(`/api/media/flagged`)
                .then(res => res.json())
                .then(data => setMediaItems(data || []))
                .catch(err => console.error(err))
                .finally(() => setLoading(false));
        }
    };

    const handleApprove = (id: string, type: 'text' | 'media') => {
        alert(`Approved ${type} item: ${id} (Stub)`);
        // TODO: Call API to unflag or delete
    };

    const handleDelete = (id: string, type: 'text' | 'media') => {
        alert(`Deleted ${type} item: ${id} (Stub)`);
        // TODO: Call API to delete permanently
    };

    return (
        <div style={{ padding: '2rem' }}>
            <h2>Moderation Queue</h2>

            <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
                <button
                    onClick={() => setActiveTab('text')}
                    style={{
                        padding: '0.5rem 1rem',
                        backgroundColor: activeTab === 'text' ? '#2563eb' : '#e5e7eb',
                        color: activeTab === 'text' ? 'white' : 'black',
                        border: 'none', borderRadius: '4px', cursor: 'pointer'
                    }}
                >
                    Flagged Messages
                </button>
                <button
                    onClick={() => setActiveTab('media')}
                    style={{
                        padding: '0.5rem 1rem',
                        backgroundColor: activeTab === 'media' ? '#2563eb' : '#e5e7eb',
                        color: activeTab === 'media' ? 'white' : 'black',
                        border: 'none', borderRadius: '4px', cursor: 'pointer'
                    }}
                >
                    Flagged Media
                </button>
            </div>

            {loading ? <p>Loading...</p> : (
                <>
                    {activeTab === 'text' && (
                        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                            <thead>
                                <tr style={{ borderBottom: '1px solid #ddd' }}>
                                    <th style={{ padding: '0.5rem' }}>Sender</th>
                                    <th style={{ padding: '0.5rem' }}>Content</th>
                                    <th style={{ padding: '0.5rem' }}>Time</th>
                                    <th style={{ padding: '0.5rem' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {messages.map(msg => (
                                    <tr key={msg.id} style={{ borderBottom: '1px solid #eee' }}>
                                        <td style={{ padding: '0.5rem' }}>{msg.senderId}</td>
                                        <td style={{ padding: '0.5rem', color: 'red' }}>{msg.content}</td>
                                        <td style={{ padding: '0.5rem' }}>{msg.timestamp}</td>
                                        <td style={{ padding: '0.5rem' }}>
                                            <button onClick={() => handleApprove(msg.id, 'text')} style={{ marginRight: '0.5rem' }}>Approve</button>
                                            <button onClick={() => handleDelete(msg.id, 'text')} style={{ color: 'red' }}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                                {messages.length === 0 && <tr><td colSpan={4} style={{ padding: '1rem', textAlign: 'center' }}>No flagged messages.</td></tr>}
                            </tbody>
                        </table>
                    )}

                    {activeTab === 'media' && (
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '1rem' }}>
                            {mediaItems.map(item => (
                                <div key={item.id} style={{ border: '1px solid #ddd', padding: '0.5rem', borderRadius: '4px' }}>
                                    <img src={item.url} alt={item.fileName} style={{ width: '100%', height: '150px', objectFit: 'cover', backgroundColor: '#eee' }} />
                                    <p style={{ fontWeight: 'bold', margin: '0.5rem 0' }}>{item.fileName}</p>
                                    <p style={{ color: 'red', fontSize: '0.8rem' }}>Reason: {item.reason}</p>
                                    <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                                        <button onClick={() => handleApprove(item.id, 'media')} style={{ flex: 1 }}>Approve</button>
                                        <button onClick={() => handleDelete(item.id, 'media')} style={{ flex: 1, color: 'red' }}>Delete</button>
                                    </div>
                                </div>
                            ))}
                            {mediaItems.length === 0 && <p>No flagged media.</p>}
                        </div>
                    )}
                </>
            )}
        </div>
    );
};

export default ModerationPage;
